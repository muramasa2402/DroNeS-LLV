namespace Drones.Interface
{
    using Drones.Utils;

    public interface IClosestPoint
    {
        float[] GetClosestPoint(float[] point, Directions dir);
        float[] GetCentre();
        void Message(object msg);
    }

}