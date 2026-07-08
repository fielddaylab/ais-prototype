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

        private void Start()
        {
            var ids = new List<StringHash32>(TestCardIds.Count);
            foreach (var s in TestCardIds) ids.Add(s);
            SwapDeckWidget.Populate(ids);
        }
    }
}