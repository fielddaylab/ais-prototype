using System;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;

namespace AIS.Narrative {
    public sealed class EvidenceDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerInventory> {
        public EvidenceDisplayWidget[] Widgets;

        protected override void Awake() {
            base.Awake();
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