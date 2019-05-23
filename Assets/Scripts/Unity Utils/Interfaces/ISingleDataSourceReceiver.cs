using System;
using System.Collections;
namespace Drones.DataStreamer
{
    using UI;

    public interface ISingleDataSourceReceiver : IDataReceiver
    {
        Type ReceiverType { get; }

        IDataSource Source { get; }

        DataField[] Data { get; }

        void SetData(IData data);

        IEnumerator StreamData();
    }
}
