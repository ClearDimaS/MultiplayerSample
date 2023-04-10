using Fusion;
using FusionExamples.FusionHelpers;
using MS.Core;
using MS.GameSession;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace MS.Level
{
    public class LevelManager : NetworkSceneManagerBase, ILevelManager
    {
        [Inject] private SceneChanger sceneChanger;

        [SerializeField] private int lobby;
        [SerializeField] private int game;
        [SerializeField] private int[] levels;
        [SerializeField] private LevelBehaviour currentLevel;

        private FusionLauncher launcher;
        private Scene loadedScene;

        public event Action LevelUnloaded;
        public event Action LevelLoaded;
        public event Action<PlayState> LevelState;

        public void SetLauncher(FusionLauncher launcher)
        {
            this.launcher = launcher;
        }

        public void LoadLevel(int nextLevelIndex)
        {
            Runner.SetActiveScene(nextLevelIndex < 0 ? game : levels[nextLevelIndex]);
        }

        public SpawnPoint GetPlayerSpawnPoint(int playerID)
        {
            return currentLevel.GetPlayerSpawnPoint(playerID);
        }

        protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene, FinishedLoadingDelegate finished)
        {
            Debug.Log($"Switching Scene from {prevScene} to {newScene}");
            if (newScene <= 0)
            {
                finished(new List<NetworkObject>());
                yield break;
            }

            if (Runner.IsServer || Runner.IsSharedModeMasterClient)
                LevelState?.Invoke(PlayState.TRANSITION);

            if (prevScene > 0)
            {
                yield return new WaitForSeconds(1.0f);
                LevelUnloaded?.Invoke();
            }

            launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loading, "");

            yield return null;

            if (loadedScene != default)
            {
                yield return sceneChanger.UnloadSceneAsync(loadedScene);
            }

            loadedScene = default;
            List<NetworkObject> sceneObjects = new List<NetworkObject>();
            if (newScene >= 0)
            {
                yield return sceneChanger.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
                yield return null;
                sceneObjects = FindNetworkObjects(loadedScene, disable: false);
            }

            yield return null;

            launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loaded, "");

            currentLevel = FindObjectOfType<LevelBehaviour>();
            finished(sceneObjects);

            if (loadedScene.buildIndex == lobby)
            {
                if (Runner.IsServer || Runner.IsSharedModeMasterClient)
                    LevelState?.Invoke(PlayState.LOBBY);
            }
            else
            {
                if (Runner != null && (Runner.IsServer || Runner.IsSharedModeMasterClient))
                {
                    LevelState?.Invoke(PlayState.MATCH);
                }
            }
            LevelLoaded?.Invoke();
        }

        protected override void Shutdown(NetworkRunner runner)
        {
            base.Shutdown(runner);
            currentLevel = null;
            if (loadedScene != default)
                SceneManager.UnloadSceneAsync(loadedScene);
            loadedScene = default;
        }
    }
}
