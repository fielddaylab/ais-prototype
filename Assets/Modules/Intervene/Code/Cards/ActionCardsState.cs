using FieldDay.SharedState;
using FieldDay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BeauUtil;
using Leaf.Runtime;
using BeauUtil.Debugger;
using FieldDay.Debugging;
using FieldDay.Scenes;
using BeauRoutine;

namespace AIS.Intervene
{
    public struct ActionCardData
    {
        public SerializedHash32 CardID;
        public string Title;
        public string Description;
        public string ImgPath;
        
        public bool IsValid;

        public ActionCardData(SerializedHash32 cardID, string title, string desc, string imgPath)
        {
            CardID = cardID;
            Title = title;
            Description = desc;
            ImgPath = imgPath;

            IsValid = true;
        }
    }

    [SharedStateInitOrder(10)]
    public sealed class ActionCardsState : SharedStateComponent, IScenePreload
    {
        [HideInInspector] public Dictionary<StringHash32, ActionCardData> AllActionCards;
        [HideInInspector] public List<StringHash32> UnlockedActionCards;

        public TextAsset[] CardSources;

        public IEnumerator<WorkSlicer.Result?> Preload()
        {
            yield return null;

            // Initialize Lists
            AllActionCards = new Dictionary<StringHash32, ActionCardData>();
            UnlockedActionCards = new List<StringHash32>();

            yield return null;

            // Populate Card data
            var populate = Async.Schedule(ActionCardsUtility.PopulateCards(this), AsyncFlags.HighPriority | AsyncFlags.MainThreadOnly);
            Game.Scenes.RegisterLoadDependency(populate);
        }
    }

    static public class ActionCardsUtility
    {

        #region Card Definition Parsing

        private static readonly string TITLE_TAG = "@title";
        private static readonly string DESC_TAG = "@desc";
        private static readonly string IMAGE_PATH_TAG = "@path";

        private static readonly string ENTRY_SEP = "::";

        private static readonly char[] END_DELIMS = new char[] { '\r', '\n' };

        #endregion // Card Definition Parsing

        static public IEnumerator PopulateCards(ActionCardsState cardsState)
        {
            List<string> cardStrings;

            foreach (TextAsset cardSource in cardsState.CardSources)
            {
                cardStrings = TextIO.TextAssetToList(cardSource, ENTRY_SEP);

                foreach (string str in cardStrings)
                {
                    try
                    {
                        ActionCardData newCard = ConvertDefToCard(str);

                        cardsState.AllActionCards.Add(newCard.CardID, newCard);

                        Debug.Log("[CardUtility] added " + newCard.CardID + ".");
                    }
                    catch (Exception e)
                    {
                        Debug.Log("[CardUtility] Parsing error! " + e.Message);
                    }
                    yield return null;
                }
            }
        }

        static private ActionCardData ConvertDefToCard(string cardDef)
        {
            Debug.Log("[CardUtility] converting card: " + cardDef);
            SerializedHash32 cardID = "";
            string title = "";
            string desc = "";
            string imgPath = "";

            // Parse into data

            // First line must be card id
            cardID = cardDef.Substring(0, cardDef.IndexOfAny(END_DELIMS));
            Debug.Log("[CardUtility] parsed card id : " + cardID);

            // Title comes after @title
            int titleIndex = cardDef.ToLower().IndexOf(TITLE_TAG);
            if (titleIndex != -1)
            {
                string afterTitle = cardDef.Substring(titleIndex);
                int offset = TITLE_TAG.Length;
                title = cardDef.Substring(titleIndex + offset, afterTitle.IndexOfAny(END_DELIMS) - offset).Trim();
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] title syntax error!");

                throw new Exception("Title");
            }

            // Description comes after @desc
            int descIndex = cardDef.ToLower().IndexOf(DESC_TAG);

            if (descIndex != -1)
            {
                string afterDesc = cardDef.Substring(descIndex);
                int offset = DESC_TAG.Length;
                desc = cardDef.Substring(descIndex + offset, afterDesc.IndexOfAny(END_DELIMS) - offset).Trim();
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] description syntax error!");

                throw new Exception("Description");
            }


            // Image Path comes after @path
            int imgPathIndex = cardDef.ToLower().IndexOf(IMAGE_PATH_TAG);

            if (imgPathIndex != -1)
            {
                string afterPathIndex = cardDef.Substring(imgPathIndex);
                int offset = IMAGE_PATH_TAG.Length;
                string imgPathStr = cardDef.Substring(imgPathIndex + offset, afterPathIndex.IndexOfAny(END_DELIMS) - offset).Trim();

                imgPath = imgPathStr;
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] image path syntax error!");

                throw new Exception("Image Path");
            }

            return new ActionCardData(cardID, title, desc, imgPath);
        }

        static public List<ActionCardData> GetUnlockedCards(ActionCardsState state)
        {
            List<ActionCardData> unlockedCards = new List<ActionCardData>();

            foreach (var cardId in state.UnlockedActionCards)
            {
                unlockedCards.Add(state.AllActionCards[cardId]);
            }

            Debug.Log("[Action Cards State] num unlocked cards: " + unlockedCards.Count);

            return unlockedCards;
        }

        static public List<ActionCardData> GetCardsFromEvidence(ActionCardsState state, List<StringHash32> evidenceCardIds)
        {
            List<ActionCardData> relevantCards = new List<ActionCardData>();
            EvidenceToActionConverterState converterState = Find.State<EvidenceToActionConverterState>();

            foreach (var evidenceCardId in evidenceCardIds)
            {
                // lookup evidence card  and extract action cards it unlocks
                List<SerializedHash32> actionIds = EvidenceActionConvertUtility.ConvertEvidenceToActionCardIds(converterState, evidenceCardId);
                // add each action card here
                foreach (var actionId in actionIds)
                {
                    var actionCard = state.AllActionCards[actionId];
                    if (relevantCards.Contains(actionCard))
                    {
                        // TODO: special handling for duplicates?
                    }
                    relevantCards.Add(actionCard);
                }
            }

            return relevantCards;
        }

        [LeafMember("UnlockCard")]
        static public void UnlockCardLeaf(StringHash32 cardId)
        {
            UnlockCard(Game.SharedState.Get<ActionCardsState>(), cardId);
        }

        [LeafMember("NumCardsUnlocked")]
        static public int NumCardsUnlocked()
        {
            return Game.SharedState.Get<ActionCardsState>().UnlockedActionCards.Count;
        }

        static public void UnlockCard(ActionCardsState state, StringHash32 id)
        {
            if (!state.UnlockedActionCards.Contains(id))
            {
                state.UnlockedActionCards.Add(id);
            }
        }

        [DebugMenuFactory]
        static private DMInfo ActionCardUnlockDebugMenu()
        {
            DMInfo info = new DMInfo("Action Cards");
            info.AddButton("Unlock All Cards", () => {
                var c = Game.SharedState.Get<ActionCardsState>();
                foreach (var cardId in c.AllActionCards.Keys) {
                    ActionCardsUtility.UnlockCard(c, cardId);
                }
            }, () => Game.SharedState.TryGet(out ActionCardsState c));

            info.AddButton("Print Num Unlocked", () => {
                var c = Game.SharedState.Get<ActionCardsState>();
                GetUnlockedCards(c);
            }, () => Game.SharedState.TryGet(out ActionCardsState c));

            return info;
        }
    }
}