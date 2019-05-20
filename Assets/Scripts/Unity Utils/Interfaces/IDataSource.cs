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

        string[] GetData(Type windowType);

        void OpenInfoWindow();
    }
}