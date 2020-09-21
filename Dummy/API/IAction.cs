using EvolutionPlugins.Dummy.Models.Users;
using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.API
{
    public interface IAction
    {
        Task Do(DummyUser dummy);
    }
}