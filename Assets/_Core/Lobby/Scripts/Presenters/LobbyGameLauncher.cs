using Fusion;
using FusionExamples.FusionHelpers;
using MS.Core;
using MS.GameSession;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace MS.Lobby
{

    public class LobbyGameLauncher : PanelBase, IGameLauncher
	{
		[Inject] private DiContainer diContainer;
        [Inject] private ILevelManager lm;

        [SerializeField] private GameSessionManager gameManagerPrefab;
        [SerializeField] private TMP_Text progressText;

		private GameSessionManager gameSessionManager;
        private FusionLauncher.ConnectionStatus status = FusionLauncher.ConnectionStatus.Disconnected;
		private GameMode gameMode;
		
		private void Awake()
		{
			DontDestroyOnLoad(this);
		}

		private void Start()
		{
			OnConnectionStatusUpdate(null, FusionLauncher.ConnectionStatus.Disconnected, "");
		}

		// What mode to play - Called from the start menu
		public void OnHostOptions(string room)
		{
			panelsManager.ShowPanel(GetType());
            gameMode = (GameMode.Host);
            InitConnection(room);
        }

		public void OnJoinOptions(string room)
        {
            panelsManager.ShowPanel(GetType());
            gameMode = (GameMode.Client);
			InitConnection(room);
        }

		private void InitConnection(string room) 
		{
            FusionLauncher launcher = FindObjectOfType<FusionLauncher>();
            if (launcher == null)
                launcher = new GameObject("Launcher").AddComponent<FusionLauncher>();
 
            lm.SetLauncher(launcher);

            launcher.Launch(gameMode, room, lm, OnConnectionStatusUpdate, OnSpawnWorld, OnSpawnPlayer, OnDespawnPlayer);
        }

		private void OnConnectionStatusUpdate(NetworkRunner runner, FusionLauncher.ConnectionStatus status, string reason)
		{
			if (!this)
				return;

			this.status = status;
			UpdateUI();
		}

		private void UpdateUI()
		{
			switch (status)
			{
				case FusionLauncher.ConnectionStatus.Disconnected:
					progressText.text = "Disconnected!";
					break;
				case FusionLauncher.ConnectionStatus.Failed:
					progressText.text = "Failed!";
					break;
				case FusionLauncher.ConnectionStatus.Connecting:
					progressText.text = "Connecting";
					break;
				case FusionLauncher.ConnectionStatus.Connected:
					progressText.text = "Connected";
					break;
				case FusionLauncher.ConnectionStatus.Loading:
					progressText.text = "Loading";
					break;
				case FusionLauncher.ConnectionStatus.Loaded:
					panelsManager.ClosePanel(GetType());
                    panelsManager.ClosePanel<LobbyMainPanel>();
                    break;
			}
		}


        private void OnSpawnWorld(NetworkRunner runner)
        {
            gameSessionManager = runner.Spawn(gameManagerPrefab, Vector3.zero, Quaternion.identity, null, InitNetworkState) as GameSessionManager;

            void InitNetworkState(NetworkRunner runner, NetworkObject world)
            {
                diContainer.InjectContexts(world.gameObject);
            }
        }

        private void OnSpawnPlayer(NetworkRunner runner, PlayerRef playerref)
        {
			Debug.Log($"on spawn player: {playerref.PlayerId}");
			gameSessionManager.SpawnPlayer(runner, playerref);
        }

        private void OnDespawnPlayer(NetworkRunner runner, PlayerRef playerref)
        {
			gameSessionManager.DespawnPlayer(runner, playerref);
        }
    }
}