using MS.Core;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MS.GameSession
{
    public class GameData 
    {
        public int CoinsCount;

        public string WinnerName;
    }

    public class GameOverPanel : PanelBase
    {
        [Inject] private SceneChanger sceneChanger;
        [Inject] private GameData gameData;

        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private Button leaveButton;

        private void Start()
        {
            winnerText.text = $"winner is: {gameData.WinnerName}";
            coinsText.text = $"coins: {gameData.CoinsCount.ToString()}";
            leaveButton.onClick.AddListener(() => 
            {
                var gameSessionManager = FindObjectOfType<GameSessionManager>(true);
                if(gameSessionManager)
                    gameSessionManager.Quit();
                panelsManager.ClosePanel(this.GetType());
                sceneChanger.LoadScene(0);
            });
        }
    }
}
