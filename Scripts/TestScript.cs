using System.Collections;
using UnityEngine;
using Drones;

public class TestScript : MonoBehaviour
{
    Battery[] batteries;
    private void Start()
    {
        batteries = new Battery[1000];

        for (int i = 0; i < batteries.Length; i++)
        {
            batteries[i] = new Battery();
            batteries[i].SetCharge(batteries[i].Health);
        }

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            StopAllCoroutines();
        }

        if (Input.GetKey(KeyCode.Return))
        {
            for (int i = 0; i < batteries.Length; i++)
            {
                StartCoroutine(batteries[i].Operate());
            }
        }
    }
}
