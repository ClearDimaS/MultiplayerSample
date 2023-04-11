using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MS.Player
{
    public class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private Image healthAmmount;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private Player player;

        private void Update()
        {
            healthAmmount.fillAmount = (float)(int)player.life / (float)(int)Player.MAX_HEALTH;
            coinsText.text = player.coins.ToString();
        }
    }
}
