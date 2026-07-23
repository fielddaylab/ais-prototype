using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
public sealed class ClockIncrements : MonoBehaviour
    {
        public Image[] Clocks;

        static public void Populate(ClockIncrements increments, int timeRemaining)
        {
            // these should later be moved to a stats/consts class, however I am unsure where to place them for now
            int timeUnitsPerHour = 4;
            int minutesPerTimeUnit = 15;

            int hours = timeRemaining / timeUnitsPerHour;
            int hourChunks = timeRemaining % timeUnitsPerHour;
            int minutes = hourChunks * minutesPerTimeUnit;

            int clocksUsed = (int) hours;
            if (hourChunks > 0)
            {
                clocksUsed++;
            }

            Image clock;
            int diff;
            for (int i = 0; i < clocksUsed; i++)
            {
                clock = increments.Clocks[i];
                diff = Math.Min(timeRemaining - (i * timeUnitsPerHour), timeUnitsPerHour);
                clock.fillAmount = (float) diff / timeUnitsPerHour;
                clock.gameObject.SetActive(true);
            }

            for (int i = clocksUsed; i < increments.Clocks.Length; i++)
            {
                increments.Clocks[i].gameObject.SetActive(false);
            }
        }
    }
}