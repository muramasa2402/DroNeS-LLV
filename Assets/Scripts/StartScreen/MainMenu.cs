using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

namespace Drones.StartScreen
{
    using Drones.Managers;
    using Drones.Utils;
    using Drones.Utils.Extensions;

    public class MainMenu : MonoBehaviour
    {
        public static MainMenu Instance { get; private set; }
        [SerializeField]
        Button _Play;
        [SerializeField]
        Button _Options;
        [SerializeField]
        Button _Quit;

        private void OnDestroy()
        {
            Instance = null;
        }

        private bool[] _TestStatus = new bool[3];

        public Button Play
        {
            get
            {
                if (_Play == null)
                {
                    _Play = transform.FindDescendent("Play").GetComponent<Button>();
                }
                return _Play;
            }
        }

        public Button Options
        {
            get
            {
                if (_Options == null)
                {
                    _Options = transform.FindDescendent("Options").GetComponent<Button>();
                }
                return _Options;
            }
        }

        public Button Quit
        {
            get
            {
                if (_Quit == null)
                {
                    _Quit = transform.FindDescendent("Quit").GetComponent<Button>();

                }
                return _Quit;
            }
        }

        private void Awake()
        {
            Instance = this;
            Quit.onClick.AddListener(Application.Quit);
            Play.onClick.AddListener(delegate {
                StartCoroutine(StartSimulation());
            });
            Options.onClick.AddListener(StartScreen.ShowOptions);
        }
        IEnumerator SchedulerTest()
        {

            var request = new UnityWebRequest(JobManager.SchedulerURL, "GET")
            {
                timeout = 15
            };
            yield return request.SendWebRequest();

            _TestStatus[0] = request.responseCode == 200;
        }

        IEnumerator RouterTest()
        {

            var request = new UnityWebRequest(RouteManager.RouterURL, "GET")
            {
                timeout = 15
            };
            yield return request.SendWebRequest();

            _TestStatus[1] = request.responseCode == 200;

        }

        IEnumerator TimeScaleTest()
        {

            var request = new UnityWebRequest(TimeKeeper.SyncURL, "GET")
            {
                timeout = 15
            };
            yield return request.SendWebRequest();

            _TestStatus[2] = request.responseCode == 200;
        }

        private IEnumerator StartSimulation()
        {
            yield return StartCoroutine(SchedulerTest());
            yield return StartCoroutine(RouterTest());
            yield return StartCoroutine(TimeScaleTest());
            if (_TestStatus[0] && _TestStatus[1] && _TestStatus[2])
                StartScreen.OnPlay();
        }

    }
}

