namespace Drones.Interface
{
    using Drones.Utils;

    public interface IClosestPoint
    {
        float[] GetClosestPoint(float[] point);
        float[] GetCentre();
        float Exp(float x);
        void Message(string msg);
    }

}