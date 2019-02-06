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
    const float delay = 0.5f;
    [SerializeField] GameObject followee;
    Camera cam;
    Vector3 panUp;
    Vector3 panLat;
    Vector3 default_pos;
    Vector3 default_dir;


    private void Awake() {
        cam = GameObject.Find("Camera").GetComponent<Camera>();
        default_pos = transform.position;
        default_dir = cam.transform.forward;
    }

    private void Update() {
        // Directional movements
        float moveHorizontal = Flip() * Input.GetAxis("Horizontal");
        float moveVertical = Flip() * Input.GetAxis("Vertical");
        // Forward direction without vertical component
        panUp = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z);
        // If camera is looking directly down or up where panUp.x and panUp.z is zero 
        if (panUp.magnitude > 0) panUp = panUp / panUp.magnitude;
        else panUp = cam.transform.up;

        // Sideways direction
        panLat = new Vector3(panUp.z, 0, -panUp.x);
        panLat = panLat / panLat.magnitude;

        transform.position += (moveVertical * panUp + moveHorizontal * panLat) 
        * panSpeed * Time.deltaTime * 1.8f * transform.position.y;

        //FPS mouse hold middle click
        if (Input.GetMouseButton(0)) {
            float h = Flip() * lookSpeed * Input.GetAxis("Mouse X");
            float v = -lookSpeed * Input.GetAxis("Mouse Y");
            transform.Rotate(v, 0, 0);
            transform.Rotate(0, h, 0, Space.World);
        }

        if (Input.GetKey(KeyCode.Space)) StartCoroutine(FollowSphere());

        // Reset view to 60 degrees
        if (Input.GetMouseButtonDown(0)) StartCoroutine(ResetView());

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

    private int Flip() {
        if (cam.transform.up.y > 0) return 1;
        return -1;
    }

    IEnumerator ResetView() {
        float startTime = Time.time;
        yield return new WaitForEndOfFrame();
        while ((Time.time - startTime)<delay) {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 newDir = Vector3.RotateTowards(cam.transform.forward, default_dir, rotationSpeed, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDir);
                //transform.position = default_pos;
                yield break;
            }
            yield return null;
        }
        yield break;
    }

    IEnumerator FollowSphere() {
        while (!Input.GetKeyDown(KeyCode.Escape)) {
            transform.position = followee.transform.position - cam.transform.forward * followedDistance;
            yield return null;
        }
        yield break;
    }


    private void OnCollisionEnter(Collision collision) {
        StartCoroutine(BlockCamera(collision));
    }

    private void OnCollisionExit(Collision collision) {
        StopCoroutine(BlockCamera(collision));
    }

    IEnumerator BlockCamera(Collision collision) {
        while (true) {
            foreach (ContactPoint contact in collision.contacts) {
                transform.position += 0.1f * transform.position.y * contact.normal * Time.deltaTime;
            }   
            yield return null;
        }
    }



}