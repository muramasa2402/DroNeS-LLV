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
            if (CurrentPosition != null)
            {
                CurrentPosition.GetComponent<Animation>().Stop();
                CurrentPosition.GetComponent<Animation>().Play();
            }
            else
            {
                CurrentPosition = Instantiate(PositionHighlightTemplate);
                CurrentPosition.name = "Current Position";
            }
            CurrentPosition.transform.position = position;
            CurrentPosition.transform.position += Vector3.up * CurrentPosition.transform.lossyScale.y;
        }

    }
}
