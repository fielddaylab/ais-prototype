using AIS.Intervene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public class ModelTagMgr : MonoBehaviour
    {
        public static ModelTagMgr Instance;

        // TODO: lots of optimizing to be done on indexing these tags for search
        private List<ModelTag> RegisteredTags = new List<ModelTag>();
        private List<ModelTag> HighlightedTags = new List<ModelTag>();

        #region Unity Callbacks

        private void Awake()
        {
            Instance = this;
        }

        #endregion // Unity Callbacks

        #region Visuals

        public void HighlightByTarget(ActionTarget target, bool clearExisting = true)
        {
            if (clearExisting)
            {
                ClearExistingHighlights();
            }

            foreach (var tag in RegisteredTags)
            {
                if ((tag.TargetType & target) != 0)
                {
                    tag.ShowHighlight();
                    HighlightedTags.Add(tag);
                }
            }
        }

        public void ClearExistingHighlights()
        {
            foreach (var tag in RegisteredTags)
            {
                tag.HideHighlight();
            }

            HighlightedTags.Clear();
        }

        #endregion // Visuals

        #region Registration

        public void Register(ModelTag tag)
        {
            if (RegisteredTags.Contains(tag)) { return; }

            RegisteredTags.Add(tag);
        }

        public void Deregister(ModelTag tag)
        {
            if (!RegisteredTags.Contains(tag)) { return; }

            RegisteredTags.Remove(tag);
        }

        #endregion // Registration
    }
}

