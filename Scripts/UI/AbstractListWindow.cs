using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace Drones.UI
{
    using DataStreamer;
    using Utils;
    using Utils.Extensions;
    using Interface;   
    using static Singletons;

    public abstract class AbstractListWindow : AbstractWindow, IMultiDataSourceReceiver, IListWindow
    {
        private static readonly Dictionary<WindowType, Vector2> _WindowSizes = new Dictionary<WindowType, Vector2>
        {
            {WindowType.DroneList, new Vector2(1000, 650)},
            {WindowType.HubList, new Vector2(1000, 650)},
            {WindowType.JobHistory, new Vector2(1000, 500)},
            {WindowType.JobQueue, new Vector2(1180, 500)},
            {WindowType.NFZList, new Vector2(730, 600)}
        };

        private ListTupleContainer _TupleContainer;

        private Dictionary<IDataSource, ListTuple> _DataReceivers;

        private event ListChangeHandler ContentChanged;

        private bool _IsConnected;

        protected override Vector2 MinimizedSize
        {
            get
            {
                return Decoration.ToRect().rect.size + Close.transform.ToRect().rect.size.x * 2 * Vector2.right;
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

        protected virtual void OnEnable()
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

        public override void OnRelease()
        {
            if (Sources != null)
            {
                Sources.ItemAdded -= OnNewSource;
                Sources.ItemRemoved -= OnLooseSource;
                IsConnected = false;
                Sources = null;
                StartCoroutine(ClearDataReceivers());
            } 
            else
            {
                gameObject.SetActive(false);
            }
            transform.SetParent(UIObjectPool.PoolContainer, false);
            Opener = null;
            CreatorEvent = null;
            WindowState = false;
        }

        protected void OnDisable()
        {
            StopAllCoroutines();
        }

        #region IListWindow
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

        public event ListChangeHandler ListChanged
        {
            add
            {
                if (ContentChanged == null || !ContentChanged.GetInvocationList().Contains(value))
                {
                    ContentChanged += value;
                }
            }
            remove
            {
                ContentChanged -= value;
            }
        }
        #endregion

        #region IMultiDataSourceReceiver
        public abstract System.Type DataSourceType { get; }

        public virtual SecureSet<IDataSource> Sources { get; set; } //TODO assigned by caller i.e. button source

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

        public Dictionary<IDataSource, ListTuple> DataReceivers
        {
            get
            {
                if (_DataReceivers == null)
                {
                    _DataReceivers = new Dictionary<IDataSource, ListTuple>();
                }
                return _DataReceivers;
            }
        }

        public IEnumerator WaitForAssignment()
        {
            var first = new WaitUntil(() => Sources != null);
            var second = new WaitUntil(() => !IsClearing);
            yield return first;
            yield return second;
            gameObject.SetActive(true);

            for (int i = 0; i < Sources.Count; i++)
            {
                OnNewSource(Sources[i]);
            }

            // If any new IDronesObject is created that this Window cares about it'll notfy this Window
            Sources.ItemAdded += OnNewSource;
            Sources.ItemRemoved += OnLooseSource;

            IsConnected = true;
            yield break;
        }

        public IEnumerator ClearDataReceivers()
        {
            IsClearing = true;
            float end = UnityEngine.Time.realtimeSinceStartup;
            foreach (var receiver in DataReceivers.Values)
            {
                receiver.SelfRelease();
                if (UnityEngine.Time.realtimeSinceStartup - end > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                    end = UnityEngine.Time.realtimeSinceStartup;
                }
            }
            DataReceivers.Clear();
            IsClearing = false;
            gameObject.SetActive(false);
            yield break;
        }

        public void UpdateConnectionToReceivers()
        {
            foreach (var receiver in DataReceivers.Values)
            {
                receiver.IsConnected = IsConnected;
            }
        }

        public void OnNewSource(IDataSource source)
        {
            var element = (ListTuple) UIObjectPool.Get(TupleType, TupleContainer.transform);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            ContentChanged?.Invoke();
            TupleContainer.AdjustDimensions();
        }

        public void OnLooseSource(IDataSource source)
        {
            DataReceivers[source].SelfRelease();
            ListChanged -= DataReceivers[source].OnListChange;
            DataReceivers.Remove(source);
            ContentChanged?.Invoke();
            TupleContainer.AdjustDimensions();
        }
        #endregion

    }

}
