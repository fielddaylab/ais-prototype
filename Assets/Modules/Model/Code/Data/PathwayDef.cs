using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    [CreateAssetMenu(menuName = "Invasion Model/New Pathway Def")]
    public class PathwayDef : ScriptableObject
    {
        public PathwayType PathType;
        public Sprite BGSprite;
        public Sprite Sprite;
        public bool IsHidden;
        public bool MirrorVertical;
    }
}