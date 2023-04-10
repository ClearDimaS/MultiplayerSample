using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.App
{
    public class LoadingSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AppInitializer>().AsSingle().NonLazy();
        }
    }
}
