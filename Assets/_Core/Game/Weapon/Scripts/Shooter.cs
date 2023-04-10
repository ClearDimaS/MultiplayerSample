using Fusion;
using System;
using UnityEngine;

namespace MS.Player
{
    public class Shooter : NetworkBehaviour, IShooter
    {
        [SerializeField] private Player player;
        [SerializeField] private Weapon weapon;

        [Networked]
        public TickTimer fireDelay { get; set; }

        public void FireWeapon()
        {
            TickTimer tickTimer = fireDelay;
            if (tickTimer.ExpiredOrNotRunning(Runner))
            {
                weapon.Fire(Runner, Object.InputAuthority, player.velocity);
                fireDelay = TickTimer.CreateFromSeconds(Runner, weapon.delay);
            }
        }
    }
}
