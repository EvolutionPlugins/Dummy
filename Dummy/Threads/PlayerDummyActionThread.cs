using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Threads
{
    public class PlayerDummyActionThread
    {
        public Queue<IAction> Actions { get; }
        public List<IAction> ContinuousActions { get; }

        private readonly PlayerDummy m_Dummy;

        public PlayerDummyActionThread(PlayerDummy dummy)
        {
            Actions = new Queue<IAction>();
            ContinuousActions = new List<IAction>();
            m_Dummy = dummy;
        }

        public bool Enabled { get; set; }

        public async Task Start()
        {
            while (Enabled)
            {
                foreach (IAction action in ContinuousActions)
                {
                    await action?.Do(m_Dummy);
                }
                if (Actions.Count > 0)
                {
                    var actionQueue = Actions.Dequeue();
                    await actionQueue?.Do(m_Dummy);
                }
                await Task.Delay(10);
            }
        }
    }
}