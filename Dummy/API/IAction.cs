using System.Threading.Tasks;
using Dummy.Users;

namespace Dummy.API
{
    public interface IAction
    {
        /// <summary>
        /// Do a action for dummy.
        /// </summary>
        /// <param name="dummy"></param>
        Task Do(DummyUser dummy);
    }
}