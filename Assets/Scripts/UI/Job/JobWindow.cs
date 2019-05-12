using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    using Drones.Utils;

    public class JobWindow : AbstractInfoWindow
    {
        public static JobWindow New() => PoolController.Get(WindowPool.Instance).Get<JobWindow>(null);
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

        protected override Vector2 MaximizedSize { get; } = new Vector2(450, 500);

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
