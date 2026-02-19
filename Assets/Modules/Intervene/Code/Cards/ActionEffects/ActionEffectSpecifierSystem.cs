using AIS.Model;
using BeauRoutine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AIS.Intervene
{
    /// <summary>
    /// System that enables the player to specify how to use their actions.
    ///
    /// For each action card selected:
    ///     TODO: modify cards according to synergies
    ///     For each action effect in card:
    ///         // Pass control to EffectChunkMgr
    ///             // Keeps track of selectedTargets
    ///             // Allows effect-level cancel and confirm
    ///         // Highlight Valid Targets
    ///         // On Click:
    ///             // Check for first valid ModelTag under mouse
    ///             // If ModelTag already selected, deselect and modify highlight (normal color)
    ///             // Else if MaxTargets is reached, do not select
    ///             // Else if more selections allowed, select and modify highlight (selected color)
    ///         // Return FinalizedEffectChunk
    ///             // Contains List of Targets for the given Effect
    /// Execute the FinalizedEffectChunks in order created
    /// </summary>
    public class ActionEffectSpecifierSystem : MonoBehaviour
    {
        public ActionEffectChunkMgr ChunkMgr;

        public List<ActionCard> SelectedActionCards = new List<ActionCard>();
        public List<ActionCard> ActionsProcessList = new List<ActionCard>();

        private int CurrActionIndex;
        private int CurrEffectIndex;
        private List<EffectChunk> ProcessedEffects = new List<EffectChunk>();

        public Routine ChunkTransitionRoutine;

        public Routine ExecuteRoutine;

        #region Unity Callbacks

        private void Awake()
        {
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyBegin, HandleEffectSpecifyBegin);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyConfirm, HandleEffectSpecifyConfirm);
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyCancel, HandleEffectSpecifyCancel);
        
            AisGame.Events.Register(InterveneEvents.OnEffectChunkBegin, HandleEffectChunkBegin);
            AisGame.Events.Register(InterveneEvents.OnEffectChunkComplete, HandleEffectChunkComplete);
            AisGame.Events.Register(InterveneEvents.OnEffectChunkCancel, HandleEffectChunkCancel);
        }

        private void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyBegin, HandleEffectSpecifyBegin);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyConfirm, HandleEffectSpecifyConfirm);
            AisGame.Events.Deregister(InterveneEvents.OnEffectSpecifyCancel, HandleEffectSpecifyCancel);

            AisGame.Events.Deregister(InterveneEvents.OnEffectChunkBegin, HandleEffectChunkBegin);
            AisGame.Events.Deregister(InterveneEvents.OnEffectChunkComplete, HandleEffectChunkComplete);
            AisGame.Events.Deregister(InterveneEvents.OnEffectChunkCancel, HandleEffectChunkCancel);
        }

        #endregion // Unity Callbacks

        #region Phase Management

        public void FilterActionCards()
        {
            SelectedActionCards.Clear();
            ActionsProcessList.Clear();

            List<CardBase> selectedCards = CardInteractionMgr.Instance.Hand.GetSelectedCards();
            foreach (var card in selectedCards)
            {
                ActionCard actionCard = (ActionCard)card;
                if (actionCard != null)
                {
                    SelectedActionCards.Add(actionCard);
                }
            }
        }

        private void LoadProcessList()
        {
            foreach (var toProcess in SelectedActionCards)
            {
                ActionsProcessList.Add(toProcess);
            }

            ProcessedEffects.Clear();

            CurrActionIndex = 0;
            CurrEffectIndex = 0;
        }      

        private void ProcessNextEffect()
        {
            if (CurrActionIndex == ActionsProcessList.Count)
            {
                // reached end of action cards -- complete
                OnAllActionsProcessed();
                return;
            }
            else
            {
                if (CurrEffectIndex == ActionsProcessList[CurrActionIndex].Effects.Length)
                {
                    // reached end of effects in curr action card -- move to next action card
                    OnActionProcessed();
                    return;
                }
            }

            // process curr effect for curr action card
            ChunkMgr.Begin(ActionsProcessList[CurrActionIndex].Effects[CurrEffectIndex]);
        }

        public void Exit()
        {
            if (ChunkMgr.IsActive)
            {
                ChunkMgr.ExternForceCancel();
            }
            ActionsProcessList.Clear();
            SelectedActionCards.Clear();
        }

        private void OnAllActionsProcessed()
        {
            AisGame.Events.Dispatch(InterveneEvents.OnEffectSpecifyAllActionsProcessed);
        }

        private void OnActionProcessed()
        {
            CurrActionIndex++;
            CurrEffectIndex = 0;
            ProcessNextEffect();
        }

        #endregion // Phase Management

        #region Handlers

        /// <summary>
        /// Begin all
        /// </summary>
        private void HandleEffectSpecifyBegin()
        {
            FilterActionCards();
            LoadProcessList();

            ProcessNextEffect();
        }

        /// <summary>
        /// Confirm all
        /// </summary>
        private void HandleEffectSpecifyConfirm()
        {
            // Execute Effects
            ExecuteRoutine.Replace(ExecuteEffectsRoutine())
                .OnComplete(() => Exit());
        }

        /// <summary>
        /// Cancel all
        /// </summary>
        private void HandleEffectSpecifyCancel()
        {
            ChunkTransitionRoutine.Stop();
            Exit();
        }

        #endregion // Handlers

        #region EffectChunk Handlers

        private void HandleEffectChunkBegin()
        {

        }

        private void HandleEffectChunkComplete()
        {
            ChunkTransitionRoutine.Replace(ChunkCompleteRoutine());
        }

        private void HandleEffectChunkCancel()
        {
            ChunkTransitionRoutine.Replace(ChunkCancelRoutine());
        }

        #endregion // EffectChunk Handlers

        #region Routines 

        private IEnumerator ChunkCompleteRoutine()
        {
            ProcessedEffects.Add(ChunkMgr.EffectChunk);

            yield return 0.5f;

            // move to next index
            CurrEffectIndex++;
            ProcessNextEffect();
        }

        private IEnumerator ChunkCancelRoutine()
        {
            yield return 1f;

            // re-trigger current index
            ProcessNextEffect();
        }

        private IEnumerator ExecuteEffectsRoutine()
        {
            foreach (var effect in ProcessedEffects)
            {
                foreach (var verb in effect.Verbs)
                {
                    foreach (var target in effect.SelectedTargets)
                    {
                        switch (verb.Verb)
                        {
                            case ActionVerb.Reduce:
                                TryReduce(target.QueriableObj, verb);
                                break;
                            case ActionVerb.Increase:
                                TryIncrease(target.QueriableObj, verb);
                                break;
                            case ActionVerb.Reveal:
                                break;
                            case ActionVerb.AddTrap:
                                break;
                            default:
                                continue;
                        }
                    }
                }

                yield return 1;
            }
        }

        #endregion // Routines

        #region Effect Execution

        private bool TryReduce(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toReduce = queriable.GetComponent<IReducible>();

            if (toReduce != null)
            {
                return toReduce.TryReduce(verbDetails.Value, verbDetails.ModType);
            }
            else
            {
                Debug.LogWarning("[Reducible] Tried to reduce on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        private bool TryIncrease(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toIncrease = queriable.GetComponent<IIncreasable>();

            if (toIncrease != null)
            {
                return toIncrease.TryIncrease(verbDetails.Value, verbDetails.ModType);
            }
            else
            {
                Debug.LogWarning("[Increasable] Tried to increase on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        #endregion // Effect Execution
    }
}
