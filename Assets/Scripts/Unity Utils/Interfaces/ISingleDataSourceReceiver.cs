using System;
using System.Collections;
namespace Drones.DataStreamer
{
    using UI;
    using Utils;

    public interface ISingleDataSourceReceiver : IDataReceiver
    {
        Type ReceiverType { get; }

        IDataSource Source { get; }

        DataField[] Data { get; }

        IEnumerator StreamData();
    }
}
