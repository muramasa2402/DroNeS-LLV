using System.Collections;
using Drones.Utils;

namespace Drones.DataStreamer
{
    public interface IDataReceiver
    {
        int UID { get; }

        IEnumerator WaitForAssignment();
    }
}
