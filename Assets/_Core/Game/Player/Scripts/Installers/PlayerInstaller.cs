using FusionExamples.Tanknarok;
using MS.GameSession;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.Player
{
    public class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CameraStrategy_MultipleTargets>().FromMethod(FromScene<CameraStrategy_MultipleTargets>).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerManager>().FromMethod(FromScene<PlayerManager>).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameSessionManager>().FromMethod(FromScene<GameSessionManager>).AsSingle().NonLazy();
        }

        private T FromScene<T>(InjectContext injectContext) where T : MonoBehaviour
        {
            return FindObjectOfType<T>(true);
        }
    }
}
