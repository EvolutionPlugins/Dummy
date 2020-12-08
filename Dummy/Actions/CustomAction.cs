using Dummy.API;
using Dummy.Users;
using System;
using System.Threading.Tasks;

namespace Dummy.Actions
{
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
