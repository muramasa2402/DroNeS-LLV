namespace Drones.Utils
{
    public enum Building { Short, Tall, Road }
    public enum Elevation { Flat, Real }
    public enum WindowType
    {
        Null,
        Drone,
        Hub,
        HubList,
        DroneList,
        Job,
        JobHistory,
        JobQueue,
        Navigation,
        Console
    }

    public enum EventType
    {
        EnteredNoFlyZone,
        Collision,
        Destroyed,
        POIMarked,
        ListUpdate,
        OutofBounds,
        OutofRange
    }
    public enum Status { Active, SemiActive, Inactive, Null };
}
