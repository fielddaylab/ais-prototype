using System.Collections;
using System.Collections.Generic;
using FieldDay;
using UnityEngine;

public sealed class AisGame : Game {
    static public new EventDispatcher<EvtArgs> Events { get; private set; }
}
