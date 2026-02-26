using System;
using System.Collections;
using System.Collections.Generic;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Tags;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scripting;
using FieldDay.UI;
using Leaf;
using Leaf.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    [DisallowMultipleComponent]
    public sealed class DialoguePanel : BaseGuiModule, IRegistrationCallbacks {
        public Canvas Renderer;
        public IInputLayer InputLayer;

        void IRegistrationCallbacks.OnDeregister() {
        }

        void IRegistrationCallbacks.OnRegister() {
            InputLayer = IInputLayer.Find(this);
        }

        public void SetVisible(bool visible) {
            Renderer.enabled = visible;
            InputLayer.SetInputOverride(visible ? null : false);
        }
    }
}