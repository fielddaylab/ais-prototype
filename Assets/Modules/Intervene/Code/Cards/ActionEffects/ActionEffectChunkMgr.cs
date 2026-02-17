using AIS.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public struct EffectChunk
    {
        public ActionTarget TargetType;
        public List<ModelTag> SelectedTargets;
        public ActionVerbDetails[] Verbs;
    }

    public class ActionEffectChunkMgr : MonoBehaviour
    {
        public EffectChunk EffectChunk { get; private set; }
        public bool IsActive { get; private set; }

        private ActionEffect m_EffectToProcess;

        #region Control

        public void Begin(ActionEffect effectToProcess)
        {
            IsActive = true;
            AisGame.Events.Dispatch(InterveneEvents.OnEffectChunkBegin);

            m_EffectToProcess = effectToProcess;

            SummonHighlights();
        }

        public void Complete()
        {
            IsActive = false;
            AisGame.Events.Dispatch(InterveneEvents.OnEffectChunkComplete);

            DisperseHighlights();
        }

        public void Cancel()
        {
            IsActive = false;
            AisGame.Events.Dispatch(InterveneEvents.OnEffectChunkCancel);

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
    }
}