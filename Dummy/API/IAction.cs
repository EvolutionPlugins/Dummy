using Dummy.Users;
using System.Threading.Tasks;

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