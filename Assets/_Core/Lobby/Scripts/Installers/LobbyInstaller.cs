using MS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.Lobby
{
    public class LobbyInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PanelsManager>().FromComponentInHierarchy().AsSingle().NonLazy();
            Container.Bind<IGameLauncher>().To<LobbyGameLauncher>().FromComponentInHierarchy().AsSingle().NonLazy();
        }
    }
}
