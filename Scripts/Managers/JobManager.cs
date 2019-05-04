using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace Drones.Managers
{
    using Utils;
    using Serializable;

    public class JobManager
    {
        private const string _defaultServerURL = "http://127.0.0.1:5000/jobs";

        public static string SchedulerURL { get; set; } = _defaultServerURL;

        private static readonly Queue<Drone> _waitingList = new Queue<Drone>();

        public static IEnumerator ProcessQueue()
        {
            while (true)
            {
                yield return new WaitUntil(() => _waitingList.Count > 0);
                // we recheck the condition here in case of spurious wakeups
                if (_waitingList.Count > 0 && TimeKeeper.TimeSpeed != TimeSpeed.Pause)
                {
                    Drone drone;
                    do
                    {
                        drone = _waitingList.Dequeue();
                    } while (drone.InPool && _waitingList.Count > 0);

                    SimManager.Instance.StartCoroutine(GetJob(drone));
                }
            }
        }

        static IEnumerator GetJob(Drone drone)
        {
            SSimulation game_state = SimManager.SerializeSimulation();

            var request = new UnityWebRequest(SchedulerURL, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(game_state));
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.responseCode == 200 || request.downloadHandler.text == "{}")
            {
                SJob s_job = JsonUtility.FromJson<SJob>(request.downloadHandler.text);
                drone.AssignedJob = new Job(s_job);
            }
            else
            {
                AddToQueue(drone);
            }
        }

        public static void AddToQueue(Drone drone) => _waitingList.Enqueue(drone);
    }
}
