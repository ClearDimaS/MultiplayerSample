using Fusion;
using MS.Core;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Player
{
    public class Weapon : NetworkBehaviourDI
    {
        [SerializeField] private Transform[] _gunExits;
        [SerializeField] private Projectile _projectilePrefab; // Networked projectile
        [SerializeField] private float _rateOfFire;
        [SerializeField] private byte _ammo;

        private int _gunExit;
        private float _visible;
        private bool _active;

        public float delay => _rateOfFire;
        public bool isShowing => _visible >= 1.0f;
        public byte ammo => _ammo;

        /// <summary>
        /// Control the visual appearance of the weapon. This is controlled by the Player based
        /// on the currently selected weapon, so the boolean parameter is entirely derived from a
        /// networked property (which is why nothing in this class is sync'ed).
        /// </summary>
        /// <param name="show">True if this weapon is currently active and should be visible</param>
        public void Show(bool show)
        {
            if (_active && !show)
            {
                gameObject.SetActive(true);
            }
            else if (!_active && show)
            {
                gameObject.SetActive(false);
            }
        }


        /// <summary>
        /// Fire a weapon, spawning the bullet or, in the case of the hitscan, the visual
        /// effect that will indicate that a shot was fired.
        /// This is called in direct response to player input, but only on the server
        /// (It's filtered at the source in Player)
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="owner"></param>
        /// <param name="ownerVelocity"></param>
        public void Fire(NetworkRunner runner, PlayerRef owner, Vector3 ownerVelocity)
        {
            Transform exit = GetExitPoint();
            SpawnNetworkShot(runner, owner, exit, ownerVelocity);
        }

        /// <summary>
        /// Spawn a bullet prefab with prediction.
        /// On the authoritative instance this is just a regular spawn (host in hosted mode or weapon owner in shared mode).
        /// In hosted mode, the client with Input Authority will spawn a local predicted instance that will be linked to
        /// the hosts network object when it arrives. This provides instant client-side feedback and seamless transition
        /// to the consolidated state.
        /// </summary>
        private void SpawnNetworkShot(NetworkRunner runner, PlayerRef owner, Transform exit, Vector3 ownerVelocity)
        {
            Debug.Log($"Spawning Shot in tick {Runner.Simulation.Tick} stage={Runner.Simulation.Stage}");
            // Create a key that is unique to this shot on this client so that when we receive the actual NetworkObject
            // Fusion can match it against the predicted local bullet.
            var key = new NetworkObjectPredictionKey { Byte0 = (byte)owner.RawEncoded, Byte1 = (byte)runner.Simulation.Tick };
            runner.Spawn(_projectilePrefab, exit.position, exit.rotation, owner, (runner, obj) =>
            {
                obj.GetComponent<Projectile>().InitNetworkState(ownerVelocity);
            }, key);
        }

        private Transform GetExitPoint()
        {
            _gunExit = (_gunExit + 1) % _gunExits.Length;
            Transform exit = _gunExits[_gunExit];
            return exit;
        }
    }
}
