using System;
using System.Collections;
using System.Collections.Generic;
using AIS.Intervene;
using AIS.Model;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class EvidenceDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerInventory> {
        [Serializable] public sealed class UICardPool : SerializablePool<UICard> { }

        public Button ToModelButton;
        public Button CloseButton;

        [Header("Sim Reveal Queue")]
        public CanvasGroup RevealQueueGroup;
        public TMP_Text RevealQueueCountText;

        [Header("Data")]
        public ScenarioData CurrScenario;

        [Header("Evidence")]
        public EvidenceDisplayWidget[] Widgets;

        [Header("Stats")]
        public StatDisplayWidget[] Stats;

        [Header("Scenario Overview")]
        public Image ScenarioIllustration;
        public TMP_Text ScenarioOverviewText;

        [Header("Action Cards")]
        public UICardPool ActionCardPool;       // spawns deck cards into the custom layout group
        public UICard FocusSlot;                 // larger card shown in the rightmost focus column
        public GameObject NoActionCardsDefault;  // default text shown when the player has no action cards
        public TMP_Text FocusDescriptionText;    // bottom-right extra text for the focused/hovered card

        [Header("Focus Behavior")]
        [Tooltip("false = hover a deck card to preview it in the focus slot; true = click to focus/return/swap.")]
        public bool ClickToFocus = false;
        public TweenSettings FocusSlideAnim = new TweenSettings(0.2f, Curve.Smooth);
        [Tooltip("Off-screen X (anchored, focus slot local space) the card slides from / to.")]
        public float FocusHiddenX = 600f;

        // Animation routines (fields so they can be manually started/stopped).
        private Routine m_FocusInRoutine;
        private Routine m_FocusOutRoutine;

        // Click-mode state: which spawned card (by index) is currently focused, -1 if none.
        [NonSerialized] private int m_FocusedCardIndex = -1;

        // Cached action card data parallel to ActionCardPool.ActiveObjects, indexed by CardIndex.
        private readonly List<ActionCardData> m_SpawnedCardData = new List<ActionCardData>(24);

        protected override void Awake() {
            base.Awake();
            Stats = GetComponentsInChildren<StatDisplayWidget>();
            ToModelButton.onClick.AddListener(PanelUtility.ToggleModel);
            CloseButton.onClick.AddListener(Hide);
            Hide();
        }

        public void Populate(in PlayerInventory parms) {
            PopulateEvidence(parms);
            PopulateScenario();
            RecomputeStats(parms);
            PopulateStats(Find.State<PlayerStats>().StatBlock);
            PopulateActionCards(parms);
            UpdateRevealQueueDisplay();
        }

        private void UpdateRevealQueueDisplay() {
            int count = InvasionModel.Instance?.SimDetailRegistry?.RevealQueue.Count ?? 0;
            RevealQueueGroup.alpha = count > 0 ? 1 : 0;
            RevealQueueCountText.SetText(count.ToString());
        }

        #region Evidence

        private void PopulateEvidence(in PlayerInventory parms) {
            int widgetIndex = 0;
            foreach (var evidenceId in parms.EvidenceChips) {
                if (widgetIndex >= Widgets.Length) { break; }

                EvidenceCard data = Find.NamedAsset<EvidenceCard>(evidenceId);

                EvidenceDisplayWidget widget = Widgets[widgetIndex++];
                widget.gameObject.SetActive(true);
                EvidenceDisplayWidgetUtility.Populate(widget, data);
            }

            for (; widgetIndex < Widgets.Length; widgetIndex++) {
                Widgets[widgetIndex].gameObject.SetActive(false);
            }
        }

        #endregion // Evidence

        #region Scenario

        private void PopulateScenario() {
            if (ScenarioIllustration != null) ScenarioIllustration.sprite = CurrScenario.Illustration;
            if (ScenarioOverviewText != null) ScenarioOverviewText.SetText(CurrScenario.OverviewText);
        }

        #endregion // Scenario

        #region Stats

        // Recomputes the single source-of-truth PlayerStats.StatBlock from the suits of every
        // card the player currently holds (evidence chips + action cards), then writes it back.
        // Runs on panel open so the displayed stats always match current inventory.
        private void RecomputeStats(in PlayerInventory inv) {
            PlayerStatBlock block = default;

            foreach (var id in inv.EvidenceChips) {
                EvidenceCard card = Find.NamedAsset<EvidenceCard>(id);
                if (card != null && card.Suit != PlayerStatId.Invalid) {
                    block[card.Suit] += 1;
                }
            }

            if (Game.SharedState.TryGet(out ActionCardsState actionCards)) {
                foreach (var id in inv.ActionCards) {
                    if (actionCards.AllActionCards.TryGetValue(id, out ActionCardData data)
                        && data.Suit != PlayerStatId.Invalid) {
                        block[data.Suit] += 1;
                    }
                }
            }

            PlayerStatBlock.Clamp(ref block);
            Find.State<PlayerStats>().StatBlock = block;
        }

        public void PopulateStats(in PlayerStatBlock parms) {
            for (int i = 0; i < Stats.Length; i++) {
                StatDisplayWidget widget = Stats[i];
                // widget.StatValueCounter.SetValue(parms[widget.StatId], GuiWidgetUpdateFlags.Force | GuiWidgetUpdateFlags.NoAnimation);

                widget.CountText.SetText(parms[widget.StatId].ToStringLookup());
                widget.Label.SetText(widget.StatId.ToString());
            }
        }

        #endregion // Stats

        #region Action Cards

        private void PopulateActionCards(in PlayerInventory parms) {
            // Return any previously-spawned cards to the pool and reset focus state.
            ClearFocus();
            ActionCardPool.Reset();
            m_SpawnedCardData.Clear();

            ActionCardsState actionCards;
            if (!Game.SharedState.TryGet(out actionCards) || parms.ActionCards.Count == 0) {
                ShowNoActionCards(true);
                return;
            }

            int index = 0;
            foreach (var id in parms.ActionCards) {
                if (!actionCards.AllActionCards.TryGetValue(id, out ActionCardData data)) {
                    continue;
                }

                UICard card = ActionCardPool.Alloc();
                ActionCardUtility.PopulateCardUI(card, data);
                m_SpawnedCardData.Add(data);

                WireCardInput(card, index);
                index++;
            }

            ShowNoActionCards(m_SpawnedCardData.Count == 0);

            // Focus slot starts empty in both modes.
            SetFocusSlotVisible(false);
            SetFocusDescription(string.Empty);
        }

        private void ShowNoActionCards(bool show) {
            if (NoActionCardsDefault != null) {
                NoActionCardsDefault.SetActive(show);
            }
        }

        // Attaches/refreshes pointer handling on a spawned card for the current focus mode.
        private void WireCardInput(UICard card, int index) {
            FieldNoteCardPointer pointer = card.GetComponent<FieldNoteCardPointer>();
            if (pointer == null) {
                pointer = card.gameObject.AddComponent<FieldNoteCardPointer>();
            }

            pointer.CardIndex = index;

            if (ClickToFocus) {
                pointer.OnEnter = null;
                pointer.OnExit = null;
                pointer.OnClick = HandleCardClicked;
            } else {
                pointer.OnEnter = HandleCardHoverEnter;
                pointer.OnExit = HandleCardHoverExit;
                pointer.OnClick = null;
            }
        }

        #endregion // Action Cards

        #region Focus Slot

        // --- Hover mode ---

        private void HandleCardHoverEnter(int index) {
            if (!IsValidIndex(index)) { return; }

            ActionCardData data = m_SpawnedCardData[index];
            ActionCardUtility.PopulateCardUI(FocusSlot, data);
            SetFocusSlotVisible(true);
            SetFocusDescription(data.FocusDescription);
        }

        private void HandleCardHoverExit(int index) {
            // Blank the focus slot when the pointer leaves (only if it still reflects this card).
            ClearFocus();
        }

        // --- Click mode ---

        private void HandleCardClicked(int index) {
            if (!IsValidIndex(index)) { return; }

            // Clicking the already-focused card returns it and empties the slot.
            if (m_FocusedCardIndex == index) {
                m_FocusedCardIndex = -1;
                m_FocusInRoutine.Stop();
                m_FocusOutRoutine.Replace(this, SlideFocusOut());
                return;
            }

            // Focus a fresh card, or swap the currently-focused card for the clicked one.
            bool swapping = m_FocusedCardIndex != -1;
            m_FocusedCardIndex = index;
            m_FocusInRoutine.Replace(this, FocusOn(index, swapping));
        }

        // Drives focusing a card into the slot. When swapping, the previous card (old content) slides
        // out first, then the new card is populated and slides in.
        private IEnumerator FocusOn(int index, bool swapping) {
            RectTransform slot = FocusSlot.transform as RectTransform;

            if (swapping) {
                // Old card (still showing previous content) slides out.
                yield return slot.AnchorPosTo(FocusHiddenX, FocusSlideAnim, Axis.X);
            }

            // Populate with the new card's content, then slide in from off-screen.
            ActionCardData data = m_SpawnedCardData[index];
            ActionCardUtility.PopulateCardUI(FocusSlot, data);
            SetFocusDescription(data.FocusDescription);

            SetFocusSlotVisible(true);
            slot.SetAnchorPos(FocusHiddenX, Axis.X);
            yield return slot.AnchorPosTo(0f, FocusSlideAnim, Axis.X);
        }

        // Slides the focus card out and blanks the slot.
        private IEnumerator SlideFocusOut() {
            RectTransform slot = FocusSlot.transform as RectTransform;
            yield return slot.AnchorPosTo(FocusHiddenX, FocusSlideAnim, Axis.X);
            SetFocusSlotVisible(false);
            SetFocusDescription(string.Empty);
            slot.SetAnchorPos(0f, Axis.X);
        }

        // Immediately clears the focus slot (no animation) and resets focus state.
        private void ClearFocus() {
            m_FocusInRoutine.Stop();
            m_FocusOutRoutine.Stop();
            m_FocusedCardIndex = -1;
            SetFocusSlotVisible(false);
            SetFocusDescription(string.Empty);
        }

        private void SetFocusSlotVisible(bool visible) {
            if (FocusSlot != null) {
                FocusSlot.gameObject.SetActive(visible);
            }
        }

        private void SetFocusDescription(string text) {
            if (FocusDescriptionText != null) {
                FocusDescriptionText.SetText(text);
            }
        }

        private bool IsValidIndex(int index) {
            return index >= 0 && index < m_SpawnedCardData.Count;
        }

        #endregion // Focus Slot

        public override void Show() {
            base.Show();
            Game.Gui.PushPriority(m_InputLayer);
            UpdateRevealQueueDisplay();
            ScriptHooks.HideToolbar();
        }

        public override void Hide() {
            Game.Gui.PopPriority(m_InputLayer);
            ClearFocus();
            base.Hide();
            ScriptHooks.RevealToolbar();
        }
    }
}
