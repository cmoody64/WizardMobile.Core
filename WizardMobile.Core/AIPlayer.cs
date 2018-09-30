using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace WizardMobile.Core
{
    public class AIPlayer : Player
    {
        public AIPlayer(IWizardFrontend frontend, string name): base(frontend, name)
        {
            _rand = new Random();
        }

        public async override Task<int> MakeBid(GameContext gameContext)
        {
            var trumpCard = gameContext.CurRound.TrumpCard;
            var playerCount = gameContext.PlayerCount;

            // simulate each card in _hand in a wide variety of tricks and record the wins for each card
            Dictionary<Card, int> winsByCard = SimulateRound(trumpCard, playerCount, SIMULATION_COUNT);
            Dictionary<Card, double> winPercentagesByCard = winsByCard.Aggregate(new Dictionary<Card, double>(), (acc, cardWinPair) =>
            {
                acc[cardWinPair.Key] = cardWinPair.Value * 1.0 / SIMULATION_COUNT;
                return acc;
            });

            var playerScores = gameContext.PlayerScores.ToList();
            // sort scores high to low => index 0 holds the player/score KeyValuePair for player for the first place player
            playerScores.Sort((a, b) => a.Value < b.Value ? 1 : -1);
            // find current placement relative to ther players (0 => 1st place)
            var curPlacement = playerScores.FindIndex(playerWinPair => playerWinPair.Key == this);

            // decide on a round strategy which determines winpctg thresholds for bidding
            AIPlayStrategy roundStrategy;
            if (curPlacement == 0)
                // first place => play conservative
                roundStrategy = AIPlayStrategy.CONSERVATIVE;
            else if (curPlacement == gameContext.PlayerCount - 1)
                // last place => play aggressive
                roundStrategy = AIPlayStrategy.AGGRESIVE;
            else
                // in between => play moderate
                roundStrategy = AIPlayStrategy.MODERATE;

            double winPctgBidThreshold = 0;
            if (roundStrategy == AIPlayStrategy.AGGRESIVE)
                winPctgBidThreshold = 0.30;
            else if (roundStrategy == AIPlayStrategy.MODERATE)
                winPctgBidThreshold = 0.45;
            else if (roundStrategy == AIPlayStrategy.CONSERVATIVE)
                winPctgBidThreshold = 0.60;

            // bid on the number of cards with win percentages above the threshold
            int bid = winPercentagesByCard
                .Select(cardWinPair => cardWinPair.Value /*select win pctg*/)
                .Where(winPctg => winPctg > winPctgBidThreshold)
                .Count();

            await _frontend.DisplayPlayerBid(bid, this);

            return bid;
        }

        public async override Task<Card> MakeTurn(GameContext gameContext)
        {
            var curRound = gameContext.CurRound;
            var curTrick = curRound.CurTrick;
            var curRoundTricks = curRound.Tricks;

            List<Card> allKnownCards = _hand.Concat(curRoundTricks.Aggregate(new List<Card>(), (acc, trick) =>
            {
                acc.AddRange(trick.CardsPlayed);
                return acc;
            })).ToList();
            allKnownCards.Add(curRound.TrumpCard);

            List<Card> remainingCards = Deck.GetDeckComplement(allKnownCards);

            // simulate trick and save the wins (strenth) of each car in _hand
            Dictionary<Card, int> winsByCard = SimulateTrick(curTrick, remainingCards, curRound.TrumpSuite, gameContext.PlayerCount, SIMULATION_COUNT);
            List<KeyValuePair<Card, int>> cardsSortedByWins = winsByCard.ToList();
            // sort it so that weakest cards are at lower indices and stronger card at higher indices                       
            cardsSortedByWins.Sort((a, b) => a.Value < b.Value ? -1 : 1);

            var curBid = curRound.Bids[this];
            var curWins = curRound.Results[this];
            var roundsLeft = curRound.RoundNum - curRound.Tricks.Count + 1;
            /*
                requieredStrengthOfPlay is a value determining strength of card to play based on game context
                given that requiredStrengthOfPlay = k:

                k>1  => impossible to win, play strongest card
                k=1 => play strongest cards from here on out to win
                0<k<1 => sort cards by strength and select closest index to k val
	                i.e. k = .6 and 5 cards => choose card 3 /5 ranked by strength
                k=0 => play weakest cards from here on out to win
                k<0 => won too many, impossible to win, play weakest card

            */
            double requiredStrengthOfPlay = (curBid - curWins) * 1.0 / roundsLeft;

            Card cardToPlay = null;
            if (requiredStrengthOfPlay >= 1)
                cardToPlay = cardsSortedByWins.Last().Key;
            else if (requiredStrengthOfPlay <= 0)
                cardToPlay = cardsSortedByWins.First().Key;
            else
            {
                // requiredStrengthOfPlay between 0 and 1
                int indexToPlay = (int)(requiredStrengthOfPlay * cardsSortedByWins.Count());
                cardToPlay = cardsSortedByWins[indexToPlay].Key;
            }

            await _frontend.DisplayTurnInProgress(this);
            _hand.Remove(cardToPlay);
            return cardToPlay;
        }

        // simulates playing each card in _hand against an in-progress trick, plays random cards to finish and score the trick
        // returns a dictionary of cards in _hand to number of wins
        // @param hiddenCards refers to the cards in the deck that could potentialy be played by other players
        private Dictionary<Card, int> SimulateTrick(TrickContext trick, List<Card> hiddenCards, CardSuite trumpSuite, int playerCount, int simCount)
        {
            var playableCards = CardUtils.GetPlayableCards(_hand, trick.LeadingSuite);
            Dictionary<Card, int> winsByCard = playableCards.Aggregate(new Dictionary<Card, int>(), (acc, card) => { acc[card] = 0; return acc; });

            for (int i = 0; i < simCount; i++)
            {
                foreach (var card in playableCards)
                {
                    var curSimRemainingCards = new List<Card>(hiddenCards);
                    var simPlayedCards = new List<Card>(trick.CardsPlayed);
                    simPlayedCards.Add(card);

                    // each remaining player plays a random card from a randomly generated hand                  
                    for (int j = simPlayedCards.Count(); j < playerCount; j++)
                    {
                        // rand hand selected from the remaining cards specific to this simulation
                        var randHand = takeRandomCardsFromList(curSimRemainingCards, _hand.Count());
                        var playableCardsFromRandHand = CardUtils.GetPlayableCards(randHand, trick.LeadingSuite);
                        simPlayedCards.Add(playableCardsFromRandHand[_rand.Next() % playableCardsFromRandHand.Count()]);
                    }
                    var winningCard = CardUtils.CalcWinningCard(simPlayedCards, trumpSuite, trick.LeadingSuite);

                    if (card.Equals(winningCard))
                    {
                        winsByCard[card]++;
                    }
                }
            }

            return winsByCard;
            //Dictionary<Card, double> winPercentagesByCard = new Dictionary<Card, double>();
            //foreach(var winCardPair in winsByCard)
            //{
            //    double winPctg = winCardPair.Value * 1.0 / SIMULATION_COUNT;
            //    winPercentagesByCard[winCardPair.Key] = winPctg;
            //}
            //return winPercentagesByCard;
        }

        // simulates each card in _hand in a wide variety of tricks and returns a win count for each card
        private Dictionary<Card, int> SimulateRound(Card trumpCard, int playerCount, int simCount)
        {
            Dictionary<Card, int> winsByCard = _hand.Aggregate(new Dictionary<Card, int>(), (acc, card) => { acc[card] = 0; return acc; });
            int simsPerTrick = 1;
            int simsPerPlayPos = simCount / (simsPerTrick * playerCount);

            var allKnownCards = new List<Card>(_hand);
            allKnownCards.Add(trumpCard);
            var remainingCards = Deck.GetDeckComplement(allKnownCards);
            
            // iterate through each possible play position and simulate each card in _hand being played
            for(int playPos = 0; playPos < playerCount; playPos++)
            {
                // simulate simPerPlayPos number of tricks for each play position
                for(int simNumber = 0; simNumber < simsPerPlayPos; simNumber++)
                {
                    // single trick simulation:
                    // because SimulateTrick simulates an in-progress trick to completion,
                    // a random trick with cards played must be built out before simulateTrick can be called

                    var curSimTrick = new TrickContext(1 /*trick context*/);
                    var curSimRemainingCards = new List<Card>(remainingCards);
                    
                    // for each sim, generate a random set of cardsPlayed by other players up to simPlayPos
                    // this means that if the current playPosition is 2, cards played for positions 0 and 1 need to be generated before the trick is simulated
                    for(int curSimPlayPos = 0; curSimPlayPos < playPos; curSimPlayPos++)
                    {
                        var randHand = takeRandomCardsFromList(curSimRemainingCards, _hand.Count);
                        var playableCards = CardUtils.GetPlayableCards(randHand, curSimTrick.LeadingSuite);
                        curSimTrick.CardsPlayed.Add(playableCards[_rand.Next() % playableCards.Count()]);
                    }

                    var curSimWinsByCard = SimulateTrick(curSimTrick, curSimRemainingCards, trumpCard.Suite, playerCount, simsPerTrick);
                    // using the results from the current trick simulation, update the overall winsByCard
                    foreach (var cardWinPair in curSimWinsByCard)
                        winsByCard[cardWinPair.Key] += cardWinPair.Value;
                }
            }

            return winsByCard;

        }

        private List<Card> takeRandomCardsFromList(List<Card> cardList, int numberToTake)
        {
            List<Card> removedCards = new List<Card>();
            for(int i = 0; i < numberToTake; i++)
            {
                var randIndex = _rand.Next() % cardList.Count();
                removedCards.Add(cardList[randIndex]);
                cardList.RemoveAt(randIndex);
            }
            return removedCards;
        }

        // updated each round, this stores the current bid to hit
        private readonly int SIMULATION_COUNT = 1000;
        private Random _rand;

        enum AIPlayStrategy
        {
            CONSERVATIVE,
            MODERATE,
            AGGRESIVE
        }
    }
}
