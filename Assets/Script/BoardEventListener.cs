using Assets;
using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class BoardEventListener : MonoBehaviour
{
    //GameObject board;
    // Start is called before the first frame update
    const float VIRTUALPAD_DEPTHOFFSET = 0.1f;
    const float GAZEPOINTER_DEPTHOFFSET = 0.01f;
    const int MAX_POINTERS = 5;
    public GameObject board;
    Bounds boardBound;
    public GameObject playerCamera;
    public GameObject virtualPad;
    public GameObject menuManip;
    public GameObject menuDraw;
    public GameObject gazePointer;
    public GameObject Finger1;
    public GameObject Finger2;
    public GameObject Finger3;
    public GameObject Finger4;
    public GameObject Finger5;
    public Material menuManipNormalMat;
    public Material menuManipPressedMat;
    public Material menuDrawNormalMat;
    public Material menuDrawPressedMat;
    GameObject[] fingers = null;
    WebSocketSharp.WebSocket wsClient;
    //simple handling of touch pointers for UI updateing
    System.Object pointerUpdateLock = new System.Object();
    TouchEventData latestTouchEvent = new TouchEventData();
    bool canGazeControl = true;
    System.Object gazeControlLocker = new System.Object();

    TouchGestureRecognizer gestureRecognizer = new TouchGestureRecognizer();
    TouchGestureRecognizer.TouchGesture latestTouchGesture = null;
    VirtualPadManager _virtualPadManager;

    void Start()
    {
        //StartCoroutine(VRActivator("Cardboard"));
        //playerCamera = GameObject.Find("Player");
        //board = GameObject.Find("Board");
        boardBound = board.transform.GetComponent<Collider>().bounds;
        //virtualPad = GameObject.Find("VirtualPad");
        //gazePointer = GameObject.Find("GazePointer");
        Finger1.SetActive(false);
        Finger2.SetActive(false);
        Finger3.SetActive(false);
        Finger4.SetActive(false);
        Finger5.SetActive(false);
        _virtualPadManager = new VirtualPadManager(virtualPad,menuManip,menuDraw);
        _virtualPadManager.setMenuMaterials(menuManipNormalMat, menuManipPressedMat, menuDrawNormalMat, menuDrawPressedMat);
        connectWebSocketServer();
    }
    public IEnumerator VRActivator(string deviceName)
    {
        XRSettings.LoadDeviceByName(deviceName);
        yield return null;
        XRSettings.enabled = true;
    }
    void OnApplicationQuit()
    {
        if (wsClient != null)
        {
            wsClient.Close();
            wsClient = null;
        }
    }
    #region network-related
    void connectWebSocketServer()
    {
        wsClient = new WebSocketSharp.WebSocket(string.Format("ws://{0}/main.html", GlobalUtilities.LoadServerAddress()));
        wsClient.OnMessage += WsClient_OnMessage;
        wsClient.Connect();
    }
    private void WsClient_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
    {
        if (e.IsText)
        {
            string msg = e.Data;
            //Debug.Log(msg);
            try
            {
                //Debug.Log(msg);
                TouchEventData touchEvent = TouchEventData.ParseToucEventFromJsonString(msg);
                //string decodedMsg = "From JSON: ";
                //decodedMsg += string.Format("Event type:{0};Pointers count: {1};AvaiPointers:{2}", touchEvent.EventType, touchEvent.PointerCount, touchEvent.AvaiPointers.Length);
                lock(pointerUpdateLock)
                {
                    latestTouchEvent.Clone(touchEvent);
                }
                latestTouchGesture = HandleTouchGestures(touchEvent);
            }
            catch(Exception ex)
            {
                Debug.Log("JSON Parsing Error:" + ex.Message);
            }
        }
    }
    #endregion
    #region UI-rendering
    // Update is called once per frame
    void Update()
    {
        //update scale here
        lock (virtualPadScaleRatioLock)
        {
            if(virtualPadScaleRatio.x != 0 && virtualPadScaleRatio.y != 0)
            {
                Vector3 curScale = virtualPad.transform.localScale;
                virtualPad.transform.localScale = new Vector3(virtualPadScaleRatio.x*curScale.x, virtualPadScaleRatio.y*curScale.y, curScale.z);
                virtualPadScaleRatio.Set(0, 0);
            }
        }
        //update location of the virtual pad
        Bounds fwBound = virtualPad.GetComponent<Collider>().bounds;
        Rect virtualPad2DContainerLimit = new Rect();
        virtualPad2DContainerLimit.xMin = boardBound.min.x + fwBound.size.x / 2;
        virtualPad2DContainerLimit.yMin = boardBound.min.y + fwBound.size.y / 2;
        virtualPad2DContainerLimit.xMax = boardBound.max.x - fwBound.size.x / 2;
        virtualPad2DContainerLimit.yMax = boardBound.max.y - fwBound.size.y / 2;

        RaycastHit hitInfo;

        //Ray ray = Camera.main.ScreenPointToRay(camScreenCenter);
        Ray ray = new Ray(gazePointer.transform.position, gazePointer.transform.position - playerCamera.transform.position);

        //check current gaze
        if(Physics.Raycast(ray, out hitInfo))
        {
            if(hitInfo.collider != null)
            {
                if (hitInfo.collider.name.CompareTo("Board") == 0 || hitInfo.collider.name.CompareTo("VirtualPad") == 0)
                {
                    //Debug.Log("Hit object: " + hitInfo.collider.name);
                    //gazePointer.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z - GAZEPOINTER_DEPTHOFFSET);
                    //computing new location of the virtual pad
                    if (hitInfo.collider.name.CompareTo("Board") == 0)
                    {
                        Vector3 newPos = hitInfo.point;
                        Vector3 curPos = virtualPad.transform.position;
                        newPos = boundPointToContainer(newPos, virtualPad2DContainerLimit);
                        lock (gazeControlLocker)
                        {
                            if (canGazeControl)
                            {
                                virtualPad.transform.Translate(new Vector3(newPos.x - curPos.x, newPos.y - curPos.y, newPos.z - VIRTUALPAD_DEPTHOFFSET - curPos.z));
                            }
                        }
                    }
                    
                }
                lock (virtualPadTouchTranslateLock)
                {
                    if (virtualPadRelTouchTranslate.x != 0 || virtualPadRelTouchTranslate.y != 0)
                    {
                        Vector2 translationByTouch = new Vector2();
                        translationByTouch.x = virtualPadRelTouchTranslate.x * virtualPad.GetComponent<Collider>().bounds.size.x;
                        translationByTouch.y = -virtualPadRelTouchTranslate.y * virtualPad.GetComponent<Collider>().bounds.size.y;
                        Vector3 curPos = virtualPad.transform.position;
                        Vector3 newPos = new Vector3(curPos.x + translationByTouch.x, curPos.y + translationByTouch.y, curPos.z);
                        newPos = boundPointToContainer(newPos, virtualPad2DContainerLimit);
                        virtualPad.transform.Translate(newPos.x - curPos.x, newPos.y - curPos.y, 0);
                        virtualPadRelTouchTranslate.Set(0, 0);
                    }
                }

            }
        }

        //Process UI based on touch input
        UpdateTouchPointersViz(latestTouchEvent);

        //virtualPadManager.ReactToTouchGesture(latestTouchGesture);
        if(_virtualPadManager == null)
        {
            _virtualPadManager = new VirtualPadManager(virtualPad,menuManip,menuDraw);
            _virtualPadManager.setMenuMaterials(menuManipNormalMat, menuManipPressedMat, menuDrawNormalMat, menuDrawPressedMat);
        }
        _virtualPadManager.ReactToTouchGesture(latestTouchGesture);
    }
    Vector3 boundPointToContainer(Vector3 src,Rect container2D)
    {
        Vector3 boundedPoint = new Vector3(src.x, src.y, src.z);
        if (boundedPoint.x < container2D.xMin)
        {
            boundedPoint.x = container2D.x;
        }
        else if(boundedPoint.x > container2D.xMax)
        {
            boundedPoint.x = container2D.xMax;
        }
        if(boundedPoint.y < container2D.yMin)
        {
            boundedPoint.y = container2D.yMin;
        }
        else if(boundedPoint.y > container2D.yMax)
        {
            boundedPoint.y = container2D.yMax;
        }
        return boundedPoint;
    }
    void UpdateTouchPointersViz(TouchEventData latestTouchEvent)
    {
        if(fingers == null)
        {
            fingers = new GameObject[] { Finger1, Finger2, Finger3, Finger4, Finger5 };
        }
        Bounds virtualPadArea = virtualPad.GetComponent<Collider>().bounds;
        lock(pointerUpdateLock)
        {
            //Update visibility
            for(int i=0;i<MAX_POINTERS;i++)
            {
                GameObject finger = fingers[i];
                if(finger == null)
                {
                    continue;
                }
                if(i < latestTouchEvent.PointerCount)
                {
                    finger.SetActive(true);
                    //position finger in virtual pad
                    /*Vector3 localPos = new Vector3();
                    localPos.x =  latestTouchEvent.AvaiPointers[i].RelX - 0.5f;
                    localPos.y =  0.5f - latestTouchEvent.AvaiPointers[i].RelY;
                    localPos.z = finger.transform.localPosition.z;
                    finger.transform.localPosition = localPos;*/
                    Vector2 relPosInPad = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(new Vector2(latestTouchEvent.AvaiPointers[i].RelX, latestTouchEvent.AvaiPointers[i].RelY));
                    finger.transform.localPosition = new Vector3(relPosInPad.x,relPosInPad.y, finger.transform.localPosition.z);

                }
                else
                {
                    //finger.transform.position = virtualPadArea.min;
                    finger.SetActive(false);
                }
            }
            
        }
    }
    #endregion
    #region Touch-event related
    object virtualPadTouchTranslateLock = new object();
    Vector2 virtualPadRelTouchTranslate = new Vector2();
    object virtualPadScaleRatioLock = new object();
    Vector2 virtualPadScaleRatio = new Vector2();
    TouchGestureRecognizer.TouchGesture HandleTouchGestures(TouchEventData touchEvent)
    {
        
        TouchGestureRecognizer.TouchGesture recognizedGesture = gestureRecognizer.recognizeGesture(touchEvent);
        lock (gazeControlLocker)
        {
            canGazeControl = true;
        }
        if (recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.PAD_TRANSLATING)
        {
            lock (gazeControlLocker)
            {
                canGazeControl = false;
            }
            Vector2 eventMetaData = (Vector2)recognizedGesture.MetaData;
            lock (virtualPadTouchTranslateLock)
            {
                virtualPadRelTouchTranslate.Set(eventMetaData.x, eventMetaData.y);
            }
            return recognizedGesture;
        }
        if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.PAD_SCALING)
        {
            lock(virtualPadScaleRatioLock)
            {
                Vector2 scaleRatio = (Vector2)recognizedGesture.MetaData;
                virtualPadScaleRatio.Set(scaleRatio.x, scaleRatio.y);
            }
            return recognizedGesture;
        }
        if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.SINGLE_TAP)
        {
            Vector2 tapPos = (Vector2)recognizedGesture.MetaData;
        }
        if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.NONE)
        {
            
        }
        return recognizedGesture;
    }
    #endregion
}
