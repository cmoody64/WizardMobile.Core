using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public class Deck: IReadonlyDeck
    {
        // populates cards list with fixed Wizard deck
        public Deck()
        {
            _cards = new List<Card>();
            var standardSuites = new[] { CardSuite.CLUBS, CardSuite.SPADES, CardSuite.HEARTS, CardSuite.DIAMONDS };

            // add in TWO to ACE in each suite besides special
            foreach (var cardVal in Enumerable.Range((int)CardValue.TWO, (int)CardValue.ACE - (int)CardValue.TWO + 1))
            {
                foreach(var cardSuite in standardSuites)
                {
                    _cards.Add(new Card((CardValue)cardVal, cardSuite));
                }
            }

            // add in special cards
            for(int i = 0; i < NUM_SPECIAL_CARDS; i++)
            {
                _cards.Add(new Card(CardValue.JESTER, CardSuite.SPECIAL));
                _cards.Add(new Card(CardValue.WIZARD, CardSuite.SPECIAL));
            }
        }

        public IReadOnlyList<Card> Cards => _cards;
        private List<Card> _cards;

        public Card PopTop()
        {
            Card top = _cards[_cards.Count-1];
            _cards.RemoveAt(_cards.Count - 1);
            return top;
        }

        public void Shuffle()
        {
            var rand = new Random();
            for(int i = 0; i < _cards.Count; i++)
            {
                SwapCards(i, rand.Next(i, _cards.Count));
            }
        }

        private void SwapCards(int i, int j)
        {
            Card temp = _cards[i];
            _cards[i] = _cards[j];
            _cards[j] = temp;
        }

        private readonly int NUM_SPECIAL_CARDS = 4;

        // receives a list of cards and returns all cards missing from the given list to make a complete deck
        public static List<Card> GetDeckComplement(List<Card> cards)
        {
            List<Card> deckComplement = new List<Card>(new Deck().Cards);
            foreach (var card in cards)
                deckComplement.Remove(card);
            return deckComplement;
        }

        public static readonly int STARTING_CARD_COUNT = 60;
    }

    public interface IReadonlyDeck
    {
        IReadOnlyList<Card> Cards { get; }
    }
}
