using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MS.Core
{
    public class SceneChanger
    {
        public void LoadScene(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void LoadScene(string sceneName) 
        {
            SceneManager.LoadScene(sceneName);
        }

        public AsyncOperation LoadSceneAsync(SceneRef newScene, LoadSceneMode additive)
        {
            return SceneManager.LoadSceneAsync(newScene, additive);
        }

        public AsyncOperation UnloadSceneAsync(Scene loadedScene)
        {
            return SceneManager.UnloadSceneAsync(loadedScene);
        }
    }
}
