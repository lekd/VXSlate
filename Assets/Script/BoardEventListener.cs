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
    const float VIRTUALPAD_DEPTHOFFSET = 0.02f;
    const float GAZEPOINTER_DEPTHOFFSET = 0.01f;
    const int MAX_POINTERS = 5;
    public GameObject board;
    Bounds boardBound;
    public GameObject playerCamera;
    public GameObject virtualPad;
    public GameObject gazePointer;
    WebSocketSharp.WebSocket wsClient;
    //simple handling of touch pointers for UI updateing
    System.Object pointerUpdateLock = new System.Object();
    TouchEventData latestTouchEvent = new TouchEventData();
    Material pointerInviMat;
    Material pointerVisMat;
    bool canGazeControl = true;
    System.Object gazeControlLocker = new System.Object();

    TouchGestureRecognizer gestureRecognizer = new TouchGestureRecognizer();
    void Start()
    {
        //StartCoroutine(VRActivator("Cardboard"));
        playerCamera = GameObject.Find("Player");
        board = GameObject.Find("Board");
        boardBound = board.transform.GetComponent<Collider>().bounds;
        virtualPad = GameObject.Find("VirtualPad");
        gazePointer = GameObject.Find("GazePointer");
        pointerInviMat = Resources.Load("Materials/TransparentMat", typeof(Material)) as Material;
        pointerVisMat = Resources.Load("Materials/PointerMarkMat", typeof(Material)) as Material;
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
        wsClient.Close();
    }
    void connectWebSocketServer()
    {
        wsClient = new WebSocketSharp.WebSocket(string.Format("ws://{0}/main.html", GlobalData.LoadServerAddress()));
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
                HandleTouchEvent(touchEvent);
            }
            catch(Exception ex)
            {
                Debug.Log("JSON Parsing Error:" + ex.Message);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //update scale here
        lock (virtualPadScaleRatioLock)
        {
            if(virtualPadScaleRatio.x != 0 && virtualPadScaleRatio.y != 0)
            {
                virtualPad.transform.localScale = new Vector3(virtualPadScaleRatio.x, virtualPadScaleRatio.y, 1);
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
                    if (hitInfo.collider.name.CompareTo("Board") == 0)
                    {
                        Vector3 newPos = hitInfo.point;
                        Vector3 curPos = virtualPad.transform.position;
                        newPos = boundPointToContainer(newPos, virtualPad2DContainerLimit);
                        //virtualPad.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z - 0.02f);
                        lock (gazeControlLocker)
                        {
                            if (canGazeControl)
                            {
                                virtualPad.transform.Translate(new Vector3(newPos.x - curPos.x, newPos.y - curPos.y, newPos.z - VIRTUALPAD_DEPTHOFFSET - curPos.z));
                            }
                        }
                        //Debug.Log("Collision Point: " + hitInfo.point.ToString());
                    }
                    lock (gazeControlLocker)
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
        }
        //Process UI based on touch input
        UpdateTouchPointersViz(latestTouchEvent);
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
        Bounds virtualPadArea = virtualPad.GetComponent<Collider>().bounds;
        
        lock(pointerUpdateLock)
        {
            //Update visibility
            for(int i=0;i<MAX_POINTERS;i++)
            {
                GameObject pointer = GameObject.Find(string.Format("Finger{0}", i + 1));
                if(pointer == null)
                {
                    continue;
                }
                if(i < latestTouchEvent.PointerCount)
                {
                    //pointer.GetComponent<MeshRenderer>().material = pointerVisMat;
                    Material[] pMats = pointer.GetComponent<MeshRenderer>().materials;
                    if(pMats.Length>0)
                    {
                        pMats[0] = pointerVisMat;
                        pointer.GetComponent<MeshRenderer>().materials = pMats;
                    }
                    //compute position in virtual pad
                    Vector3 pos = new Vector3();
                    pos.x = virtualPadArea.min.x + virtualPadArea.size.x * latestTouchEvent.AvaiPointers[i].RelX;
                    pos.y = virtualPadArea.max.y - virtualPadArea.size.y * latestTouchEvent.AvaiPointers[i].RelY;
                    pos.z = virtualPadArea.min.z;
                    pointer.transform.position = pos;
                }
                else
                {
                    pointer.GetComponent<Renderer>().material = pointerInviMat;
                    pointer.transform.position = virtualPadArea.min;
                }
            }
            
        }
    }

    object virtualPadTouchTranslateLock = new object();
    Vector2 virtualPadRelTouchTranslate = new Vector2();
    object virtualPadScaleRatioLock = new object();
    Vector2 virtualPadScaleRatio = new Vector2();
    void HandleTouchEvent(TouchEventData touchEvent)
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
            //Debug.Log("Translating Focus Window");
            return;
        }
        if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.PAD_SCALING)
        {
            lock(virtualPadScaleRatioLock)
            {
                Vector2 scaleRatio = (Vector2)recognizedGesture.MetaData;
                virtualPadScaleRatio.Set(scaleRatio.x, scaleRatio.y);
            }
            return;
        }
        if(recognizedGesture.GestureType == TouchGestureRecognizer.TouchGestureType.NONE)
        {
            
        }
    }
}
