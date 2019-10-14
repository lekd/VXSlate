using Assets;
using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditMode { OBJECT_MANIP, DRAW, MENU_SELECTION }
public class VirtualPadController : MonoBehaviour, IRemoteController
{
    const int MAX_FINGERS_COUNT = 5;
    const float VIRTUALPAD_DEPTHOFFSET = -0.02f;
    float PAD_Z = 0;
    //Public Game Objects
    public GameObject eventListenerObject;
    public GameObject gameCameraObject;
    public GameObject gazePointObject;
    public GameObject boardObject;
    public GameObject finger1;
    public GameObject finger2;
    public GameObject finger3;
    public GameObject finger4;
    public GameObject finger5;
    public GameObject menuManip;
    public GameObject menuDraw;
    GameObject[] fingers;

    Bounds boardObjectBound;

    Camera gameCamera;
    IGeneralPointerEventListener eventTouchListener;
    MenuItemListener[] menuItems = new MenuItemListener[2];
    
    GestureRecognizedEventCallback gestureRecognizedListener = null;
    event GestureRecognizedEventCallback gestureRecognizedBroadcaster = null;
    event MenuItemListener.EditModeSelectedCallBack editModeChangedListener = null;

    bool _gazeCanShift = true;
    public bool _gazeCanShiftWithOneFinger = true;
    Vector3 latestGazeInPad = new Vector3();
    EditMode _currentMode;
    public EditMode CurrentMode
    {
        get
        {
            return _currentMode;
        }

        set
        {
            _currentMode = value;
        }
    }
    
    CriticVar padTranslationByPointers = new CriticVar();
    CriticVar padScaleByPointers = new CriticVar();
    CriticVar padRotationByPointers = new CriticVar();
    Rect boardFlatBound = new Rect();
    Rect padFlatBound = new Rect();
    Vector2 localPadFlatCenter = new Vector2();
    public string getControllerName()
    {
        return "TabletController";
    }
    void Start()
    {
        _currentMode = EditMode.OBJECT_MANIP;
        if (gameCameraObject)
        {
            gameCamera = gameCameraObject.GetComponent<Camera>();
        }
        if(eventListenerObject)
        {
            eventTouchListener = eventListenerObject.GetComponent<TabletTouchEventManager>();
        }
        gestureRecognizedListener = this.gestureRecognizedHandler;
        if(eventTouchListener != null)
        {
            eventTouchListener.SetGestureRecognizedListener(gestureRecognizedListener);
        }
        menuItems[0] = menuManip.GetComponent<MenuItemListener>();
        menuItems[0].edidModeSelectedListener += VirtualPadController_menuSelectedListener;
        menuItems[0].CorrespondingMode = EditMode.OBJECT_MANIP;
        menuItems[1] = menuDraw.GetComponent<MenuItemListener>();
        menuItems[1].edidModeSelectedListener += VirtualPadController_menuSelectedListener;
        menuItems[1].CorrespondingMode = EditMode.DRAW;

        boardObjectBound = boardObject.GetComponent<Collider>().bounds;
        boardFlatBound.min = new Vector2(boardObjectBound.min.x, boardObjectBound.min.y);
        boardFlatBound.max = new Vector2(boardObjectBound.max.x, boardObjectBound.max.y);
        fingers = new GameObject[] { finger1, finger2, finger3, finger4, finger5 };
        for(int i=0; i< fingers.Length; i++)
        {
            fingers[i].SetActive(false);
        }

        padTranslationByPointers.CriticData = new Vector2(0, 0);
        padScaleByPointers.CriticData = new Vector2(0, 0);

        PAD_Z = gameObject.transform.position.z;
        setMenuActiveness(false);
    }
    // Update is called once per frame
    void Update()
    {
        
        //Update virtual pad scale
        lock (padScaleByPointers.AccessLock)
        {
            Vector2 scaleRatio = (Vector2)padScaleByPointers.CriticData;
            if(scaleRatio.x != 0 && scaleRatio.y != 0)
            {
                Vector3 curScale = gameObject.transform.localScale;
                gameObject.transform.localScale = new Vector3(scaleRatio.x * curScale.x, scaleRatio.y * curScale.y, curScale.z);
                padScaleByPointers.CriticData = new Vector2(0, 0);
            }
        }
        //get the 2D boundary of the board, which serve as the position limits of the virtual pad
        Bounds virtualPadBound = gameObject.GetComponent<Collider>().bounds;
        Rect padContainer2DBoundary = new Rect();
        padContainer2DBoundary.xMin = boardObjectBound.min.x + virtualPadBound.size.x / 2;
        padContainer2DBoundary.xMax = boardObjectBound.max.x - virtualPadBound.size.x / 2;
        padContainer2DBoundary.yMin = boardObjectBound.min.y + virtualPadBound.size.y / 2;
        padContainer2DBoundary.yMax = boardObjectBound.max.y - virtualPadBound.size.y / 2;
        //Update Virtual Pad Following Camera Lookat Vector
        UpdateVirtualPadBasedOnCamera(padContainer2DBoundary);
        UpdateFingersBasedOnTouch();
        //Update virtual pad translation caused by pointers
        lock(padTranslationByPointers.AccessLock)
        {
            
            Vector2 relTranslation = (Vector2)padTranslationByPointers.CriticData;
            if (relTranslation.x != 0 || relTranslation.y != 0)
            {
                Vector2 translationByTouch = new Vector2();
                translationByTouch.x = relTranslation.x * gameObject.GetComponent<Collider>().bounds.size.x;
                translationByTouch.y = -relTranslation.y * gameObject.GetComponent<Collider>().bounds.size.y;
                Vector3 curPos = gameObject.transform.position;
                Vector3 newPos = new Vector3(curPos.x + translationByTouch.x, curPos.y + translationByTouch.y, curPos.z);
                newPos = GlobalUtilities.boundPointToContainer(newPos, padContainer2DBoundary);
                gameObject.transform.Translate(newPos.x - curPos.x, newPos.y - curPos.y, 0);
                padTranslationByPointers.CriticData = new Vector2(0, 0);
            }
        }
        if (_currentMode == EditMode.MENU_SELECTION)
        {
            setMenuActiveness(true);
        }
        else
        {
            setMenuActiveness(false);
        }
        //update current size of the virtual flat boundary
        padFlatBound.min = new Vector2(gameObject.GetComponent<Collider>().bounds.min.x, gameObject.GetComponent<Collider>().bounds.min.y);
        padFlatBound.max = new Vector2(gameObject.GetComponent<Collider>().bounds.max.x, gameObject.GetComponent<Collider>().bounds.max.y);
        localPadFlatCenter.Set((padFlatBound.center.x - boardFlatBound.center.x)/boardFlatBound.width, 
                                (padFlatBound.center.y - boardFlatBound.center.y)/boardFlatBound.height);
    }

    private void UpdateVirtualPadBasedOnCamera(Rect board2DBound)
    {

        if (!gameCamera)
        {
            return;
        }


        /*RaycastHit hitInfo;
        Ray camRay = new Ray(gazePointObject.transform.position, gazePointObject.transform.position - gameCamera.transform.position);
        if (Physics.Raycast(camRay, out hitInfo))
        {
            if (hitInfo.collider != null)
            {
                if (hitInfo.collider.name.CompareTo(boardObject.name) == 0)
                {
                    Vector3 hitPos = hitInfo.point;
                    Vector3 curPadPos = gameObject.transform.position;
                    Vector3 newPadPos = GlobalUtilities.boundPointToContainer(hitPos, board2DBound);
                    gameObject.transform.Translate(new Vector3(newPadPos.x - curPadPos.x, newPadPos.y - curPadPos.y, newPadPos.z - curPadPos.z + VIRTUALPAD_DEPTHOFFSET));
                }
            }
        }*/
        int hittableLayerMask = 1 << 8;
        RaycastHit[] allHits;
        Ray camRay = new Ray(gameCamera.transform.position, gazePointObject.transform.position - gameCamera.transform.position);
        //allHits = Physics.RaycastAll(camRay, 1000, hittableLayerMask);
        allHits = Physics.RaycastAll(camRay);
        if (allHits != null && allHits.Length>0)
        {
            bool isHittingPad = false;
            for (int i = 0; i < allHits.Length; i++)
            {
                if(allHits[i].collider.name.CompareTo(gameObject.name) == 0)
                {
                    isHittingPad = true;
                    break;
                }
            }
            if(!isHittingPad || isHittingPad)
            {
                for (int i = 0; i < allHits.Length; i++)
                {
                    if (allHits[i].collider.name.CompareTo(boardObject.name) == 0)
                    {
                        Vector3 hitPos = allHits[i].point;
                        hitPos.z = PAD_Z;
                        Vector3 curPadCenter = gameObject.transform.position;
                        //smoothly dragging virtual pad, virtual pad is not centered at the collision of the board and the gaze
                        /*Vector2 transDistance2D = new Vector2(hitPos.x - latestGazeInPad.x, hitPos.y - latestGazeInPad.y);
                        Vector3 newPadCenter = new Vector3(curPadCenter.x + transDistance2D.x, curPadCenter.y + transDistance2D.y, PAD_Z);*/
                        //jerkly moving virtual pad by gaze, the virtual pad is centered at the collision of the board and the gaze
                        Vector3 newPadCenter = new Vector3(hitPos.x, hitPos.y, PAD_Z);
                        newPadCenter = GlobalUtilities.boundPointToContainer(newPadCenter, board2DBound);
                        if (_gazeCanShift)
                        {
                            gameObject.transform.Translate(newPadCenter.x - curPadCenter.x, newPadCenter.y - curPadCenter.y, 0);
                            latestGazeInPad = hitPos;
                        }
                        //Vector3 newPadPos = GlobalUtilities.boundPointToContainer(hitPos, board2DBound);
                        //gameObject.transform.Translate(new Vector3(newPadPos.x - curPadPos.x, newPadPos.y - curPadPos.y, newPadPos.z - curPadPos.z + VIRTUALPAD_DEPTHOFFSET));

                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < allHits.Length; i++)
                {
                    if (allHits[i].collider.name.CompareTo(boardObject.name) == 0)
                    {
                        latestGazeInPad = allHits[i].point;
                        latestGazeInPad.z = PAD_Z;
                        break;
                    }
                }
            }
        }
        
    }

    private void UpdateFingersBasedOnTouch()
    {
        if(fingers == null)
        {
            fingers = new GameObject[] { finger1, finger2, finger3, finger4, finger5 };
        }
        CriticVar curAvaiPointers = eventTouchListener.getCurrentAvaiPointers();
        lock (curAvaiPointers.AccessLock)
        {
            if(curAvaiPointers.CriticData == null)
            {
                return;
            }
            TouchPointerData[] avaiPointers = (TouchPointerData[])curAvaiPointers.CriticData;
            for (int i = 0; i < fingers.Length; i++)
            {
                GameObject finger = fingers[i];
                if (finger == null)
                {
                    continue;
                }
                if (i < avaiPointers.Length)
                {
                    finger.SetActive(true);

                    Vector2 relPosInPad = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(new Vector2(avaiPointers[i].RelX, avaiPointers[i].RelY));
                    finger.transform.localPosition = new Vector3(relPosInPad.x, relPosInPad.y, finger.transform.localPosition.z);

                }
                else
                {
                    finger.SetActive(false);
                }
            }
        }
    }
    private void setMenuActiveness(bool isActive)
    {
        if(isActive == false)
        {
            for(int i=0;i<menuItems.Length; i++)
            {
                menuItems[i].off();
            }
        }
        menuManip.SetActive(isActive);
        menuDraw.SetActive(isActive);
    }
    #region handle touch/pointer data
    public void setGestureRecognizedCallback(GestureRecognizedEventCallback gestureRecognizedListener)
    {
        gestureRecognizedBroadcaster += gestureRecognizedListener;
    }
    public void setModeSwitchedCallback(MenuItemListener.EditModeSelectedCallBack modeChangeListener)
    {
        editModeChangedListener += modeChangeListener;
    }
    bool isMultiTouch = false;
    TouchGesture prevMultiTouchGesture;
    TouchGesture latestSingleTouchDown = null;
    void gestureRecognizedHandler(TouchGesture recognizedGesture)
    {
        if(recognizedGesture.GestureType == GestureType.NONE)
        {
            isMultiTouch = false;
        }
        if(_currentMode == EditMode.MENU_SELECTION)
        {
            if (recognizedGesture.GestureType == GestureType.NONE
                || recognizedGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN
                || recognizedGesture.GestureType == GestureType.SINGLE_TAP)
            {
                if(recognizedGesture.GestureType != GestureType.NONE)
                {
                    Vector2 rawLocalTouchPos = (Vector2)recognizedGesture.MetaData;
                    recognizedGesture.MetaData = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(rawLocalTouchPos);
                }
                if (!isMultiTouch)
                {
                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        menuItems[i].HandlePointerGesture(recognizedGesture);
                    }
                }
            }
        }
        else
        {
            if (recognizedGesture.GestureType == GestureType.FIVE_POINTERS)
            {
                /*_currentMode = EditMode.MENU_SELECTION;
                prevMultiTouchGesture = recognizedGesture;
                isMultiTouch = true;*/
                latestSingleTouchDown = null;
                return;
            }
            else if (recognizedGesture.GestureType == GestureType.PAD_TRANSLATING)
            {
                Vector2 eventMetaData = (Vector2)recognizedGesture.MetaData;
                lock (padTranslationByPointers.AccessLock)
                {
                    padTranslationByPointers.CriticData = eventMetaData;
                }
                isMultiTouch = true;
                prevMultiTouchGesture = recognizedGesture;
                latestSingleTouchDown = null;
                return;
            }
            else if(recognizedGesture.GestureType == GestureType.PAD_SCALING)
            {
                lock(padScaleByPointers)
                {
                    padScaleByPointers.CriticData = recognizedGesture.MetaData;
                }
                isMultiTouch = true;
                prevMultiTouchGesture = recognizedGesture;
                latestSingleTouchDown = null;
                return;
            }

            if (recognizedGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN
            || recognizedGesture.GestureType == GestureType.SINGLE_TOUCH_MOVE
            || recognizedGesture.GestureType == GestureType.SINGLE_LONG_TOUCH)
            {
                if (!isMultiTouch)
                {
                    if(!_gazeCanShiftWithOneFinger)
                    {
                        _gazeCanShift = false;
                    }
                    Vector2 rawTouchLocalPos = (Vector2)recognizedGesture.MetaData;
                    /*if(recognizedGesture.GestureType != GestureType.SINGLE_TOUCH_MOVE)
                    {
                        rawTouchLocalPos = (Vector2)recognizedGesture.MetaData;
                    }
                    else
                    {
                        Vector2[] singleTouchMoveData = (Vector2[])recognizedGesture.MetaData;
                        rawTouchLocalPos = singleTouchMoveData[0];
                    }*/
                    Vector2 localPosOnPad = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(rawTouchLocalPos);
                    Vector2 localPosOnBoard = toLocalPosOnBoard(localPosOnPad);
                    Vector2[] eventData = new Vector2[3];
                    eventData[0] = localPosOnBoard;
                    eventData[1] = eventData[2] = new Vector2();
                    recognizedGesture.MetaData = eventData;
                    if(recognizedGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                    {
                        //latestSingleTouchDown = recognizedGesture;
                        if (gestureRecognizedBroadcaster != null)
                        {
                            gestureRecognizedBroadcaster(recognizedGesture);
                        }
                    }
                    else if (recognizedGesture.GestureType == GestureType.SINGLE_LONG_TOUCH)
                    {
                        //Debug.Log("Long touch at: " + localPosOnBoard.ToString());
                        /*if (gestureRecognizedBroadcaster != null)
                        {
                            if (latestSingleTouchDown != null)
                            {
                                gestureRecognizedBroadcaster(latestSingleTouchDown);
                                latestSingleTouchDown = null;
                            }
                            //gestureRecognizedBroadcaster(recognizedGesture);
                        }*/
                    }
                    else
                    {
                        /*if(latestSingleTouchDown != null)
                        {
                            Vector2 latestTouchDownPos = (Vector2)latestSingleTouchDown.MetaData;
                            Vector2 curTouchPos = (Vector2)recognizedGesture.MetaData;
                            if(Math.Abs(latestTouchDownPos.x - curTouchPos.x)>=0.01 || Math.Abs(latestTouchDownPos.y - curTouchPos.y) >= 0.01)
                            {
                                if (gestureRecognizedBroadcaster != null)
                                {
                                    gestureRecognizedBroadcaster(latestSingleTouchDown);
                                    latestSingleTouchDown = null;
                                    gestureRecognizedBroadcaster(recognizedGesture);
                                }
                            }
                        }
                        else*/
                        //{
                            if (gestureRecognizedBroadcaster != null)
                            {
                                gestureRecognizedBroadcaster(recognizedGesture);
                            }
                        //}
                    }
                }
            }
            else if (recognizedGesture.GestureType == GestureType.OBJECT_SCALING
                || recognizedGesture.GestureType == GestureType.OBJECT_ROTATING)
            {
                _gazeCanShift = false;
                Vector2[] gestureData = (Vector2[])recognizedGesture.MetaData;
                for (int i = 1; i < gestureData.Length; i++)
                {
                    Vector2 rawTouchLocalPos = gestureData[i];
                    Vector2 localPosOnPad = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(rawTouchLocalPos);
                    Vector2 localPosOnBoard = toLocalPosOnBoard(localPosOnPad);
                    gestureData[i].Set(localPosOnBoard.x, localPosOnBoard.y);
                }
                recognizedGesture.MetaData = gestureData;
                isMultiTouch = true;
                if (prevMultiTouchGesture!= null && !TouchGestureRecognizer.isGestureTypeRelatedPad(prevMultiTouchGesture.GestureType))
                {
                    if (gestureRecognizedBroadcaster != null)
                    {
                        /*if (latestSingleTouchDown == null)
                        {
                            latestSingleTouchDown = new TouchGesture();
                            latestSingleTouchDown.GestureType = GestureType.SINGLE_TOUCH_DOWN;
                            Vector2[] singleTouchData = new Vector2[3];
                            singleTouchData[0] = new Vector2((gestureData[1].x + gestureData[2].x) / 2,
                                                                        (gestureData[1].y + gestureData[2].y) / 2);
                            latestSingleTouchDown.MetaData = singleTouchData;
                        }
                        gestureRecognizedBroadcaster(latestSingleTouchDown);
                        latestSingleTouchDown = null;*/
                        gestureRecognizedBroadcaster(recognizedGesture);
                    }
                }
                prevMultiTouchGesture = recognizedGesture;
            }
            else if(recognizedGesture.GestureType == GestureType.NONE)
            {
                _gazeCanShift = true;
                prevMultiTouchGesture = null;
                if (gestureRecognizedBroadcaster != null)
                {
                    gestureRecognizedBroadcaster(recognizedGesture);
                }
                latestSingleTouchDown = null;
            }
            
            
        }
    }
    private void VirtualPadController_menuSelectedListener(EditMode selectedMode)
    {
        _currentMode = selectedMode;
        if(editModeChangedListener != null)
        {
            editModeChangedListener(selectedMode);
        }
    }
    private Vector2 toLocalPosOnBoard(Vector2 localPosOnPad)
    {
        Vector2 localPostOnBoard = new Vector2();
        Vector2 sizeRatioPadBoard = new Vector2(padFlatBound.width / boardFlatBound.width, padFlatBound.height / boardFlatBound.height);
        localPostOnBoard.x = localPadFlatCenter.x + localPosOnPad.x * sizeRatioPadBoard.x;
        localPostOnBoard.y = localPadFlatCenter.y + localPosOnPad.y * sizeRatioPadBoard.y;
        return localPostOnBoard;
    }
    #endregion
}
