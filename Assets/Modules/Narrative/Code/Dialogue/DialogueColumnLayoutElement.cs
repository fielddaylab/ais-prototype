using System;
using BeauUtil;
using UnityEngine;

namespace AIS.Narrative {
    [RequireComponent(typeof(RectTransform))]
    public sealed class DialogueColumnLayoutElement : MonoBehaviour {
        [NonSerialized] public float DesiredY;
        [NonSerialized] public RectTransform RectTransform;

        private void Awake() {
            this.CacheComponent(ref RectTransform);
        }
    }
}