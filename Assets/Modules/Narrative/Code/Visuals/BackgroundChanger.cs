using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leaf.Runtime;
using EasyAssetStreaming;
using System;
using BeauUtil;

namespace AIS.Narrative
{
    // temporary script to swap background from Leaf files
    public class BackgroundChanger : MonoBehaviour
    {
        public StreamingQuadTexture BackgroundTexture = null;

        [LeafMember("SetBackground")]
        public void SetBackground(string path)
        {
            BackgroundTexture.Path = path;
        }
    }
}
