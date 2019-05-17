using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacter : MonoBehaviour
{
    public GameObject gameContainerObj;
    Vector2 local2DTranslate = new Vector2(0, 0);
    Vector2 local2DScale = new Vector2(1,1);
    float rotation = 0;
    Rect _local2DBounds = new Rect();
    bool _isBeingSelected = false;
    Vector2 prevLocalTouch = new Vector2(0, 0);
    // Start is called before the first frame update
    void Start()
    {
        _local2DBounds = getLocal2DBoundOfCharacter(gameObject, gameContainerObj);
        
    }
    bool isInitated = false;
    // Update is called once per frame
    void Update()
    {
        if(!isInitated)
        {
            gameObject.transform.localPosition.Set(0, 0, -0.001f);
            isInitated = true;
        }
        //process translate character based on touch
        //now have no idea, messing up with coordinates
        //process scale character based on touch
        if (local2DScale.x != 1 || local2DScale.y != 1)
        {
            Vector3 curScale = gameObject.transform.localScale;
            gameObject.transform.localScale = new Vector3(curScale.x*local2DScale.x,curScale.y,curScale.z * local2DScale.y);
            //Debug.Log("Object scaled at ratio: " + string.Format("({0},{1})", local2DScale.x, local2DScale.y));
            local2DScale.Set(1, 1);
        }
                
        //gameObject.transform.Rotate(0, rotation, 0);
        gameObject.transform.RotateAround(gameObject.transform.position, Vector3.forward, 1.23f);
        //rotation = 0;
        
        _local2DBounds = getLocal2DBoundOfCharacter(gameObject, gameContainerObj);
    }
    public void handleGesture(TouchGesture gesture)
    {
        //raised when there are Touch-up or touch-canceled event on tablet and there are no touch left
        if (gesture.GestureType == GestureType.NONE)
        {
            _isBeingSelected = false;
        }
        else if(gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
        {
            //meta data of SINGLE_TOUCH_DOWN including 1 Vector2 value
            //which is the local position on the board of the single touch (-0.5<=x,y<=0.5, (0,0) is the center of the board)
            //noted that this touch point is calculated in relation to the location of the virtual pad on the board
            //hence when the pad moves by gaze, this touch point will be also updated accordingly
            Vector2 localTouchPos = (Vector2)gesture.MetaData;
            if (_local2DBounds.Contains(localTouchPos))
            {
                _isBeingSelected = true;
                prevLocalTouch = localTouchPos;
            }
            else
            {
                _isBeingSelected = false;
            }
        }
        else if(gesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
        {
            //meta data of SINGLE_TOUCH_MOVE including 1 Vector2 value
            //which is the local position on the board of the single touch (-0.5<=x,y<=0.5, (0,0) is the center of the board)
            //noted that this touch point is calculated in relation to the location of the virtual pad on the board
            //hence when the pad moves by gaze, this touch point will be also updated accordingly
            Vector2 localTouchPos = (Vector2)gesture.MetaData;
            //Debug.Log(string.Format("Local touch pos on board: ({0},{1})", localTouchPos.x, localTouchPos.y));
            if(_isBeingSelected)
            {
                local2DTranslate = localTouchPos - prevLocalTouch;
            }
        }
        else if(gesture.GestureType == GestureType.OBJECT_SCALING)
        {
            //get meta data of object scaling gesture, including 3 Vector2 values
            // 0: scale ratios in two direction
            // 1: finger1's local position to the board (-0.5<=x,y<=0.5, (0,0) is the center of the board)
            // 2: finger2's local position to the board (-0.5<=x,y<=0.5, (0,0) is the center of the board)
            //noted that these touch points are calculated in relation to the location of the virtual pad on the board
            //hence when the pad moves by gaze, these touch points will be also updated accordingly
            Vector2[] scaleData = (Vector2[])gesture.MetaData;
            if(_local2DBounds.Contains(scaleData[1]) && _local2DBounds.Contains(scaleData[2]))
            {
                local2DScale.Set(scaleData[0].x, scaleData[0].y);
            }
        }
        else if (gesture.GestureType == GestureType.OBJECT_ROTATING)
        {
            //get meta data of object rotating gesture, including 3 Vector2 values
            // 0: rotation angle, Vector2 values store the same rotation values
            // 1: finger1's local position to the board (-0.5<=x,y<=0.5, (0,0) is the center of the board)
            // 2: finger2's local position to the board (-0.5<=x,y<=0.5, (0,0) is the center of the board)
            //noted that these touch points are calculated in relation to the location of the virtual pad on the board
            //hence when the pad moves by gaze, these touch points will be also updated accordingly
            Vector2[] rotData = (Vector2[])gesture.MetaData;
            Debug.Log("RotationAngle: " + rotData[0].x.ToString());
            if (_local2DBounds.Contains(rotData[1]) && _local2DBounds.Contains(rotData[2]))
            {
                rotation = rotData[0].x;
                
            }
        }
    }
    public Rect getLocal2DBoundOfCharacter(GameObject characterObj,GameObject containerObj)
    {
        Rect local2DBounds = new Rect();
        local2DBounds.center = characterObj.transform.localPosition;
        float wRatio = characterObj.transform.GetComponent<Collider>().bounds.size.x/ containerObj.transform.GetComponent<Collider>().bounds.size.x;
        float hRatio = characterObj.transform.GetComponent<Collider>().bounds.size.y / containerObj.transform.GetComponent<Collider>().bounds.size.y;
        local2DBounds.min = new Vector2(characterObj.transform.localPosition.x - wRatio / 2, characterObj.transform.localPosition.y - hRatio / 2);
        local2DBounds.max = new Vector2(characterObj.transform.localPosition.x + wRatio / 2, characterObj.transform.localPosition.y + hRatio / 2);
        return local2DBounds;
    }
}
