using System.Collections;
using UnityEngine;

namespace Drones
{
    using Drones.UI;
    using Drones.Utils;
    using static Drones.Utils.Constants;
    using Drones.DataStreamer;
    using static Singletons;
    using System;

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
        private static GameObject _PositionHighlight;
        private static GameObject _HubHighlight;
        private static float _Revenue;
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
        public static SecureSortedSet<uint, IDataSource> AllDestroyedDrones
        {
            get
            {
                if (_AllDestroyedDrones == null)
                {
                    _AllDestroyedDrones = new SecureSortedSet<uint, IDataSource>
                    {
                        MemberCondition = (item) => item is DestroyedDrone
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

        public static void MakeMoney(float value)
        {
            _Revenue += value;
        }

        public static void LoseMoney(float value)
        {
            _Revenue -= value;
        }

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

        #region IDataSource
        public IEnumerator StreamData()
        {
            var wait = new WaitForSeconds(1 / 10f);
            //TODO
            yield return wait;
        }

        public string[] GetData()
        {
            //TODO
            return null;
        }
        #endregion
    }
}
