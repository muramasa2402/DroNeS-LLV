using System.Collections;
using System.Collections.Generic;

namespace Drones.DataStreamer
{
    using UI;
    using Utils;
    public interface IMultiDataSourceReceiver : IDataReceiver
    {
        AlertHashSet<IDataSource> Sources { get; set; }
        HashSet<ListTuple> DataReceivers { get; }
        void OnNewSource(IDataSource source);
        void UpdateConnectionToReceivers();
        bool IsClearing { get; }
        IEnumerator ClearDataReceivers();
    }
}