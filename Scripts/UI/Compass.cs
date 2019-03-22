using UnityEngine;
 
public class Compass : MonoBehaviour
{
    public GameObject target;
    Vector3 startPosition;
    float movementPerDegree;
 
    void Start()
    {
        startPosition = transform.localPosition;
        movementPerDegree = GetComponent<RectTransform>().sizeDelta.x / 720f;
    }
 
    void Update ()
    {
        Vector3 localForward = Vector3.Cross(target.transform.right, Vector3.up);
        Vector3 perp = Vector3.Cross(Vector3.forward, localForward);
        float dir = -Vector3.Dot(perp, Vector3.up);
        transform.localPosition = startPosition + (new Vector3(Vector3.Angle(localForward, Vector3.forward) * Mathf.Sign(dir) * movementPerDegree, 0, 0));
    }
}
 