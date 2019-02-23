using UnityEngine;
using System.Collections;

public class RTSCamera : MonoBehaviour {

    public float panSpeed = 1.0f; //regular speed
    public float zoomSpeed = 1.0f;
    public float lookSpeed = 30.0f;
    public float rotationSpeed = 3f;
    public float followedDistance = 3f;
    public float floor = 56f;
    public float ceiling = 75f;
    [SerializeField] GameObject followee;
    Camera cam;
    Vector3 panUp;
    Vector3 panLat;

    private void Awake()
    {
        cam = GameObject.Find("Camera").GetComponent<Camera>();
    }

    private void Update() 
    {
        // Directional movements
        float moveHorizontal = Flip() * Input.GetAxis("Horizontal");
        float moveVertical = Flip() * Input.GetAxis("Vertical");
        // Forward direction without vertical component
        panUp = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z);
        // If camera is looking directly down or up where panUp.x and panUp.z is zero 
        if (panUp.magnitude > 0.01f) panUp = panUp.normalized;
        else panUp = cam.transform.up;

        // Sideways direction
        panLat = new Vector3(panUp.z, 0, -panUp.x);
        panLat = panLat.normalized;

        var direction = (moveVertical * panUp + moveHorizontal * panLat);
        direction *= panSpeed * Time.deltaTime * (1.8f * transform.position.y + panSpeed);
        transform.position += direction;


        //FPS mouse hold click
        if (Input.GetMouseButton(0)) 
        {
            float h = lookSpeed * Input.GetAxis("Mouse X");
            float v = - lookSpeed * Input.GetAxis("Mouse Y");
            transform.Rotate(v, 0, 0);
            transform.Rotate(0, h, 0, Space.World);
            Vector3 angles = transform.eulerAngles;
            if (angles.x > 270) 
            {
                angles.x = Mathf.Clamp(angles.x, 315f, 360f);
            }
            if (cam.transform.up.y <= 0) 
            {
                angles.x = Mathf.Clamp(angles.x, 89.99f, 90.0f);
            }
            transform.eulerAngles = angles;
        }

        if (Input.GetKey(KeyCode.Space)) { StartCoroutine(FollowSphere()); }

        // RTS rotation requires a floor height
        float moveRotate = Input.GetAxis("Rotate");
        Vector3 point = transform.position + cam.transform.forward / cam.transform.forward.y *
        (floor - transform.position.y);
        transform.RotateAround(point, Vector3.up, rotationSpeed * moveRotate);

        //Zoom
        float moveZoom = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomIn = cam.transform.forward;
        if (zoomIn.y < 0 && (moveZoom > 0 && transform.position.y > floor || moveZoom < 0 && transform.position.y < ceiling))
            transform.position += moveZoom * zoomIn * zoomSpeed * Time.deltaTime * 1.8f * transform.position.y;

        // Bounds
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, floor, ceiling);
        transform.position = pos;
    }

    private int Flip() 
    {
        if (cam.transform.up.y > 0) return 1;
        return -1;
    }

    IEnumerator FollowSphere() 
    {
        while (!Input.GetKeyDown(KeyCode.Escape)) 
        {
            transform.position = followee.transform.position - cam.transform.forward * followedDistance;
            yield return null;
        }
        yield break;
    }


    private void OnCollisionEnter(Collision collision) 
    {
        StartCoroutine(BlockCamera(collision));
    }

    private void OnCollisionExit(Collision collision) 
    {
        StopCoroutine(BlockCamera(collision));
    }

    IEnumerator BlockCamera(Collision collision) 
    {
        Vector3 previousPosition = transform.position;
        Vector3 currentPosition = transform.position;
        Vector3 motion;
        while (true) 
        {
            currentPosition = transform.position;
            foreach (ContactPoint contact in collision.contacts) 
            {
                motion = currentPosition - previousPosition;
                float dot = Vector3.Dot(motion, contact.normal.normalized);
                if (dot < 0)
                {
                    Vector3 deviation = dot * contact.normal.normalized;
                    transform.position -= deviation;
                }
            }
            previousPosition = transform.position;
            yield return null;
        }
    }

    //IEnumerator ResetView() {
    //    float startTime = Time.time;
    //    yield return new WaitForEndOfFrame();
    //    while ((Time.time - startTime)<delay) {
    //        if (Input.GetMouseButtonDown(0)) {
    //            Vector3 newDir = Vector3.RotateTowards(cam.transform.forward, default_dir, rotationSpeed, 0.0f);
    //            transform.rotation = Quaternion.LookRotation(newDir);
    //            //transform.position = default_pos;
    //            yield break;
    //        }
    //        yield return null;
    //    }
    //    yield break;
    //}

}