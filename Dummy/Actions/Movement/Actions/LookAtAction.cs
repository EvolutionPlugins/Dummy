using System;
using System.Threading.Tasks;
using Dummy.API;
using Dummy.Users;
using OpenMod.UnityEngine.Extensions;
using UnityEngine;

namespace Dummy.Actions.Movement.Actions
{
    public class LookAtAction : IAction
    {
        private readonly Vector3 m_Pos;

        public LookAtAction(Vector3 pos)
        {
            m_Pos = pos;
        }

        public Task Do(DummyUser dummy)
        {
            Vector3 dir = (m_Pos - dummy.Player.Transform.Position.ToUnityVector()).normalized;
            float yaw = (float) Math.Atan2(dir.x, dir.y);
            float pitch = (float) Math.Atan2(dir.z, Math.Sqrt((dir.x * dir.x) + (dir.y * dir.y)));
            RotateAction rotation = new RotateAction(yaw, pitch);
            return rotation.Do(dummy);
        }
    }
}