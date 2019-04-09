namespace Drones.Utils
{
    public enum Building { Short, Tall, Road }
    public enum Elevation { Flat, Real }
    public enum WindowType
    {
        Navigation,
        Console,
        Drone,
        Hub,
        HubList,
        DroneList,
        Job,
        JobHistory,
        JobQueue,
        NFZList,
        Null
    }

    public enum ListElement
    {
        Console,
        DroneList,
        HubList,
        JobHistory,
        JobQueue,
        NFZList
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

    public enum PositionHighlight { Normal, Waypoint };

    public enum Status { Active, SemiActive, Inactive, Null };

    public enum SortOrder { Ascending, Descending };

    public enum TimeSpeed { Normal, Fast, Slow, Ultra, RealTime };

    public enum FractionFormat { Decimal, Percentage };

    public enum DroneMovement { Hover, Ascend, Descend, Horizontal, Idle };

    public enum BatteryStatus { Discharge, Charge, Idle };

    public enum SimulationStatus { Running, Paused, Stopped, EditMode };

    public enum DashboardMode { Simulation, EditMode };
}
