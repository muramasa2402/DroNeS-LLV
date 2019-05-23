using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
namespace Drones.UI
{
    using Drones.Utils;
    using Data;
    using Utils.Extensions;
    using static Utils.UnitConverter;
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
                var position = ((Job)Source).DropOff;
                AbstractCamera.LookHere(position);
            });
        }

        public override void SetData(IData data)
        {
            var job = (JobData)data;
            Data[0].SetField(Source);
            Data[1].SetField(job.pickup.ToStringXZ());
            Data[2].SetField(job.dropoff.ToStringXZ());
            Data[3].SetField(job.created);
            Data[4].SetField(job.assignment);
            Data[5].SetField(job.deadline);
            Data[6].SetField(job.completed);
            Data[7].SetField(Convert(Mass.g, job.packageWeight));
            Data[8].SetField(job.earnings.ToString("C", CultureInfo.CurrentCulture));
            Data[9].SetField((job.deadline is null) ? "" : Convert(Chronos.min, job.deadline.Timer()));
            Data[10].SetField("D" + job.drone.ToString("000000"));
            Data[11].SetField(((Job)Source).Progress());
        }
    }

}
