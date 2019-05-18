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

    public abstract class ObjectListWindow : AbstractWindow, IMultiDataSourceReceiver, IListWindow
    {
        private ListTupleContainer _TupleContainer;

        private Dictionary<IDataSource, ObjectTuple> _DataReceivers;

        private event ListChangeHandler ContentChanged;

        protected override Vector2 MinimizedSize
        {
            get
            {
                return Decoration.ToRect().rect.size + Close.transform.ToRect().rect.size.x * 2 * Vector2.right;
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

        public override void OnGet(Transform parent)
        {
            base.OnGet(parent);
            MaximizeWindow();
            StartCoroutine(WaitForAssignment());
            ListChanged += TupleContainer.AdjustDimensions;
        }

        public override void OnRelease()
        {
            StopAllCoroutines();
            if (Sources != null)
            {
                Sources.ItemAdded -= OnNewSource;
                Sources.ItemRemoved -= OnLooseSource;
                Sources = null;
                StartCoroutine(ClearDataReceivers());
            } 
            else
            {
                gameObject.SetActive(false);
            }
            ListChanged -= TupleContainer.AdjustDimensions;
            transform.SetParent(PC().PoolParent, false);
            Opener = null;
            CreatorEvent = null;
            IsOpen = false;
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
        public int UID => GetInstanceID();

        public virtual SecureSortedSet<uint, IDataSource> Sources { get; set; } 

        public bool IsClearing { get; set; }

        public Dictionary<IDataSource, ObjectTuple> DataReceivers
        {
            get
            {
                if (_DataReceivers == null)
                {
                    _DataReceivers = new Dictionary<IDataSource, ObjectTuple>();
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

            foreach (IDataSource source in Sources.Values)
            {
                OnNewSource(source);
            }

            // If any new IDronesObject is created that this Window cares about it'll notfy this Window
            Sources.ItemAdded += OnNewSource;
            Sources.ItemRemoved += OnLooseSource;
            yield break;
        }

        public IEnumerator ClearDataReceivers()
        {
            IsClearing = true;
            foreach (var receiver in DataReceivers.Values)
            {
                receiver.Delete();
                if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                {
                    yield return null;
                }
            }
            DataReceivers.Clear();
            IsClearing = false;
            gameObject.SetActive(false);
            yield break;
        }

        public void OnNewSource(IDataSource source)
        {
            var element = AbstractListElement.New<ObjectTuple>(this);
            element.Source = source;
            DataReceivers.Add(source, element);
            ListChanged += element.OnListChange;
            ContentChanged?.Invoke();
        }

        public void OnLooseSource(IDataSource source)
        {
            DataReceivers[source].Delete();
            ListChanged -= DataReceivers[source].OnListChange;
            DataReceivers.Remove(source);
            ContentChanged?.Invoke();
        }
        #endregion

    }

}
