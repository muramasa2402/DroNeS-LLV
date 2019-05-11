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
            yield return new WaitForSeconds(1);
            for (int i = 0; i < 24; i++)
            {
                ConsoleLog.WriteToConsole(new POIMarked(i.ToString(), (Random.insideUnitSphere * 150).ToArray()));
                yield return wait;
            }
            yield break;
        }

    }
}
