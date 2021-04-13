using System.Threading.Tasks;
using Dummy.Users;

namespace Dummy.API
{
    public interface IAction
    {
        /// <summary>
        /// Do a action for dummy. Not in the main thread!
        /// </summary>
        /// <param name="dummy"></param>
        /// <returns></returns>
        Task Do(DummyUser dummy);
    }
}