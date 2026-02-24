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

        public List<ModelTag> FilterTagsByTargetDetails(ActionTargetDetails[] allTargetDetails, List<ActionVerb> allVerbs, List<ModelTag> toFilter = null, bool filterExternal = false)
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
                        // skip external ecosystems
                        if ((tag.TargetType & ActionTarget.Ecosystem) != 0)
                        {
                            Ecosystem eco = tag.QueriableObj.GetComponent<Ecosystem>();
                            if (eco != null && eco.IsExternal && filterExternal)
                            {
                                continue;
                            }
                        }

                        if (allVerbs.Contains(ActionVerb.Reveal))
                        {
                            // skip revealed pathways
                            if ((tag.TargetType & ActionTarget.Pathway) != 0)
                            {
                                Pathway path = tag.QueriableObj.GetComponent<Pathway>();
                                if (path != null && !path.IsHidden)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            // skip hidden pathways
                            if ((tag.TargetType & ActionTarget.Pathway) != 0)
                            {
                                Pathway path = tag.QueriableObj.GetComponent<Pathway>();
                                if (path != null && path.IsHidden)
                                {
                                    continue;
                                }
                            }
                        }

                        // match conditions
                        bool allTrue = true;
                        foreach (var condition in targetDetails.Conditions)
                        {
                            if (!ActionCardUtility.Evaluate(condition, tag))
                            {
                                allTrue = false;
                                break;
                            }

                            // TODO: check if hidden information from player
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

