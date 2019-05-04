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

        #region Buttons
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
        #endregion

        public override System.Type DataSourceType { get; } = typeof(Job);

        public override WindowType Type { get; } = WindowType.Job;

        protected override void Awake()
        {
            base.Awake();
            GoToOrigin.onClick.AddListener(delegate
            {
                var position = ((Job)Source).Pickup;
                AbstractCamera.LookHere(position);
            });

            GoToDestination.onClick.AddListener(delegate
            {
                var position = ((Job)Source).Dest;
                AbstractCamera.LookHere(position);
            });
        }

    }

}
