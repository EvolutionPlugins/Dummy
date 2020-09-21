using EvolutionPlugins.Dummy.API;
using EvolutionPlugins.Dummy.Models.Users;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.Threads
{
    public class DummyUserActionThread
    {
        public Queue<IAction> Actions { get; }
        public List<IAction> ContinuousActions { get; }

        private readonly DummyUser m_Dummy;

        public DummyUserActionThread(DummyUser dummy)
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
                try
                {
                    foreach (IAction action in ContinuousActions)
                    {
                        await action?.Do(m_Dummy);
                    }
                    if (Actions.Count > 0)
                    {
                        var action = Actions.Dequeue();
                        await action?.Do(m_Dummy);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
    }
}