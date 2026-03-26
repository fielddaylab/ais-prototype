using System;
using FieldDay;
using FieldDay.UI.Widgets;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class TimeWidgetStyle : GuiCounter.Style {
        public Image[] Segments;

        public override void Populate(in int data, GuiWidgetUpdateFlags flags) {
            Find.GlobalAsset(out GameIcons icons);

            int toFill = data;
            for(int i = 0; i < Segments.Length; i++) {
                int frags = Math.Max(0, Math.Min(4, toFill));
                Segments[i].sprite = icons.TimeIcons[frags];
                toFill -= 4;
            }
        }
    }
}