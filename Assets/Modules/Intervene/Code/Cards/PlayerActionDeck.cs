using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FieldDay.Audio;

namespace AIS.Intervene
{
    public class PlayerActionDeck : CardStack
    {
        public void PopulateDeck(List<ActionCardData> actionCardDatas)
        {
            foreach(var cardData in actionCardDatas)
            {
                ActionCard newCard = new ActionCard();

                // populate data
                newCard.Attributes |= CardAttributes.Action;

                newCard.CardID = cardData.CardID;
                newCard.Title = cardData.Title;
                newCard.Description = cardData.Description;
                newCard.ImgPath = cardData.ImgPath;
                newCard.Suit = cardData.Suit;
                newCard.SetBaseCost(cardData.Cost);

                newCard.DiscoverResults = cardData.DiscoverResults;
                newCard.Effects = cardData.Effects;

                CardStackUtility.AddToTop(this, newCard, false);
            }

            AisGame.Events.Dispatch(InterveneEvents.OnActionDeckConstructed);
        }

        public void Shuffle()
        {
            Sfx.Play("Oneshot.ShuffleDeck");
            CardStackUtility.ShuffleStack(this);
        }
    }
}