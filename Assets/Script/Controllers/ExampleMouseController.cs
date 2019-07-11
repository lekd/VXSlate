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

    // Start is called before the first frame update
    void Start()
    {
        previousGesture = new TouchGesture();
        previousGesture.GestureType = GestureType.NONE;
    }

    // Update is called once per frame
    void Update()
    {
        TouchGesture touchDownGesture = new TouchGesture();

        if (Input.GetMouseButton(0))
        {
            touchDownGesture.MetaData = GetMousePosition();// new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
                                                           //then notify the game to know about the event

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
            else if (previousGesture.GestureType == GestureType.NONE && previousGesture.GestureType != GestureType.SINGLE_TOUCH_DOWN)
            {
                touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_DOWN;

                if (gestureRecognizedBroadcaster != null)
                {
                    gestureRecognizedBroadcaster(touchDownGesture);
                    previousGesture = touchDownGesture;
                }

                Debug.Log("Mouse is down!");
            }
        }
        else
        {
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
        RaycastHit hitInfo = new RaycastHit();

        bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

        if (hit)
        {
            if (boardObject != null && hitInfo.transform.gameObject.name == boardObject.name)
            {
                return new Vector2(hitInfo.point.x, hitInfo.point.y);
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
