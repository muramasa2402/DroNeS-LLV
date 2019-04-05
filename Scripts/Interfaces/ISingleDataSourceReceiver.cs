using System.Collections;

namespace Drones.DataStreamer
{
    using UI;
    using Utils;

    public interface ISingleDataSourceReceiver : IDataReceiver
    {
        WindowType ReceiverType { get; }

        IDataSource Source { get; }

        DataField[] Data { get; }

        void OnDataUpdate(IDataSource datasource);
    }
}
