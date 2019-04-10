using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Drones.UI
{
    using Drones.Interface;
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
        [SerializeField]
        private Button _AddDrone;
        [SerializeField]
        private Button _RemoveDrone;
        [SerializeField]
        private Button _AddBattery;
        [SerializeField]
        private Button _RemoveBattery;

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

        private Button AddDrone
        {
            get
            {
                if (_AddDrone == null)
                {
                    _AddDrone = transform.Find("Drones").Find("Add Button").GetComponent<Button>();
                }
                return _AddDrone;
            }
        }

        private Button RemoveDrone
        {
            get
            {
                if (_RemoveDrone == null)
                {
                    _RemoveDrone = transform.Find("Drones").Find("Subtract Button").GetComponent<Button>();
                }
                return _RemoveDrone;
            }
        }

        private Button AddBattery
        {
            get
            {
                if (_AddBattery == null)
                {
                    _AddBattery = transform.Find("Batteries").Find("Add Button").GetComponent<Button>();
                }
                return _AddBattery;
            }
        }

        private Button RemoveBattery
        {
            get
            {
                if (_RemoveBattery == null)
                {
                    _RemoveBattery = transform.Find("Batteries").Find("Subtract Button").GetComponent<Button>();
                }
                return _RemoveBattery;
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
            AddDrone.onClick.AddListener(GetDrone);
            RemoveDrone.onClick.AddListener(ReleaseDrone);
            AddBattery.onClick.AddListener(BuildBattery);
            RemoveBattery.onClick.AddListener(DestroyBattery);
        }

        private void OpenDroneList()
        {
            var dronelist = (DroneListWindow)UIObjectPool.Get(WindowType.DroneList, Singletons.UICanvas);
            dronelist.Sources = ((Hub)Source).Drones;
            dronelist.Opener = OpenDroneList;
            dronelist.CreatorEvent = ShowDroneList.onClick;
            ShowDroneList.onClick.RemoveAllListeners();
            ShowDroneList.onClick.AddListener(dronelist.transform.SetAsLastSibling);
        }

        private void GetDrone()
        {
            Drone drone = (Drone) ObjectPool.Get(typeof(Drone));
            ((Hub)Source).IdleDrones.Add(drone);
        }

        private void ReleaseDrone()
        {
            ObjectPool.Release((IPoolable)((Hub)Source).IdleDrones.Get());
        }

        private void BuildBattery()
        {
            ((Hub)Source).IdleBatteries.Add(new Battery());
        }

        private void DestroyBattery()
        {
            ((Hub)Source).IdleBatteries.Get();
        }



    }

}
