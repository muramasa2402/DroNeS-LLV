using System.Collections.Generic;
namespace Drones.DataStreamer
{
    using Utils;

    public interface IDataSource
    {
        string[] GetData(WindowType windowType);

        SecureHashSet<ISingleDataSourceReceiver> Connections { get; }

        int TotalConnections { get; }
    }
}