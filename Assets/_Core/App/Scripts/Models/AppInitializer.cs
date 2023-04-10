using MS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.App
{
    public class AppInitializer
    {
        [Inject] private SceneChanger sceneChanger;

        public void Initialize()
        {
            sceneChanger.LoadScene(CONSTANTS.SCENE_LOBBY);
        }
    }
}
