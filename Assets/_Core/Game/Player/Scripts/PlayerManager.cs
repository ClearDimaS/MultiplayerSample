using Fusion;
using FusionExamples.Tanknarok;
using MS.GameSession;
using MS.Level;
using MS.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.Player
{
    public class PlayerManager : MonoBehaviour, IPlayerManager
    {
        [Inject] private CameraStrategy_MultipleTargets cameraStrategy;
        [Inject] private LevelManager levelManager;

        private static List<IPlayer> allPlayers = new List<IPlayer>();
        private Queue<IPlayer> playerQueue = new Queue<IPlayer>();

        private void Awake()
        {
            levelManager.LevelLoaded += OnLevelLoaded;
            levelManager.LevelUnloaded += OnLevelUnloaded;
        }

        public IPlayer Get(PlayerRef playerRef)
        {
            for (int i = allPlayers.Count - 1; i >= 0; i--)
            {
                if (allPlayers[i] == null || allPlayers[i].NT_Object == null)
                {
                    allPlayers.RemoveAt(i);
                    Debug.Log("Removing null player");
                }
                else if (allPlayers[i].NT_Object.InputAuthority == playerRef)
                    return allPlayers[i];
            }

            return null;
        }

        public void HandleNewPlayers()
        {
            if (playerQueue.Count > 0)
            {
                var player = playerQueue.Dequeue();
                cameraStrategy.AddTarget(player.GameObject);
                player.Respawn(0);
            }
        }

        public void AddPlayer(IPlayer player)
        {
            playerQueue.Enqueue(player);
        }

        public void RemovePlayer(IPlayer player)
        {
            if (player == null || !allPlayers.Contains(player))
                return;

            allPlayers.Remove(player);
            if (cameraStrategy) 
                cameraStrategy.RemoveTarget(player.GameObject);
        }

        private void OnLevelLoaded()
        {
            RespawnAll();
        }

        private void OnLevelUnloaded()
        {
            DespawnAll();
        }

        private void DespawnAll()
        {

        }

        private void RespawnAll()
        {

        }
    }
}
