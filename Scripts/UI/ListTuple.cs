using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using DataStreamer;
    using Drones.Utils;
    using static Singletons;
    public class ListTuple : AbstractListElement, ISingleDataSourceReceiver
    {
        private DataField[] _Data;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(WaitForAssignment());
        }

        protected override void OnDisable()
        {
            DataStreamer.UnregisterListener(DataSourceType, OnDataUpdate);
            Source = null; //TODO set Source to null only when sent to garbage disposal
            base.OnDisable();
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

        public IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Source != null);
            DataStreamer.RegisterListener(DataSourceType, OnDataUpdate);
            DataStreamer.Invoke(DataSourceType, Source);
        }

        public void OnDataUpdate(IDronesObject datasource)
        {
            if (!IsConnected || datasource != Source) { return; }

            string[] assignment = datasource.GetData(Window.Type);
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i].SetField(assignment[i]);
            }
        }

        public IDronesObject Source { get; set; }
        public bool IsConnected { get; set; }

        public System.Type DataSourceType
        {
            get
            {
                return ((AbstractListWindow)Window).DataSourceType;
            }
        }
        #endregion

    }
}
