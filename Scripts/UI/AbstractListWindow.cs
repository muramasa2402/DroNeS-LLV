using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using DataStreamer;
    using Utils;
    using Utils.Extensions;

    public abstract class AbstractListWindow : AbstractWindow, IMultiDataSourceReceiver
    {
        private Transform _ElementsParent;
        public Transform ElementsParent
        {
            get
            {
                if (_ElementsParent == null)
                {
                    _ElementsParent = ContentPanel.GetComponentInChildren<ListContent>().transform;
                }
                return _ElementsParent;
            }
        }
        private GameObject _ElementTemplate;
        public GameObject ElementTemplate
        {
            get
            {
                if (_ElementTemplate == null)
                {
                    _ElementTemplate = Resources.Load<GameObject>(PrefabType[Type]);
                }
                return _ElementTemplate;
            }
        }
         
        private static Dictionary<WindowType, string> _PrefabType;
        public static Dictionary<WindowType, string> PrefabType
        {
            get
            {
                if (_PrefabType == null)
                {
                    _PrefabType = new Dictionary<WindowType, string>
                    {
                        {WindowType.DroneList, Constants.DroneListElementPath},
                        {WindowType.HubList, Constants.HubListElementPath},
                        {WindowType.JobHistory, Constants.JobHistoryElementPath},
                        {WindowType.JobQueue, Constants.JobQueueElementPath}
                    };
                }
                return _PrefabType;
            }
        }
        protected override void Start()
        {
            MinimizedSize = Decoration.ToRect().sizeDelta;
            MaximizeWindow();
            DisableOnMinimize = new List<GameObject>
            {
                ContentPanel
            };
            base.Start();
            StartCoroutine(WaitForAssignment());
        }

        protected override void MinimizeWindow()
        {
            IsConnected = false;
            base.MinimizeWindow();
        }

        protected override void MaximizeWindow()
        {
            base.MaximizeWindow();
            IsConnected = true;
        }

        private bool _IsConnected;

        protected void OnDisable()
        {
            StopAllCoroutines();
            Sources.Alert -= OnNewSource;
            Sources = null;
        }

        #region IMultiDataSourceReceiver
        public abstract System.Type DataSourceType { get; }

        public AlertHashSet<IDronesObject> Sources { get; set; } //TODO

        public bool IsConnected
        {
            get
            {
                return _IsConnected;
            }

            private set
            {
                _IsConnected = value;

                UpdateConnectionToReceivers();
            }
        }

        public HashSet<ListTuple> DataReceivers { get; } //TODO assigned by caller

        public IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Sources != null);
            // Add alert function to source 
            // If any new IDronesObject is created that this Window cares about it'll notfy this Window
            Sources.Alert += OnNewSource;

            //HACK Maybe unnecessary copy
            // Create a receiver for each source
            var tmp = new HashSet<IDronesObject>(Sources);
            foreach (var source in tmp)
            {
                OnNewSource(source);
            }

            IsConnected = true;
        }

        public void UpdateConnectionToReceivers()
        {
            foreach (var receiver in DataReceivers)
            {
                receiver.IsConnected = IsConnected;
            }
        }

        public void OnNewSource(IDronesObject source)
        {
            var element = Instantiate(ElementTemplate, ElementsParent).GetComponent<ListTuple>();
            element.Source = source;
            element.gameObject.name = source.ToString();
            DataReceivers.Add(element);
            //TODO add recycling centre/pool
        }


        #endregion

    }

}
