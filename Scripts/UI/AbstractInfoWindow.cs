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

        protected override Vector2 MaximizedSize 
        {
            get
            {
                return _WindowSizes[Type];
            }
        }

        protected override Vector2 MinimizedSize 
        {
            get
            {
                return Decoration.ToRect().sizeDelta;
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

        protected virtual void OnDisable()
        {
            if (Source != null)
            {
                DataStreamer.UnregisterListener(DataSourceType, OnDataUpdate);
                Source = null;
            }
            IsConnected = false;
        }

        #region ISingleDataSourceReceiver
        public WindowType ReceiverType
        {
            get
            {
                return Type;
            }
        }

        public bool IsConnected { get; protected set; }

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

        public IDataSource Source { get; set; }

        public void OnDataUpdate(IDataSource datasource)
        {
            if (!IsConnected || datasource != Source) { return; }

            string[] assignment = datasource.GetData(Type);
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i].SetField(assignment[i]);
            }
        }

        public virtual IEnumerator WaitForAssignment()
        {
            var wait = new WaitUntil(() => Source != null);
            yield return wait;
            WindowName.SetText(Source.ToString());
            DataStreamer.RegisterListener(DataSourceType, OnDataUpdate);
            IsConnected = true;
            DataStreamer.Invoke(DataSourceType, Source);
            yield break;
        }

        #endregion


    }

}
