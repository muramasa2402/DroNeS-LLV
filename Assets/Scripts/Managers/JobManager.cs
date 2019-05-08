using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace Drones.Managers
{
    using Utils;
    using Serializable;

    public static class JobManager
    {
        public const string DEFAULT_URL = "http://127.0.0.1:5000/jobs";

        public static string SchedulerURL { get; set; } = DEFAULT_URL;

        private static readonly Queue<Drone> _waitingList = new Queue<Drone>();

        public static IEnumerator ProcessQueue()
        {
            while (true)
            {
                yield return new WaitUntil(() => (_waitingList.Count > 0) && (TimeKeeper.TimeSpeed != TimeSpeed.Pause));
                // we recheck the condition here in case of spurious wakeups
                while (_waitingList.Count > 0 && TimeKeeper.TimeSpeed != TimeSpeed.Pause)
                {
                    Drone drone = _waitingList.Dequeue();
                    SimManager.Instance.StartCoroutine(GetJob(drone));
                }
            }
        }

        static IEnumerator GetJob(Drone drone)
        {
            SSimulation game_state = SimManager.SerializeSimulation();

            var request = new UnityWebRequest(SchedulerURL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(game_state))),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.responseCode == 200 && request.downloadHandler.text != "{}")
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
