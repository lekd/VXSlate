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

    Vector3 screenSize;
    Vector3 screenCenter;
    // Start is called before the first frame update
    void Start()
    {
        _local2DBounds = getLocal2DBoundOfCharacter(gameObject, gameContainerObj);
        screenSize = gameContainerObj.GetComponent<Collider>().bounds.size;
        screenCenter = gameContainerObj.transform.position;
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
        //Vector3 localPos = gameObject.transform.localPosition;

        if (local2DTranslate.x != 0 || local2DTranslate.y != 0)
        {
            Vector2 abs2DTranslate = new Vector2(screenSize.x * local2DTranslate.x, screenSize.y * local2DTranslate.y);
            Vector3 curCharacterPos = gameObject.transform.position;
            gameObject.transform.position = new Vector3(curCharacterPos.x + abs2DTranslate.x, curCharacterPos.y + abs2DTranslate.y, curCharacterPos.z);
            //gameContainerObj.transform.localPosition = new Vector3(localPos.x, localPos.y + local2DScale.y, localPos.z);
            local2DTranslate.Set(0, 0);
        }
        //Vector3 curCharacterPos = gameObject.transform.position;
        //Vector3 newAbsPos = new Vector3(screenCenter.x + prevLocalTouch.x * screenSize.x, screenCenter.y + prevLocalTouch.y * screenSize.y, curCharacterPos.z);
        //gameObject.transform.position = newAbsPos;
        //process scale character based on touch
        if (local2DScale.x != 1 || local2DScale.y != 1)
        {
            Vector3 curScale = gameObject.transform.localScale;
            gameObject.transform.localScale = new Vector3(curScale.x*local2DScale.x,curScale.y * local2DScale.y, curScale.z);
            //Debug.Log("Object scaled at ratio: " + string.Format("({0},{1})", local2DScale.x, local2DScale.y));
            local2DScale.Set(1, 1);
        }
                
        //gameObject.transform.Rotate(0, rotation, 0);
        gameObject.transform.RotateAround(gameObject.transform.position, Vector3.forward, -rotation);
        rotation = 0;
        
        _local2DBounds = getLocal2DBoundOfCharacter(gameObject, gameContainerObj);
    }
    public bool handleGesture(TouchGesture gesture)
    {
        bool characterHandled = false;
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
            //Debug.Log("NewPadLocalCenter: " + string.Format("({0},{1})", _local2DBounds.center.x, _local2DBounds.center.y));
            //Debug.Log("Local2DBound: " + string.Format("({0},{1},{2},{3})",_local2DBounds.xMin,_local2DBounds.yMin,_local2DBounds.xMax,_local2DBounds.yMax));
            //Debug.Log("LocalTouchPos: " + string.Format("({0},{1})", localTouchPos.x, localTouchPos.y));
            if (isPointInRect(localTouchPos,_local2DBounds))
            {
                _isBeingSelected = true;
                prevLocalTouch = localTouchPos;
                characterHandled = true;
            }
            else
            {
                _isBeingSelected = false;
            }

            //prevLocalTouch = localTouchPos;
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
                prevLocalTouch = localTouchPos;
                characterHandled = true;
            }

            //prevLocalTouch = localTouchPos;
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
                characterHandled = true;
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
            //Debug.Log("RotationAngle: " + rotData[0].x.ToString());
            if (_local2DBounds.Contains(rotData[1]) && _local2DBounds.Contains(rotData[2]))
            {
                rotation = rotData[0].x;
                characterHandled = true;
            }
            
        }
        return characterHandled;
    }
    public Rect getLocal2DBoundOfCharacter(GameObject characterObj,GameObject containerObj)
    {
        Vector3 containerSize = containerObj.GetComponent<Collider>().bounds.size;
        Rect local2DBounds = new Rect();

        Vector2 boundcenter = new Vector2((characterObj.transform.position.x - containerObj.transform.position.x)/ containerSize.x, 
                                             (characterObj.transform.position.y - containerObj.transform.position.y)/containerSize.y);
        float wRatio = characterObj.transform.GetComponent<Collider>().bounds.size.x/ containerObj.transform.GetComponent<Collider>().bounds.size.x;
        float hRatio = characterObj.transform.GetComponent<Collider>().bounds.size.y / containerObj.transform.GetComponent<Collider>().bounds.size.y;
        local2DBounds.width = wRatio;
        local2DBounds.height = hRatio;
        local2DBounds.min = new Vector2(boundcenter.x - wRatio / 2, boundcenter.y - hRatio / 2);
        local2DBounds.max = new Vector2(boundcenter.x + wRatio / 2, boundcenter.y + hRatio / 2);
        
        return local2DBounds;
    }
    bool isPointInRect(Vector2 point,Rect bound)
    {
        return (point.x >= bound.xMin && point.x <= bound.xMax && point.y >= bound.yMin && point.y <= bound.yMax);
    }
}
