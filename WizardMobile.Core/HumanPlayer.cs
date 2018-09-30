using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(IWizardFrontend frontend, string Name): base(frontend, Name)
        {                
        }

        public async override Task<int> MakeBid(GameContext gameContext)
        {
            return await _frontend.PromptPlayerBid(this);
        }

        public async override Task<Card> MakeTurn(GameContext gameContext)
        {
            var playableCards = CardUtils.GetPlayableCards(_hand, gameContext.CurRound.CurTrick.LeadingSuite);
            var cardToPlay = await _frontend.PromptPlayerCardSelection(this, playableCards);
            _hand.Remove(cardToPlay);
            return cardToPlay;
        }
    }
}
