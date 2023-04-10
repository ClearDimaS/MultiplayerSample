using Fusion;
using FusionExamples.FusionHelpers;
using System;

namespace MS.GameSession
{
    public interface ILevelManager : INetworkSceneManager
    {
        public void SetLauncher(FusionLauncher launcher);

        public void LoadLevel(int nextLevelIndex);

        public event Action<PlayState> LevelState;
        public event Action LevelUnloaded;
        public event Action LevelLoaded;
    }
}
