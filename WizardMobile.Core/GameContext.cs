using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public class GameContext
    {
        // state that persists across the scope of a single game
        public GameContext(List<Player> players)
        {
            PlayerCount = players.Count;
            Rounds = new List<RoundContext>();
            PlayerScores = new Dictionary<Player, int>();

            // initialize player scores based off of the current player list passed in by the engine
            players.ForEach(player => PlayerScores[player] = 0);
        }
        public int PlayerCount { get; }
        public List<RoundContext> Rounds { get; }
        public Dictionary<Player, int> PlayerScores { get; }
        public RoundContext CurRound => Rounds.Last();
        public RoundContext PrevRound => Rounds.Count > 1 ? Rounds[Rounds.Count - 2] : null;
        public int MaxRoundCount => PlayerCount / Deck.STARTING_CARD_COUNT;
    }

    // state that persists across a single round
    public class RoundContext
    {
        public RoundContext(int roundNum, Card trumpCard)
        {
            RoundNum = roundNum;
            TrumpCard = trumpCard;
            Tricks = new List<TrickContext>();
            Bids = new Dictionary<Player, int>();
            Results = new Dictionary<Player, int>();
            PlayerDealOrder = null;
        }
        public int RoundNum { get; }
        public List<TrickContext> Tricks { get; }
        public Dictionary<Player, int> Bids { get; }
        public Dictionary<Player, int> Results { get; }
        public CardSuite TrumpSuite => TrumpCard?.Suite ?? CardSuite.SPECIAL;
        public Player Dealer { get; set; }
        public Card TrumpCard { get; set; }
        public TrickContext CurTrick => Tricks.Last();
        public TrickContext PrevTrick => Tricks.Count > 1 ? Tricks[Tricks.Count - 2] : null;
        public List<string> PlayerDealOrder { get; set; }  // player names arranged in the order they will be dealt cards (i.e. dealer last)      
    }

    // state that persists across a single trick
    public class TrickContext
    {
        public TrickContext()
        {
            CardsPlayed = new List<Card>();
        }
        public TrickContext(int trickNum)
        {
            TrickNum = trickNum;
            CardsPlayed = new List<Card>();
        }
        public int TrickNum { get; set; }
        public List<Card> CardsPlayed { get; }
        public CardSuite? LeadingSuite => CardsPlayed.Count > 0 ? CardsPlayed[0].Suite : (CardSuite?)null;
        public Player Winner { get; set; }
        public Card WinningCard { get; set; }
    }
}
