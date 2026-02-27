using System.Collections;
using System.Collections.Generic;
using AIS.Narrative;
using FieldDay;
using UnityEngine;

public sealed class AisGame : Game {
    static public new EventDispatcher<EvtArgs> Events { get; private set; }

    [InvokePreBoot]
    static private void OnPreBoot()
    {
        Events = new EventDispatcher<EvtArgs>();
        SetEventDispatcher(Events);
    }

    [InvokeOnBoot]
    static private void OnBoot() {
        SharedState.Register(new PlayerInventory());
        SharedState.Register(new PlayerStats());
    }
}
