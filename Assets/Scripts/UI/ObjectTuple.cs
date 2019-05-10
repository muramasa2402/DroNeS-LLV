using UnityEngine;
using System.Collections;
using System;

namespace Drones.UI
{
    using DataStreamer;
    using Drones.Utils;
    using Random = UnityEngine.Random;
    public class ObjectTuple : AbstractListElement, ISingleDataSourceReceiver
    {
        private DataField[] _Data;

        void Awake()
        {
            Link.onClick.AddListener(delegate
            {
                Source.OpenInfoWindow();
            });
        }

        public override void OnGet(Transform parent)
        {
            base.OnGet(parent);
            StartCoroutine(WaitForAssignment());
        }

        public override void OnRelease()
        {
            StopAllCoroutines();
            if (Source != null)
            {
                Source.Connections.Remove(this);
                Source = null;
            }
            base.OnRelease();
        }

        #region ISingleDataSourceReceiver
        public DataField[] Data
        {
            get
            {
                if (_Data == null)
                {
                    _Data = GetComponentsInChildren<DataField>();
                }
                return _Data;
            }
        }

        public Type ReceiverType => Window.GetType();

        public IDataSource Source { get; set; }

        public int UID => GetInstanceID();

        public IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Source != null);
            Source.Connections.Add(UID, this);
            StartCoroutine(StreamData());
            yield break;
        }

        public IEnumerator StreamData()
        {
            var wait = new WaitForSeconds(Random.Range(1, 2));
            while (Source != null && Source.Connections.Contains(this))
            {
                var datasource = Source.GetData(ReceiverType);

                for (int i = 0; i < datasource.Length; i++)
                {
                    Data[i].SetField(datasource[i]);
                    if (TimeKeeper.DeltaFrame() > Constants.CoroutineTimeSlice)
                    {
                        yield return null;
                    }
                }
                if (Source.IsDataStatic) { break; }
                yield return wait;
            }
            yield break;
        }
        #endregion

    }
}
