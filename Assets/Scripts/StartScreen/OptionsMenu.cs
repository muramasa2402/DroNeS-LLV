using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Drones.StartScreen
{
    using Drones.Managers;
    using Drones.Utils;
    using Drones.Utils.Extensions;


    public class OptionsMenu : MonoBehaviour
    {
        public static OptionsMenu Instance { get; private set; }

        public StatusDisplay schedulerStatus;
        public StatusDisplay routerStatus;
        public TMP_InputField schedulerInput;
        public TMP_InputField routerInput;
        public TextMeshProUGUI sliderDisplay;
        public Image schedulerInputDisabler;
        public Image routerInputDisabler;

        [SerializeField]
        Slider _RenderLimit;
        [SerializeField]
        Button _Back;
        [SerializeField]
        Button _Reset;
        [SerializeField]
        Button _Test;

        public Slider RenderLimit
        {
            get
            {
                if (_RenderLimit == null)
                {
                    _RenderLimit = GetComponentInChildren<Slider>(true);
                }
                return _RenderLimit;
            }
        }

        public Button Back
        {
            get
            {
                if (_Back == null)
                {
                    _Back = transform.FindDescendent("Back").GetComponent<Button>();
                }
                return _Back;
            }
        }

        public Button Reset
        {
            get
            {
                if (_Reset == null)
                {
                    _Reset = transform.FindDescendent("Reset").GetComponent<Button>();
                }
                return _Reset;
            }
        }

        public Button Test
        {
            get
            {
                if (_Test == null)
                {
                    _Test = transform.FindDescendent("Test").GetComponent<Button>();

                }
                return _Test;
            }
        }

        private void Awake()
        {
            Instance = this;

            RenderLimit.onValueChanged.AddListener((float value) =>
            {
                value = Mathf.Clamp(value * 5, 0, 600);
                sliderDisplay.SetText(value.ToString());
                CustomMap.FilterHeight = value;
            });
            schedulerInput.onValueChanged.AddListener((arg0) =>
            {
                StopAllCoroutines();
                schedulerStatus.ClearStatus();
            });
            routerInput.onValueChanged.AddListener((arg0) =>
            {
                StopAllCoroutines();
                routerStatus.ClearStatus();
            });
            Reset.onClick.AddListener(OnReset);
            Back.onClick.AddListener(GoBack);
            Test.onClick.AddListener(() => StartCoroutine(StartTest()));
            StartCoroutine(StartTest());
        }

        private void GoBack()
        {
            if (schedulerStatus.Status)
            {
                JobManager.SchedulerURL = schedulerInput.text;
            } 
            else
            {
                schedulerInput.text = null;
            }

            if (routerStatus.Status)
            {
                RouteManager.RouterURL = routerInput.text;
            }
            else
            {
                routerInput.text = null;
            }

            StartScreen.ShowMain();
        }

        private void OnReset()
        {
            StopAllCoroutines();
            JobManager.SchedulerURL = JobManager.DEFAULT_URL;
            RouteManager.RouterURL = RouteManager.DEFAULT_URL;
            schedulerStatus.ClearStatus();
            routerStatus.ClearStatus();
            StartCoroutine(StartTest());
        }

        IEnumerator StartTest()
        {
            yield return StartCoroutine(SchedulerTest());

            yield return StartCoroutine(RouterTest());
        }

        IEnumerator SchedulerTest()
        {
            schedulerInput.readOnly = true;
            schedulerInputDisabler.gameObject.SetActive(true);
            var request = new UnityWebRequest(JobManager.SchedulerURL, "GET");

            yield return request.SendWebRequest();

            schedulerStatus.SetStatus(request.responseCode == 200);
            schedulerInputDisabler.gameObject.SetActive(false);
            schedulerInput.readOnly = false;

        }

        IEnumerator RouterTest()
        {
            routerInput.readOnly = true;
            routerInputDisabler.gameObject.SetActive(true);

            var request = new UnityWebRequest(RouteManager.RouterURL, "GET");

            yield return request.SendWebRequest();

            routerStatus.SetStatus(request.responseCode == 200);
            routerInputDisabler.gameObject.SetActive(false);
            routerInput.readOnly = false;
        }

    }
}
