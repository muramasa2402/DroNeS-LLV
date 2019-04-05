using System.Collections;
using UnityEngine;
using Drones.UI;


public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    VariableBarField varbar;

    private IEnumerator Start()
    {
        varbar = GetComponent<VariableBarField>();

        yield return new WaitForSeconds(3);
        var wait = new WaitForSeconds(0.25f);

        for (int i = 100; i >= 0; i--)
        {
            varbar.SetField(i.ToString());
            yield return wait;
        }

    }
}
