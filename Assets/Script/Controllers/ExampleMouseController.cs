using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMouseController : MonoBehaviour, IRemoteController
{
    //large screen game object
    public GameObject boardObject;

    //callback to notify the game about new event from the controller
    event GestureRecognizedEventCallback gestureRecognizedBroadcaster = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //handle mouse event here, once a mouse event detected, map it to its corresponding TouchGesture, which is the event understood by the game
        //for example
        //if detectMouseDown
        TouchGesture touchDownGesture = new TouchGesture();
        touchDownGesture.GestureType = GestureType.SINGLE_TOUCH_DOWN;
        touchDownGesture.MetaData = new Vector2(0, 0);//some mouse down position normalized to -0.5 and 0.5
        //then notify the game to know about the event
        if(gestureRecognizedBroadcaster != null)
        {
            Debug.Log("Mouse down happened");
            gestureRecognizedBroadcaster(touchDownGesture);
        }
        //do it similarly for other mouse event
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
}
