using System.Globalization;

namespace Drones.UI
{
    using Data;
    using Utils.Extensions;
    using Utils;
    using Interface;
    public class RetiredDroneTuple : ObjectTuple
    {
        public override void SetData(IData data)
        {
            var rd = (RetiredDroneData)data;

            Data[0].SetField(Source);
            Data[1].SetField(rd.destroyedTime);
            Data[2].SetField(rd.collisionLocation.ToStringXYZ());
            Data[3].SetField(rd.packageWorth.ToString("C", CultureInfo.CurrentCulture));

        }

        public static RetiredDroneTuple New(IListWindow window)
        {
            var pc = PoolController.Get(ListElementPool.Instance);
            var le = pc.Get<RetiredDroneTuple>(window.TupleContainer.transform);
            le._Window = (AbstractWindow)window;
            return le;
        }
    }
}

