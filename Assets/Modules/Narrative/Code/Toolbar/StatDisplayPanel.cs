using System;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;

namespace AIS.Narrative {
    public sealed class StatDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerStatBlock> {
        public StatDisplayWidget[] Widgets;

        protected override void Awake() {
            base.Awake();
            Hide();
        }

        public void Populate(in PlayerStatBlock parms) {
            for(int i = 0; i < Widgets.Length; i++) {
                StatDisplayWidget widget = Widgets[i];
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