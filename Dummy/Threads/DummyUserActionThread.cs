using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Dummy.Threads
{
    public class DummyUserActionThread
    {
        public Queue<IAction> Actions { get; }
        public List<IAction> ContinuousActions { get; }

        private readonly DummyUser m_Dummy;
        private readonly ILogger m_Logger;

        public DummyUserActionThread(DummyUser dummy, ILogger logger)
        {
            Actions = new Queue<IAction>();
            ContinuousActions = new List<IAction>();
            m_Dummy = dummy;
            m_Logger = logger;
        }

        public bool Enabled { get; set; }

        public async UniTask Start()
        {
            while (Enabled)
            {
                await UniTask.WaitForFixedUpdate();
                try
                {
                    foreach (var action in ContinuousActions)
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
                    m_Logger.LogError("Something goes wrong when doing action: " + e.ToString());
                }
            }
        }
    }
}