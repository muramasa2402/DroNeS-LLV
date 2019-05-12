using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


namespace Drones.Managers
{
    using Utils;
    using Serializable;

    public static class RouteManager
    {
        public static bool IsSending { get; private set; } = false;
        public const string DEFAULT_URL = "http://127.0.0.1:5000/routes";

        public static string RouterURL { get; set; } = DEFAULT_URL;

        private static readonly Queue<Drone> _waitingList = new Queue<Drone>();

        public static IEnumerator ProcessQueue()
        {
            while (true)
            {

                yield return new WaitUntil(() => (_waitingList.Count > 0) && (TimeKeeper.TimeSpeed != TimeSpeed.Pause));
                // we recheck the condition here in case of spurious wakeups
                RouterPayload payload = SimManager.GetRouterPayload();
                while (_waitingList.Count > 0 && TimeKeeper.TimeSpeed != TimeSpeed.Pause)
                {
                    Drone drone = _waitingList.Dequeue();

                    SimManager.Instance.StartCoroutine(GetRoute(drone, payload));
                    if (TimeKeeper.DeltaFrame() > 12)
                    {
                        payload = SimManager.GetRouterPayload();
                        yield return null;
                    }
                }
            }
        }

        private static IEnumerator GetRoute(Drone drone, RouterPayload payload)
        {
            payload.origin = drone.Position;

            payload.destination =
                drone.AssignedJob == null ? drone.AssignedHub.Position :
                drone.AssignedJob.Status == JobStatus.Pickup ? drone.AssignedJob.Pickup :
                drone.AssignedJob.Status == JobStatus.Delivering ? drone.AssignedJob.Dest :
                drone.AssignedHub.Position;

            if (drone.AssignedJob != null)
            {
                payload.onJob = true;
                payload.status = drone.AssignedJob.Status;
            }

            var request = new UnityWebRequest(RouterURL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload))),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.responseCode == 200 && request.downloadHandler.text != "{}")
            {
                SRoute route = JsonUtility.FromJson<SRoute>(request.downloadHandler.text);
                drone.NavigateWaypoints(route.waypoints);
            }
            else
            {
                AddToQueue(drone);
            }
        }

        public static void AddToQueue(Drone drone)
        {
            if (!_waitingList.Contains(drone))
            {
                _waitingList.Enqueue(drone);
            }
        }

        public static void Reset()
        {
            _waitingList.Clear();
            IsSending = false;
        }
    }
}
