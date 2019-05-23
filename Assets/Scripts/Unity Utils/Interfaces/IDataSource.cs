using System;

namespace Drones.DataStreamer
{
    using Drones.UI;
    using Utils;

    public interface IDataSource
    {
        uint UID { get; }

        bool IsDataStatic { get; }

        AbstractInfoWindow InfoWindow { get; set; }

        void GetData(ISingleDataSourceReceiver receiver);

        void OpenInfoWindow();
    }
}