using System.Collections.Generic;
namespace Drones.DataStreamer
{
    using Utils;

    public interface IDataSource
    {
        string[] GetData(WindowType windowType);

        AlertHashSet<ISingleDataSourceReceiver> Connections { get; }

        int TotalConnections { get; }
    }
}