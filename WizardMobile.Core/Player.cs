using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public abstract class Player
    {
        public Player(IWizardFrontend frontend, string name)
        {
            Name = name;
            _hand = new List<Card>();
            _frontend = frontend;
        }
        
        public abstract Task<Card> MakeTurn(GameContext gameContext);
        public abstract Task<int> MakeBid(GameContext gameContext);

        public void TakeCard(Card card)
        {
            _hand.Add(card);
        }

        public override int GetHashCode()
        {
            return 17 * Name.GetHashCode();
        }

        protected IWizardFrontend _frontend;
        protected List<Card> _hand;
        public IReadOnlyList<Card> Hand => _hand;
        public string Name { get; }
    }
}
