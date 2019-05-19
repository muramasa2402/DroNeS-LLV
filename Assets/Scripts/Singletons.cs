using UnityEngine;
namespace Drones
{
    using Utils;
    using static Utils.Constants;

    public static class Singletons
    {
        private static CustomMap _Manhattan;
        private static CustomMap _Brooklyn;
        private static GameObject _Boundary;
        private static GameObject _PositionHighlightTemplate;
        private static GameObject _HubHighlightTemplate;
        private static GameObject _ToolTipTemplate;

        public static void ResetSingletons()
        {
            _Manhattan = null;
            _Brooklyn = null;
            _Boundary = null;
            _PositionHighlightTemplate = null;
            _HubHighlightTemplate = null;
            _ToolTipTemplate = null;
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

    }
}