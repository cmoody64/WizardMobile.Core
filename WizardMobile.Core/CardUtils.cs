using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public static class CardUtils
    {
        public static Card CalcWinningCard(List<Card> cardsPlayed, CardSuite trumpSuite, CardSuite? leadingSuite)
        {
            Card winningCard = null;
            foreach (var curCard in cardsPlayed)
            {
                if (curCard.Value == CardValue.WIZARD)
                {
                    winningCard = curCard;
                    break;
                }
                else if(curCard.Value == CardValue.JESTER)
                {
                    continue;
                }

                if (winningCard == null)
                {
                    winningCard = curCard;
                }
                else if (curCard.Suite == trumpSuite)
                {
                    if (winningCard.Suite == trumpSuite)
                    {
                        if (curCard.Value > winningCard.Value)
                            winningCard = curCard;
                    }
                    else
                    {
                        // if winning suite is not trump suite, current card is now winner
                        winningCard = curCard;
                    }
                }
                else if (curCard.Suite == leadingSuite)
                {
                    if (winningCard.Suite == leadingSuite && curCard.Value > winningCard.Value)
                    {
                        winningCard = curCard;
                    }
                }
            }
            return winningCard;
        }

        public static List<Card> GetPlayableCards(List<Card> hand, CardSuite? leadingSuite)
        {
            var leadingSuiteCards = hand.Where(card => card.Suite == leadingSuite);
            return leadingSuiteCards.Count() > 0
                ? leadingSuiteCards.Concat(hand.Where(card => card.Suite == CardSuite.SPECIAL)).ToList()
                : hand;
        }
    }
}
