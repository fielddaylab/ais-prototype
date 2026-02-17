using AIS.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AIS.Intervene
{
    public struct EffectChunk
    {
        public List<ModelTag> SelectedTargets;
        public ActionVerbDetails[] Verbs;
    }

    public class ActionEffectChunkMgr : MonoBehaviour
    {
        public EffectChunk EffectChunk;
        public bool IsActive { get; private set; }

        public Button ConfirmChunkBtn;
        public Button CancelChunkBtn;

        public LayerMask ModelTagLayer;

        private ActionEffect m_EffectToProcess;

        private void Awake()
        {
            ConfirmChunkBtn.onClick.AddListener(HandleConfirmChunkClicked);
            CancelChunkBtn.onClick.AddListener(HandleCancelChunkClicked);

            ConfirmChunkBtn.interactable = false;

            EffectChunk = new EffectChunk();
            EffectChunk.SelectedTargets = new List<ModelTag>();

            RefreshUIElements();
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            ConfirmChunkBtn.onClick.RemoveAllListeners();
            CancelChunkBtn.onClick.RemoveAllListeners();

            ConfirmChunkBtn.interactable = false;
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

                    // int highestPriority = int.MinValue;
                    foreach (var hit in hits)
                    {
                        if (hit != null)
                        {
                            var tag = hit.GetComponent<ModelTag>();
                            if (tag != null)
                            {
                                highestPriorityHit = hit;
                            }
                            /*
                            if (tag.Priority > highestPriority)
                            {
                                highestPriorityHit = hit;
                                highestPriority = box.Priority;
                            }
                            */
                        }
                    }

                    if (highestPriorityHit != null)
                    {
                        // if clicked over a valid tag
                        var tag = highestPriorityHit.GetComponent<ModelTag>();

                        List<ModelTag> validTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(m_EffectToProcess.AllTargets);
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
            }
        }

        private void AddTagToChunk(ModelTag tag)
        {
            EffectChunk.SelectedTargets.Add(tag);
            ConfirmChunkBtn.interactable = EffectChunk.SelectedTargets.Count > 0;
        }

        private void RemoveTagFromChunk(ModelTag tag)
        {
            EffectChunk.SelectedTargets.Remove(tag);
            ConfirmChunkBtn.interactable = EffectChunk.SelectedTargets.Count > 0;
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

            EffectChunk.SelectedTargets.Clear();
            ConfirmChunkBtn.interactable = false;

            EffectChunk.Verbs = effectToProcess.Verbs;

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
            ConfirmChunkBtn.interactable = false;

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
            List<ModelTag> filteredTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(m_EffectToProcess.AllTargets);

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