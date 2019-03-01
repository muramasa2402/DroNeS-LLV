using System;[Serializable]public class RTSCameraController{
    private readonly ICameraMovement move;    private float _upperPitch = 45;    private float _lowerPitch = 90;    public RTSCameraController(ICameraMovement movementController)    {        move = movementController;    }

    #region Properties
    public float MoveSpeed { get; set; } = 1;    public float ZoomSpeed { get; set; } = 1;    public float RotationSpeed { get; set; } = 3;    public float MouseSensitivity { get; set; } = 30;    public float Floor { get; set; } = 0;    public float Ceiling { get; set; } = 200;    public float SpeedToHeightGradient { get; set; } = 2;    public bool InvertYAxis { get; set; } = false;    public float UpperPitch
    {
        get { return _upperPitch; }

        set
        {
            _upperPitch = MakeAcute(value);        }
    }    public float LowerPitch    {        get { return _lowerPitch; }        set        {            _lowerPitch = MakeAcute(value);        }    }
    #endregion

    #region Methods    private float MakeAcute(float angle)
    {
        if (angle < 0) { return 0; }
        if (angle > 90) { return 90; }
        return angle;
    }
    public void MoveLongitudinal(float input)    {        move.MoveLongitudinal(input * MoveSpeed);    }    public void MoveLateral(float input)    {        move.MoveLateral(input * MoveSpeed);    }    public void Zoom(float input)    {        move.Zoom(input * ZoomSpeed);    }    public void Pitch(float input)    {        move.Pitch((InvertYAxis ? input : -input) * MouseSensitivity);    }    public void Yaw(float input)    {        move.Yaw(input * MouseSensitivity);    }    public void Rotate(float input)    {        move.Rotate(input * RotationSpeed);    }    public void ClampVertical()    {        move.ClampVertical(Floor, Ceiling);    }    public void ClampPitch()    {        move.ClampPitch(LowerPitch, UpperPitch);    }

    #endregion}