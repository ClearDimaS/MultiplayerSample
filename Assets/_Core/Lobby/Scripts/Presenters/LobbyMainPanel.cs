using MS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MS.Lobby
{
    public class LobbyMainPanel : PanelBase
    {
        [SerializeField] private Button hostButton, joinButton;

        private void Awake()
        {
            hostButton.onClick.AddListener(() => panelsManager.ShowPanel<HostPanel>());
            joinButton.onClick.AddListener(() => panelsManager.ShowPanel<JoinPanel>());
        }
    }
}
