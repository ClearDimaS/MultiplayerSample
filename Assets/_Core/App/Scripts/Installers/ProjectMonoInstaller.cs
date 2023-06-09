using MS.Core;
using MS.GameSession;
using MS.Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.App
{
    public class ProjectMonoInstaller : MonoInstaller
    {
        [SerializeField] private LevelManager levelManager;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameData>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelManager>().FromComponentInNewPrefab(levelManager).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SceneChanger>().AsSingle().NonLazy();
        }
    }
}
