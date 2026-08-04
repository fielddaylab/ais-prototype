using AIS.Intervene;
using AIS.Model;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.UI;
using Leaf.Runtime;
using System.Collections;

namespace AIS.Narrative {
    static public class ScriptHooks
    {
        [LeafMember("Stat")]
        static public int GetStat(PlayerStatId statId)
        {
            return Find.State<PlayerStats>().StatBlock[statId];
        }

        [LeafMember("StatCheck")]
        static public bool StatCheck(PlayerStatId statId, int value) {
            var evidencePanel = Find.Panel<EvidenceDisplayPanel>();
            var inv = Find.State<PlayerInventory>();
            evidencePanel.RecomputeStats(inv);
            return Find.State<PlayerStats>().StatBlock[statId] >= value;
        }

        [LeafMember("SetStat")]
        static public void SetStat(PlayerStatId statId, int value)
        {
            ref PlayerStatBlock statBlock = ref Find.State<PlayerStats>().StatBlock;
            statBlock[statId] = (sbyte)PlayerStatBlock.Clamp(value);
        }

        [LeafMember("SilentAdjustStat")]
        static public void SilentAdjustStat(PlayerStatId statId, int adjustment)
        {
            ref PlayerStatBlock statBlock = ref Find.State<PlayerStats>().StatBlock;
            int currentStat = statBlock[statId];
            if (adjustment != 0)
            {
                currentStat = PlayerStatBlock.Clamp(currentStat + adjustment);
                statBlock[statId] = (sbyte)currentStat;
            }
        }

        [LeafMember("AdjustStat")]
        static public IEnumerator AdjustStat([BindThread] ScriptThread thread, PlayerStatId statId, int adjustment)
        {
            PlayerStats stats = Find.State<PlayerStats>();
            PlayerInventory inv = Find.State<PlayerInventory>();
            PlayerStatBlock statBlock = stats.StatBlock;
            int currentStat = statBlock[statId];
            int originalValue = currentStat;
            if (adjustment != 0)
            {
                currentStat = PlayerStatBlock.Clamp(currentStat + adjustment);
                statBlock[statId] = (sbyte)currentStat;
                stats.StatBlock = statBlock;

                if (thread.IsSkipping())
                {
                    yield break;
                }

                DialogueColumn column = (DialogueColumn)thread.GetPrinter();
                if (column)
                {
                    yield return TextUtility.DisplayStatUpdate(column, statId, originalValue, currentStat);
                    //yield return EnsureModelVisible(inv);
                    yield return column.CompleteLine();
                }
                else
                {
                    //yield return EnsureModelVisible(inv);
                }
            }
        }

        [LeafMember("BeginIntervention")]
        static public void LoadIntoInterventionScene()
        {
            Game.Scenes.LoadMainScene(SceneReference.FromName("Intervene"));
        }

        [LeafMember("GiveEvidenceChip")]
        static public IEnumerator ScriptGiveEvidence([BindThread] ScriptThread thread, StringHash32 id)
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            if (inv.EvidenceChips.Add(id))
            {
                InvasionModel.Instance?.SimDetailRegistry?.TryEnqueueReveal(id);
                PlayerUtility.OnCardCollected(id);

                if (thread.IsSkipping())
                {
                    yield break;
                }

                DialogueColumn column = (DialogueColumn)thread.GetPrinter();
                if (column)
                {
                    NewCardElement card = TextUtility.SpawnEvidenceCard(column, id);
                    yield return TextUtility.WaitForConfirm(card);
                    yield return EnsureNotesVisible(inv);
                    yield return TextUtility.FlyChipToToolbar(card, Find.GuiModule<ToolbarPanel>().EvidenceButton);
                }
                else
                {
                    yield return EnsureNotesVisible(inv);
                }
            }
        }

        [LeafMember("GiveActionCard")]
        static public IEnumerator ScriptGiveAction([BindThread] ScriptThread thread, StringHash32 id)
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            if (inv.ActionCards.Add(id))
            {
                PlayerUtility.OnCardCollected(id);

                if (thread.IsSkipping())
                {
                    yield break;
                }

                DialogueColumn column = (DialogueColumn)thread.GetPrinter();
                if (column)
                {
                    NewCardElement card = TextUtility.SpawnActionCard(column, id);
                    yield return TextUtility.WaitForConfirm(card);
                    yield return EnsureNotesVisible(inv);
                    yield return TextUtility.FlyCardToToolbar(card, Find.GuiModule<ToolbarPanel>().EvidenceButton);
                }
                else
                {
                    yield return EnsureNotesVisible(inv);
                }
            }
        }

        [LeafMember("SetScenarioData")]
        static public IEnumerator ScriptSetScenarioData([BindThread] ScriptThread thread, StringHash32 id)
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            if (thread.IsSkipping())
            {
                yield break;
            }

            DialogueColumn column = (DialogueColumn)thread.GetPrinter();
            if (column)
            {
                NewCardElement card = TextUtility.SpawnScenarioCard(column, id);
                yield return TextUtility.WaitForConfirm(card);
                yield return EnsureNotesVisible(inv);
                yield return TextUtility.FlyScenarioToToolbar(card, Find.GuiModule<ToolbarPanel>().EvidenceButton);
            }
            else
            {
                yield return EnsureNotesVisible(inv);
            }
        }

        [LeafMember("EnableFieldNotesButton")]
        static public IEnumerator EnableNotesButton()
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            yield return EnsureNotesVisible(inv);
        }

        [LeafMember("EnableMapButton")]
        static public IEnumerator EnableMapButton()
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            yield return EnsureMapVisible(inv);
        }

        // Opens the map panel, revealing the map toolbar button first if it is still hidden.
        // Also backs the "OpenMap" choice data flag (see the MapChoice macro in Header.leaf).
        [LeafMember("OpenMap")]
        static public IEnumerator OpenMap()
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            yield return EnsureMapVisible(inv);
            PanelUtility.OpenMap();
        }

        [LeafMember("DisableMapButton")]
        static public void DisableMapButton()
        {
            var toolbar = Find.GuiModule<ToolbarPanel>();
            toolbar.MapButton.Fader.blocksRaycasts = false;
            toolbar.MapButton.Fader.alpha = 0;
        }

        //[LeafMember("EnableModelButton")]
        //static public IEnumerator EnableModelButton()
        //{
        //    PlayerInventory inv = Find.State<PlayerInventory>();
        //    yield return EnsureModelVisible(inv);
        //}

        static private IEnumerator EnsureNotesVisible(PlayerInventory inv)
        {
            if ((inv.ToolbarItems & PlayerToolbarMask.Evidence) == 0)
            {
                inv.ToolbarItems |= PlayerToolbarMask.Evidence;
                ToolbarPanel toolbar = Find.GuiModule<ToolbarPanel>();
                return ToolbarPanel.UnlockToolbarButtonAnimation(toolbar.EvidenceMissing, toolbar.EvidenceButton);
            }
            return null;
        }

        static private IEnumerator EnsureMapVisible(PlayerInventory inv)
        {
            ToolbarPanel toolbar = Find.GuiModule<ToolbarPanel>();

            if ((inv.ToolbarItems & PlayerToolbarMask.Map) == 0)
            {
                inv.ToolbarItems |= PlayerToolbarMask.Map;
                return ToolbarPanel.UnlockToolbarButtonAnimation(toolbar.MapMissing, toolbar.MapButton);
            }
            else if (toolbar.MapButton.Fader.alpha == 0)
            {
                return ToolbarPanel.UnlockToolbarButtonAnimation(toolbar.MapMissing, toolbar.MapButton);
            }
            return null;
        }

        // TODO: stats are not a toolbar tab anymore
        //static private IEnumerator EnsureModelVisible(PlayerInventory inv)
        //{
        //    if ((inv.ToolbarItems & PlayerToolbarMask.Model) == 0)
        //    {
        //        inv.ToolbarItems |= PlayerToolbarMask.Model;
        //        ToolbarPanel toolbar = Find.GuiModule<ToolbarPanel>();
        //        return ToolbarPanel.UnlockToolbarButtonAnimation(toolbar.ModelMissing, toolbar.ModelButton);
        //    }
        //    return null;
        //}

        [LeafMember("ClearVisibleLines")]
        static public void ScriptClearVisibleLines([BindThread] ScriptThread thread)
        {
            DialogueColumn column = (DialogueColumn)thread.GetPrinter();
            if (!column)
            {
                column = DialogueColumn.Instance;
                if (!column)
                {
                    return;
                }
            }

            TextUtility.ClearAllLines(column.Layout);
        }

        [LeafMember("HasEvidenceCard")]
        static public bool HasEvidenceCard(StringHash32 id)
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            return inv.EvidenceChips.Contains(id);
        }

        [LeafMember("HasEvidence")]
        static public bool HasEvidence(StringHash32 id)
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            return inv.EvidenceChips.Contains(id);
        }

        [LeafMember("HasActionCard")]
        static public bool HasActionCard(StringHash32 id)
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            return inv.ActionCards.Contains(id);
        }

        [LeafMember("HideToolbar")]
        static public void HideToolbar()
        {
            var toolbar = Find.GuiModule<ToolbarPanel>();
            toolbar.gameObject.SetActive(false);

        }

        [LeafMember("RevealToolbar")]
        static public void RevealToolbar()
        {
            var toolbar = Find.GuiModule<ToolbarPanel>();
            toolbar.gameObject.SetActive(true);
        }

        /*
         * Lock every location. To unlock a location in thread, SetInThreadLocks disables every 
         * location on the map at first (to clean up previously unlocked places), and need to 
         * unlock designed location by calling UnlockLocation next.
         */
        [LeafMember("SetInThreadLocks")]
        static public void SetInThreadLocks()
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            mapPanel.SetInThreadLocks();
        }

        [LeafMember("UnlockLocation")]
        static public void UnlockLocation(MapLocation location)
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            mapPanel.UnlockLocation(location);
        }

        [LeafMember("SetTravelTime")]
        static public void SetTravelTimeFromCurrentTo(MapLocation location, int chunks, bool isRevealed = true)
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            mapPanel.SetTravelTimeFromCurrentTo(location, chunks, isRevealed);
        }

        // Advertises the card waiting at a location on the map. The id may name either an
        // EvidenceCard or an ActionCard; the card's type picks the pip color and its suit the icon.
        [LeafMember("SetNextAssetAtLocation")]
        static public void SetNextAssetAtLocation(MapLocation location, StringHash32 assetId)
        {
            if (!TryResolveAsset(assetId, out NextAssetInfo asset))
            {
                Log.Warn("[ScriptHooks] No evidence or action card found with id '{0}'.", assetId.ToDebugString());
                return;
            }

            var mapPanel = Find.Panel<MapDisplayPanel>();
            mapPanel.SetNextAssetAtLocation(location, asset);
        }

        [LeafMember("ClearNextAssetAtLocation")]
        static public void ClearNextAssetAtLocation(MapLocation location)
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            mapPanel.ClearNextAssetAtLocation(location);
        }

        // Evidence cards are NamedAssets, while action cards are parsed at runtime into
        // ActionCardsState, so each type is resolved from a different registry.
        static private bool TryResolveAsset(StringHash32 assetId, out NextAssetInfo asset)
        {
            if (Game.Assets.TryGetNamed(assetId, out EvidenceCard evidence))
            {
                asset = new NextAssetInfo() { Id = assetId, Type = NextAssetType.Evidence, Suit = evidence.Suit };
                return true;
            }

            if (Game.SharedState.TryGet(out ActionCardsState cardsState)
                && cardsState.AllActionCards.TryGetValue(assetId, out ActionCardData action))
            {
                asset = new NextAssetInfo() { Id = assetId, Type = NextAssetType.Action, Suit = action.Suit };
                return true;
            }

            asset = default;
            return false;
        }

        [LeafMember("SetTime")]
        static public void SetTime(int chunks)
        {
            PlayerUtility.SetTime(chunks);
        }

        [LeafMember("DecreaseTime")]
        static public void DecreaseTime(int chunks) {
            PlayerUtility.DecreaseTime(chunks);
        }

        [LeafMember("HasTime")]
        static public bool HasTime(int chunks = 1) {
            return Find.State<PlayerInventory>().TimeRemaining >= chunks;
        }

        [LeafMember("IsOutOfTime")]
        static public bool IsOutOfTime() {
            return Find.State<PlayerInventory>().TimeRemaining <= 0;
        }

        // If the player is out of time, redirect the current thread to its OutOfTime fallback
        // node (honoring any per-file <basePath>.Fallback.OutOfTime override); otherwise a no-op.
        [LeafMember("CheckIfOutOfTime")]
        static public void CheckIfOutOfTime([BindThread] ScriptThread thread) {
            if (!IsOutOfTime()) {
                return;
            }
            DialogueChoiceUtility.GotoFallback(thread, true);
        }

        [LeafMember("HasChoices")]
        static public bool HasChoices([BindThread] ScriptThread thread) {
            return thread.AvailableOptionCount(DialogueChoiceUtility.SelectablePredicate) > 0;
        }

        [LeafMember("ReturnTo")]
        static public void ReturnTo(int index)
        {
            var mapPanel = Find.Panel<MapDisplayPanel>();
            mapPanel.ReturnTo(index);
        }
    }
}