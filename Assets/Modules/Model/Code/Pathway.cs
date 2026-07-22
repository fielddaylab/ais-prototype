using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using AIS.Intervene;
using AIS.Narrative;
using TMPro;
using Debug = UnityEngine.Debug;

namespace AIS.Model
{
    [Serializable]
    public struct PathwaySetupData
    {
        public SerializedHash32 PathwayId;
        [EcosystemId] public SerializedHash32 OrigEcosystemId;
        [EcosystemId] public SerializedHash32 DestEcosystemId;
        public bool IsBidirectional;
        public Vector2 Pos;
        public float Rotation;
        public Sprite Sprite;

        public RateType TransferRateType;
        public float StartingTransferRate;
        public float StartingTriggerChance;
        public PathwayType PathwayType;
        public bool IsNotHidden;
        public PathDir StartingDir;
    }

    [Flags]
    public enum PathwayType
    {
        // Currents = 0x01,
        PetTrade = 0x02,
        BoatHulls = 0x04,
        BaitBuckets = 0x08,
        BallastWater = 0x10,
        Aquarium = 0x20,
        Upstream = 0x40,
        Downstream = 0x80,
    }

    public enum RateType
    {
        Ratio,
        Fixed
    }

    [Flags]
    public enum PathwayEffectType
    {
        BlockAll = 0x01,
        Trapped = 0x02,
    }

    public struct PathwayEffect
    {
        public string EffectId;
        public PathwayEffectType EffectType;
        public ActionTarget TargetType;
        public float Value;
    }

    /// <summary>
    /// A standing boost to how many individuals the traps on a pathway catch.
    /// Kept separate from the Trapped effects themselves so a boost can be played before any trap
    /// is installed, and so it carries over to every trap that later lands on the pathway.
    /// Value is signed: positive catches more, negative catches fewer.
    /// Fixed shifts the catch; Ratio scales the trap's own strength (1.0 = catches twice as many).
    /// </summary>
    [Serializable]
    public struct TrapModifier
    {
        public float Value;
        public ModifierType ModType;
    }

    public enum PathDir
    {
        None,
        Input,
        Output
    }

    public class Pathway : MonoBehaviour, IReducible, IIncreasable, IRemovable, IRevealable, ITrapModifiable, ISimDetail
    {
        #region Inspector

        public StringHash32 PathwayId;
        public SpriteRenderer MainRenderer;
        public SpriteRenderer PathwayTypeBGRenderer;
        public SpriteRenderer PathwayTypeRenderer;
        public TMP_Text TransferRateText;

        public List<PathwayEffect> OnTryMove = new List<PathwayEffect>();

        // Standing trap boosts applied by action cards
        public List<TrapModifier> TrapModifiers = new List<TrapModifier>();

        public SerializedHash32 OrigEcosystemId;
        public SerializedHash32 DestEcosystemId;
        public bool IsBidirectional { get; private set; }
        public PathwayType PathwayType { get; private set; }
        public RateType TransferRateType { get; private set; }
        public float TransferTriggerChance { get; private set; }
        public float TransferRate { get; private set; }
        public bool IsHidden { get; private set; }
        public PathDir Dir { get; private set; }

        #endregion // Inspector

        // True while IsHidden is the registry's evidence mask rather than the setup data's own request.
        private bool m_RegistryObscured;

        public void LoadData(PathwaySetupData setupData)
        {
            PathwayId = setupData.PathwayId;
            OrigEcosystemId = setupData.OrigEcosystemId;
            DestEcosystemId = setupData.DestEcosystemId;
            IsBidirectional = setupData.IsBidirectional;
            PathwayType = setupData.PathwayType;
            m_RegistryObscured = false;
            SetIsHidden(!setupData.IsNotHidden);
            Dir = setupData.StartingDir;

            TrapModifiers.Clear();

            this.transform.position = setupData.Pos;
            var angles = MainRenderer.transform.localEulerAngles;
            angles.z = setupData.Rotation;
            MainRenderer.transform.localEulerAngles = angles;
            MainRenderer.sprite = setupData.Sprite;
            MainRenderer.sortingOrder = InvasionModelSorting.PATHWAY_SORTING;
            PathwayTypeBGRenderer.sortingOrder = InvasionModelSorting.PATHWAY_ICON_BG_SORTING;
            PathwayTypeRenderer.sortingOrder = InvasionModelSorting.PATHWAY_ICON_SORTING;

            TransferRateType = setupData.TransferRateType;
            SetTransferRate(setupData.StartingTransferRate);
            SetTriggerChance(setupData.StartingTriggerChance);

            UpdateVisuals();

            // Masks or hides this pathway if its type is gated behind evidence the player lacks.
            // Ungated pathways keep whatever IsNotHidden asked for.
            InvasionModel.Instance?.SimDetailRegistry?.ApplyTo(this);
        }

        public void AddPathwayType(PathwayType type)
        {
            PathwayType |= type;

            UpdateVisuals();
        }

        public void RemovePathwayType(PathwayType type)
        {
            PathwayType &= ~type;

            UpdateVisuals();
        }

        public void SetTransferRate(float newRate)
        {
            TransferRate = newRate;

            UpdateVisuals();
        }

        public void AdjustTransferRate(float adjustAmt)
        {
            bool wasShut = TransferRate == 0;

            TransferRate += adjustAmt;
            TransferRate = Mathf.Max(TransferRate, 0);
            
            if (!wasShut && TransferRate == 0)
            {
                // TODO: trigger pathway shutting visuals
            }
            else if (wasShut && TransferRate != 0)
            {
                // TODO: trigger pathway opening visuals
            }

            UpdateVisuals();
        }

        public void SetTriggerChance(float newChance)
        {
            TransferTriggerChance = newChance;
        }

        public void SetIsHidden(bool isHidden)
        {
            IsHidden = isHidden;

            UpdateVisuals();
        }

        public void AddEffectOnTryMove(PathwayEffect toAdd)
        {
            OnTryMove.Add(toAdd);

            UpdateVisuals();
        }

        public bool OnTryMoveContains(string effectId)
        {
            foreach (var effect in OnTryMove)
            {
                if (effect.EffectId.Equals(effectId))
                {
                    return true;
                }
            }

            return false;
        }

        #region Traps

        /// <summary>
        /// True when any effect on this pathway traps individuals trying to move through it.
        /// Keys off the effect type rather than a specific effect id, so every trap card counts.
        /// </summary>
        public bool IsTrapped()
        {
            foreach (var effect in OnTryMove)
            {
                if ((effect.EffectType & PathwayEffectType.Trapped) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// How many individuals this pathway's traps catch in total when it activates, boosts included.
        /// </summary>
        public int GetTotalTrapAmt()
        {
            int total = 0;

            foreach (var effect in OnTryMove)
            {
                if ((effect.EffectType & PathwayEffectType.Trapped) != 0)
                {
                    total += ApplyTrapModifiers(Mathf.FloorToInt(effect.Value));
                }
            }

            return total;
        }

        public void AddTrapModifier(float value, ModifierType modType)
        {
            TrapModifier modifier = new TrapModifier();
            modifier.Value = value;
            modifier.ModType = modType;

            TrapModifiers.Add(modifier);

            UpdateVisuals();
        }

        /// <summary>
        /// Boosts how many individuals a single trap on this pathway catches when it fires.
        /// Ratios all scale the trap's unmodified strength, so two +1.0 ratios triple the catch rather than quadrupling it.
        /// </summary>
        public int ApplyTrapModifiers(int baseAmt)
        {
            if (TrapModifiers.Count == 0) { return baseAmt; }

            float modifiedAmt = baseAmt;

            foreach (var modifier in TrapModifiers)
            {
                if (modifier.ModType == ModifierType.Ratio)
                {
                    modifiedAmt += baseAmt * modifier.Value;
                }
                else
                {
                    modifiedAmt += modifier.Value;
                }
            }

            // rounded down, never negative
            return Mathf.Max(0, Mathf.FloorToInt(modifiedAmt));
        }

        #endregion // Traps

        #region Interfaces

        // IReducible

        public bool TryReduce(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustTransferRate(-amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                float newRate = TransferRate - TransferRate * amts[0];
                SetTransferRate(newRate);

                return true;
            }

            return false;
        }

        // IIncreasable

        public bool TryIncrease(List<float> amts, ModifierType modType)
        {
            if (modType == ModifierType.Fixed)
            {
                AdjustTransferRate(amts[0]);

                return true;
            }
            else if (modType == ModifierType.Ratio)
            {
                float newRate = TransferRate + TransferRate * amts[0];
                SetTransferRate(newRate);

                return true;
            }

            return false;
        }

        // IRemovable

        public bool TryRemove()
        {
            AdjustTransferRate(-TransferRate);

            return true;
        }

        // ITrapModifiable

        public bool TryModifyTrap(List<float> amts, ModifierType modType)
        {
            if (amts.Count == 0) { return false; }

            // Stored on the pathway rather than on its Trapped effects, so a boost played before any
            // trap exists still pays off once one is installed here.
            AddTrapModifier(amts[0], modType);

            return true;
        }

        // IRevealable

        public bool TryReveal()
        {
            SetIsHidden(false);

            return true;
        }

        // ISimDetail

        public void SetDisplay(SimDetailDisplay display, PlayerStatId lockSuit)
        {
            this.gameObject.SetActive(display != SimDetailDisplay.Hidden);

            // IsHidden already renders the "present but unknown" look: an unmarked type icon and
            // no transfer rate. That is exactly what an obscured pathway should show.
            if (display == SimDetailDisplay.Obscured)
            {
                m_RegistryObscured = true;
                SetIsHidden(true);
            }
            else if (m_RegistryObscured)
            {
                // Only lifts a mask the registry put here. A pathway the setup data asked to hide
                // stays hidden until something explicitly reveals it.
                m_RegistryObscured = false;
                SetIsHidden(false);
            }
        }

        public StringHash32 Id()
        {
            return PathwayId;
        }

        #endregion // Interfaces

        #region Visuals

        private void UpdateVisuals()
        {
            if (IsHidden)
            {
                // update icon to hidden
                PathwayTypeRenderer.sprite = ModelSpriteLookup.Instance.LookupPathwayIcon(PathwayType, true);
                // hide transfer rate
                TransferRateText.gameObject.SetActive(false);
            }
            else
            {
                // update icon
                PathwayTypeRenderer.sprite = ModelSpriteLookup.Instance.LookupPathwayIcon(PathwayType);
                // update transfer rate
                TransferRateText.gameObject.SetActive(true);
                string text = TransferRate + " per turn";
                if (TransferRateType == RateType.Ratio) {
                    text = TransferRate * 100 + "% per turn";
                }
                // update trapped visuals
                if (IsTrapped())
                {
                    text += "\n(Traps " + GetTotalTrapAmt() + ")";
                    if (ColorUtility.TryParseHtmlString("#f3b7b7", out Color trapColor)) {
                        PathwayTypeBGRenderer.color = trapColor;
                    }
                }
                else if (TrapModifiers.Count > 0)
                {
                    // boosted before any trap landed here -- it pays off once one does
                    text += "\n(Traps boosted)";
                }
                TransferRateText.SetText(text);
            }
        }
        #endregion // Visuals
    }

    public static class PathwayUtility
    {
        public static PathwayType StrToPathwayType(string toParse)
        {
            PathwayType type = 0;

            if (Enum.TryParse<PathwayType>(toParse, true, out PathwayType result))
            {
                return result;
            }

            return type;
        }

        public static PathwayEffectType StrToPathwayEffectType(string toParse)
        {
            PathwayEffectType type = 0;

            if (Enum.TryParse<PathwayEffectType>(toParse, true, out PathwayEffectType result))
            {
                return result;
            }

            return type;
        }
    }
}