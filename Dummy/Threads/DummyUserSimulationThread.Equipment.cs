using System;
using Dummy.Actions.Interaction;
using SDG.Unturned;
using UnityEngine;

namespace Dummy.Threads;
public partial class DummyUserSimulationThread
{
    private bool m_LastPrimary;
    private bool m_LastSecondary;
    private uint m_LastPunch;

    private void SimulateEquipment()
    {
        if (Player.equipment.isSelected)
        {
            // todo: simulate useable
            return;
        }

        SimulatePunch();
    }

    private void SimulatePunch()
    {
        Punch(MouseState.Left, ref m_LastPrimary);
        Punch(MouseState.Right, ref m_LastSecondary);
    }

    private void Punch(MouseState punch, ref bool lastPunch)
    {
        var state = MouseState;
        var isBusy = Player.equipment.isBusy;

        if (state.HasFlag(punch) != lastPunch)
        {
            lastPunch = state.HasFlag(punch);
            if (!isBusy && Player.stance.stance is not EPlayerStance.PRONE && lastPunch && m_Simulation - m_LastPunch > 5)
            {
                m_LastPunch = m_Simulation;
                SendPunch();
            }
        }

        void SendPunch()
        {
            var raycastInfo = DamageTool.raycast(new Ray(Player.look.aim.position, Player.look.aim.forward),
            1.75f, RayMasks.DAMAGE_CLIENT, Player);

            (m_Packet.clientsideInputs ??= new()).Add(new(raycastInfo, ERaycastInfoUsage.Punch));
        }
    }
}
