using System;
using System.Collections;
using System.Collections.Generic;
using BeauPools;
using BeauUtil;
using BeauUtil.Tags;
using BeauUtil.UI;
using FieldDay;
using FieldDay.Components;
using FieldDay.Scripting;
using FieldDay.UI;
using Leaf.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AIS.Narrative {
    public class DialogueColumn : BatchedComponent, IDialoguePrinter, IRegistrationCallbacks {
        public SerializedHash32 Id;

        #region IScriptThreadOwned

        void IScriptThreadOwned.ClearThreadOwner() {
            
        }

        LeafThreadHandle IScriptThreadOwned.GetThreadOwner() {
            throw new NotImplementedException();
        }

        #endregion // IScriptThreadOwned

        public void CancelSkip() {
            throw new NotImplementedException();
        }

        public IEnumerator CompleteLine() {
            throw new NotImplementedException();
        }

        public void FastForwardLine(int visibleCount, int richCount) {
            throw new NotImplementedException();
        }

        public TagStringEventHandler PrepareLine(TagString text, DialogueCharacterState character, TagStringEventHandler parentHandler) {
            throw new NotImplementedException();
        }

        public void SetThreadOwner(LeafThreadHandle handle) {
            throw new NotImplementedException();
        }

        public void StartSkip() {
            throw new NotImplementedException();
        }

        public bool TryClearThreadOwner(LeafThreadHandle handle, ScriptThreadOwnershipClearReason cancelType) {
            throw new NotImplementedException();
        }

        public IEnumerator TypeLine(TagString text, TagTextData textData) {
            throw new NotImplementedException();
        }

        public void UpdateCharacter(DialogueCharacterState character) {
            throw new NotImplementedException();
        }

        void IRegistrationCallbacks.OnDeregister() {
            ScriptUtility.DeregisterDialoguePrinter(Id, this);
        }

        void IRegistrationCallbacks.OnRegister() {
            ScriptUtility.RegisterDialoguePrinter(Id, this);
        }
    }

}