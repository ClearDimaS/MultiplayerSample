using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MS.Core
{
    public class PanelBase : MonoBehaviour
    {
        [Inject] protected PanelsManager panelsManager;

        [SerializeField] private Button closeButton;

        private CanvasGroup group;
        private bool isInited = false;
        private void Awake()
        {
            if (!isInited)
                Init();
        }

        private void Init() 
        {
            isInited = true;
            group = GetComponent<CanvasGroup>();
            closeButton?.onClick.AddListener(() => panelsManager.ClosePanel(GetType()));
        }

        public void Show()
        {
            if (!isInited)
                Init();

            gameObject.SetActive(true);
            group.interactable = true;
            group.alpha = 1.0f;
        }

        public void Close()
        {
            if (!isInited)
                Init();

            gameObject.SetActive(false);
            group.interactable = false;
            group.alpha = .0f;
        }
    }
}
