using System;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine;

namespace AIS.Narrative {
    public sealed class EvidenceDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerInventory> {
        public EvidenceDisplayWidget[] Widgets;
        public StatDisplayWidget[] Stats;

        protected override void Awake() {
            base.Awake();
            Stats = GetComponentsInChildren<StatDisplayWidget>();
            Hide();
        }

        public void Populate(in PlayerInventory parms) {
            int widgetIndex = 0;
            foreach(var evidenceId in parms.EvidenceCards) {
                EvidenceCard data = Find.NamedAsset<EvidenceCard>(evidenceId);

                EvidenceDisplayWidget widget = Widgets[widgetIndex++];
                widget.gameObject.SetActive(true);
                widget.Content.SetText(data.Label);
            }

            for(; widgetIndex < Widgets.Length; widgetIndex++) {
                Widgets[widgetIndex].gameObject.SetActive(false);
            }
        }

        public void PopulateStats(in PlayerStatBlock parms)
        {
            Debug.Log($"stats: communicate: {parms.Communicate}, tech: {parms.Tech}");
            for (int i = 0; i < Stats.Length; i++)
            {
                StatDisplayWidget widget = Stats[i];
                Debug.Log($"stats length: {Stats.Length}");
                widget.StatValueCounter.SetValue(parms[widget.StatId], GuiWidgetUpdateFlags.Force | GuiWidgetUpdateFlags.NoAnimation);
            }
        }

        public override void Show() {
            base.Show();
            Game.Gui.PushPriority(m_InputLayer);
        }

        public override void Hide() {
            Game.Gui.PopPriority(m_InputLayer);
            base.Hide();
        }
    }
}