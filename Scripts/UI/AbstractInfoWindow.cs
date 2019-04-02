using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using DataStreamer;
    using Drones.Utils;
    using Utils.Extensions;
    using static Singletons;

    public abstract class AbstractInfoWindow : AbstractWindow, ISingleDataSourceReceiver
    {
        private DataField[] _Data;
        private static readonly Dictionary<WindowType, Vector2> _WindowSizes = new Dictionary<WindowType, Vector2>
        {
            {WindowType.Drone, new Vector2(450, 700)},
            {WindowType.Hub, new Vector2(450, 465)},
            {WindowType.Job, new Vector2(450, 500)},
        };

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

        protected virtual void OnDisable()
        {
            DataStreamer.UnregisterListener(DataSourceType, OnDataUpdate);
            Source = null;
        }

        #region ISingleDataSourceReceiver
        public WindowType ReceiverType
        {
            get
            {
                return Type;
            }
        }

        public IDronesObject Source { get; set; }

        public bool IsConnected { get; private set; }

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

        public abstract System.Type DataSourceType { get; }

        public void OnDataUpdate(IDronesObject datasource)
        {
            if (!IsConnected || datasource != Source) { return; }

            string[] assignment = datasource.GetData(Type);
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i].SetField(assignment[i]);
            }
        }

        public IEnumerator WaitForAssignment()
        {
            yield return new WaitUntil(() => Source != null);
            WindowName.SetText(Source.ToString());
            MaximizedSize = _WindowSizes[Type];
            DataStreamer.RegisterListener(DataSourceType, OnDataUpdate);
            IsConnected = true;
            DataStreamer.Invoke(DataSourceType, Source);
        }

        #endregion


    }

}
