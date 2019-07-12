using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMouseController : MonoBehaviour, IRemoteController
{
    //large screen game object
    public GameObject boardObject;

    //callback to notify the game about new event from the controller
    event GestureRecognizedEventCallback gestureRecognizedBroadcaster = null;
    TouchGesture previousGesture;
    Vector2 _previousMousePosition;
    Vector2 _currentMousePosition;
    bool _isMouseDown;
    bool _isMouseUp;

    // Start is called before the first frame update
    void Start()
    {
        previousGesture = new TouchGesture();
        previousGesture.GestureType = GestureType.NONE;
        _isMouseDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        TouchGesture touchDownGesture = new TouchGesture();

        if (Input.GetMouseButton(0))
        {
            _currentMousePosition = GetMousePosition();
            if (!_isMouseDown)
            {
                _isMouseDown = true;

                touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_DOWN;
                touchDownGesture.MetaData = _currentMousePosition;// new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                   //then notify the game to know about the event

                _previousMousePosition = _currentMousePosition;

                if (gestureRecognizedBroadcaster != null)
                {
                    gestureRecognizedBroadcaster(touchDownGesture);
                    previousGesture = touchDownGesture;
                }

                Debug.Log("Mouse is down to (" + _currentMousePosition + ")");
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.DownArrow) && previousGesture.GestureType != GestureType.OBJECT_SCALING)
                {
                    touchDownGesture.GestureType = GestureType.OBJECT_SCALING;
                    touchDownGesture.MetaData = new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                  //then notify the game to know about the event
                    if (gestureRecognizedBroadcaster != null)
                    {
                        gestureRecognizedBroadcaster(touchDownGesture);
                        previousGesture = touchDownGesture;
                    }

                    Debug.Log("Puzzle piece is scaled down!");
                }
                else if (Input.GetKeyUp(KeyCode.UpArrow) && previousGesture.GestureType != GestureType.OBJECT_SCALING)
                {
                    touchDownGesture.GestureType = GestureType.OBJECT_SCALING;
                    touchDownGesture.MetaData = new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                  //then notify the game to know about the event
                    if (gestureRecognizedBroadcaster != null)
                    {
                        gestureRecognizedBroadcaster(touchDownGesture);
                        previousGesture = touchDownGesture;
                    }

                    Debug.Log("Puzzle piece is scaled up!");
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) && previousGesture.GestureType != GestureType.OBJECT_ROTATING)
                {
                    touchDownGesture.GestureType = GestureType.OBJECT_ROTATING;
                    touchDownGesture.MetaData = new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                  //then notify the game to know about the event
                    if (gestureRecognizedBroadcaster != null)
                    {
                        gestureRecognizedBroadcaster(touchDownGesture);
                        previousGesture = touchDownGesture;
                    }

                    Debug.Log("Puzzle piece is rotated left!");
                }
                else if (Input.GetKeyUp(KeyCode.RightArrow) && previousGesture.GestureType != GestureType.OBJECT_ROTATING)
                {
                    touchDownGesture.GestureType = GestureType.OBJECT_ROTATING;
                    touchDownGesture.MetaData = new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                                  //then notify the game to know about the event
                    if (gestureRecognizedBroadcaster != null)
                    {
                        gestureRecognizedBroadcaster(touchDownGesture);
                        previousGesture = touchDownGesture;
                    }

                    Debug.Log("Puzzle piece is rotated right!");
                }

                if(_previousMousePosition != null && _previousMousePosition != _currentMousePosition)
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

                    Debug.Log("Mouse is moved to (" + _currentMousePosition + ")");
                }
            }
            
            
            //else if (previousGesture.GestureType == GestureType.NONE && previousGesture.GestureType != GestureType.SINGLE_TOUCH_DOWN)
            //{
                
            //}
        }
        else
        {
            _isMouseDown = false;

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


        //handle mouse event here, once a mouse event detected, map it to its corresponding TouchGesture, which is the event understood by the game
        //for example
        //if detectMouseDown
        
        //do it similarly for other mouse event
    }

    private Vector2 GetMousePosition()
    {
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));

        foreach(var hitInfo in hits)
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
        return "MouseController";
    }
}
