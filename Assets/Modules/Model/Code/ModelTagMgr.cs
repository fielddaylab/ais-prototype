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

        #region Queries 

        public List<ModelTag> FilterTagsByTargetDetails(ActionTargetDetails[] allTargetDetails, List<ModelTag> toFilter = null)
        {
            List<ModelTag> filtered = new List<ModelTag>();
            if (toFilter == null)
            {
                // by default use full set
                toFilter = new List<ModelTag>();
                toFilter.AddRange(RegisteredTags);
            }

            foreach (var targetDetails in allTargetDetails)
            {
                foreach (var tag in toFilter)
                {
                    // match target type
                    if ((tag.TargetType & targetDetails.Target) != 0)
                    {
                        // match conditions
                        bool allTrue = true;
                        foreach (var condition in targetDetails.Conditions)
                        {
                            if (!ActionCardUtility.Evaluate(condition, tag))
                            {
                                allTrue = false;
                                break;
                            }
                        }
                        if (allTrue)
                        {
                            filtered.Add(tag);
                        }
                    }
                }
            }


            return filtered;
        }

        #endregion // Queries

        #region Visuals

        /*
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
        */

        public void HighlightTags(List<ModelTag> toHighlight, bool clearExisting = true)
        {
            if (clearExisting)
            {
                ClearExistingHighlights();
            }

            foreach (var tag in toHighlight)
            {
                tag.ShowHighlight();
                HighlightedTags.Add(tag);
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

