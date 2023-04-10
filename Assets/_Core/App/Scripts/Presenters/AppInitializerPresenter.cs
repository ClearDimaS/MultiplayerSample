using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.App
{
    public class AppInitializerPresenter : MonoBehaviour
    {
        [Inject] private AppInitializer appInitializer;

        void Start()
        {
            appInitializer.Initialize();
        }
    }
}
