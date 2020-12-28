using EvolutionPlugins.Dummy.API;
using UnityEngine;

namespace EvolutionPlugins.Dummy.Extensions.Look.Actions
{
    public class FullRotationAction : IAction
    {

        private int _times;
        private readonly int _speed;

        public FullRotationAction(int speed, int times = 1)
        {
            _speed = speed;
            _times = times;
        }
        
        public void Do(PlayerDummy dummy)
        {
            dummy.Data.UnturnedUser.Player.Player.transform.rotation = Quaternion.Slerp(dummy.Data.UnturnedUser.Player.Player.transform.localRotation,Quaternion.LookRotation(dummy.Data.UnturnedUser.Player.Player.transform.forward), Time.deltaTime * _speed);
        }
    }
}