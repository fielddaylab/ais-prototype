using AIS.Model;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    public class DiscoverEffectMgr : MonoBehaviour
    {
        private void Awake()
        {
            AisGame.Events.Register(InterveneEvents.OnActionDeckConstructed, HandleActionDeckConstructed);
        }

        private void OnDestroy()
        {
            if (Game.IsShuttingDown) { return; }

            AisGame.Events.Deregister(InterveneEvents.OnActionDeckConstructed, HandleActionDeckConstructed);
        }

        /// <summary>
        /// For each card in Action Deck, process it's discovery action
        /// </summary>
        private void ApplyDiscoverEffects()
        {
            var cards = CardInteractionMgr.Instance.Hand.Cards;
            foreach (var card in cards)
            {
                var actionCard = (ActionCard)card;
                if (actionCard == null) { continue; }

                foreach (var discovery in actionCard.DiscoverResults)
                {
                    ProcessDiscoverResult(discovery.ActionEffect);
                }
            }
        }

        private void ProcessDiscoverResult(ActionEffect result)
        {
            List<ModelTag> filteredTags = ModelTagMgr.Instance.FilterTagsByTargetDetails(result.AllTargets, result.GetAllVerbs(), filterExternal: true);

            if (result.Specificity == ActionSpecificity.All)
            {
                ProcessDiscoverResultForTag(result, filteredTags);
            }
            else if (result.Specificity == ActionSpecificity.Random)
            {
                // randomly select from available tags up to max targets
                List<ModelTag> randomlySelected = new List<ModelTag>();
                for (int i = 0; i < result.MaxTargets; i++)
                {
                    if (filteredTags.Count == 0) { break; }

                    int randIndex = Random.Range(0, filteredTags.Count);
                    randomlySelected.Add(filteredTags[randIndex]);
                    filteredTags.RemoveAt(randIndex);
                }

                ProcessDiscoverResultForTag(result, randomlySelected);
            }
            else
            {
                Debug.LogWarning("[DiscoverEffectMgr] Discover effect defined as 'specific', but no player inputs are allowed!");
            }
        }

        private void ProcessDiscoverResultForTag(ActionEffect result, List<ModelTag> targets)
        {
            foreach (var verb in result.Verbs)
            {
                if (verb.Odds != 1)
                {
                    // TODO: roll dice animation
                    var success = Random.Range(0, 1.0f) >= verb.Odds;
                    // Evaluate odds to see if effect triggers
                    if (!success) { continue; }
                }

                foreach (var target in targets)
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
                        default:
                            continue;
                    }
                }
            }
        }

        #region Handlers

        private void HandleActionDeckConstructed()
        {
            ApplyDiscoverEffects();
        }

        #endregion // Handlers
    }
}

