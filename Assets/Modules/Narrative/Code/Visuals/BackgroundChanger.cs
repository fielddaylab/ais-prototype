using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leaf.Runtime;
using EasyAssetStreaming;
using System;
using BeauUtil;
using FieldDay.Scripting;
using FieldDay.SharedState;
using FieldDay;

namespace AIS.Narrative
{
    // temporary script to swap background from Leaf files
    public class BackgroundChanger : SharedStateComponent
    {
        [SerializeField]
        private StreamingQuadTexture m_BackgroundTexture = null;

        [LeafMember("SetBackground")]
        static public void SetBackground(string path)
        {
            Find.State<BackgroundChanger>().m_BackgroundTexture.Path = path;
        }
    }
}
