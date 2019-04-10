namespace Drones.DataStreamer
{
    using Drones.UI;
    using Utils;

    public interface IDataSource
    {
        AbstractInfoWindow InfoWindow { get; set; }

        string[] GetData(WindowType windowType);

        SecureSet<ISingleDataSourceReceiver> Connections { get; }

        int TotalConnections { get; }

        void OpenInfoWindow();
    }
}