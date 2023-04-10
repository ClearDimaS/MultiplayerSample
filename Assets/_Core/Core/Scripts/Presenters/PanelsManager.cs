using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MS.Core
{
    public class PanelsManager : MonoBehaviour
    {
        [Inject] private DiContainer diContainer;

        [SerializeField] private Transform panelsParent;

        private Dictionary<string, PanelBase> panels = new Dictionary<string, PanelBase>();
        private List<PanelBase> panelsStack = new List<PanelBase>();

        private void Start()
        {
            panels = new Dictionary<string, PanelBase>();
            var spawnedPanels = GetComponentsInChildren<PanelBase>(true);
            foreach ( var panel in spawnedPanels )
            {
                AddPanel(panel);
            }

            ShowPanel(spawnedPanels[0].GetType());
            for (int i = 1; i < spawnedPanels.Length; i++)
            {
                spawnedPanels[i].Close();
            }
        }

        public void ShowPanel<T>()
        {
            ShowPanel(typeof(T));
        }

        public void ClosePanel<T>()
        {
            ClosePanel(typeof(T));
        }

        public void ShowPanel(Type type)
        {
            var name = type.Name;
            var panel = GetPanel(name);
            panel.Show();
            RemoveFromStack(name);
            if (panelsStack.Count > 0)
                panelsStack.Last().Close();
            panelsStack.Add(panel);
        }

        public void ClosePanel(Type type)
        {
            var name = type.Name;
            if (!panels.ContainsKey(name))
                return;

            var panel = GetPanel(name);
            panel.Close();
            RemoveFromStack(name);
            if (panelsStack.Count > 0)
                panelsStack.Last().Show();
        }

        private PanelBase GetPanel(string type)
        {
            if (panels.ContainsKey(type))
            {
                if (panels[type] == null)
                    panels.Remove(type);
                else
                    return panels[type];
            }

            var panel = Resources.Load(type) as GameObject;
            var newPanelGO = diContainer.InstantiatePrefab(panel, panelsParent);
            var newPanel = newPanelGO.GetComponent<PanelBase>();
            AddPanel(newPanel);
            return newPanel;
        }

        private void AddPanel(PanelBase newPanel)
        {
            panels[newPanel.GetType().Name] = newPanel;
        }

        private void RemoveFromStack(string name)
        {
            for (int i = 0; i < panelsStack.Count; i++)
            {
                if (panelsStack[i].GetType().Name == name)
                {
                    panelsStack.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
