using MS.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MS.GameSession
{
    public class QuitButtonPresenter : MonoBehaviour
    {
        [Inject] private GameSessionManager gameSessionManager;
        [Inject] private SceneChanger sceneChanger;

        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.AddListener(QuitGame);
        }

        private void QuitGame()
        {
            gameSessionManager.Quit();
            sceneChanger.LoadScene(0);
        }
    }
}
