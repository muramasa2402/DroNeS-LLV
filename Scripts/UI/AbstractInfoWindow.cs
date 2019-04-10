using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Drones.UI
{
    using DataStreamer;
    using Utils;
    using Utils.Extensions;

    public abstract class AbstractInfoWindow : AbstractWindow, ISingleDataSourceReceiver
    {

        private static readonly Dictionary<WindowType, Vector2> _WindowSizes = new Dictionary<WindowType, Vector2>
        {
            {WindowType.Drone, new Vector2(450, 650)},
            {WindowType.Hub, new Vector2(450, 465)},
            {WindowType.Job, new Vector2(450, 500)},
        };

        private DataField[] _Data;

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
                return Decoration.ToRect().rect.size + Close.transform.ToRect().rect.size.x * 2 * Vector2.right;
            }
        }

        #region IPoolable
        public override void OnRelease()
        {
            if (Source != null)
            {
                Source.Connections.Remove(this);
                Source.InfoWindow = null;
                Source = null;
                IsConnected = false;
            }
            base.OnRelease();
        }
        #endregion

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

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
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

        public virtual IEnumerator WaitForAssignment()
        {
            var wait = new WaitUntil(() => Source != null);
            yield return wait;
            WindowName.SetText(Source.ToString());
            IsConnected = true;
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
