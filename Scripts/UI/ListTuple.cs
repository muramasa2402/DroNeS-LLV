using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Drones.UI
{
    using DataStreamer;
    using Drones.Utils;
    public class ListTuple : AbstractListElement, ISingleDataSourceReceiver
    {
        [SerializeField]
        private Button _Link;
        private DataField[] _Data;

        private Button Link
        {
            get
            {
                if (_Link == null)
                {
                    _Link = GetComponent<Button>();
                }
                return _Link;
            }
        }

        private void Awake()
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

        public System.Type DataSourceType
        {
            get
            {
                return ((AbstractListWindow)Window).DataSourceType;
            }
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

        public WindowType ReceiverType
        {
            get
            {
                return Window.Type;
            }
        }

        public IDataSource Source { get; set; }

        public bool IsConnected { get; set; }

        public int UID => GetInstanceID();

        public TimeKeeper.Chronos OpenTime => throw new System.NotImplementedException();

        public IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Source != null);
            Source.Connections.Add(UID, this);
            StartCoroutine(StreamData());
            yield break;
        }

        public IEnumerator StreamData()
        {
            var wait = new WaitForSeconds(Random.Range(1,2));
            var end = Time.realtimeSinceStartup;
            while (Source != null && Source.Connections.Contains(this))
            {
                var datasource = Source.GetData(ReceiverType);

                for (int i = 0; i < datasource.Length; i++)
                {
                    Data[i].SetField(datasource[i]);
                    if (Time.realtimeSinceStartup - end > Constants.CoroutineTimeSlice)
                    {
                        yield return null;
                        end = Time.realtimeSinceStartup;
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
