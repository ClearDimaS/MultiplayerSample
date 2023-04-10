using Fusion;
using UnityEngine;

namespace MS.GameSession
{
    public interface IPlayer
    {
        public int PlayerID { get; }
        public GameObject GameObject { get; }
        public NetworkObject NT_Object { get; }

        public void InitNetworkState(int maxLives);
        public void TriggerDespawn();
        public void Respawn(float seconds);
    }
}
