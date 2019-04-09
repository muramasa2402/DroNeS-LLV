using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Drones.UI
{
    using Utils;
    using Utils.Extensions;

    public class HubWindow : AbstractInfoWindow
    {
        private static HashSet<HubWindow> _OpenHubWindows;
        private static HashSet<HubWindow> OpenHubWindows
        {
            get
            {
                if (_OpenHubWindows == null)
                {
                    _OpenHubWindows = new HashSet<HubWindow>();
                }
                return _OpenHubWindows;
            }
        }

        [SerializeField]
        private Button _GoToLocation;
        [SerializeField]
        private Button _ShowDroneList;

        private Button GoToLocation
        {
            get
            {
                if (_GoToLocation == null)
                {
                    _GoToLocation = transform.Find("Goto Button").GetComponent<Button>();
                }
                return _GoToLocation;
            }
        }

        private Button ShowDroneList
        {
            get
            {
                if (_ShowDroneList == null)
                {
                    _ShowDroneList = transform.Find("List Button").GetComponent<Button>();
                }
                return _ShowDroneList;
            }
        }

        public override System.Type DataSourceType { get; } = typeof(Hub);

        public override WindowType Type { get; } = WindowType.Hub;

        protected override void Awake()
        {
            base.Awake();
            GoToLocation.onClick.AddListener(delegate
            {
                var position = ((Hub)Source).transform.position;
                position.y = 0;
                StaticFunc.LookHere(position);
            });

            ShowDroneList.onClick.AddListener(OpenDroneList);
        }

        private void OpenDroneList()
        {
            var dronelist = (DroneListWindow)Singletons.UIPool.Get(WindowType.DroneList, Singletons.UICanvas);
            dronelist.Sources = ((Hub)Source).Drones;
            dronelist.Opener = OpenDroneList;
            dronelist.CreatorEvent = ShowDroneList.onClick;
            ShowDroneList.onClick.RemoveAllListeners();
            ShowDroneList.onClick.AddListener(dronelist.transform.SetAsLastSibling);
        }

    }

}
