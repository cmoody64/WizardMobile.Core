using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public enum CardValue
    {
        JESTER = 1,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING,
        ACE,
        WIZARD
    }

    public enum CardSuite
    {
        SPADES,
        CLUBS,
        HEARTS,
        DIAMONDS,
        SPECIAL
    }

    public class Card: IEquatable<Card>
    {
        public Card(CardValue value, CardSuite suite)
        {
            Value = value;
            Suite = suite;
        }

        public CardValue Value { get; }
        public CardSuite Suite { get; }



        public override int GetHashCode()
        {
            return 17 * Value.GetHashCode() + 23 * Suite.GetHashCode();
        }

        public bool Equals(Card other)
        {
            return other != null && other.Suite == Suite && other.Value == Value;
        }

        public override string ToString()
        {
            return Suite != CardSuite.SPECIAL
                ? $"{Value.ToString().ToLower()}_of_{Suite.ToString().ToLower()}"
                : Value.ToString().ToLower();
        }

        public string Name => ToString();
        public string DisplayName => ToString().Replace('_', ' ');
    }
}
