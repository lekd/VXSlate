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

    Vector2 _currentMousePosition;
    Vector2 _previousMousePosition;

    Vector3 _previousLeftControllerPosition;
    Vector3 _currentLeftControllerPosition;
    Vector3 _previousRightControllerPosition;
    Vector3 _currentRightControllerPosition;

    Vector3 _previousLeftControllerOrientation;
    Vector3 _previousRightControllerOrientation;

    bool _isLeftControllerDown;
    bool _isRightControllerDown;
    bool _isPreviousPositionLogReset = false;

    // Start is called before the first frame update
    void Start()
    {
        _laserLine = GetComponent<LineRenderer>();
        _laserLine.startWidth = 0.0075f;
        _laserLine.endWidth = 0.25f;
        previousGesture = new TouchGesture();
        previousGesture.GestureType = GestureType.NONE;

        _isLeftControllerDown = false;
        _isRightControllerDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_rightController != null)
        {
            _laserLine.SetPosition(0, _rightController.transform.position);

            RaycastHit hitInfo = new RaycastHit();

            bool hit = Physics.Raycast(_rightController.transform.position, _rightController.transform.rotation * Vector3.forward, out hitInfo);

            if(hit)
            {               
                _laserLine.SetPosition(1, hitInfo.point);
            }
        }
        else
        {
            _laserLine.SetPosition(0, new Vector3(0, 0, 0));
        }

        TouchGesture touchDownGesture = new TouchGesture();

        _currentMousePosition = GetMousePosition();

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (!_isRightControllerDown)
            {
                _isRightControllerDown = true;

                touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_DOWN;
                touchDownGesture.MetaData = _currentMousePosition;// new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                  //then notify the game to know about the event

                _previousMousePosition = _currentMousePosition;

                if (gestureRecognizedBroadcaster != null)
                {
                    gestureRecognizedBroadcaster(touchDownGesture);
                    previousGesture = touchDownGesture;
                }

                Debug.Log("Controller is down at (" + _currentMousePosition + ")");
            }            
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            _isRightControllerDown = false;
            _isPreviousPositionLogReset = false;

            if (previousGesture.GestureType != GestureType.NONE)
            {
                touchDownGesture.GestureType = GestureType.NONE;
                touchDownGesture.MetaData = new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                              //then notify the game to know about the event
                if (gestureRecognizedBroadcaster != null)
                {
                    gestureRecognizedBroadcaster(touchDownGesture);
                    previousGesture = touchDownGesture;
                }

                Debug.Log("Mouse is up!");
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            _isLeftControllerDown = true;
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
        {
            _isLeftControllerDown = false;
            _isPreviousPositionLogReset = false;
        }
        
        if (_isRightControllerDown)
        {
            if(_isLeftControllerDown)
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

                if(disRatio < 0.95f || disRatio > 1.05f)
                {
                    touchDownGesture.GestureType = GestureType.OBJECT_SCALING;
                    touchDownGesture.MetaData = new Vector2(disRatio, disRatio);//some mouse down position normalized to -0.5 and 0.5
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

                if (angle > 2.5f)
                {
                    float a = Mathf.Atan2(previousDirection.x * afterDirection.y - previousDirection.y * afterDirection.x, previousDirection.x * afterDirection.x + previousDirection.y * afterDirection.y) * Mathf.Rad2Deg;

                    if (a > 0)
                        angle *= -1;

                    touchDownGesture.GestureType = GestureType.OBJECT_ROTATING;
                    touchDownGesture.MetaData = new Vector2(a, a);//some mouse down position normalized to -0.5 and 0.5
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
                if (_previousMousePosition != null && _previousMousePosition != _currentMousePosition)
                {
                    touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_MOVE;
                    touchDownGesture.MetaData = _currentMousePosition;// new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                      //then notify the game to know about the event
                    _previousMousePosition = _currentMousePosition;

                    if (gestureRecognizedBroadcaster != null)
                    {
                        gestureRecognizedBroadcaster(touchDownGesture);
                        previousGesture = touchDownGesture;
                    }

                    Debug.Log("Controller is moved to (" + _currentMousePosition + ")");
                }
            }
        }        
    }

    private Vector2 GetMousePosition()
    {
        RaycastHit[] hits = Physics.RaycastAll(_rightController.transform.position, _rightController.transform.rotation * Vector3.forward);

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
