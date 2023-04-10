using UnityEngine;

namespace MS.Level
{
    public class LevelBehaviour  : MonoBehaviour
    {
        private SpawnPoint[] playerSpawnPoints;

        private void Awake()
        {
            playerSpawnPoints = GetComponentsInChildren<SpawnPoint>(true);
        }

        public SpawnPoint GetPlayerSpawnPoint(int id)
        {
            return playerSpawnPoints[id % playerSpawnPoints.Length];
        }
    }
}
