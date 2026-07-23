using AIS.Model;
using BeauRoutine;
using FieldDay.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
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
        public List<EffectChunk> PostConfirmChunks = new List<EffectChunk>();

        private int ProcessCost;
        private int CurrActionIndex;
        private int CurrEffectIndex;
        private List<EffectChunk> ProcessedEffects = new List<EffectChunk>();
        private List<ModelTag> PendingPhantoms = new List<ModelTag>();

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
            PendingPhantoms.Clear();

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
            ProcessCost = 0;

            foreach (var toProcess in SelectedActionCards)
            {
                ActionsProcessList.Add(toProcess);
                ProcessCost += toProcess.GetAdjustedCost();
            }

            ProcessedEffects.Clear();
            PendingPhantoms.Clear();

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
            // If increase, create phantom clusters
            var effectBundleToProcess = ActionsProcessList[CurrActionIndex].Effects[CurrEffectIndex];

            var effectToProcess = effectBundleToProcess.ActionEffect;

            // Check if override
            if (effectBundleToProcess.EffectOverride.Condition.Condition != 0)
            {
                if (ActionCardUtility.Evaluate(effectBundleToProcess.EffectOverride.Condition))
                {
                    effectToProcess = effectBundleToProcess.EffectOverride.Override;
                }
            }

            // hardcoded modifications
            effectToProcess = ModifyHardCode(effectToProcess);

            if (effectToProcess.GetAllVerbs().Contains(ActionVerb.Increase))
            {
                CreatePhantomClusters(effectToProcess);
            }

            ChunkMgr.Begin(effectToProcess);
        }

        public void Exit()
        {
            if (ChunkMgr.IsActive)
            {
                ChunkMgr.ExternForceCancel();
            }

            ActionsProcessList.Clear();
            SelectedActionCards.Clear();
            PendingPhantoms.Clear();
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

        private void CreatePhantomClusters(ActionEffect effectToProcess)
        {
            foreach (var target in effectToProcess.AllTargets)
            {
                foreach (var eco in InvasionModelContainer.Instance.GetAllEcosystems())
                {
                    if (eco.IsExternal) { continue; }

                    // no phantoms where the target cannot land anyway
                    if (!ActionCardUtility.MatchesScope(target.Scope, ActionCardUtility.ScopeOf(eco))) { continue; }

                    if ((target.Target & ActionTarget.Invasive) != 0)
                    {
                        var defInvasive = InvasionModel.Instance.CurrModelSetupData.DefaultInvasive;
                        if (eco.GetPopulation(defInvasive.SpeciesId) == 0)
                        {
                            eco.AddPopulation(defInvasive.SpeciesId, 0, defInvasive.StartingTravelType, target.Target, isPhantom: true);
                        }
                    }
                    else if ((target.Target & ActionTarget.Predator) != 0)
                    {
                        var defPredator = InvasionModel.Instance.CurrModelSetupData.DefaultPredator;
                        if (eco.GetPopulation(defPredator.SpeciesId) == 0)
                        {
                            eco.AddPopulation(defPredator.SpeciesId, 0, defPredator.StartingTravelType, target.Target, isPhantom: true);
                        }
                    }
                    else if ((target.Target & ActionTarget.Prey) != 0)
                    {
                        var defPrey = InvasionModel.Instance.CurrModelSetupData.DefaultPrey;
                        if (eco.GetPopulation(defPrey.SpeciesId) == 0)
                        {
                            eco.AddPopulation(defPrey.SpeciesId, 0, defPrey.StartingTravelType, target.Target, isPhantom: true);
                        }
                    }
                    else if ((target.Target & ActionTarget.Nest) != 0)
                    {
                        if (eco.GetPopulation("nest") == 0)
                        {
                            eco.AddPopulation("nest", 0, 0, target.Target, isSecondary: true, isPhantom: true);

                        }
                    }
                    else if ((target.Target & ActionTarget.Trap) != 0)
                    {
                        if (eco.GetPopulation("trap") == 0)
                        {
                            eco.AddPopulation("trap", 0, 0, target.Target, isSecondary: true, isPhantom: true);
                        }
                    }
                }
            }
        }

        private void RemovePhantomClusters()
        {
            foreach (var eco in InvasionModelContainer.Instance.GetAllEcosystems())
            {
                eco.RemovePhantomPopulations();
            }
        }

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
            // Pay Costs
            BudgetUtility.Spend(InterveneBudgetInterfacer.Instance, ProcessCost);
            ProcessCost = 0;

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

            // clean up partially-confirmed phantoms
            for (int i = 0; i < PendingPhantoms.Count; i++) {
                PendingPhantoms[i].IsPhantom = true;
            }
            RemovePhantomClusters();

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
            ProcessedEffects.Add(ChunkMgr.EffectChunk.Copy());

            foreach (var tag in ChunkMgr.EffectChunk.SelectedTargets)
            {
                if (tag.IsPhantom)
                {
                    tag.IsPhantom = false;
                    PendingPhantoms.Add(tag);
                }
            }

            RemovePhantomClusters();

            yield return 0.5f;

            // move to next index
            CurrEffectIndex++;
            ProcessNextEffect();
        }

        private IEnumerator ChunkCancelRoutine()
        {
            RemovePhantomClusters();

            yield return 2f;

            // re-trigger current index
            ProcessNextEffect();
        }

        private IEnumerator ExecuteEffectsRoutine()
        {
            foreach (var effect in ProcessedEffects)
            {
                if (effect.HardCodedId != null && !effect.HardCodedId.Equals(string.Empty))
                {
                    // handle hard coding
                    ExecuteHardCode(effect);
                    continue;
                }

                foreach (var verb in effect.Verbs)
                {
                    if (verb.Odds != 1)
                    {
                        // TODO: roll dice animation
                        var success = Random.Range(0, 1.0f) >= verb.Odds;
                        // Evaluate odds to see if effect triggers
                        if (!success) { continue; }
                    }

                    foreach (var target in effect.SelectedTargets)
                    {
                        switch (verb.Verb)
                        {
                            case ActionVerb.Reduce:
                                ActionEffectUtility.TryReduce(target.QueriableObj, verb);
                                break;
                            case ActionVerb.Increase:
                                ActionEffectUtility.TryIncrease(target.QueriableObj, verb);
                                break;
                            case ActionVerb.Remove:
                                ActionEffectUtility.TryRemove(target.QueriableObj, verb);
                                break;
                            case ActionVerb.Reveal:
                                ActionEffectUtility.TryReveal(target.QueriableObj, verb);
                                break;
                            case ActionVerb.AddTrap:
                                ActionEffectUtility.TryAddTrap(target.QueriableObj, verb);
                                break;
                            case ActionVerb.AddNest:
                                ActionEffectUtility.TryAddNest(target.QueriableObj, verb);
                                break;
                            case ActionVerb.Modify:
                                ActionEffectUtility.TryModify(target.QueriableObj, verb);
                                break;
                            case ActionVerb.Match:
                                ActionEffectUtility.TryMatch(target.QueriableObj, verb);
                                break;
                            case ActionVerb.ModifyReproduction:
                                ActionEffectUtility.TryModifyReproduction(target.QueriableObj, verb);
                                break;
                            case ActionVerb.ModifyTrap:
                                ActionEffectUtility.TryModifyTrap(target.QueriableObj, verb);
                                break;
                            case ActionVerb.TrapPathway:
                                ActionEffectUtility.TryTrapPathway(target.QueriableObj, verb);
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

        #region HardCoded Modification

        /// <summary>
        /// Modifying happens BEFORE selections are made
        /// </summary>
        private ActionEffect ModifyHardCode(ActionEffect toModify)
        {
            if (toModify.HardCodedId == null || toModify.HardCodedId.Equals(string.Empty))
            {
                return toModify;
            }

            switch (toModify.HardCodedId)
            {
                case "eat-the-invasive":
                    return ModifyEatInvasive(toModify);
                default:
                    Debug.LogWarning("[ActionEffectSpecifierSystem] Tried to modify hard code on " + toModify.HardCodedId + ", but no handling is in place!");
                    return toModify;
            }
        }

        private ActionEffect ModifyEatInvasive(ActionEffect toModify)
        {
            // For every 2 Awareness, remove 1 Invasive from a different ecosystem. || OUTDOOR - 1 additional Invasive(not per Awareness).
            toModify.MaxTargets = InterveneAwarenessInterfacer.Instance.GetValue() * 1;
            if (StatsInterfacer.Instance.GetValue(StatsInterfacer.OUTDOOR_KEY) > 1)
            {
                toModify.MaxTargets++;
            }

            return toModify;
        }

        #endregion // HardCoded Modification

        #region HardCoded Execution

        /// <summary>
        ///  Executing happens AFTER selections are made
        /// </summary>
        /// <param name="toExecute"></param>
        private void ExecuteHardCode(EffectChunk toExecute)
        {
            switch (toExecute.HardCodedId)
            {
                case "aquaculturist-outreach":
                    ExecuteAqua();
                    break;
                case "angler-outreach":
                    ExecuteAngler();
                    break;
                case "ballast-treatment":
                    ExecuteBallastTreatment();
                    break;
                default:
                    Debug.LogWarning("[ActionEffectSpecifierSystem] Tried to execute hard code on " + toExecute.HardCodedId + ", but no handling is in place!");
                    break;
            }
        }

        private void ExecuteAqua()
        {
            ExecuteRollBelowAwarenessReducePathway("aquarium");
        }

        private void ExecuteAngler()
        {
            ExecuteRollBelowAwarenessReducePathway("baitbuckets");
        }

        private void ExecuteBallastTreatment()
        {
            // When a "Ballast" pathway activates, -1 Invasive from source instead of moving.
            ExecuteTrapInvasiveOnPathwayType("ballastwater", "ballast-treatment");
        }

        /// <summary>
        /// For every pathway of the given type, block movement and trap -1 Invasive at the source instead.
        /// </summary>
        /// <param name="pathId">Pathway type to filter on (see PathwayType)</param>
        /// <param name="effectId">Unique id for the applied pathway effect</param>
        private void ExecuteTrapInvasiveOnPathwayType(string pathId, string effectId)
        {
            // find all pathways of the given type
            ActionTargetDetails[] targets = new ActionTargetDetails[1];
            ActionTargetDetails pathwayDetails = new ActionTargetDetails();
            pathwayDetails.Target = ActionTarget.Pathway;
            ActionTargetCondition[] conditions = new ActionTargetCondition[1];
            ActionTargetCondition mainCondition = new ActionTargetCondition();
            mainCondition.Condition = ActionCondition.PathwayType;
            mainCondition.StrCheck = pathId;
            conditions[0] = mainCondition;
            pathwayDetails.Conditions = conditions;
            targets[0] = pathwayDetails;

            List<ActionVerb> verbs = new List<ActionVerb>();

            List<ModelTag> filteredTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(targets, verbs, filterExternal: true);

            // apply -1 invasive from source instead of move
            foreach (var tag in filteredTags)
            {
                var pathway = tag.QueriableObj.GetComponent<Pathway>();
                if (pathway != null)
                {
                    if (!pathway.OnTryMoveContains(effectId))
                    {
                        PathwayEffect trapEffect = new PathwayEffect();
                        trapEffect.EffectId = effectId;
                        // trapEffect.EffectType |= PathwayEffectType.BlockAll;
                        trapEffect.EffectType |= PathwayEffectType.Trapped;
                        trapEffect.TargetType = ActionTarget.Invasive;
                        trapEffect.Value = 1;

                        pathway.AddEffectOnTryMove(trapEffect);
                    }
                }
            }
        }

        private void ExecuteRollBelowAwarenessReducePathway(string pathId)
        {
            // Roll 1d6. If below Awareness, decrease all pathways of given type.
            int roll = UnityEngine.Random.Range(1, 7);
            if (roll < InterveneAwarenessInterfacer.Instance.GetValue())
            {
                // decrease all Bait Buckets pathways.
                ActionTargetDetails[] targets = new ActionTargetDetails[1];
                ActionTargetDetails anglerDetails = new ActionTargetDetails();
                anglerDetails.Target = ActionTarget.Pathway;
                ActionTargetCondition[] anglerConditions = new ActionTargetCondition[1];
                ActionTargetCondition mainCondition = new ActionTargetCondition();
                mainCondition.Condition = ActionCondition.PathwayType;
                mainCondition.StrCheck = pathId;
                anglerConditions[0] = mainCondition;
                anglerDetails.Conditions = anglerConditions;
                targets[0] = anglerDetails;

                List<ActionVerb> verbs = new List<ActionVerb>();
                verbs.Add(ActionVerb.Reduce);

                List<ModelTag> filteredTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(targets, verbs, filterExternal: true);

                ModelTagMgr.Instance.HighlightTags(filteredTags);

                // TODO: space of time between highlight and decrease
                ActionVerbDetails verbDetails = new ActionVerbDetails();
                verbDetails.Verb = ActionVerb.Reduce;
                verbDetails.Values = new List<float>();
                verbDetails.Values.Add(1);
                verbDetails.ModType = ModifierType.Fixed;
                verbDetails.Odds = 1;

                foreach (var tag in filteredTags)
                {
                    ActionEffectUtility.TryReduce(tag.QueriableObj, verbDetails);
                }

                ModelTagMgr.Instance.ClearExistingHighlights();
            }
        }

        #endregion // HardCoded Execution
    }

    public static class ActionEffectUtility
    {
        #region Effect Execution

        public static bool TryReduce(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toReduce = queriable.GetComponent<IReducible>();

            if (toReduce != null)
            {
                return toReduce.TryReduce(verbDetails.Values, verbDetails.ModType);
            }
            else
            {
                Debug.LogWarning("[Reducible] Tried to reduce on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryIncrease(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toIncrease = queriable.GetComponent<IIncreasable>();

            if (toIncrease != null)
            {
                return toIncrease.TryIncrease(verbDetails.Values, verbDetails.ModType);
            }
            else
            {
                Debug.LogWarning("[Increasable] Tried to increase on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryRemove(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toRemove = queriable.GetComponent<IRemovable>();

            if (toRemove != null)
            {
                return toRemove.TryRemove();
            }
            else
            {
                Debug.LogWarning("[Increasable] Tried to remove on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryReveal(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toReveal = queriable.GetComponent<IRevealable>();

            if (toReveal != null)
            {
                return toReveal.TryReveal();
            }
            else
            {
                Debug.LogWarning("[Increasable] Tried to reveal on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryAddTrap(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toAddTrapTo = queriable.GetComponent<IAddTrapable>();

            if (toAddTrapTo != null)
            {
                return toAddTrapTo.TryAddTrap((int)verbDetails.Values[0]);
            }
            else
            {
                Debug.LogWarning("[Increasable] Tried to add trap on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryAddNest(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toAddNestTo = queriable.GetComponent<IAddNestable>();

            if (toAddNestTo != null)
            {
                return toAddNestTo.TryAddNest((int)verbDetails.Values[0]);
            }
            else
            {
                Debug.LogWarning("[Increasable] Tried to add nest on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryModify(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toModify = queriable.GetComponent<IModifiable>();

            if (toModify != null)
            {
                return toModify.TryModify(verbDetails.Values, verbDetails.ModType);
            }
            else
            {
                Debug.LogWarning("[Increasable] Tried to modify on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryModifyReproduction(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toModify = queriable.GetComponent<IReproductionModifiable>();

            if (toModify != null)
            {
                return toModify.TryModifyReproduction(verbDetails.Values, verbDetails.ModType);
            }
            else
            {
                Debug.LogWarning("[ReproductionModifiable] Tried to modify reproduction on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryModifyTrap(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toModify = queriable.GetComponent<ITrapModifiable>();

            if (toModify != null)
            {
                return toModify.TryModifyTrap(verbDetails.Values, verbDetails.ModType);
            }
            else
            {
                Debug.LogWarning("[TrapModifiable] Tried to modify traps on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryTrapPathway(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toTrap = queriable.GetComponent<IPathwayTrappable>();

            if (toTrap != null)
            {
                int amt = verbDetails.Values.Count > 0 ? (int)verbDetails.Values[0] : 1;
                return toTrap.TryTrapPathway(amt);
            }
            else
            {
                Debug.LogWarning("[PathwayTrappable] Tried to trap on a tag (" + queriable.name + ") that does not support it!");
                return false;
            }
        }

        public static bool TryMatch(GameObject queriable, ActionVerbDetails verbDetails)
        {
            var toMatch = queriable.GetComponent<IMatchable>();

            if (toMatch != null)
            {
                return toMatch.TryMatch(queriable.GetComponent<IComparable>().GetId(), verbDetails.RelativeId, verbDetails.Values[0]);
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
