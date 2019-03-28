using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

namespace Drones
{
    using Struct;

    public static class SceneAttributes
    {
        public static GameObject CurrentPosition { get; set; } = null;
        public static GameObject Sun { get; } = GameObject.Find("Sun");
        public static PostProcessingProfile PostProcessing { get; } = Resources.Load("PostProcessing/CityLights") as PostProcessingProfile;
        public static Transform CameraContainer { get; } = GameObject.FindWithTag("MainCamera").transform;
        public static Transform CamTrans { get; } = CameraContainer.transform.GetChild(0).GetComponent<Camera>().transform;
        public static RTSCamera CameraControl { get; } = CameraContainer.GetComponent<RTSCamera>();
        public static List<SimulationEventInfo> Events { get; } = new List<SimulationEventInfo>();
    }
}