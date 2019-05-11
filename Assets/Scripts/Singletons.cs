using UnityEngine;
using System.Collections.Generic;
namespace Drones
{
    using Utils;
    using UI;
    using static Utils.Constants;
    using Drones.Serializable;

    public static class Singletons
    {
        private static GameObject _Sun;
        private static CustomMap _Manhattan;
        private static CustomMap _Brooklyn;
        private static GameObject _Boundary;
        private static Camera _MinimapCamera;
        private static NavigationWindow _Navigation;
        private static GameObject _PositionHighlightTemplate;
        private static GameObject _HubHighlightTemplate;
        private static GameObject _ToolTipTemplate;
        private static Transform _UICanvas;
        private static List<StaticObstacle> _Buildings;


        public static NavigationWindow Navigation
        {
            get
            {
                if (_Navigation == null)
                {
                    for (int i = 0; i < UICanvas.childCount && _Navigation == null; i++)
                    {
                        _Navigation = UICanvas.GetChild(i).GetComponent<NavigationWindow>();
                    }
                }
                return _Navigation;
            }
        }

        public static List<StaticObstacle> Buildings
        {
            get
            {
                if (_Buildings == null)
                {
                    _Buildings = new List<StaticObstacle>();
                    var t = GameObject.FindWithTag("Building").GetComponentsInChildren<Transform>();
                    foreach (var building in t)
                        _Buildings.Add(new StaticObstacle(building));
                }
                return _Buildings;
            }
        }

        public static GameObject Sun 
        {
            get
            {
                if (_Sun == null)
                {
                    _Sun = GameObject.Find("Sun");
                }
                return _Sun;
            }
        }

        public static CustomMap Manhattan
        {
            get
            {
                if (_Manhattan == null)
                {
                    _Manhattan = GameObject.FindWithTag("Manhattan").GetComponent<CustomMap>();
                }
                return _Manhattan;
            }
        }

        public static CustomMap Brooklyn
        {
            get
            {
                if (_Brooklyn == null)
                {
                    _Brooklyn = GameObject.FindWithTag("Brooklyn").GetComponent<CustomMap>();
                }
                return _Brooklyn;
            }
        }

        public static GameObject Boundary
        {
            get
            {
                if (_Boundary == null)
                {
                    _Boundary = GameObject.FindWithTag("Boundary");
                }
                return _Boundary;
            }
        }

        public static Camera MinimapCamera
        {
            get
            {
                if (_MinimapCamera == null)
                {
                    _MinimapCamera = GameObject.FindWithTag("Minimap").GetComponent<Camera>();
                }
                return _MinimapCamera;
            }
        }

        public static GameObject PositionHighlightTemplate
        {
            get
            {
                if (_PositionHighlightTemplate == null)
                {
                    _PositionHighlightTemplate = (GameObject) Resources.Load(PositionHighlightPath);
                }
                return _PositionHighlightTemplate;
            }
        }

        public static GameObject HubHighlightTemplate
        {
            get
            {
                if (_HubHighlightTemplate == null)
                {
                    _HubHighlightTemplate = (GameObject)Resources.Load(HubHighlightPath);
                }
                return _HubHighlightTemplate;
            }
        }

        public static GameObject ToolTipTemplate
        {
            get
            {
                if (_ToolTipTemplate == null)
                {
                    _ToolTipTemplate = (GameObject)Resources.Load(ToolTipPath);
                }
                return _ToolTipTemplate;
            }
        }

        public static Transform UICanvas
        {
            get
            {
                if (_UICanvas == null)
                {
                    _UICanvas = GameObject.FindWithTag("UICanvas").transform;
                }
                return _UICanvas;
            }
        }

    }
}