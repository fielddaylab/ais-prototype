using System;
using System.Collections;
using System.Collections.Generic;
using AIS.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
public sealed class ClockIncrements : MonoBehaviour
    {
        public Image[] Clocks;

        static public void Populate(ClockIncrements increments, int timeRemaining)
        {
            int hours = timeRemaining / Constants.TimeUnitsPerHour;
            int hourChunks = timeRemaining % Constants.TimeUnitsPerHour;

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
                diff = Math.Min(timeRemaining - (i * Constants.TimeUnitsPerHour), Constants.TimeUnitsPerHour);
                clock.fillAmount = (float) diff / Constants.TimeUnitsPerHour;
                clock.gameObject.SetActive(true);
            }

            for (int i = clocksUsed; i < increments.Clocks.Length; i++)
            {
                increments.Clocks[i].gameObject.SetActive(false);
            }
        }
    }
}