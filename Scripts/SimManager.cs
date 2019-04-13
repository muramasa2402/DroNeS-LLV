using System.Collections;
using UnityEngine;

namespace Drones
{
    using Drones.UI;
    using Drones.Utils;
    using static Drones.Utils.Constants;
    using Drones.DataStreamer;
    using static Singletons;

    public class SimManager : MonoBehaviour
    {
        private static SimManager _Instance;
        private static SimulationStatus _SimStatus;
        private static SecureSet<IDataSource> _AllDrones;
        private static SecureSet<IDataSource> _AllHubs;
        private static SecureSet<IDataSource> _AllNFZ;
        private static SecureSet<IDataSource> _AllIncompleteJobs;
        private static SecureSet<IDataSource> _AllCompleteJobs;
        private static SecureSet<IDataSource> _AllDestroyedDrones;
        private static GameObject _PositionHighlight;
        private static GameObject _HubHighlight;
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
        public static SimulationStatus SimStatus
        {
            get
            {
                return _SimStatus;
            }
            set
            {
                _SimStatus = value;
                if (_SimStatus != SimulationStatus.EditMode)
                {
                    Selectable.Deselect();
                } 
                else
                {
                    TimeKeeper.TimeSpeed = TimeSpeed.Pause;
                    Edit.gameObject.SetActive(true);
                }
            }
        }
        public static SecureSet<IDataSource> AllDestroyedDrones
        {
            get
            {
                if (_AllDestroyedDrones == null)
                {
                    _AllDestroyedDrones = new SecureSet<IDataSource>()
                    {
                        MemberCondition = (item) => item is DestroyedDrone
                    };
                }
                return _AllDestroyedDrones;
            }

        }
        public static SecureSet<IDataSource> AllDrones
        {
            get
            {
                if (_AllDrones == null)
                {
                    _AllDrones = new SecureSet<IDataSource>()
                    {
                        MemberCondition = (item) => item is Drone
                    };
                }
                return _AllDrones;
            }

        }
        public static SecureSet<IDataSource> AllHubs
        {
            get
            {
                if (_AllHubs == null)
                {
                    _AllHubs = new SecureSet<IDataSource>()
                    {
                        MemberCondition = (item) => item is Hub
                    };
                }
                return _AllHubs;
            }
        }
        public static SecureSet<IDataSource> AllNFZ
        {
            get
            {
                if (_AllNFZ == null)
                {
                    _AllNFZ = new SecureSet<IDataSource>
                    {
                        MemberCondition = (item) => item is NoFlyZone
                    };
                }
                return _AllNFZ;
            }
        }
        public static SecureSet<IDataSource> AllIncompleteJobs
        {
            get
            {
                if (_AllIncompleteJobs == null)
                {
                    _AllIncompleteJobs = new SecureSet<IDataSource>
                    {
                        MemberCondition = (item) => item is Job && ((Job)item).JobStatus == Status.Yellow
                    };
                }
                return _AllIncompleteJobs;
            }
        }
        public static SecureSet<IDataSource> AllCompleteJobs
        {
            get
            {
                if (_AllCompleteJobs == null)
                {
                    _AllCompleteJobs = new SecureSet<IDataSource>
                    {
                        MemberCondition = (item) => item is Job && ((Job)item).JobStatus == Status.Red
                    };
                }
                return _AllCompleteJobs;
            }
        }
        #endregion

        private void Awake()
        {
            _Instance = this;
            StartCoroutine(StartPools());
        }

        IEnumerator StartPools()
        {
            // Wait for framerate
            yield return new WaitUntil(() => Time.unscaledDeltaTime < 1 / 60f);
            StartCoroutine(UIObjectPool.Init());
            StartCoroutine(ObjectPool.Init());
            SimStatus = SimulationStatus.EditMode;
            yield break;
        }

        public static void HighlightPosition(Vector3 position)
        {
            if (_PositionHighlight != null)
            {
                _PositionHighlight.GetComponent<Animation>().Stop();
                _PositionHighlight.GetComponent<Animation>().Play();
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

    }
}
