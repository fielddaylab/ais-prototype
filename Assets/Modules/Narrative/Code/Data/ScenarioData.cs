using AIS.Narrative;
using BeauUtil;
using FieldDay.Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative
{
    [CreateAssetMenu(menuName = "Narrative/Scenario")]
    public sealed class ScenarioData : NamedAsset
    {
        public Sprite Illustration;
        [Multiline] public string OverviewText;
    }
}