using MS.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WeaponInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<Shooter>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
}
