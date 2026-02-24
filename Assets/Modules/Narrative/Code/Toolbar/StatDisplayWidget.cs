using AIS.Narrative;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace AIS {
    public class StatDisplayWidget : GuiWidget {
        public PlayerStatId StatId;

        [Header("Components")]
        public GuiCounter StatValueCounter;
        public Graphic Flash;
        public CursorHint Hint;
    }
}