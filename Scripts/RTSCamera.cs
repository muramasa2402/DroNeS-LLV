using UnityEngine;
using System.Collections;
using Utilities;
using System;

public class RTSCamera : MonoBehaviour {

    public float moveSpeed = 1.0f; //regular speed
    public float zoomSpeed = 1.0f;
    public float mouseSensitivity = 30.0f;
    public bool invertYAxis;
    public float rotationSpeed = 3f;
    public float followedDistance = 3f;
    public float floor;
    public float ceiling = 200f;
    public float upperPitch = 45.0f;
    public float lowerPitch = 90;
    private float speedScale; 
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
        speedScale = Constants.MOVEMENT_SPEED_GRADIENT * transform.position.y + 1;

        MoveLongitudinal(Input.GetAxis("Vertical"));
        MoveLateral(Input.GetAxis("Horizontal"));
        RotateCamera(Input.GetAxis("Rotate"));
        Zoom(Input.GetAxis("Mouse ScrollWheel"));

        //FPS mouse hold click
        if (Input.GetMouseButton(0)) 
        {
            PitchCamera(Input.GetAxis("Mouse Y"));
            YawCamera(Input.GetAxis("Mouse X"));
            ClampCameraPitch(transform.eulerAngles, upperPitch, lowerPitch);
        }
        // Bounds
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, floor, ceiling);
        transform.position = pos;

        if (followee != null && Input.GetKey(KeyCode.Space)) { StartCoroutine(FollowSphere()); }
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
            yield return new WaitForFixedUpdate();
        }
    }

    private void MoveLongitudinal(float longitudinalInput)
    {
        var positiveDirection = Vector3.Cross(cam.transform.right, Vector3.up).normalized;

        transform.position += longitudinalInput * positiveDirection * moveSpeed * speedScale * Time.deltaTime;
    }

    private void MoveLateral(float lateralInput)
    {
        var positiveDirection = cam.transform.right;

        transform.position += lateralInput * positiveDirection * moveSpeed * speedScale * Time.deltaTime;
    }

    private void Zoom(float zoomInput)
    {
        Vector3 positiveDirection = cam.transform.forward;
        // Cannot zoom when facing up
        if (positiveDirection.y < 0)
        {
            transform.position += zoomInput * positiveDirection * zoomSpeed * speedScale * Time.deltaTime;
        }
    }

    private void PitchCamera(float yAxis)
    {
        var v = mouseSensitivity * yAxis;
        v = invertYAxis ? v : -v;
        transform.Rotate(v, 0, 0);
    }

    private void YawCamera(float xAxis)
    {
        var h = mouseSensitivity * xAxis;
        transform.Rotate(0, h, 0, Space.World);
    }

    private void ClampCameraPitch(Vector3 currentOrientation, float upperAngle = 90, float lowerAngle = 90)
    {
        if (upperAngle > 90 || lowerAngle > 90 || upperAngle < 0 || lowerAngle < 0) 
        {
            Debug.LogError("Angles in Camera Pitch Limit must be between 0 and 90");
            upperAngle = 90;
            lowerAngle = 90;
        }

        if (currentOrientation.x > 270)
        {
            currentOrientation.x = Mathf.Clamp(currentOrientation.x, 360f - upperAngle, 360f);
        }

        if (lowerAngle >= 90 && cam.transform.up.y < Constants.EPSILON)
        {
            currentOrientation.x = Mathf.Clamp(currentOrientation.x, 89.99f, 90.0f);
        }
        else if (lowerAngle < 90 && cam.transform.forward.y < 0)
        {
            currentOrientation.x = Mathf.Clamp(currentOrientation.x, 0, lowerAngle);
        }

        transform.eulerAngles = currentOrientation;
    }

    private void RotateCamera(float rotationInput)
    {
        Vector3 point = transform.position + cam.transform.forward / cam.transform.forward.y * (floor - transform.position.y);
        transform.RotateAround(point, Vector3.up, rotationSpeed * rotationInput);
    }


    /* Double click do something coroutine */
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