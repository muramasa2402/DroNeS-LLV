using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using DataStreamer;
    using Utils;
    using Utils.Extensions;
    using static Singletons;

    public abstract class AbstractListWindow : AbstractWindow, IMultiDataSourceReceiver
    {

        private static readonly Dictionary<WindowType, Vector2> _WindowSizes = new Dictionary<WindowType, Vector2>
        {
            {WindowType.DroneList, new Vector2(1000, 700)},
            {WindowType.HubList, new Vector2(1000, 465)},
            {WindowType.JobHistory, new Vector2(1000, 500)},
            {WindowType.JobQueue, new Vector2(1180, 500)}
        };

        private ListTupleContainer _TupleContainer;
        public ListTupleContainer TupleContainer
        {
            get
            {
                if (_TupleContainer == null)
                {
                    _TupleContainer = ContentPanel.GetComponentInChildren<ListTupleContainer>();
                }
                return _TupleContainer;
            }
        }

        public abstract ListElement TupleType { get; }

        protected override Vector2 MinimizedSize
        {
            get
            {
                return Decoration.ToRect().sizeDelta;
            }
        }

        protected override Vector2 MaximizedSize
        {
            get
            {
                return _WindowSizes[Type];
            }
        }

        protected override void Awake()
        {
            DisableOnMinimize = new List<GameObject>
            {
                ContentPanel
            };
            base.Awake();
        }

        protected virtual void Start()
        {
            MaximizeWindow();
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
            StopCoroutine(WaitForAssignment());
            if (Sources != null)
            {
                Sources.SetChanged -= OnNewSource;
                IsConnected = false;
                Sources = null;
                StartCoroutine(ClearDataReceivers());
            }
        }

        #region IMultiDataSourceReceiver
        public abstract System.Type DataSourceType { get; }

        public virtual AlertHashSet<IDataSource> Sources { get; set; } //TODO assigned by caller i.e. button source

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

        public bool IsClearing { get; set; }

        public HashSet<ListTuple> DataReceivers { get; } = new HashSet<ListTuple>();

        public IEnumerator WaitForAssignment()
        {
            var first = new WaitUntil(() => Sources != null);
            var second = new WaitUntil(() => !IsClearing);
            yield return first;
            yield return second;
            // If any new IDronesObject is created that this Window cares about it'll notfy this Window
            Sources.SetChanged += OnNewSource;

            //HACK Maybe unnecessary copy
            // Create a receiver for each source
            var tmp = new HashSet<IDataSource>(Sources);
            foreach (var source in tmp)
            {
                OnNewSource(source);
            }

            IsConnected = true;
        }

        public IEnumerator ClearDataReceivers()
        {
            IsClearing = true;
            float end = Time.realtimeSinceStartup;
            foreach (var receiver in DataReceivers)
            {
                UIPool.Dump(receiver.Type, receiver);
                if (Time.realtimeSinceStartup - end > Constants.CoroutineTimeLimit)
                {
                    yield return null;
                    end = Time.realtimeSinceStartup;
                }
            }
            IsClearing = false;
            yield break;
        }

        public void UpdateConnectionToReceivers()
        {
            foreach (var receiver in DataReceivers)
            {
                receiver.IsConnected = IsConnected;
            }
        }

        public void OnNewSource(IDataSource source)
        {
            var element = (ListTuple) UIPool.Get(TupleType, TupleContainer.transform);
            element.Source = source;
            DataReceivers.Add(element);
        }



        #endregion

    }

}
