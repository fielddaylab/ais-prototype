using System.Collections;
using System.Collections.Generic;
using BeauUtil;
using FieldDay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public sealed class TimeDisplayPanel : SharedPanel
    {
        public Button CloseButton;
        public ClockIncrements Clocks;
        public TMP_Text Hour0, Hour1, Min0, Min1;

        protected override void Awake()
        {
            base.Awake();
            CloseButton.onClick.AddListener(PanelUtility.ToggleTime);
            Hide();
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void SetTime(int timeRemaining)
        {
            ClockIncrements.Populate(Clocks, timeRemaining);

            // these should later be moved to a stats/consts class, however I am unsure where to place them for now
            int timeUnitsPerHour = 4;
            int minutesPerTimeUnit = 15;

            int hours = timeRemaining / timeUnitsPerHour;
            int minutes = (timeRemaining % timeUnitsPerHour) * minutesPerTimeUnit;

            Hour0.SetText((hours / 10).ToStringLookup());
            Hour1.SetText((hours % 10).ToStringLookup());

            Min0.SetText((minutes / 10).ToStringLookup());
            Min1.SetText((minutes % 10).ToStringLookup());
        }
    }
}
