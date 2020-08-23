using EvolutionPlugins.Dummy.API;
using System.Collections.Generic;
using System.Linq;

namespace EvolutionPlugins.Dummy.Threads
{
    public class PlayerDummyActionThread
    {
        public Queue<IAction> Actions { get; private set; }
        private Queue<IAction> _actions => (Queue<IAction>)Actions.Concat(ContinuousActions);
        public List<IAction> ContinuousActions { get; }

        private readonly PlayerDummy _dummy;

        public PlayerDummyActionThread(PlayerDummy dummy)
        {
            Actions = new Queue<IAction>();
            ContinuousActions = new List<IAction>();
            _dummy = dummy;
        }

        public bool Enabled { get; set; }

        public void Start()
        {
            while (Enabled)
            {
                foreach (IAction action in _actions)
                {
                    action.Do(_dummy);
                    //Lol
                    Actions = (Queue<IAction>)Actions.Where(ac => ac != action);
                }
            }
        }
    }
}