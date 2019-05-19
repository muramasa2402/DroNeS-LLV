using System;
using System.Collections.Generic;
using System.IO;
using Drones.EventSystem;
using Drones.Managers;
using Newtonsoft.Json;

namespace Drones.Serializable
{
    [Serializable]
    public class SchedulerPayload
    {
        public uint requester;
        public float revenue;
        public float delay;
        public float audible;
        public float energy;
        public Dictionary<uint, StrippedDrone> drones;
        public Dictionary<uint, SHub> hubs;
        public Dictionary<uint, SBattery> batteries;
        public Dictionary<uint, SJob> incompleteJobs;
        public Dictionary<uint, StaticObstacle> noFlyZones;
        public STime currentTime;

        public string ToJson()
        {
            try
            {
                string str = JsonConvert.SerializeObject(this, Formatting.None);
                var n = new DebugLog("Serialization success! Saving File to savepath");
                var nice = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(Path.Combine(SaveManager.SavePath, "debug" + requester + ".json"), nice);

                return str;
            }
            catch (Exception e)
            {
                var n = new DebugLog("Serialization Failed! " + e.Message);
                return "{}";
            }
        }
    }

}
