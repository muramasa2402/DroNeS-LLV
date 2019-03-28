using System.Collections;
using UnityEngine;

namespace Drones.UI
{
    using Utils.Extensions;
    using Struct;

    public class LogEventTest : MonoBehaviour
    {
        private EventLogger eventLog;

        void Start()
        {
            eventLog = GetComponent<EventLogger>();
            StartCoroutine(DoJob());
        }

        IEnumerator DoJob()
        {
            for (int i = 0; i < 24; i++)
            {
                eventLog.LogEvent(new SimulationEventInfo(i.ToString(), (Random.insideUnitSphere * 150).ToArray()));
                yield return new WaitForSeconds(.1f);
            }

            yield break;
        }
    }
}
