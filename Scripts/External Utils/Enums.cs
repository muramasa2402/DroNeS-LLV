namespace Drones.Utils
{
    public enum Building { Short, Tall, Road }
    public enum Elevation { Flat, Real }
    public enum WindowType
    {
        Navigation,
        Console,
        Drone,
        DestroyedDrone,
        Hub,
        HubList,
        DroneList,
        DestroyedDroneList,
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
        DestroyedDroneList,
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

    public enum Status { Green, Yellow, Red, Null };

    public enum SortOrder { Ascending, Descending };

    public enum TimeSpeed { Normal, Fast, Slow, Ultra, Pause };

    public enum FractionFormat { Decimal, Percentage };

    public enum JobStatus { InProgress, PickUp, Completed, Failed };

    public enum DroneMovement { Hover, Ascend, Descend, Horizontal, Idle };

    public enum BatteryStatus { Discharge, Charge, Idle, Dead };

    public enum SimulationStatus { Running, Paused, Stopped, EditMode };

    public enum DashboardMode { Simulation, EditMode };

    public enum Length { m, km, mi, yd, ft, inch };
    public enum Mass { kg, g, mt, lt, sht, lb, oz };
    public enum Energy { J, kWh, Wh, BTU };
    public enum Power { W, kW, hpe };
    public enum Area { sqm, sqmi, sqyd, sqft, sqin };
    public enum Chronos { s, min, h, day };
    public enum Force { N, kgf, lbf };
    public enum Current { A, mA };
    public enum Charge { C, mC, Ah, mAh };
    public enum Voltage { V, mV };
}
