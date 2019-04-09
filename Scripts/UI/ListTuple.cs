using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using DataStreamer;
    using Drones.Utils;
    public class ListTuple : AbstractListElement, ISingleDataSourceReceiver
    {

        private DataField[] _Data;

        protected void OnEnable()
        {
            StartCoroutine(WaitForAssignment());
        }

        protected void OnDisable()
        {
            StopAllCoroutines();
        }

        public override void OnRelease()
        {
            Source = null;
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

        public IEnumerator WaitForAssignment()
        {
            var get = Data.Length;
            yield return new WaitUntil(() => Source != null);
            StartCoroutine(StreamData());
            yield break;
        }

        public IEnumerator StreamData()
        {
            var end = Time.realtimeSinceStartup;
            while (Source != null && Source.Connections.Contains(this))
            {
                var datasource = Source.GetData(ReceiverType);

                for (int i = 0; i < datasource.Length; i++)
                {
                    Data[i].SetField(datasource[i]);
                    if (Time.realtimeSinceStartup - end > Constants.CoroutineTimeLimit)
                    {
                        yield return null;
                        end = Time.realtimeSinceStartup;
                    }
                }
            }
            yield break;
        }
        #endregion

    }
}
