using System;
using System.Collections;
using System.Collections.Generic;
using AIS.Narrative;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Tags;
using BeauUtil.Variants;
using FieldDay;
using FieldDay.Scripting;
using UnityEngine;

public sealed class AisGame : Game {
    static public new EventDispatcher<EvtArgs> Events { get; private set; }

    [InvokePreBoot]
    static private void OnPreBoot()
    {
        Events = new EventDispatcher<EvtArgs>();
        SetEventDispatcher(Events);
    }

    static private readonly TableKeyPair PG = new TableKeyPair("global", "gender");

    [InvokeOnBoot]
    static private void OnBoot() {
        SharedState.Register(new PlayerInventory());
        SharedState.Register(new PlayerStats());

        ScriptUtility.RegisterReplaceRule("pg").ReplaceWith((TagData inTag, object inContext) => {
            TempList8<StringSlice> args = default;
            int count = inTag.Data.Split(PipeChars, new StringSliceOptions(StringSplitOptions.None, 3), ref args);
            StringHash32 gender = ScriptUtility.ReadVariable(PG, "x").AsStringHash();
            if (count == 0) {
                Log.Error("No things to split");
                return string.Empty;
            } else if (count == 1) {
                return args[0].ToString();
            } else if (count == 2) {
                return (gender == "f" ? args[1] : args[0]).ToString();
            } else {
                return (gender == "m" ? args[0] : (gender == "f" ? args[1] : args[2])).ToString();
            }
        });
    }

    static private readonly char[] PipeChars = new char[] { '|' };
}
