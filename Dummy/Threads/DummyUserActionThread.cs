using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using Microsoft.Extensions.Logging;
using SDG.Unturned;

namespace Dummy.Threads
{
    public class DummyUserActionThread : IAsyncDisposable
    {
        public ConcurrentQueue<IAction?> Actions { get; }
        public List<IAction?> ContinuousActions { get; }

        private readonly DummyUser m_Dummy;
        private readonly ILogger m_Logger;

        public DummyUserActionThread(DummyUser dummy, ILogger logger)
        {
            Actions = new ConcurrentQueue<IAction?>();
            ContinuousActions = new List<IAction?>();
            m_Dummy = dummy;
            m_Logger = logger;
        }

        public bool Enabled { get; set; }

        public async UniTaskVoid Start()
        {
            await UniTask.SwitchToThreadPool();

            while (Enabled)
            {
                try
                {
                    foreach (var action in ContinuousActions)
                    {
                        if (action == null)
                            continue;

                        await action.Do(m_Dummy);
                    }

                    if (!Actions.IsEmpty)
                    {
                        if (!Actions.TryDequeue(out IAction? action) || action == null)
                            continue;

                        await action.Do(m_Dummy);
                    }

                    if (Thread.CurrentThread.IsGameThread())
                    {
                        m_Logger.LogWarning("Action loop is on main thread!");
                        await UniTask.SwitchToThreadPool();
                    }
                }
                catch (Exception e)
                {
                    m_Logger.LogError(e, "Exception on Dummy action thread");
                }

                await Task.Delay(100);
            }
        }

        public ValueTask DisposeAsync()
        {
            Enabled = false;
            return new();
        }
    }
}