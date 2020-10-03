using Dummy.Users;
using System.Threading.Tasks;

namespace Dummy.API
{
    public interface IAction
    {
        Task Do(DummyUser dummy);
    }
}