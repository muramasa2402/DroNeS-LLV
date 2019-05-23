namespace Drones.UI
{
    using Data;
    using Utils.Extensions;
    using Utils;
    using Interface;
    public class DroneTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var drone = (DroneData)data;
            Data[0].SetField(((Drone)Source).Name);
            Data[1].SetField(((Drone)Source).GetHub().Name);
            if (drone.job != 0)
            {
                var job = ((Drone)Source).GetJob();
                Data[2].SetField(job.Pickup.ToStringXZ());
                Data[3].SetField(job.DropOff.ToStringXZ());
            }
        }

        public static DroneTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<DroneTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}
