using System.Collections;
using UnityEngine;

namespace Drones
{
    using Drones.UI;
    using Drones.Utils;
    using static Drones.Utils.Constants;
    using Drones.DataStreamer;
    using static Singletons;
    using Drones.Utils.Extensions;

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
                    _Instance = ((GameObject)Instantiate(Resources.Load(ManagerPath))).GetComponent<SimManager>();
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
            get
            {
                return _SimStatus;
            }
            set
            {
                _SimStatus = value;
                if (_SimStatus == SimulationStatus.Paused || _SimStatus == SimulationStatus.EditMode)
                {
                    PauseFrame.SetActive(true);
                }
                else
                {
                    PauseFrame.SetActive(false);
                }

                if (_SimStatus != SimulationStatus.EditMode)
                {
                    Selectable.Deselect();
                    Instance.StartCoroutine(StreamDataToDashboard());
                } 
                else
                {
                    Instance.StopCoroutine(StreamDataToDashboard());
                    TimeKeeper.TimeSpeed = TimeSpeed.Pause;
                    Edit.gameObject.SetActive(true);
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
                        MemberCondition = (item) => item is Job && ((Job)item).JobStatus == Status.Yellow
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
                        MemberCondition = (item) => item is Job && ((Job)item).JobStatus == Status.Red
                    };
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

        private void Awake()
        {
            _Instance = this;
            SimStatus = SimulationStatus.EditMode;
            StartCoroutine(StartPools());
        }

        IEnumerator StartPools()
        {
            // Wait for framerate
            yield return new WaitUntil(() => Time.unscaledDeltaTime < 1 / 40f);
            StartCoroutine(UIObjectPool.Init());
            StartCoroutine(ObjectPool.Init());
            yield break;
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
            _HubHighlight.transform.position = obj.transform.position;
            _HubHighlight.transform.SetParent(obj.transform, true);
        }

        public static void DehighlightHub()
        {
            if (_HubHighlight != null)
            {
                _HubHighlight.SetActive(false);
            }

        }

        public static void UpdateDelay(float dt)
        {
            _TotalDelay += dt;
        }

        public static void UpdateAudible(float dt)
        {
            _TotalAudible += dt;
        }

        public static void UpdateEnergy(float dE)
        {
            _TotalEnergy += dE;
        }

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

    }
}
