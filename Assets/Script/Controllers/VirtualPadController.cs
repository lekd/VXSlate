using Assets;
using Assets.Script;
using Assets.Script.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditMode { OBJECT_MANIP, DRAW, MENU_SELECTION }
public class VirtualPadController : MonoBehaviour, IRemoteController
{
    const int MAX_FINGERS_COUNT = 5;
    const float VIRTUALPAD_DEPTHOFFSET = -0.02f;
    const int NUM_BUFFERED_GAZE_VELOCITIES = 10;
    float PAD_Z = 0;
    //Public Game Objects
    public GameObject eventListenerObject;
    public GameObject gameCameraObject;
    public GameObject gazePointObject;
    public GameObject boardObject;
    public GameObject gazeTolerantAreaObject;
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
    Vector3 latestGazeInTolerantArea = new Vector3();
    Vector3 padTargetPosition = new Vector3();
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
    //store past gaze position to detect intentional gaze move vs saccade in order to stabilize the pad
    Vector2[] pastGazeVelocities = new Vector2[NUM_BUFFERED_GAZE_VELOCITIES];
    Vector2[] pastGazeAccelerations = new Vector2[NUM_BUFFERED_GAZE_VELOCITIES - 1];
    Vector3 previousGazePos = new Vector3(0, 0, 0);
    int curBufferedGazeVeloIndex = 0;
    public string getControllerName()
    {
        return "TabletController";
    }

    VXSlateLogger padStateLogger = null;
    void Start()
    {
        _currentMode = EditMode.OBJECT_MANIP;
        if (gameCameraObject)
        {
            gameCamera = gameCameraObject.GetComponent<Camera>();
        }
        if (eventListenerObject)
        {
            eventTouchListener = eventListenerObject.GetComponent<TabletTouchEventManager>();
        }
        gestureRecognizedListener = this.gestureRecognizedHandler;
        if (eventTouchListener != null)
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
        for (int i = 0; i < fingers.Length; i++)
        {
            fingers[i].SetActive(false);
        }

        padTranslationByPointers.CriticData = new Vector2(0, 0);
        padScaleByPointers.CriticData = new Vector2(0, 0);

        PAD_Z = gameObject.transform.position.z;
        padTargetPosition = gameObject.transform.position;
        setMenuActiveness(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (padStateLogger == null && GlobalUtilities.curControlDevice == GlobalUtilities.DEVICES.TABLET)
        {
            padStateLogger = new VXSlateLogger();
            padStateLogger.init(GlobalUtilities._curParticipantID, GlobalUtilities.globalGameState.ExperimentOrder, GlobalUtilities.globalGameState.TextureID, GlobalUtilities.globalGameState.IsTraining);
        }
        //Update virtual pad scale
        lock (padScaleByPointers.AccessLock)
        {
            Vector2 scaleRatio = (Vector2)padScaleByPointers.CriticData;
            if (scaleRatio.x != 0 && scaleRatio.y != 0)
            {
                Vector3 curScale = gameObject.transform.localScale;
                if (padStateLogger != null)
                {
                    padStateLogger.WriteVirtualPadScaling(GlobalUtilities._curParticipantID,
                                                          GlobalUtilities.globalGameState.ExperimentOrder,
                                                          GlobalUtilities.globalGameState.TextureID,
                                                          GlobalUtilities.globalGameState.Stage,
                                                          scaleRatio.x,
                                                          scaleRatio.y);
                }
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
        lock (padTranslationByPointers.AccessLock)
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
        localPadFlatCenter.Set((padFlatBound.center.x - boardFlatBound.center.x) / boardFlatBound.width,
                                (padFlatBound.center.y - boardFlatBound.center.y) / boardFlatBound.height);
    }
    #region gaze_handling
    private void UpdateVirtualPadBasedOnCamera(Rect board2DBound)
    {

        if (!gameCamera)
        {
            return;
        }

        //int hittableLayerMask = 1 << 8;
        RaycastHit[] allHits;
        Ray camRay = new Ray(gameCamera.transform.position, gazePointObject.transform.position - gameCamera.transform.position);
        //allHits = Physics.RaycastAll(camRay, 1000, hittableLayerMask);
        allHits = Physics.RaycastAll(camRay);
        bool hitLargeDisplay = false;
        if (allHits != null && allHits.Length > 0)
        {
            bool isGazeTolerantArea = false;
            for (int i = 0; i < allHits.Length; i++)
            {
                if (allHits[i].collider.name.CompareTo(gazeTolerantAreaObject.name) == 0)
                {
                    isGazeTolerantArea = true;
                    break;
                }
            }
            //if(!isGazeTolerantArea)
            bool isIntentionalGazeMove = false;
            {
                for (int i = 0; i < allHits.Length; i++)
                {
                    if (allHits[i].collider.name.CompareTo(boardObject.name) == 0)
                    {
                        hitLargeDisplay = true;
                        Vector3 hitPos = allHits[i].point;
                        //calculating and storing gaze velo and accels to determine intentional movement
                        if (previousGazePos == Vector3.zero)
                        {
                            previousGazePos = hitPos;
                            curBufferedGazeVeloIndex = 0;
                        }
                        else
                        {
                            Vector3 gazeMove = hitPos - previousGazePos;
                            pastGazeVelocities[curBufferedGazeVeloIndex] = new Vector2(gazeMove.x / Time.deltaTime, gazeMove.y / Time.deltaTime);
                            if (curBufferedGazeVeloIndex > 0)
                            {
                                Vector2 gazeAccel = pastGazeVelocities[curBufferedGazeVeloIndex] - pastGazeVelocities[curBufferedGazeVeloIndex - 1];
                                gazeAccel.x /= Time.deltaTime;
                                gazeAccel.y /= Time.deltaTime;
                                pastGazeAccelerations[curBufferedGazeVeloIndex - 1] = gazeAccel;
                            }
                            if (curBufferedGazeVeloIndex == NUM_BUFFERED_GAZE_VELOCITIES - 1)
                            {
                                isIntentionalGazeMove = detectIntentionalGazeMovement(pastGazeVelocities, pastGazeAccelerations);
                                //remove the oldest velocity, shift everything to the left
                                for (int veloIdx = 0; veloIdx < pastGazeVelocities.Length - 1; veloIdx++)
                                {
                                    pastGazeVelocities[veloIdx] = pastGazeVelocities[veloIdx + 1];
                                }
                                //remove the oldest acceleration, shift everything to the left
                                for (int accelIdx = 0; accelIdx < pastGazeAccelerations.Length - 1; accelIdx++)
                                {
                                    pastGazeAccelerations[accelIdx] = pastGazeAccelerations[accelIdx + 1];
                                }
                            }
                            else
                            {
                                curBufferedGazeVeloIndex++;
                            }
                        }
                        hitPos.z = PAD_Z;
                        Vector3 curPadCenter = gameObject.transform.position;
                        //smoothly dragging virtual pad, virtual pad is not centered at the collision of the board and the gaze
                        //Vector2 transDistance2D = new Vector2(hitPos.x - latestGazeInTolerantArea.x, hitPos.y - latestGazeInTolerantArea.y);
                        //Vector2 shiftFactor = new Vector2(1f, 1f);
                        //Vector3 newPadCenter = new Vector3(curPadCenter.x + transDistance2D.x*shiftFactor.x, curPadCenter.y + transDistance2D.y*shiftFactor.y, PAD_Z);
                        //jerkly moving virtual pad by gaze, the virtual pad is centered at the collision of the board and the gaze
                        Vector3 newPadCenter = new Vector3(hitPos.x, hitPos.y, PAD_Z);
                        newPadCenter = GlobalUtilities.boundPointToContainer(newPadCenter, board2DBound);
                        padTargetPosition = newPadCenter;
                        double euclDist = Math.Sqrt((newPadCenter.x - curPadCenter.x) * (newPadCenter.x - curPadCenter.x)
                                                    + (newPadCenter.y - curPadCenter.y) * (newPadCenter.y - curPadCenter.y)) * 15;
                        if (_gazeCanShift && isIntentionalGazeMove)
                        {
                            //gameObject.transform.Translate(newPadCenter.x - curPadCenter.x, newPadCenter.y - curPadCenter.y, 0);
                            gameObject.transform.position = Vector3.MoveTowards(curPadCenter, newPadCenter, Time.deltaTime * (float)euclDist);
                            if (padStateLogger != null)
                            {
                                padStateLogger.WriteVirtualPadTranslation(GlobalUtilities._curParticipantID,
                                                                          GlobalUtilities.globalGameState.ExperimentOrder,
                                                                          GlobalUtilities.globalGameState.TextureID,
                                                                          GlobalUtilities.globalGameState.Stage,
                                                                          newPadCenter.x - curPadCenter.x,
                                                                          newPadCenter.y - curPadCenter.y);
                            }
                        }
                        //Vector3 newPadPos = GlobalUtilities.boundPointToContainer(hitPos, board2DBound);
                        //gameObject.transform.Translate(new Vector3(newPadPos.x - curPadPos.x, newPadPos.y - curPadPos.y, newPadPos.z - curPadPos.z + VIRTUALPAD_DEPTHOFFSET));

                        break;
                    }
                }
            }
            /*else
            {
                //double euclDist = Math.Sqrt((padTargetPosition.x - gameObject.transform.position.x) * (padTargetPosition.x - gameObject.transform.position.x)
                                                    //+ (padTargetPosition.y - gameObject.transform.position.y) * (padTargetPosition.y - gameObject.transform.position.y)) * 15;
                //gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, padTargetPosition, Time.deltaTime * (float)euclDist);
            }*/
        }

        if (hitLargeDisplay == false)
        {
            previousGazePos = Vector3.zero;
        }
    }

    private bool detectIntentionalGazeMovement(Vector2[] pastGazeVelocities, Vector2[] pastGazeAccelerations)
    {
        Vector2 avgGazeVelo = new Vector2(0, 0);
        for (int i = 0; i < pastGazeVelocities.Length; i++)
        {
            avgGazeVelo = avgGazeVelo + pastGazeVelocities[i];
        }
        avgGazeVelo.x /= pastGazeVelocities.Length;
        avgGazeVelo.y /= pastGazeVelocities.Length;
        double avgVeloAmp = Math.Sqrt(avgGazeVelo.x * avgGazeVelo.x + avgGazeVelo.y * avgGazeVelo.y);
        //compute angle differences between past velo vectors and the avg one
        double[] gazeVeloAngleDiffs = new double[pastGazeVelocities.Length];
        string veloAnglesStr = "";
        double avgAngleDif = 0;
        for (int i = 0; i < gazeVeloAngleDiffs.Length; i++)
        {
            Vector2 velo = pastGazeVelocities[i];
            gazeVeloAngleDiffs[i] = Math.Atan2(velo.x * avgGazeVelo.y - velo.y * avgGazeVelo.x, velo.x * avgGazeVelo.x + velo.y * avgGazeVelo.y) * 180 / Math.PI;
            avgAngleDif += gazeVeloAngleDiffs[i];
            veloAnglesStr += string.Format("{0};", gazeVeloAngleDiffs[i]);
        }
        avgAngleDif /= gazeVeloAngleDiffs.Length;
        //Debug.Log(string.Format("Avg velo angle diff = {0}; Avg velo amplitude = {1}", avgAngleDif, avgVeloAmp));
        if (Math.Abs(avgAngleDif) > 0.008 && Math.Abs(avgVeloAmp) > 50)
        {
            return true;
        }
        //Debug.Log("VeloAngles: " + veloAnglesStr);
        return false;
    }
    #endregion
    private void UpdateFingersBasedOnTouch()
    {
        if (fingers == null)
        {
            fingers = new GameObject[] { finger1, finger2, finger3, finger4, finger5 };
        }
        CriticVar curAvaiPointers = eventTouchListener.getCurrentAvaiPointers();
        lock (curAvaiPointers.AccessLock)
        {
            if (curAvaiPointers.CriticData == null)
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
        if (isActive == false)
        {
            for (int i = 0; i < menuItems.Length; i++)
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
        if (recognizedGesture.GestureType == GestureType.NONE)
        {
            isMultiTouch = false;
        }
        if (_currentMode == EditMode.MENU_SELECTION)
        {
            if (recognizedGesture.GestureType == GestureType.NONE
                || recognizedGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN
                || recognizedGesture.GestureType == GestureType.SINGLE_TAP)
            {
                if (recognizedGesture.GestureType != GestureType.NONE)
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
            else if (recognizedGesture.GestureType == GestureType.PAD_SCALING)
            {
                lock (padScaleByPointers)
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
                    if (!_gazeCanShiftWithOneFinger)
                    {
                        _gazeCanShift = false;
                    }
                    //Vector2 rawTouchLocalPos = (Vector2)recognizedGesture.MetaData;
                    Vector2 rawTouchLocalPos;
                    if (recognizedGesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
                    {
                        Vector2[] touchMoveData = (Vector2[])recognizedGesture.MetaData;
                        rawTouchLocalPos = touchMoveData[0];
                        Vector2 rawTouchMoveVelo = touchMoveData[1];
                        /*if(Math.Abs(rawTouchMoveVelo.x) < 0.01 && Math.Abs(rawTouchMoveVelo.x) < 0.01)
                        {
                            _gazeCanShift = true;
                        }
                        else
                        {
                            _gazeCanShift = false;
                        }*/
                    }
                    else
                    {
                        rawTouchLocalPos = (Vector2)recognizedGesture.MetaData;
                    }

                    Vector2 localPosOnPad = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(rawTouchLocalPos);
                    Vector2 localPosOnBoard = toLocalPosOnBoard(localPosOnPad);
                    Vector2[] eventData = new Vector2[3];
                    eventData[0] = localPosOnBoard;
                    eventData[1] = eventData[2] = new Vector2();
                    recognizedGesture.MetaData = eventData;
                    if (recognizedGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                    {
                        //latestSingleTouchDown = recognizedGesture;
                        if (gestureRecognizedBroadcaster != null)
                        {
                            gestureRecognizedBroadcaster(recognizedGesture);
                        }
                    }
                    else
                    {
                        if (gestureRecognizedBroadcaster != null)
                        {
                            gestureRecognizedBroadcaster(recognizedGesture);
                        }
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
                if (prevMultiTouchGesture != null && !TouchGestureRecognizer.isGestureTypeRelatedPad(prevMultiTouchGesture.GestureType))
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
            else if (recognizedGesture.GestureType == GestureType.NONE)
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
        if (editModeChangedListener != null)
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
    private void OnApplicationQuit()
    {
        if (padStateLogger != null)
        {
            padStateLogger.Close();
        }
    }
}
