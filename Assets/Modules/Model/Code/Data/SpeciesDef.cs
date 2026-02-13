using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    [CreateAssetMenu(menuName = "Invasion Model/New Species Def")]
    public class SpeciesDef : ScriptableObject
    {
        public SerializedHash32 SpeciesId;
        public Sprite Sprite;
    }
}