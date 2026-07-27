using AIS.Intervene;
using BeauUtil;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AIS.Model
{
    /// <summary>
    /// Data used by ActionEffectSpecifierSystem to manage action decisions
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ModelTag : MonoBehaviour
    {
        public ActionTarget TargetType;
        public SpriteRenderer Highlight;
        public Image UIHighlight;
        public Color NormalColor; // TODO: move to lookup
        public Color SelectedColor;  // TODO: move to lookup
        [Required] public GameObject QueriableObj;

        [HideInInspector] public bool IsPhantom;

        #region Unity Callbacks

        public void OnEnable()
        {
            Game.Scenes.QueueOnEnable(this, () => ModelTagMgr.Instance.Register(this));
        }

        public void OnDestroy()
        {
            if (AisGame.IsShuttingDown) { return; }

            ModelTagMgr.Instance.Deregister(this);
        }

        #endregion // Unity Callbacks

        #region Visuals

        public void ShowHighlight(bool isCustom = false)
        {
            if (Highlight != null) {
                Highlight.enabled = true;
            }
            else if (UIHighlight != null)
            {
                UIHighlight.enabled = true;
            }
            if (!isCustom)
            {
                SetNormalHighlight();
            }
        }

        public void HideHighlight()
        {
            if (Highlight != null) {
                Highlight.enabled = false;
            }
            else if (UIHighlight != null) {
                UIHighlight.enabled = false;
            }
            SetNormalHighlight();
        }

        public void SetNormalHighlight()
        {
            if (Highlight != null) {
                Highlight.color = NormalColor;
            }
            else if (UIHighlight != null) {
                UIHighlight.color = NormalColor;
            }
        }

        public void SetCustomHighlight(Color customColor)
        {
            if (Highlight != null)
            {
                Highlight.color = customColor;
            }
            else if (UIHighlight != null)
            {
                UIHighlight.color = customColor;
            }
        }

        public void SetSelectedHighlight()
        {
            if (Highlight != null) {
                Highlight.color = SelectedColor;
            }
            else if (UIHighlight != null) {
                UIHighlight.color = SelectedColor;
            }
        }

        #endregion // Visuals
    }
}
