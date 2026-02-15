using System.Collections;
using System.Collections.Generic;
using FieldDay;
using UnityEngine;

public sealed class AisGame : Game {
    static public new EventDispatcher<EvtArgs> Events { get; private set; }

    [InvokePreBoot]
    static private void OnPreBoot()
    {
        Events = new EventDispatcher<EvtArgs>();
        SetEventDispatcher(Events);
        Rendering.EnableAspectClamping(4, 3);
    }
}
