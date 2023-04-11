using Fusion;
using MS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Zenject;

namespace MS.GameSession
{
    public enum PlayState 
    {
        TRANSITION,
        LOBBY,
        MATCH
    }

    public class GameSessionManager : NetworkBehaviourDI, IStateAuthorityChanged
    {
        [Inject] private PanelsManager panelsManager;
        [Inject] private DiContainer diContainer;
        [Inject] private IPlayerManager playerManager;
        [Inject] private GameData gameData;

        [SerializeField] private GameObject playerPrefab;

        private ILevelManager levelManager;
        private const int MAX_LIVES = 1;

        public PlayState PlayState { get; private set; }

        [Inject]
        private void Construct(ILevelManager levelManager)
        {
            this.levelManager = levelManager;
            levelManager.LevelState += state => SetPlayState(state);
        }

        private void Update()
        {
            playerManager.HandleNewPlayers();
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                LoadLevel(-1);
            }
        }

        public void DespawnPlayer(NetworkRunner runner, PlayerRef playerref)
        {
            IPlayer player = playerManager.Get(playerref);
            if(player != null)
                player.TriggerDespawn();
        }

        public void SpawnPlayer(NetworkRunner runner, PlayerRef playerref)
        {
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, playerref, InitNetworkState);
            void InitNetworkState(NetworkRunner runner, NetworkObject networkObject)
            {
                IPlayer player = networkObject.gameObject.GetComponent<IPlayer>();
                diContainer.InjectContexts(networkObject.gameObject);
                player.InitNetworkState(MAX_LIVES);
            }
        }

        public void SetPlayState(PlayState playState)
        {
            Debug.Log($"setting play state: {playState}");
            this.PlayState = playState;
        }

        public void StateAuthorityChanged()
        {
            Debug.Log($"State Authority of {GetType().Name} changed: {Object.StateAuthority}");
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!Object.HasStateAuthority)
                return;
            Debug.Log($"loading level!");
            levelManager.LoadLevel(nextLevelIndex);
        }

        public void OnPlayerDeath(IPlayer player)
        {
            Debug.Log($"player dead!");
            playerManager.MarkPlayerDead(player);
            var remaining = playerManager.GetAlivePlayers();
            Debug.Log($"remaining: {remaining.Length}");
            if (remaining.Length == 1)
            {
                gameData.WinnerName = $"Player_{remaining.First().PlayerID}";
                gameData.CoinsCount = remaining.First().CoinsCount;
                panelsManager.ShowPanel<GameOverPanel>();
            }
        }

        public void Quit()
        {
            Camera.main.transform.parent = null;
            NetworkRunner runner = FindObjectOfType<NetworkRunner>();
            if (runner != null && !runner.IsShutdown)
            {
                // Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
                runner.Shutdown(false);
            }
        }
    }
}
