namespace Drones.Utils
{
    public enum Building { Short, Tall, Road }
    public enum Elevation { Flat, Real }
    public enum WindowType
    {
        Navigation,
        Console,
        Drone,
        RetiredDrone,
        Hub,
        HubList,
        DroneList,
        RetiredDroneList,
        Job,
        JobHistory,
        JobQueue,
        NFZList,
        Dashboard,
        Null
    }

    public enum ListElement
    {
        Console,
        DroneList,
        RetiredDroneList,
        HubList,
        JobHistory,
        JobQueue,
        NFZList
    }

    public enum EventType
    {
        EnteredNoFlyZone,
        Collision,
        POIMarked,
        DroneRetired,
        OutofBounds,
        OutofRange
    }

    public enum ComparisonType { Max, Min };

    public enum PositionHighlight { Normal, Waypoint };

    public enum Status { Green, Yellow, Red, Null };

    public enum SortOrder { Ascending, Descending };

    public enum TimeSpeed { Normal, Fast, Slow, Ultra, Pause };

    public enum FractionFormat { Decimal, Percentage };

    public enum DroneMovement { Hover, Ascend, Descend, Horizontal, Idle };

    public enum FlightStatus { Idle, PreparingHeight, AwatingWaypoint, Delivering };

    public enum BatteryStatus { Discharge, Charge, Idle, Dead };

    public enum SimulationStatus { Running, Paused, Stopped, EditMode };

    public enum DashboardMode { Simulation, EditMode };

    public enum FunctionType { Heaviside, Polynomial, Tanh, Exp }

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
