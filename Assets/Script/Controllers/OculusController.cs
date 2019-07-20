using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusController : MonoBehaviour, IRemoteController
{
    //large screen game object
    public GameObject boardObject;

    public GameObject _leftController;
    public GameObject _rightController;

    LineRenderer _laserLine;

    //callback to notify the game about new event from the controller
    event GestureRecognizedEventCallback gestureRecognizedBroadcaster = null;
    TouchGesture previousGesture;

    Vector2 _currentLeftPosition;
    Vector2 _currentRightPosition;
    Vector2 _previousLeftPosition;
    Vector2 _previousRightPosition;

    Vector3 _previousLeftControllerPosition;
    Vector3 _currentLeftControllerPosition;
    Vector3 _previousRightControllerPosition;
    Vector3 _currentRightControllerPosition;

    Vector3 _previousLeftControllerOrientation;
    Vector3 _previousRightControllerOrientation;

    bool _isLeftFirst = false;
    bool _isLeftControllerDown = false;
    bool _isRightControllerDown = false;
    bool _isPreviousPositionLogReset = false;

    // Start is called before the first frame update
    void Start()
    {
        _laserLine = GetComponent<LineRenderer>();
        _laserLine.startWidth = 0.0025f;
        _laserLine.endWidth = 0.01f;

        previousGesture = new TouchGesture();
        previousGesture.GestureType = GestureType.NONE;

        _isLeftControllerDown = false;
        _isRightControllerDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        TouchGesture touchDownGesture = new TouchGesture();

        _currentLeftPosition = GetPosition(true);
        _currentRightPosition = GetPosition(false);

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            if (!_isLeftControllerDown && !_isRightControllerDown)
            {
                touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_DOWN;

                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
                {
                    _isRightControllerDown = true;

                    _isLeftFirst = true;
                    touchDownGesture.MetaData = new Vector2[] { _currentRightPosition, Vector2.zero, Vector2.zero }; // new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5 //then notify the game to know about the event

                    Debug.Log("Controller is down at (" + _currentRightPosition + ")");
                }
                else if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
                {
                    _isLeftControllerDown = true;
                    _isLeftFirst = true;
                    touchDownGesture.MetaData = new Vector2[] { _currentLeftPosition, Vector2.zero, Vector2.zero }; // new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5 //then notify the game to know about the event
                    
                    Debug.Log("Controller is down at (" + _currentLeftPosition + ")");
                }

                _previousLeftPosition = _currentLeftPosition;
                _previousRightPosition = _currentRightPosition;

                if (gestureRecognizedBroadcaster != null)
                {
                    gestureRecognizedBroadcaster(touchDownGesture);
                    previousGesture = touchDownGesture;
                }
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            {
                _isLeftControllerDown = true;
            }
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                _isRightControllerDown = true;
            }
        }
        
        if (_isRightControllerDown || _isLeftControllerDown)
        {
            if(_isLeftControllerDown && _isRightControllerDown)
            {
                if (!_isPreviousPositionLogReset)
                {
                    _previousLeftControllerPosition = _leftController.transform.position;
                    _previousRightControllerPosition = _rightController.transform.position;

                    _isPreviousPositionLogReset = true;
                }

                float previousDif = (_previousLeftControllerPosition - _previousRightControllerPosition).magnitude;
                float newDif = (_rightController.transform.position - _leftController.transform.position).magnitude;
                float disRatio = newDif / previousDif;

                if(disRatio < 0.975f || disRatio > 1.025f)
                {
                    touchDownGesture.GestureType = GestureType.OBJECT_SCALING;
                    touchDownGesture.MetaData = new Vector2[] { new Vector2(disRatio, disRatio), Vector2.zero, Vector2.zero }; //some mouse down position normalized to -0.5 and 0.5
                                                                        //then notify the game to know about the event
                    if (gestureRecognizedBroadcaster != null)
                    {
                        gestureRecognizedBroadcaster(touchDownGesture);
                        previousGesture = touchDownGesture;
                    }

                    Debug.Log("Puzzle piece is scaled by" + disRatio + "!");

                    _previousLeftControllerPosition = _leftController.transform.position;
                    _previousRightControllerPosition = _rightController.transform.position;
                }

                Vector3 previousDirection = _previousRightControllerPosition - _previousLeftControllerPosition;
                Vector3 afterDirection = _rightController.transform.position - _leftController.transform.position;

                float angle = Vector3.Angle(previousDirection, afterDirection);

                if (angle > 1.75f)
                {
                    float a = Mathf.Atan2(previousDirection.x * afterDirection.y - previousDirection.y * afterDirection.x, previousDirection.x * afterDirection.x + previousDirection.y * afterDirection.y) * Mathf.Rad2Deg;

                    if (a > 0)
                        angle *= -1;

                    touchDownGesture.GestureType = GestureType.OBJECT_ROTATING;
                    touchDownGesture.MetaData = new Vector2[] { new Vector2(angle, angle), Vector2.zero, Vector2.zero }; //some mouse down position normalized to -0.5 and 0.5
                                                                                                                               //then notify the game to know about the event
                    if (gestureRecognizedBroadcaster != null)
                    {
                        gestureRecognizedBroadcaster(touchDownGesture);
                        previousGesture = touchDownGesture;
                    }

                    Debug.Log("Puzzle piece is rotated by " + angle + " degrees!");

                    _previousLeftControllerPosition = _leftController.transform.position;
                    _previousRightControllerPosition = _rightController.transform.position;
                }

                //Vector3 difAngle = _rightController.transform.rotation.eulerAngles - _previousOrientation;
                //Debug.Log("difAngle: " + difAngle);

                //if (difAngle.z > 1f)
                //{
                //    touchDownGesture.GestureType = GestureType.OBJECT_ROTATING;
                //    touchDownGesture.MetaData = new Vector2(difAngle.z, difAngle.z);//some mouse down position normalized to -0.5 and 0.5
                //                                                                    //then notify the game to know about the event
                //    if (gestureRecognizedBroadcaster != null)
                //    {
                //        gestureRecognizedBroadcaster(touchDownGesture);
                //        previousGesture = touchDownGesture;
                //    }

                //    _previousOrientation = _rightController.transform.rotation.eulerAngles;
                //    Debug.Log("Puzzle piece is rotated by " + difAngle + " degrees!");
                //}

                //if (Input.GetKeyDown(KeyCode.DownArrow) && _isMouseDown)
                //{
                //    touchDownGesture.GestureType = GestureType.OBJECT_SCALING;
                //    touchDownGesture.MetaData = new Vector2(0.9f, 0.9f);//some mouse down position normalized to -0.5 and 0.5
                //                                                        //then notify the game to know about the event
                //    if (gestureRecognizedBroadcaster != null)
                //    {
                //        gestureRecognizedBroadcaster(touchDownGesture);
                //        previousGesture = touchDownGesture;
                //    }

                //    Debug.Log("Puzzle piece is scaled down!");
                //}
                //else if (Input.GetKeyUp(KeyCode.UpArrow) && _isMouseDown)
                //{
                //    touchDownGesture.GestureType = GestureType.OBJECT_SCALING;
                //    touchDownGesture.MetaData = new Vector2(1.1f, 1.1f);//some mouse down position normalized to -0.5 and 0.5
                //                                                        //then notify the game to know about the event
                //    if (gestureRecognizedBroadcaster != null)
                //    {
                //        gestureRecognizedBroadcaster(touchDownGesture);
                //        previousGesture = touchDownGesture;
                //    }

                //    Debug.Log("Puzzle piece is scaled up!");
                //}

                //if (Input.GetKeyDown(KeyCode.LeftArrow) && _isMouseDown)
                //{
                //    touchDownGesture.GestureType = GestureType.OBJECT_ROTATING;
                //    touchDownGesture.MetaData = new Vector2(-5, -5);//some mouse down position normalized to -0.5 and 0.5
                //                                                    //then notify the game to know about the event
                //    if (gestureRecognizedBroadcaster != null)
                //    {
                //        gestureRecognizedBroadcaster(touchDownGesture);
                //        previousGesture = touchDownGesture;
                //    }

                //    Debug.Log("Puzzle piece is rotated left!");
                //}
                //else if (Input.GetKeyUp(KeyCode.RightArrow) && _isMouseDown)
                //{
                //    touchDownGesture.GestureType = GestureType.OBJECT_ROTATING;
                //    touchDownGesture.MetaData = new Vector2(5, 5);//some mouse down position normalized to -0.5 and 0.5
                //                                                  //then notify the game to know about the event
                //    if (gestureRecognizedBroadcaster != null)
                //    {
                //        gestureRecognizedBroadcaster(touchDownGesture);
                //        previousGesture = touchDownGesture;
                //    }

                //    Debug.Log("Puzzle piece is rotated right!");
                //}
            }
            else
            {
                if(_isLeftFirst)
                {
                    if (_isLeftControllerDown && _isLeftFirst && _previousLeftPosition != null && _previousLeftPosition != _currentLeftPosition)
                    {
                        touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_MOVE;
                        touchDownGesture.MetaData = new Vector2[] { _currentLeftPosition, Vector2.zero, Vector2.zero }; // new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                                                                        //then notify the game to know about the event
                        _previousLeftPosition = _currentLeftPosition;

                        if (gestureRecognizedBroadcaster != null)
                        {
                            gestureRecognizedBroadcaster(touchDownGesture);
                            previousGesture = touchDownGesture;
                        }

                        Debug.Log("Left Controller is moved to (" + _currentLeftPosition + ")");
                    }
                    else if (!_isLeftControllerDown && _isRightControllerDown && _previousRightPosition != null && _previousRightPosition != _currentRightPosition)
                    {
                        touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_MOVE;
                        touchDownGesture.MetaData = new Vector2[] { _currentRightPosition, Vector2.zero, Vector2.zero }; // new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                                                                         //then notify the game to know about the event
                        _previousRightPosition = _currentRightPosition;

                        if (gestureRecognizedBroadcaster != null)
                        {
                            gestureRecognizedBroadcaster(touchDownGesture);
                            previousGesture = touchDownGesture;
                        }

                        Debug.Log("Right Controller is moved to (" + _currentRightPosition + ")");
                    }
                }
                else
                {
                    if (_isRightControllerDown && _previousRightPosition != null && _previousRightPosition != _currentRightPosition)
                    {
                        touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_MOVE;
                        touchDownGesture.MetaData = new Vector2[] { _currentRightPosition, Vector2.zero, Vector2.zero }; // new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                                                                         //then notify the game to know about the event
                        _previousRightPosition = _currentRightPosition;

                        if (gestureRecognizedBroadcaster != null)
                        {
                            gestureRecognizedBroadcaster(touchDownGesture);
                            previousGesture = touchDownGesture;
                        }

                        Debug.Log("Right Controller is moved to (" + _currentRightPosition + ")");
                    }
                    else if (!_isRightControllerDown && _isLeftControllerDown && _previousLeftPosition != null && _previousLeftPosition != _currentLeftPosition)
                    {
                        touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_MOVE;
                        touchDownGesture.MetaData = new Vector2[] { _currentLeftPosition, Vector2.zero, Vector2.zero }; // new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                                                                        //then notify the game to know about the event
                        _previousLeftPosition = _currentLeftPosition;

                        if (gestureRecognizedBroadcaster != null)
                        {
                            gestureRecognizedBroadcaster(touchDownGesture);
                            previousGesture = touchDownGesture;
                        }

                        Debug.Log("Left Controller is moved to (" + _currentLeftPosition + ")");
                    }
                }
                
            }
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            _isRightControllerDown = false;
            _isPreviousPositionLogReset = false;            
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            _isLeftControllerDown = false;
            _isPreviousPositionLogReset = false;
        }

        if(!_isLeftControllerDown && !_isRightControllerDown)
        {
            if (previousGesture.GestureType != GestureType.NONE)
            {
                touchDownGesture.GestureType = GestureType.NONE;
                touchDownGesture.MetaData = new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero }; //some mouse down position normalized to -0.5 and 0.5
                                                                                                        //then notify the game to know about the event
                if (gestureRecognizedBroadcaster != null)
                {
                    gestureRecognizedBroadcaster(touchDownGesture);
                    previousGesture = touchDownGesture;
                }

                _isLeftFirst = false;

                Debug.Log("Mouse is up!");
            }
        }
    }

    private Vector2 GetPosition(bool isLeft)
    {
        RaycastHit[] hits;
        
        if(isLeft)
            hits = Physics.RaycastAll(_leftController.transform.position, _leftController.transform.rotation * Vector3.forward);
        else
            hits = Physics.RaycastAll(_rightController.transform.position, _rightController.transform.rotation * Vector3.forward);

        foreach (var hitInfo in hits)
        {
            if (boardObject != null && hitInfo.transform.gameObject.name == boardObject.name)
            {
                Vector2 point = hitInfo.point - boardObject.transform.position;

                return new Vector2(point.x / boardObject.transform.localScale.x, point.y / boardObject.transform.localScale.y);
            }
        }

        return new Vector2(0, 0);
    }

    public void setGestureRecognizedCallback(GestureRecognizedEventCallback gestureRecognizedListener)
    {
        //set listener for event broadcaster. This setGestureRecognizedCallback method will be called in the game (see SimpleGame.cs)
        gestureRecognizedBroadcaster += gestureRecognizedListener;
    }

    public void setModeSwitchedCallback(MenuItemListener.EditModeSelectedCallBack modeChangeListener)
    {
        //not really needed here to consider
    }

    public string getControllerName()
    {
        return "OculusController";
    }
}
