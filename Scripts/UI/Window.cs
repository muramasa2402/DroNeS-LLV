using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Drones.UI
{
    using Drones.Utils.Extensions;

    public abstract class Window : MonoBehaviour
    {
        [SerializeField]
        protected TextMeshProUGUI windowName;
        [SerializeField]
        protected Button close;
        [SerializeField]
        protected Button minimizeButton;
        [SerializeField]
        protected Button maximizeButton;
        [SerializeField]
        protected Vector2 maximizedSize;
        [SerializeField]
        protected Vector2 minimizedSize;
        [SerializeField]
        protected GameObject contentPanel;
        [SerializeField]
        protected List<GameObject> disableOnMinimize;

        Transform decoration;

        protected void SetUp()
        {
            if (decoration == null) 
            { 
                decoration = transform.Find("Decoration"); 
            }
            if (windowName == null)
            {
                windowName = decoration.Find("Name").GetComponent<TextMeshProUGUI>();
            }
            if (close == null)
            {
                close = decoration.Find("Close Button").GetComponent<Button>();
            }
            if (minimizeButton == null)
            {
                minimizeButton = decoration.Find("Minimize Button").GetComponent<Button>();
            }
            if (maximizeButton == null)
            {
                maximizeButton = decoration.Find("Maximize Button").GetComponent<Button>();
            }
            if (contentPanel == null)
            {
                contentPanel = transform.Find("Content Panel").gameObject;
            }
            minimizeButton.onClick.AddListener(MinimizeWindow);
            maximizeButton.onClick.AddListener(MaximizeWindow);
        }

        protected void MinimizeWindow()
        {
            foreach(GameObject go in disableOnMinimize)
            {
                go.SetActive(false);
            }
            maximizeButton.gameObject.SetActive(true);
            minimizeButton.gameObject.SetActive(false);
            transform.ToRect().sizeDelta = minimizedSize;
        }

        protected void MaximizeWindow()
        {
            foreach (GameObject go in disableOnMinimize)
            {
                go.SetActive(true);
            }
            maximizeButton.gameObject.SetActive(false);
            minimizeButton.gameObject.SetActive(true);
            transform.ToRect().sizeDelta = maximizedSize;
        }
    }
}
