using UnityEngine;
using System.Collections;
using Utilities;

public class RTSCamera : MonoBehaviour, ICameraMovement 
{
    public RTSCameraController controller;
    public GameObject Followee { get; set; }
    public float followedDistance = 3f;
    Camera mainCamera;

    private void Awake()
    {
        mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
        controller = new RTSCameraController(this);
    }

    private void Update() 
    {
        float speedScale = controller.speedToHeightGradient * transform.position.y + 1;

        controller.MoveLongitudinal(Input.GetAxis("Vertical") * speedScale);
        controller.MoveLateral(Input.GetAxis("Horizontal") * speedScale);
        controller.Rotate(Input.GetAxis("Rotate"));
        controller.Zoom(Input.GetAxis("Mouse ScrollWheel") * speedScale);

        //FPS mouse hold click
        if (Input.GetMouseButton(0)) 
        {
            controller.Pitch(Input.GetAxis("Mouse Y"));
            controller.Yaw(Input.GetAxis("Mouse X"));
            controller.ClampPitch(controller.lowerPitch, controller.upperPitch);
        }
        // Bounds
        controller.ClampVertical(controller.floor, controller.ceiling);

        if (Followee != null && Input.GetKey(KeyCode.Space)) { StartCoroutine(FollowObject()); }
    }

    IEnumerator FollowObject() 
    {
        while (!Input.GetKeyDown(KeyCode.Escape)) 
        {
            transform.position = Followee.transform.position - mainCamera.transform.forward * followedDistance;
            yield return null;
        }
        yield break;
    }

    IEnumerator StopCamera(Collision collision)
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

    private void OnCollisionEnter(Collision collision) 
    {
        StartCoroutine(StopCamera(collision));
    }

    private void OnCollisionExit(Collision collision) 
    {
        StopCoroutine(StopCamera(collision));
    }

    #region ICameraMovement Implementation

    public void MoveLongitudinal(float longitudinalInput)
    {
        var positiveDirection = Vector3.Cross(mainCamera.transform.right, Vector3.up).normalized;

        transform.position += longitudinalInput * positiveDirection * Time.deltaTime;
    }

    public void MoveLateral(float lateralInput)
    {
        var positiveDirection = mainCamera.transform.right;

        transform.position += lateralInput * positiveDirection * Time.deltaTime;
    }

    public void Zoom(float zoomInput)
    {
        Vector3 positiveDirection = mainCamera.transform.forward;
        // Cannot zoom when facing up
        if (positiveDirection.y < 0)
        {
            transform.position += zoomInput * positiveDirection * Time.deltaTime;
        }
    }

    public void Pitch(float pitchInput)
    {
        transform.Rotate(pitchInput, 0, 0);
    }

    public void Yaw(float yawInput)
    {
        transform.Rotate(0, yawInput, 0, Space.World);
    }

    public void Rotate(float rotationInput)
    {
        float scale = (controller.floor - transform.position.y) / mainCamera.transform.forward.y;
        Vector3 point = transform.position + mainCamera.transform.forward * scale;
        transform.RotateAround(point, Vector3.up, rotationInput);
    }

    public void ClampVertical(float lowerBound, float upperBound)
    {
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, lowerBound, upperBound);
        transform.position = position;
    }

    public void ClampPitch(float lowerAngle, float upperAngle)
    {
        if (upperAngle > 90 || lowerAngle > 90 || upperAngle < 0 || lowerAngle < 0) 
        {
            Debug.LogError("Angles in Camera Pitch Limit must be between 0 and 90");
            upperAngle = 90;
            lowerAngle = 90;
        }
        var eulerAngles = transform.eulerAngles;
        if (eulerAngles.x > 270)
        {
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, 360f - upperAngle, 360f);
        }

        if (lowerAngle >= 90 && mainCamera.transform.up.y < Constants.EPSILON)
        {
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, 89.99f, 90.0f);
        }
        else if (lowerAngle < 90 && mainCamera.transform.forward.y < 0)
        {
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, 0, lowerAngle);
        }

        transform.eulerAngles = eulerAngles;
    }

    public void MoveVertical(float verticalInput)
    {
        return;
    }

    public void Roll(float rollInput)
    {
        return;
    }

    public void ClampLateral(float lowerBound, float upperBound)
    {
        return;
    }

    public void ClampLongitudinal(float lowerBound, float upperBound)
    {
        return;
    }

    public void ClampZoom(float lowerBound, float upperBound)
    {
        return;
    }

    public void ClampYaw(float lowerBound, float upperBound)
    {
        return;
    }

    public void ClampRoll(float lowerBound, float upperBound)
    {
        return;
    }

    public void ClampRotate(float lowerBound, float upperBound)
    {
        return;
    }

    #endregion

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