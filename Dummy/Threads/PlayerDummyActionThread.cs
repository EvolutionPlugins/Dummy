using EvolutionPlugins.Dummy.API;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Threads
{
    public class PlayerDummyActionThread
    {
        public Queue<IAction> Actions { get; }
        public List<IAction> ContinuousActions { get; }

        private readonly PlayerDummy _dummy;

        public PlayerDummyActionThread(PlayerDummy dummy)
        {
            Actions = new Queue<IAction>();
            ContinuousActions = new List<IAction>();
            _dummy = dummy;
        }

        public bool Enabled { get; set; }

        public async Task Start()
        {
            while (Enabled)
            {
                foreach (IAction action in ContinuousActions)
                {
                    await action.Do(_dummy);
                }
                if (Actions.Count > 0)
                {
                    var actionQueue = Actions.Dequeue();
                    await actionQueue.Do(_dummy);
                }
            }
        }
    }
}