using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Drones.UI
{
    using Utils.Extensions;
    using Drones.Utils;

    public class JobWindow : AbstractInfoWindow
    {
        [SerializeField]
        private Button _GoToOrigin;
        [SerializeField]
        private Button _GoToDestination;
        [SerializeField]
        private StatusSwitch _StatusSwitch;

        private Button GoToOrigin
        {
            get
            {
                if (_GoToOrigin == null)
                {
                    _GoToOrigin = transform.Find("Origin").GetComponentInChildren<Button>();
                }
                return _GoToOrigin;
            }
        }
        private Button GoToDestination
        {
            get
            {
                if (_GoToDestination == null)
                {
                    _GoToDestination = transform.Find("Dest.").GetComponentInChildren<Button>();
                }
                return _GoToDestination;
            }

        }       
        private StatusSwitch StatusSwitch
        {
            get
            {
                if (_StatusSwitch == null)
                {
                    _StatusSwitch = GetComponentInChildren<StatusSwitch>();
                }
                return _StatusSwitch;
            }
        }

        public override System.Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.Job;

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((Job)Source).Origin.ToUnity();
                position.y = 0;
                StaticFunc.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((Job)Source).Destination.ToUnity();
                position.y = 0;
                StaticFunc.LookHere(position);
            });
        }

    }

}
