using System;[Serializable]public class RTSCameraController{
    private readonly ICameraMovement move;    private float _upperPitch = 45;    private float _lowerPitch = 90;    private float _moveSpeed = 1;    private float _zoomSpeed = 1;    private float _rotationSpeed = 3;    private float _mouseSensitivity = 30;    private float _floor;    private float _ceiling = 200;    private float _speedToHeightGradient = 2;    public RTSCameraController(ICameraMovement movementController)    {        move = movementController;    }

    #region Properties
    public float MoveSpeed     {        get { return _moveSpeed; }        set        {            _moveSpeed = KeepPositive(value);        }
    }    public float ZoomSpeed     {
        get { return _zoomSpeed; }        set        {            _zoomSpeed = KeepPositive(value);        }
    }    public float RotationSpeed 
    {
        get { return _rotationSpeed; }        set        {            _rotationSpeed = KeepPositive(value);        }
    }    public float MouseSensitivity 
    {        get { return _mouseSensitivity; }        set        {            _mouseSensitivity = KeepPositive(value);        }    }    public float Floor
    {        get { return _floor; }        set        {            _floor = KeepPositive(value);            if (_floor > _ceiling) { _floor = _ceiling; }        }    }    public float Ceiling
    {        get { return _ceiling; }        set        {            _ceiling = KeepPositive(value);            if (_floor > _ceiling) { _ceiling = _floor; }        }    }    public float SpeedToHeightGradient
    {        get { return _speedToHeightGradient; }        set        {            _speedToHeightGradient = KeepPositive(value);        }    }    public bool InvertYAxis { get; set; } = false;    public float UpperPitch
    {
        get { return _upperPitch; }

        set
        {
            _upperPitch = KeepAcute(value);        }
    }    public float LowerPitch    {        get { return _lowerPitch; }        set        {            _lowerPitch = KeepAcute(value);        }    }
    #endregion

    #region Methods    private float KeepAcute(float angle)
    {
        if (angle < 0) { return 0; }
        if (angle > 90) { return 90; }
        return angle;
    }    private float KeepPositive(float number)
    {
        return (number < 0) ? 0 : number;
    }
    public void MoveLongitudinal(float input)    {        move.MoveLongitudinal(input * MoveSpeed);    }    public void MoveLateral(float input)    {        move.MoveLateral(input * MoveSpeed);    }    public void Zoom(float input)    {        move.Zoom(input * ZoomSpeed);    }    public void Pitch(float input)    {        move.Pitch((InvertYAxis ? input : -input) * MouseSensitivity);    }    public void Yaw(float input)    {        move.Yaw(input * MouseSensitivity);    }    public void Rotate(float input)    {        move.Rotate(input * RotationSpeed);    }    public void ClampVertical()    {        move.ClampVertical(Floor, Ceiling);    }    public void ClampPitch()    {        move.ClampPitch(LowerPitch, UpperPitch);    }

    #endregion}