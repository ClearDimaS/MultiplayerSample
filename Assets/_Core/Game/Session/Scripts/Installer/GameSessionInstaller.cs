using FusionExamples.Tanknarok;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.GameSession
{
    public class GameSessionInstaller : MonoInstaller
    {
        [SerializeField] private GameObject playerManager;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CameraStrategy_MultipleTargets>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<IPlayerManager>().FromComponentInNewPrefab(playerManager).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameSessionManager>().FromComponentInHierarchy().AsSingle().NonLazy();
        }
    }
}
