using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour {
    Vector3 centre;
    public float speed = 1;
    Rigidbody body;
    private void Awake() {
        body = GetComponent<Rigidbody>();
        centre = GameObject.Find("Cube").transform.position;
        centre.y = transform.position.y;
    }
    // Update is called once per frame
    void Start() {
        StartCoroutine(Orbit());
    }

    IEnumerator Orbit() {
        while (true) {
            transform.RotateAround(centre, new Vector3(0, 1, 0), speed);
            yield return null;
        }
    }
}
