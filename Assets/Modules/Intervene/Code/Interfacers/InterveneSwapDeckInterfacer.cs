using AIS.Narrative;
using BeauRoutine;
using BeauUtil;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AIS.Narrative.EvidenceDisplayPanel;

namespace AIS.Intervene
{
    public class InterveneSwapDeckInterfacer : MonoBehaviour
    {
        public static InterveneSwapDeckInterfacer Instance;

        [Header("Visuals")]
        public ActionCardDeckWidget SwapDeckWidget;

        [Header("Playtest Data — delete later")]
        public List<string> TestCardIds = new List<string> {
            "example-action-card-1",
            "example-action-card-2",
            "example-action-card-6",
        };

        private void Awake()
        {
            Instance = this;

        }

        // Only for playtesting
        public void AddActionCards()
        {
            var inv = Find.State<PlayerInventory>();
            inv.ActionCards.Add("example-action-card-1");
            inv.ActionCards.Add("example-action-card-2");
        }

        public void LoadSwapDeck(IEnumerable<StringHash32> cardIds)
        {
            SwapDeckWidget.Populate(cardIds);
        }
    }
}