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
                newCard.Cost = cardData.Cost;

                newCard.Effects = cardData.Effects;

                CardStackUtility.AddToTop(this, newCard, false);
            }
        }

        public void Shuffle()
        {
            Sfx.Play("Oneshot.ShuffleDeck");
            CardStackUtility.ShuffleStack(this);
        }
    }
}