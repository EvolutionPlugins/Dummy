using System.Threading.Tasks;

namespace EvolutionPlugins.Dummy.API
{
    public interface IAction
    {
        Task Do(PlayerDummy dummy);
    }
}