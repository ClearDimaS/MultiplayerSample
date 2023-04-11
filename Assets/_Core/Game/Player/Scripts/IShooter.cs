
using UnityEngine;

namespace MS.Player
{
    public interface IShooter
    {
        public Vector3 WorldPosition { get; }

        public void FireWeapon();
    }
}
