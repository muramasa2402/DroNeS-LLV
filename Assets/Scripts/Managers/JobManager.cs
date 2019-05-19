using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace Drones.Managers
{
    using Utils;
    using Serializable;
    using Drones.EventSystem;

    public class JobManager : MonoBehaviour
    {
        private static JobManager Instance { get; set; }

        public const string DEFAULT_URL = "http://127.0.0.1:5000/jobs";

        public static string SchedulerURL { get; set; } = DEFAULT_URL;

        private readonly Queue<Drone> _waitingList = new Queue<Drone>();

        private void Awake()
        {
            Instance = this;
        }

        private bool _Started;

        private static bool Started
        {
            get => Instance._Started;
            set
            {
                Instance._Started = value;
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
        private IEnumerator ProcessQueue()
        {
            Started = true;
            while (true)
            {
                yield return new WaitUntil(() => (_waitingList.Count > 0) && (TimeKeeper.TimeSpeed != TimeSpeed.Pause));
                // we recheck the condition here in case of spurious wakeups
                SchedulerPayload payload = SimManager.GetSchedulerPayload();
                //SSimulation payload = SimManager.SerializeSimulation();
                while (_waitingList.Count > 0 && TimeKeeper.TimeSpeed != TimeSpeed.Pause)
                {
                    Drone drone = _waitingList.Dequeue();
                    if (drone.InPool) continue;
                    StartCoroutine(GetJob(drone, payload));
                    if (TimeKeeper.DeltaFrame() > 15)
                    {
                        yield return null;
                        payload = SimManager.GetSchedulerPayload();
                        //payload = SimManager.SerializeSimulation();
                    }
                }
            }
        }

        private IEnumerator GetJob(Drone drone, SchedulerPayload payload)
        {
            payload.requester = drone.UID;
            var request = new UnityWebRequest(SchedulerURL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload.ToJson())),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.responseCode == 200 && request.downloadHandler.text != "{}")
            {
                SJob s_job = JsonUtility.FromJson<SJob>(request.downloadHandler.text);
                if (s_job.droneUID != drone.UID)
                {
                    AddToQueue(drone);
                    yield break;
                }
                drone.AssignedJob = new Job(s_job);
            }
            else
            {
                yield return null;
                AddToQueue(drone);
            }
        }

        private IEnumerator GetJob(Drone drone, SSimulation payload)
        {
            payload.requester = drone.UID;
            var request = new UnityWebRequest(SchedulerURL, "POST")
            {
                //uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload.ToJson())),
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload))),
                downloadHandler = new DownloadHandlerBuffer()
            };

            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.responseCode == 200 && request.downloadHandler.text != "{}")
            {
                SJob s_job = JsonUtility.FromJson<SJob>(request.downloadHandler.text);
                if (s_job.droneUID != drone.UID) { 
                    AddToQueue(drone);
                    yield break;
                }
                drone.AssignedJob = new Job(s_job);
            }
            else
            {
                AddToQueue(drone);
            }
        }

        public static void AddToQueue(Drone drone)
        {
            if (!Started)
            {
                Instance.StartCoroutine(Instance.ProcessQueue());
            }
            if (!Instance._waitingList.Contains(drone))
            {
                Instance._waitingList.Enqueue(drone);
            }
        }
    }
}
