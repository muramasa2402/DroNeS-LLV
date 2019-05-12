using System.Collections;
using UnityEngine;

namespace Drones.Managers
{
    using Drones.UI;
    using Drones.Utils;
    using static Drones.Utils.Constants;
    using Drones.DataStreamer;
    using static Singletons;
    using Drones.Utils.Extensions;
    using Drones.Serializable;
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    public class SimManager : MonoBehaviour
    {
        #region Fields
        private static SimManager _Instance;
        private static SimulationStatus _SimStatus;
        private static SecureSortedSet<uint, IDataSource> _AllDrones;
        private static SecureSortedSet<uint, IDataSource> _AllHubs;
        private static SecureSortedSet<uint, IDataSource> _AllNFZ;
        private static SecureSortedSet<uint, IDataSource> _AllIncompleteJobs;
        private static SecureSortedSet<uint, IDataSource> _AllCompleteJobs;
        private static SecureSortedSet<uint, IDataSource> _AllDestroyedDrones;
        private static SecureSortedSet<uint, Battery> _AllBatteries;
        private static GameObject _PositionHighlight;
        private static GameObject _HubHighlight;
        private static float _Revenue;

        private static float _TotalDelay;
        private static float _TotalAudible;
        private static float _TotalEnergy;
        private static DataField[] _Data;
        private static GameObject _PauseFrame;
        #endregion

        #region Properties
        public static SimManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = ((GameObject)Instantiate(Resources.Load(SimulationManagerPath))).GetComponent<SimManager>();
                }
                return _Instance;
            }
        }
        public static GameObject PauseFrame 
        {
            get
            {
                if (_PauseFrame == null)
                {
                    _PauseFrame = UICanvas.FindDescendent("PauseFrame", 0).gameObject;
                }
                return _PauseFrame;
            }

        }
        public static SimulationStatus SimStatus
        {
            get => _SimStatus;

            set
            {
                _SimStatus = value;
                if (value == SimulationStatus.Paused || value == SimulationStatus.EditMode)
                {
                    OnPause();
                }
                else
                {
                    OnPlay();
                }

                if (value != SimulationStatus.EditMode)
                {
                    Selectable.Deselect();
                }
                else
                {
                    EditPanel.Instance.gameObject.SetActive(true);
                }
            }
        }
        public static SecureSortedSet<uint, IDataSource> AllRetiredDrones
        {
            get
            {
                if (_AllDestroyedDrones == null)
                {
                    _AllDestroyedDrones = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is RetiredDrone
                    };
                }
                return _AllDestroyedDrones;
            }

        }
        public static SecureSortedSet<uint, IDataSource> AllDrones
        {
            get
            {
                if (_AllDrones == null)
                {
                    _AllDrones = new SecureSortedSet<uint, IDataSource>()
                    {
                        MemberCondition = (item) => item is Drone
                    };
                    _AllDrones.ItemRemoved += (obj) =>
                    {
                        ((Drone)obj).AssignedHub?.Drones.Remove(obj);
                    };
                }
                return _AllDrones;
            }

        }
        public static SecureSortedSet<uint, IDataSource> AllHubs
        {
            get
            {
                if (_AllHubs == null)
                {
                    _AllHubs = new SecureSortedSet<uint, IDataSource>()
                    {
                        MemberCondition = (item) => item is Hub
                    };
                }
                return _AllHubs;
            }
        }
        public static SecureSortedSet<uint, IDataSource> AllNFZ
        {
            get
            {
                if (_AllNFZ == null)
                {
                    _AllNFZ = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is NoFlyZone
                    };
                }
                return _AllNFZ;
            }
        }
        public static SecureSortedSet<uint, IDataSource> AllIncompleteJobs
        {
            get
            {
                if (_AllIncompleteJobs == null)
                {
                    _AllIncompleteJobs = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is Job
                    };
                }
                return _AllIncompleteJobs;
            }
        }
        public static SecureSortedSet<uint, IDataSource> AllCompleteJobs
        {
            get
            {
                if (_AllCompleteJobs == null)
                {
                    _AllCompleteJobs = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is Job
                    };
                    _AllCompleteJobs.ItemAdded += (item) => { AllIncompleteJobs.Remove(item); };
                }
                return _AllCompleteJobs;
            }
        }
        public static SecureSortedSet<uint, Battery> AllBatteries
        {
            get
            {
                if (_AllBatteries == null)
                {
                    _AllBatteries = new SecureSortedSet<uint, Battery>();
                }
                return _AllBatteries;
            }
        }
        private static DataField[] Data
        {
            get
            {
                if (_Data == null)
                {
                    _Data = UICanvas.FindDescendent("Drone Network", 1).GetComponentsInChildren<DataField>();
                }
                return _Data;
            }
        }
        #endregion

        public static bool LoadComplete => _mapsLoaded == 2;

        public static uint _mapsLoaded;

        public static void OnMapLoaded() => _mapsLoaded++;

        private void OnEnable()
        {
            _Instance = this;
            Instance.StartCoroutine(StreamDataToDashboard());
            StartCoroutine(Initialize());
            UICanvas.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            StopCoroutine(JobManager.ProcessQueue());
            StopCoroutine(RouteManager.ProcessQueue());

            PoolController.Reset();
            JobManager.Reset();
            RouteManager.Reset();
            Drone.Reset();
            Hub.Reset();
            NoFlyZone.Reset();
        }

        IEnumerator Initialize()
        {
            // Wait for framerate
            yield return new WaitUntil(() => LoadComplete);
            yield return new WaitUntil(() => SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1));
            SimStatus = SimulationStatus.EditMode;
            UICanvas.gameObject.SetActive(true);
            TimeKeeper.TimeSpeed = TimeSpeed.Pause;
            StartCoroutine(JobManager.ProcessQueue());
            StartCoroutine(RouteManager.ProcessQueue());
            PoolController.Get(ListElementPool.Instance);
            PoolController.Get(ObjectPool.Instance);
            PoolController.Get(WindowPool.Instance);
            _mapsLoaded = 0;
            yield break;
        }

        IEnumerator DroneUpdate()
        {
            TimeKeeper.Chronos time = TimeKeeper.Chronos.Get();
            while (true)
            {
                foreach (Drone d in AllDrones.Values)
                {
                    if (d.Movement != DroneMovement.Idle && d.Movement != DroneMovement.Drop)
                        RouteManager.AddToQueue(d);
                }
                yield return new WaitUntil(() => time.Timer() > 5);
                time.Now();
            }

        }

        public static void OnPlay()
        {
            Selectable.Deselect();
            PauseFrame.SetActive(false);
            Instance.StartCoroutine(StreamDataToDashboard());
        }

        public static void OnPause()
        {
            Instance.StopCoroutine(StreamDataToDashboard());
            TimeKeeper.TimeSpeed = TimeSpeed.Pause;
            PauseFrame.SetActive(true);
        }

        public static void UpdateRevenue(float value) => _Revenue += value;

        public static void HighlightPosition(Vector3 position)
        {
            if (_PositionHighlight != null)
            {
                _PositionHighlight.GetComponent<Animation>().Stop();
                _PositionHighlight.GetComponent<Animation>().Play();
                _PositionHighlight.transform.GetChild(0).GetComponent<Animation>().Stop();
                _PositionHighlight.transform.GetChild(0).GetComponent<Animation>().Play();
            }
            else
            {
                _PositionHighlight = Instantiate(PositionHighlightTemplate);
                _PositionHighlight.name = "Current Position";
            }
            _PositionHighlight.transform.position = position;
            _PositionHighlight.transform.position += Vector3.up * _PositionHighlight.transform.lossyScale.y;
        }

        public static void HighlightHub(Selectable obj)
        {
            if (_HubHighlight == null)
            {
                _HubHighlight = Instantiate(HubHighlightTemplate);
                _HubHighlight.name = "Hub Highlight";
            }
            _HubHighlight.SetActive(true);
            _HubHighlight.transform.SetParent(obj.transform, true);
            _HubHighlight.transform.localPosition = Vector3.zero;

        }

        public static void DehighlightHub() => _HubHighlight?.SetActive(false);

        public static void UpdateDelay(float dt) => _TotalDelay += dt;

        public static void UpdateAudible(float dt) => _TotalAudible += dt;

        public static void UpdateEnergy(float dE) => _TotalEnergy += dE;

        private static IEnumerator StreamDataToDashboard()
        {
            var wait = new WaitForSeconds(0.75f);
            while (true)
            {
                GetData();
                for (int i = 0; i < Data.Length; i++)
                {
                    Data[i].SetField(_DataOutput[i]);
                }
                yield return wait;
            }

        }

        private static readonly string[] _DataOutput = new string[6];

        private static void GetData()
        {
            _DataOutput[0] = AllDrones.Count.ToString();
            _DataOutput[1] = AllHubs.Count.ToString();
            _DataOutput[2] = "$" + _Revenue.ToString("0.00");
            _DataOutput[3] = UnitConverter.Convert(Chronos.min, _TotalDelay / AllCompleteJobs.Count);
            _DataOutput[4] = UnitConverter.Convert(Energy.kWh, _TotalEnergy);
            _DataOutput[5] = UnitConverter.Convert(Chronos.min, _TotalAudible);
        }

        public static SSimulation SerializeSimulation()
        {
            var output = new SSimulation
            {
                revenue = _Revenue,
                delay = _TotalDelay,
                audible = _TotalAudible,
                energy = _TotalEnergy,
                drones = new List<SDrone>(),
                retiredDrones = new List<SRetiredDrone>(),
                batteries = new List<SBattery>(),
                hubs = new List<SHub>(),
                completedJobs = new List<SJob>(),
                incompleteJobs = new List<SJob>(),
                noFlyZones = new List<SNoFlyZone>()
            };

            foreach (Drone drone in AllDrones.Values)
                output.drones.Add(drone.Serialize());
            foreach (Hub hub in AllHubs.Values)
                output.hubs.Add(hub.Serialize());
            foreach (RetiredDrone rDrone in AllRetiredDrones.Values)
                output.retiredDrones.Add(rDrone.Serialize());
            foreach (Battery bat in AllBatteries.Values)
                output.batteries.Add(bat.Serialize());
            foreach (Job job in AllCompleteJobs.Values)
                output.completedJobs.Add(job.Serialize());
            foreach (Job job in AllIncompleteJobs.Values)
                output.incompleteJobs.Add(job.Serialize());
            foreach (NoFlyZone nfz in AllNFZ.Values)
                output.noFlyZones.Add(nfz.Serialize());

            return output;
        }

        public static void LoadSimulation(SSimulation data)
        {
            _Revenue = data.revenue;
            _TotalDelay = data.delay;
            _TotalAudible = data.audible;
            _TotalEnergy = data.energy;
            NoFlyZone[] nfzArr = new NoFlyZone[AllNFZ.Count];
            AllNFZ.Values.CopyTo(nfzArr, 0);
            for (int i = 0; i < nfzArr.Length; i++)
            {
                nfzArr[i].Delete();
            }
            Hub[] hubArr = new Hub[AllHubs.Count];
            AllHubs.Values.CopyTo(hubArr, 0);
            for (int i = 0; i < hubArr.Length; i++)
            {
                hubArr[i].Delete();
            }

            AllNFZ.Clear();
            AllHubs.Clear();

            AllCompleteJobs.Clear();
            foreach (var job in data.completedJobs)
            {
                var loaded = new Job(job);
                AllCompleteJobs.Add(loaded.UID, loaded);
            }
            AllIncompleteJobs.Clear();

            foreach (var job in data.incompleteJobs)
            {
                var loaded = new Job(job);
                AllIncompleteJobs.Add(loaded.UID, loaded);
            }
            foreach (var nfz in data.noFlyZones)
            {
                NoFlyZone.Load(nfz);
            }
            AllRetiredDrones.Clear();
            foreach (var rDrone in data.retiredDrones)
            {
                var loaded = new RetiredDrone(rDrone);
            }
            AllBatteries.Clear();
            foreach (var hub in data.hubs)
            {
                Hub.Load(hub, data.drones, data.batteries);
            }

        }

        public static RouterPayload GetRouterPayload()
        {
            RouterPayload output = new RouterPayload
            {
                noFlyZones = new List<StaticObstacle>(),
                drone = new List<uint>(),
                dronePositions = new List<SVector3>(),
                droneDirections = new List<SVector3>()
            };

            foreach (NoFlyZone nfz in AllNFZ.Values)
                output.noFlyZones.Add(new StaticObstacle(nfz.transform));

            foreach (Drone drone in AllDrones.Values)
            {
                output.drone.Add(drone.UID);
                output.dronePositions.Add(drone.Position);
                output.droneDirections.Add(drone.Direction);
            }
            return output;
        }

    }
}
