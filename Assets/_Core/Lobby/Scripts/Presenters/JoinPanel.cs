using MS.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MS.Lobby
{
    public class JoinPanel : PanelBase
    {
        [Inject] private IGameLauncher gameLauncher;

        [SerializeField] private Button enterButton;
        [SerializeField] private TMP_InputField inputField;

        void Start()
        {
            enterButton.onClick.AddListener(() => 
            {
                panelsManager.ClosePanel(this.GetType()); 
                gameLauncher.OnJoinOptions(inputField.text);
            });
        }

        // Update is called once per frame
        void Update()
        {
            enterButton.interactable = !string.IsNullOrEmpty(inputField.text);
        }
    }
}
