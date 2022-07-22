using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Logging;

namespace Dummy.Threads
{
    public class DummyUserActionThread
    {
        public Queue<IAction?> Actions { get; }
        public List<IAction?> ContinuousActions { get; }

        private readonly DummyUser m_Dummy;
        private readonly ILogger m_Logger;

        public DummyUserActionThread(DummyUser dummy, ILogger logger)
        {
            Actions = new();
            ContinuousActions = new();
            m_Dummy = dummy;
            m_Logger = logger;
        }

        public bool Enabled { get; set; }

        public async UniTask Start()
        {
            while (Enabled)
            {
                await UniTask.SwitchToThreadPool();
                await UniTask.DelayFrame(1, PlayerLoopTiming.FixedUpdate);
                
                try
                {
                    foreach (var action in ContinuousActions)
                    {
                        if (action == null)
                        {
                            continue;
                        }

                        await action.Do(m_Dummy);
                    }

                    if (Actions.Count > 0)
                    {
                        var action = Actions.Dequeue();
                        if (action == null)
                        {
                            continue;
                        }

                        await action.Do(m_Dummy);
                    }
                }
                catch (Exception e)
                {
                    m_Logger.LogError(e, "Action thread catch the exception");
                }
            }
        }
    }
}