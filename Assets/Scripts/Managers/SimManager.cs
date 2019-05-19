using System.Collections;
using UnityEngine;

namespace Drones.Managers
{
    using Drones.UI;
    using Drones.Utils;
    using Drones.DataStreamer;
    using static Singletons;
    using Drones.Utils.Extensions;
    using Drones.Serializable;
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    public class SimManager : MonoBehaviour
    {
        #region Fields
        private SimulationStatus _SimStatus;
        private SecureSortedSet<uint, IDataSource> _AllDrones;
        private SecureSortedSet<uint, IDataSource> _AllHubs;
        private SecureSortedSet<uint, IDataSource> _AllNFZ;
        private SecureSortedSet<uint, IDataSource> _AllIncompleteJobs;
        private SecureSortedSet<uint, IDataSource> _AllCompleteJobs;
        private SecureSortedSet<uint, IDataSource> _AllDestroyedDrones;
        private SecureSortedSet<uint, Battery> _AllBatteries;
        private GameObject _PositionHighlight;
        private GameObject _HubHighlight;
        private float _Revenue;
        private uint _mapsLoaded;
        private float _TotalDelay;
        private float _TotalAudible;
        private float _TotalEnergy;
        private DataField[] _Data;
        private GameObject _PauseFrame;
        #endregion

        #region Properties
        public static SimManager Instance { get; private set;}
        public static GameObject PauseFrame 
        {
            get
            {
                if (Instance._PauseFrame == null)
                {
                    Instance._PauseFrame = OpenWindows.Transform.FindDescendent("PauseFrame", 0).gameObject;
                }
                return Instance._PauseFrame;
            }

        }
        public static SimulationStatus SimStatus
        {
            get => Instance._SimStatus;

            set
            {
                Instance._SimStatus = value;
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
                if (Instance._AllDestroyedDrones == null)
                {
                    Instance._AllDestroyedDrones = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is RetiredDrone
                    };
                }
                return Instance._AllDestroyedDrones;
            }

        }
        public static SecureSortedSet<uint, IDataSource> AllDrones
        {
            get
            {
                if (Instance._AllDrones == null)
                {
                    Instance._AllDrones = new SecureSortedSet<uint, IDataSource>()
                    {
                        MemberCondition = (item) => item is Drone
                    };
                    Instance._AllDrones.ItemRemoved += (obj) =>
                    {
                        ((Drone)obj).AssignedHub?.Drones.Remove(obj);
                    };
                }
                return Instance._AllDrones;
            }

        }
        public static SecureSortedSet<uint, IDataSource> AllHubs
        {
            get
            {
                if (Instance._AllHubs == null)
                {
                    Instance._AllHubs = new SecureSortedSet<uint, IDataSource>()
                    {
                        MemberCondition = (item) => item is Hub
                    };
                }
                return Instance._AllHubs;
            }
        }
        public static SecureSortedSet<uint, IDataSource> AllNFZ
        {
            get
            {
                if (Instance._AllNFZ == null)
                {
                    Instance._AllNFZ = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is NoFlyZone
                    };
                }
                return Instance._AllNFZ;
            }
        }
        public static SecureSortedSet<uint, IDataSource> AllIncompleteJobs
        {
            get
            {
                if (Instance._AllIncompleteJobs == null)
                {
                    Instance._AllIncompleteJobs = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is Job
                    };
                }
                return Instance._AllIncompleteJobs;
            }
        }
        public static SecureSortedSet<uint, IDataSource> AllCompleteJobs
        {
            get
            {
                if (Instance._AllCompleteJobs == null)
                {
                    Instance._AllCompleteJobs = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is Job
                    };
                    Instance._AllCompleteJobs.ItemAdded += (item) => { AllIncompleteJobs.Remove(item); };
                }
                return Instance._AllCompleteJobs;
            }
        }
        public static SecureSortedSet<uint, Battery> AllBatteries
        {
            get
            {
                if (Instance._AllBatteries == null)
                {
                    Instance._AllBatteries = new SecureSortedSet<uint, Battery>();
                }
                return Instance._AllBatteries;
            }
        }
        private static DataField[] Data
        {
            get
            {
                if (Instance._Data == null)
                {
                    Instance._Data = OpenWindows.Transform.FindDescendent("Drone Network", 1).GetComponentsInChildren<DataField>();
                }
                return Instance._Data;
            }
        }
        #endregion

        public static bool LoadComplete {

            get
            {
                if (Manhattan == null || Brooklyn == null)
                {
                    return false;
                }

                return Manhattan.RedrawComplete && Brooklyn.RedrawComplete;
            }

        }

        public static uint MapsLoaded => Instance._mapsLoaded;

        public static void OnMapLoaded() => Instance._mapsLoaded++;

        public bool _Initialized;

        public bool Initialized
        {
            get => _Initialized;

            set
            {
                if (value == true)
                {
                    SimStatus = SimulationStatus.EditMode;
                }
                _Initialized = value;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            ResetSingletons();
        }

        private void Awake()
        {
            if (Drone.ActiveDrones == null) { }
            DontDestroyOnLoad(PoolController.Get(ListElementPool.Instance).PoolParent.gameObject);
            DontDestroyOnLoad(PoolController.Get(ObjectPool.Instance).PoolParent.gameObject);
            DontDestroyOnLoad(PoolController.Get(WindowPool.Instance).PoolParent.gameObject);
            Instance = this;
            StartCoroutine(OnAwake());
        }

        private IEnumerator OnAwake()
        {
            yield return new WaitUntil(() => SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1));
            Initialized = true;
            Instance.StartCoroutine(StreamDataToDashboard());
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

        public static void UpdateRevenue(float value) => Instance._Revenue += value;

        public static void HighlightPosition(Vector3 position)
        {
            if (Instance._PositionHighlight != null)
            {
                Instance._PositionHighlight.GetComponent<Animation>().Stop();
                Instance._PositionHighlight.GetComponent<Animation>().Play();
                Instance._PositionHighlight.transform.GetChild(0).GetComponent<Animation>().Stop();
                Instance._PositionHighlight.transform.GetChild(0).GetComponent<Animation>().Play();
            }
            else
            {
                Instance._PositionHighlight = Instantiate(PositionHighlightTemplate);
                Instance._PositionHighlight.name = "Current Position";
            }
            Instance._PositionHighlight.transform.position = position;
            Instance._PositionHighlight.transform.position += Vector3.up * Instance._PositionHighlight.transform.lossyScale.y;
        }

        public static void HighlightHub(Selectable obj)
        {
            if (Instance._HubHighlight == null)
            {
                Instance._HubHighlight = Instantiate(HubHighlightTemplate);
                Instance._HubHighlight.name = "Hub Highlight";
            }
            Instance._HubHighlight.SetActive(true);
            Instance._HubHighlight.transform.SetParent(obj.transform, true);
            Instance._HubHighlight.transform.localPosition = Vector3.zero;

        }

        public static void DehighlightHub() => Instance._HubHighlight?.SetActive(false);

        public static void UpdateDelay(float dt) => Instance._TotalDelay += dt;

        public static void UpdateAudible(float dt) => Instance._TotalAudible += dt;

        public static void UpdateEnergy(float dE) => Instance._TotalEnergy += dE;

        private static IEnumerator StreamDataToDashboard()
        {
            var wait = new WaitForSeconds(0.75f);
            while (true)
            {
                GetData();
                for (int i = 0; i < Data.Length; i++)
                {
                    Data[i].SetField(Instance._DataOutput[i]);
                }
                yield return wait;
            }

        }

        private readonly string[] _DataOutput = new string[7];

        private static void GetData()
        {
            Instance._DataOutput[0] = AllDrones.Count.ToString();
            Instance._DataOutput[1] = AllHubs.Count.ToString();
            Instance._DataOutput[2] = AllCompleteJobs.Count.ToString();
            Instance._DataOutput[3] = "$" + Instance._Revenue.ToString("0.00");
            Instance._DataOutput[4] = UnitConverter.Convert(Chronos.min, Instance._TotalDelay / AllCompleteJobs.Count);
            Instance._DataOutput[5] = UnitConverter.Convert(Energy.kWh, Instance._TotalEnergy);
            Instance._DataOutput[6] = UnitConverter.Convert(Chronos.min, Instance._TotalAudible);
        }

        public static SSimulation SerializeSimulation()
        {
            var output = new SSimulation
            {
                revenue = Instance._Revenue,
                delay = Instance._TotalDelay,
                audible = Instance._TotalAudible,
                energy = Instance._TotalEnergy,
                drones = new List<SDrone>(),
                retiredDrones = new List<SRetiredDrone>(),
                batteries = new List<SBattery>(),
                hubs = new List<SHub>(),
                completedJobs = new List<SJob>(),
                incompleteJobs = new List<SJob>(),
                noFlyZones = new List<SNoFlyZone>(),
                currentTime = TimeKeeper.Chronos.Get().Serialize(),
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

        public static void ClearObjects()
        {
            NoFlyZone[] nfzArr = new NoFlyZone[AllNFZ.Count];
            AllNFZ.Values.CopyTo(nfzArr, 0);
            for (int i = 0; i < nfzArr.Length; i++)
            {
                nfzArr[i]?.Delete();
            }
            Hub[] hubArr = new Hub[AllHubs.Count];
            AllHubs.Values.CopyTo(hubArr, 0);
            for (int i = 0; i < hubArr.Length; i++)
            {
                hubArr[i]?.Delete();
            }
            while (Drone.ActiveDrones.childCount > 0)
            {
                Drone.ActiveDrones?.GetChild(0)?.GetComponent<Drone>()?.Delete();
            }
        }

        public static void LoadSimulation(SSimulation data)
        {
            Instance._Revenue = data.revenue;
            Instance._TotalDelay = data.delay;
            Instance._TotalAudible = data.audible;
            Instance._TotalEnergy = data.energy;
            TimeKeeper.SetTime(data.currentTime);
            ClearObjects();
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
                drone = new Dictionary<uint, int>(),
                dronePositions = new List<SVector3>(),
                droneDirections = new List<SVector3>()
            };

            foreach (NoFlyZone nfz in AllNFZ.Values)
                output.noFlyZones.Add(new StaticObstacle(nfz.transform));

            int i = 0;
            foreach (Drone d in AllDrones.Values)
            {
                output.drone.Add(d.UID, i);
                output.dronePositions.Add(d.Position);
                output.droneDirections.Add(d.Direction);
                i++;
            }

            return output;
        }

        public static SchedulerPayload GetSchedulerPayload()
        {
            SchedulerPayload output = new SchedulerPayload
            {
                revenue = Instance._Revenue,
                delay = Instance._TotalDelay,
                audible = Instance._TotalAudible,
                energy = Instance._TotalEnergy,
                drones = new Dictionary<uint, StrippedDrone>(),
                batteries = new Dictionary<uint, SBattery>(),
                hubs = new Dictionary<uint, SHub>(),
                incompleteJobs = new Dictionary<uint, SJob>(),
                noFlyZones = new Dictionary<uint, StaticObstacle>(),
                currentTime = TimeKeeper.Chronos.Get().Serialize()
            };

            foreach (Drone d in AllDrones.Values)
                output.drones.Add(d.UID, d.Strip());
            foreach (Hub hub in AllHubs.Values)
                output.hubs.Add(hub.UID, hub.Serialize());
            foreach (Battery bat in AllBatteries.Values)
                output.batteries.Add(bat.UID, bat.Serialize());
            foreach (Job job in AllIncompleteJobs.Values)
                output.incompleteJobs.Add(job.UID, job.Serialize());
            foreach (NoFlyZone nfz in AllNFZ.Values)
                output.noFlyZones.Add(nfz.UID, new StaticObstacle(nfz.transform));

            return output;
        }

    }
}
