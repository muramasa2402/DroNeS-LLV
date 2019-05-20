using System.Collections;
using UnityEngine;
using Drones;
using Drones.Utils;
using Drones.Serializable;
using Drones.Routing;
using System.Collections.Generic;
using System.Linq;

public class TestScript : MonoBehaviour
{

    private void Start()
    {
        TestRoute();
    }

    public class Payload
    {
        public List<StaticObstacle> Buildings;
        public List<StaticObstacle> NFZs;
    }
    public static void TestRoute()
    {
        Transform b = FindObjectOfType<TestScript>().transform;
        StaticObstacle[] o = new StaticObstacle[b.childCount];
        int i = 0;
        foreach (Transform building in b)
        {
            o[i] = new StaticObstacle(building);
            i++;
        }

        AirTraffic.GetBuildings(o);
        b = GameObject.Find("NoFlyZones").transform;
        List<StaticObstacle> nfzs = new List<StaticObstacle>();
        foreach (Transform nfz in b)
        {
            nfzs.Add(new StaticObstacle(nfz));
        }

        var p = new Payload
        {
            Buildings = o.ToList(),
            NFZs = nfzs
        };
        //string payload = JsonUtility.ToJson(p,true);
        //File.WriteAllText(Path.Combine(SaveManager.SavePath, "statics.json"), payload);

        int[] com = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        AirTraffic.UpdateGameState(1, com, nfzs);
        Transform way = GameObject.Find("WAYPOINTS").transform;
        var list = AirTraffic.Route(way.GetChild(0).position, way.GetChild(1).position, false);
        foreach (var pos in list)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = pos;
            cube.transform.localScale = 25 * Vector3.one;
        }
    }
}
