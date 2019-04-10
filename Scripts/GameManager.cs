using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Drones.UI;
    using Drones.Utils;
    using static Drones.Utils.Constants;
    using Drones.DataStreamer;

    public class GameManager : MonoBehaviour
    {
        private static GameManager _Instance;
        private static SimulationStatus _SimStatus;
        private static SecureSet<IDataSource> _AllDrones;
        private static SecureSet<IDataSource> _AllHubs;
        private static SecureSet<IDataSource> _AllNFZ;
        private static SecureSet<IDataSource> _AllIncompleteJobs;
        private static SecureSet<IDataSource> _AllCompleteJobs;

        public static GameManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = ((GameObject)Instantiate(Resources.Load(ManagerPath))).GetComponent<GameManager>();
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

                if (_SimStatus == SimulationStatus.Paused || _SimStatus == SimulationStatus.EditMode)
                {
                    Time.timeScale = 0;
                }
                else
                {
                    Time.timeScale = 1;
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

        private void Awake()
        {
            _Instance = this;
            StartCoroutine(StartPools());
        }

        IEnumerator StartPools()
        {
            yield return new WaitUntil(() => Time.deltaTime < 1 / 60f);
            StartCoroutine(UIObjectPool.Init());
            yield break;
        }

    }
}
