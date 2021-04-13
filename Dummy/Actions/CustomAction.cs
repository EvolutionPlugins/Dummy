extern alias JetBrainsAnnotations;
using Dummy.API;
using Dummy.Users;
using System;
using System.Threading.Tasks;
using JetBrainsAnnotations::JetBrains.Annotations;

namespace Dummy.Actions
{
    [UsedImplicitly]
    public class CustomAction : IAction
    {
        public CustomAction(Func<DummyUser, Task> func)
        {
            Func = func;
        }

        public Func<DummyUser, Task> Func { get; }

        public Task Do(DummyUser dummy)
        {
            return Func(dummy);
        }
    }
}
