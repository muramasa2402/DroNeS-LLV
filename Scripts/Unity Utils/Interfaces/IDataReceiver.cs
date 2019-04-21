using System.Collections;
using Drones.Utils;

namespace Drones.DataStreamer
{
    public interface IDataReceiver
    {
        int UID { get; }

        TimeKeeper.Chronos OpenTime { get; }

        System.Type DataSourceType { get; }

        bool IsConnected { get; }

        IEnumerator WaitForAssignment();
    }
}
