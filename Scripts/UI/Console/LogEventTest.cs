using System.Collections;
using UnityEngine;

namespace Drones.UI
{
    using Utils.Extensions;
    using EventSystem;
    using Drones.Utils;
    using static Singletons;

    public class LogEventTest : MonoBehaviour
    {
        IEnumerator Start()
        {
            var wait = new WaitForSeconds(.1f);
            yield return new WaitUntil(() => !UIPool.Initializing);
            yield return new WaitForSeconds(1);

            for (int i = 0; i < 24; i++)
            {
                SimulationEvent.Invoke(EventType.POIMarked, new POIMarked(i.ToString(), (Random.insideUnitSphere * 150).ToArray(), Console.gameObject));
                yield return wait;
            }
            yield break;
        }

    }
}
