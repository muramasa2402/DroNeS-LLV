using System.Collections;
using System.Collections.Generic;

namespace Drones.DataStreamer
{
    using UI;
    using Utils;
    public interface IMultiDataSourceReceiver : IDataReceiver
    {
        SecureSortedSet<uint, IDataSource> Sources { get; set; }
        Dictionary<IDataSource, ListTuple> DataReceivers { get; }
        void OnNewSource(IDataSource source);
        void OnLooseSource(IDataSource source);
        void UpdateConnectionToReceivers();
        bool IsClearing { get; }
        IEnumerator ClearDataReceivers();
    }
}