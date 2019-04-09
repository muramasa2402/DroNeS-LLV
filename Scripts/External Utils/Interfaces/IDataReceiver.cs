using System.Collections;

namespace Drones.DataStreamer
{
    public interface IDataReceiver
    {
        System.Type DataSourceType { get; }

        bool IsConnected { get; }

        IEnumerator WaitForAssignment();
    }
}
