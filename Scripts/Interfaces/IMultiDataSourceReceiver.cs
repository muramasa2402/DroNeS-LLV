using System.Collections.Generic;

namespace Drones.DataStreamer
{
    using UI;
    using Utils;
    public interface IMultiDataSourceReceiver : IDataReceiver
    {
        AlertHashSet<IDronesObject> Sources { get; }
        HashSet<ListTuple> DataReceivers { get; }
        void OnNewSource(IDronesObject source);
        void UpdateConnectionToReceivers();
    }
}