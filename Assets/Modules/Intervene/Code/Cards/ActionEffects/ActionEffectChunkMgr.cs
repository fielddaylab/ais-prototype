using AIS.Model;
using BeauUtil;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace AIS.Intervene
{
    public struct EffectChunk
    {
        public List<ModelTag> SelectedTargets;
        public ActionVerbDetails[] Verbs;
        public string HardCodedId;

        public EffectChunk Copy()
        {
            EffectChunk newChunk = new EffectChunk();
            newChunk.SelectedTargets = new List<ModelTag>();
            foreach (var target in SelectedTargets) {
                newChunk.SelectedTargets.Add(target);
            }
            newChunk.Verbs = new ActionVerbDetails[Verbs.Length];
            for (int i = 0; i < Verbs.Length; i++)
            {
                newChunk.Verbs[i] = Verbs[i];
            }
            newChunk.HardCodedId = HardCodedId;

            return newChunk;
        }

        public void Clear()
        {
            SelectedTargets.Clear();
            HardCodedId = null;
            Verbs = new ActionVerbDetails[0];
        }
    }

    public class ActionEffectChunkMgr : MonoBehaviour
    {
        public EffectChunk EffectChunk;
        public bool IsActive { get; private set; }

        public Button ConfirmChunkBtn;
        public Button CancelChunkBtn;

        public bool RequireAtLeastOne = false;

        public LayerMask ModelTagLayer;

        private ActionEffect m_EffectToProcess;

        private void Awake()
        {
            ConfirmChunkBtn.onClick.AddListener(HandleConfirmChunkClicked);
            CancelChunkBtn.onClick.AddListener(HandleCancelChunkClicked);

            if (RequireAtLeastOne)
            {
                ConfirmChunkBtn.interactable = false;
            }

            EffectChunk = new EffectChunk();
            EffectChunk.SelectedTargets = new List<ModelTag>();

            RefreshUIElements();
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            ConfirmChunkBtn.onClick.RemoveAllListeners();
            CancelChunkBtn.onClick.RemoveAllListeners();

            if (RequireAtLeastOne)
            {
                ConfirmChunkBtn.interactable = false;
            }
        }

        private void Update()
        {
            if (IsActive)
            {
                AwaitInputs();
            }
        }

        private void AwaitInputs()
        {
            if (m_EffectToProcess.Specificity == ActionSpecificity.All)
            {
                // TODO: auto-highlight all and await continue
                if (EffectChunk.SelectedTargets.Count != 0) { return; }

                List<ModelTag> filteredTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(m_EffectToProcess.AllTargets, m_EffectToProcess.GetAllVerbs());
                foreach (var tag in filteredTags)
                {
                    AddTagToChunk(tag);
                    tag.SetSelectedHighlight();
                }
            }
            else if (m_EffectToProcess.Specificity == ActionSpecificity.Random)
            {
                // TODO: randomly select from available tags up to max targets
                if (EffectChunk.SelectedTargets.Count != 0) { return; }

                // TODO: hide selections from player?
            }
            else if (m_EffectToProcess.Specificity == ActionSpecificity.Specific)
            {
                // await player input
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    var screenPos = Input.mousePosition;
                    screenPos.z = -Camera.main.transform.position.z;
                    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
                    Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos, ModelTagLayer);
                    Collider2D highestPriorityHit = null;

                    // highest sprite renderer layer is highest priority
                    int highestPriority = int.MinValue;
                    foreach (var hit in hits)
                    {
                        if (hit != null)
                        {
                            var tag = hit.GetComponent<ModelTag>();
                            if (tag != null)
                            {
                                var priorityLayer = 0;
                                var renderer = tag.GetComponent<SpriteRenderer>();
                                if (renderer != null) {
                                    priorityLayer = renderer.enabled ? renderer.sortingOrder : int.MinValue;
                                }
                                if (priorityLayer > highestPriority)
                                {
                                    highestPriorityHit = hit;
                                    highestPriority = priorityLayer;
                                }
                            }
                        }
                    }

                    if (highestPriorityHit != null)
                    {
                        // if clicked over a valid tag
                        var tag = highestPriorityHit.GetComponent<ModelTag>();

                        List<ModelTag> validTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(m_EffectToProcess.AllTargets, m_EffectToProcess.GetAllVerbs());
                        if (validTags.Contains(tag))
                        {
                            ToggleTagSelected(tag);
                        }
                    }
                }

            }
        }

        private void ToggleTagSelected(ModelTag tag)
        {
            // If ModelTag already selected, deselect and modify highlight (normal color)
            if (EffectChunk.SelectedTargets.Contains(tag))
            {
                RemoveTagFromChunk(tag);
                tag.SetNormalHighlight();
                AisGame.Events.Dispatch(InterveneEvents.OnUiSelected);
            }
            // Else if MaxTargets is reached, do not select
            else if (m_EffectToProcess.MaxTargets == EffectChunk.SelectedTargets.Count)
            { 
                // do nothing
            }
            // Else if more selections allowed, select and modify highlight (selected color)
            else if (EffectChunk.SelectedTargets.Count < m_EffectToProcess.MaxTargets)
            {
                AddTagToChunk(tag);
                tag.SetSelectedHighlight();
                AisGame.Events.Dispatch(InterveneEvents.OnUiSelected);
            }
        }

        private void AddTagToChunk(ModelTag tag)
        { 
            EffectChunk.SelectedTargets.Add(tag);
            if (RequireAtLeastOne)
            {
                ConfirmChunkBtn.interactable = EffectChunk.SelectedTargets.Count > 0;
            }
        }

        private void RemoveTagFromChunk(ModelTag tag)
        {
            EffectChunk.SelectedTargets.Remove(tag);
            if (RequireAtLeastOne)
            {
                ConfirmChunkBtn.interactable = EffectChunk.SelectedTargets.Count > 0;
            }
        }

        private void RefreshUIElements()
        {
            ConfirmChunkBtn.gameObject.SetActive(IsActive);
            CancelChunkBtn.gameObject.SetActive(IsActive);
        }

        #region Control

        public void Begin(ActionEffect effectToProcess)
        {
            IsActive = true;
            AisGame.Events.Dispatch(InterveneEvents.OnEffectChunkBegin);
            RefreshUIElements();

            m_EffectToProcess = effectToProcess;

            EffectChunk.Clear();
            if (RequireAtLeastOne)
            {
                ConfirmChunkBtn.interactable = false;
            }

            EffectChunk.Verbs = effectToProcess.Verbs;
            if (effectToProcess.IsHardCoded())
            {
                EffectChunk.HardCodedId = effectToProcess.HardCodedId;
            }

            SummonHighlights();
        }

        public void Complete()
        {
            IsActive = false;
            AisGame.Events.Dispatch(InterveneEvents.OnEffectChunkComplete);
            RefreshUIElements();

            DisperseHighlights();
        }

        public void Cancel()
        {
            IsActive = false;
            EffectChunk.SelectedTargets.Clear();
            if (RequireAtLeastOne)
            {
                ConfirmChunkBtn.interactable = false;
            }

            AisGame.Events.Dispatch(InterveneEvents.OnEffectChunkCancel);
            RefreshUIElements();

            DisperseHighlights();
        }

        public void ExternForceCancel()
        {
            IsActive = false;
            RefreshUIElements();
            DisperseHighlights();
        }

        #endregion // Control

        #region Visuals

        private void SummonHighlights()
        {
            Debug.Log("[ActionEffectChunkMgr] Summoning highlights for effect " + m_EffectToProcess.AllTargets.Length);
            List<ModelTag> filteredTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(m_EffectToProcess.AllTargets, m_EffectToProcess.GetAllVerbs(), filterExternal: true);

            ModelTagMgr.Instance.HighlightTags(filteredTags);
        }

        private void DisperseHighlights()
        {
            ModelTagMgr.Instance.ClearExistingHighlights();
        }

        #endregion // Visuals

        #region Handlers

        private void HandleConfirmChunkClicked()
        {
            Complete();
        }

        private void HandleCancelChunkClicked()
        {
            Cancel();
        }

        #endregion // Handlers
    }
}