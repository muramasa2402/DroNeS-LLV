using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Drones.Singletons;
using Drones.Utils;
public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        GameObject go = Resources.Load<GameObject>(Constants.DroneListElementPath);
        yield return new WaitForSeconds(3);

        Debug.Log("Start");
        float start = Time.realtimeSinceStartup;
        float pause = Time.realtimeSinceStartup;
        for (int i = 0; i < 1500; i++)
        {
            Instantiate(go, transform);
            if (Time.realtimeSinceStartup - pause > 1/60f)
            {
                yield return null;
                pause = Time.realtimeSinceStartup;
            }
        }
        float end = Time.realtimeSinceStartup;
        Debug.Log("Ran for " + (end - start) + " s");

    }
}
