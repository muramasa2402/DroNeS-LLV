using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using Mapbox.Unity.Map;

namespace Drones
{
    using DataStreamer;
    using EventSystem;
    using Utils;
    using UI;
    using static Utils.Constants;

    public static class Singletons
    {
        private static GameObject _Sun;
        private static AbstractMap _Manhattan;
        private static AbstractMap _Brooklyn;
        private static GameObject _Boundary;
        private static RTSCameraComponent _RTS;
        private static EagleEyeCameraComponent _EagleEye;
        private static Camera _MinimapCamera;
        private static NavigationWindow _Navigation;
        private static GameObject _PositionHighlightTemplate;
        private static GameObject _HubHighlightTemplate;
        private static Transform _UICanvas;
        private static EventSystem<EventType, IEvent> _SimulationEvent;
        private static SimulationPanel _Control;
        private static EditPanel _Edit;
        private static TimeKeeper _Clock;

        public static SimulationPanel Control
        {
            get
            {
                if (_Control == null)
                {
                    _Control = UICanvas.GetComponentInChildren<SimulationPanel>(true);
                }
                return _Control;
            }
        }

        public static EditPanel Edit
        {
            get
            {
                if (_Edit == null)
                {
                    _Edit = UICanvas.GetComponentInChildren<EditPanel>(true);
                }
                return _Edit;
            }
        }

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

        public static TimeKeeper Clock
        {
            get
            {
                if (_Clock == null)
                {
                    _Clock = Sun.GetComponent<TimeKeeper>();
                }
                return _Clock;
            }
        }

        public static EagleEyeCameraComponent EagleEye
        {
            get
            {
                if (_EagleEye == null)
                {
                    _EagleEye = GameObject.FindWithTag("EagleEye").GetComponent<EagleEyeCameraComponent>();
                }
                return _EagleEye;
            }
        }

        public static EventSystem<EventType, IEvent> SimulationEvent
        {
            get
            {
                if (_SimulationEvent == null)
                {
                    _SimulationEvent = new EventSystem<EventType, IEvent>();
                }
                return _SimulationEvent;
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

        public static AbstractMap Manhattan
        {
            get
            {
                if (_Manhattan == null)
                {
                    _Manhattan = GameObject.FindWithTag("Manhattan").GetComponent<AbstractMap>();
                }
                return _Manhattan;
            }
        }

        public static AbstractMap Brooklyn
        {
            get
            {
                if (_Brooklyn == null)
                {
                    _Brooklyn = GameObject.FindWithTag("Brooklyn").GetComponent<AbstractMap>();
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

        public static RTSCameraComponent RTS
        {
            get
            {
                if (_RTS == null)
                {
                    _RTS = GameObject.FindWithTag("RTSCamera").GetComponent<RTSCameraComponent>();
                }
                return _RTS;
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