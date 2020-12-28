using EvolutionPlugins.Dummy.API;
using UnityEngine;

namespace EvolutionPlugins.Dummy.Extensions.Look.Actions
{
    public class FullRotationAction : IAction
    {
        private readonly int _speed;

        public FullRotationAction(int speed)
        {
            _speed = speed;
        }
        
        public void Do(PlayerDummy dummy)
        {
            dummy.Data.UnturnedUser.Player.Player.transform.rotation = Quaternion.Slerp(dummy.Data.UnturnedUser.Player.Player.transform.localRotation,Quaternion.LookRotation(dummy.Data.UnturnedUser.Player.Player.transform.forward), Time.deltaTime * _speed);
        }
    }
}