using FieldDay.UI;
using FieldDay.UI.Widgets;

namespace AIS.Narrative {
    public sealed class StatDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerStatBlock> {
        public StatDisplayWidget[] Widgets;

        public void Populate(in PlayerStatBlock parms) {
            for(int i = 0; i < Widgets.Length; i++) {
                StatDisplayWidget widget = Widgets[i];
                widget.StatValueCounter.SetValue(parms[widget.StatId], GuiWidgetUpdateFlags.Force | GuiWidgetUpdateFlags.NoAnimation);
            }
        }
    }
}