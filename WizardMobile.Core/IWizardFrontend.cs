using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public interface IWizardFrontend
    {
        Task<bool> DisplayStartGame();
        Task<bool> DisplayStartRound(int roundNum);
        Task<bool> DisplayEndRound(int roundNum);
        Task<bool> DisplayStartTrick(int trickNum);
        Task<bool> DisplayEndTrick(int trickNum);
        Task<bool> DisplayTrumpCardSelected(Card trumpCard);
        Task<bool> DisplayTurnInProgress(Player player);
        Task<bool> DisplayTurnTaken(Card cardPlayed, Player player);
        Task<bool> DisplayPlayerBid(int bid, Player player);
        Task<bool> DisplayShuffle(IReadonlyDeck startingDeck);
        Task<bool> DisplayDeal(GameContext gameContext, List<Player> players);
        Task<bool> DisplayTrickWinner(RoundContext curRound);
        Task<bool> DisplayRoundScores(GameContext gameContext);
        Task<bool> DisplayBidOutcome(int roundNum, int totalBids);
        Task<Card> PromptPlayerCardSelection(HumanPlayer player, IReadOnlyList<Card> playableCards);
        Task<int> PromptPlayerBid(HumanPlayer player);
        Task<List<string>> PromptPlayerCreation();
    }
}
