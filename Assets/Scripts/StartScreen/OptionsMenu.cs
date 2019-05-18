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
        public StatusDisplay timeScaleStatus;
        public TMP_InputField schedulerInput;
        public TMP_InputField routerInput;
        public TMP_InputField syncInput;
        public TextMeshProUGUI schedulerPlaceholder;
        public TextMeshProUGUI routerPlaceholder;
        public TextMeshProUGUI syncPlaceholder;
        public TextMeshProUGUI sliderDisplay;
        public Image schedulerInputDisabler;
        public Image routerInputDisabler;
        public Image timeScaleInputDisabler;

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
            syncInput.onValueChanged.AddListener((arg0) =>
            {
                StopAllCoroutines();
                routerStatus.ClearStatus();
            });
            Reset.onClick.AddListener(OnReset);
            Back.onClick.AddListener(GoBack);
            Test.onClick.AddListener(() => StartCoroutine(StartTest()));
            routerPlaceholder.SetText(RouteManager.RouterURL);
            schedulerPlaceholder.SetText(JobManager.SchedulerURL);
            syncPlaceholder.SetText(TimeKeeper.SyncURL);
        }

        private void GoBack()
        {
            if (schedulerStatus.Status && !string.IsNullOrWhiteSpace(schedulerInput.text))
            {
                JobManager.SchedulerURL = schedulerInput.text;
                schedulerPlaceholder.SetText(JobManager.SchedulerURL);
            } 
            else
            {
                schedulerInput.text = null;
                schedulerPlaceholder.SetText(JobManager.DEFAULT_URL);
            }

            if (routerStatus.Status && !string.IsNullOrWhiteSpace(routerInput.text))
            {
                RouteManager.RouterURL = routerInput.text;
                routerPlaceholder.SetText(RouteManager.RouterURL);
            }
            else
            {
                routerInput.text = null;
                routerPlaceholder.SetText(RouteManager.DEFAULT_URL);
            }

            if (timeScaleStatus.Status && !string.IsNullOrWhiteSpace(syncInput.text))
            {
                TimeKeeper.SyncURL = syncInput.text;
                syncPlaceholder.SetText(TimeKeeper.SyncURL);
            }
            else
            {
                syncInput.text = null;
                syncPlaceholder.SetText(TimeKeeper.DEFAULT_URL);
            }

            StartScreen.ShowMain();
        }

        private void OnEnable()
        {
            StartCoroutine(StartTest());
        }

        private void OnReset()
        {
            StopAllCoroutines();
            JobManager.SchedulerURL = JobManager.DEFAULT_URL;
            RouteManager.RouterURL = RouteManager.DEFAULT_URL;
            TimeKeeper.SyncURL = TimeKeeper.DEFAULT_URL;
            syncPlaceholder.SetText(TimeKeeper.DEFAULT_URL);
            schedulerPlaceholder.SetText(JobManager.DEFAULT_URL);
            routerPlaceholder.SetText(RouteManager.DEFAULT_URL);
            schedulerStatus.ClearStatus();
            routerStatus.ClearStatus();
            timeScaleStatus.ClearStatus();
            StartCoroutine(StartTest());
        }

        IEnumerator StartTest()
        {
            yield return null;
            schedulerInput.readOnly = true;
            schedulerInputDisabler.gameObject.SetActive(true);
            routerInput.readOnly = true;
            routerInputDisabler.gameObject.SetActive(true);
            syncInput.readOnly = true;
            timeScaleInputDisabler.gameObject.SetActive(true);

            yield return StartCoroutine(SchedulerTest());

            yield return StartCoroutine(RouterTest());

            yield return StartCoroutine(TimeScaleTest());

            schedulerInputDisabler.gameObject.SetActive(false);
            schedulerInput.readOnly = false;
            routerInputDisabler.gameObject.SetActive(false);
            routerInput.readOnly = false;
            timeScaleInputDisabler.gameObject.SetActive(false);
            syncInput.readOnly = false;


        }

        IEnumerator SchedulerTest()
        {

            var request = new UnityWebRequest(JobManager.SchedulerURL, "GET")
            {
                timeout = 15
            };
            yield return request.SendWebRequest();

            schedulerStatus.SetStatus(request.responseCode == 200);
        }

        IEnumerator RouterTest()
        {

            var request = new UnityWebRequest(RouteManager.RouterURL, "GET")
            {
                timeout = 15
            };
            yield return request.SendWebRequest();

            routerStatus.SetStatus(request.responseCode == 200);

        }

        IEnumerator TimeScaleTest()
        {

            var request = new UnityWebRequest(TimeKeeper.SyncURL, "GET")
            {
                timeout = 15
            };
            yield return request.SendWebRequest();

            timeScaleStatus.SetStatus(request.responseCode == 200);

        }

    }
}
