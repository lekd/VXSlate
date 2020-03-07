﻿using Assets.Script;
using Assets.Script.TestGame;
using BoardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SimpleGame : MonoBehaviour
{
    //public GameObject _eventListennerObject;

    [Header("Experiment Log")]
    public string _participantID;

    [Space(15)]
    [Tooltip("Select experiment session")]
    public bool _isTraining = false;
    public bool _isExperimental = false;

    [Space(5)]
    public int _textureID = 1;
    public int _experimentOrder = 1;

    [Space(15)]
    public bool _usingTablet = false;
    public bool _usingController = false;
    public bool _usingMouse = false;

    [Header("Experiment Stage")]
    [Tooltip("For observing which stage the experiment is running")]
    public bool _isExperimentStarted = false;
    public bool _isExperimentFinished = false;
    public bool _isMatchingLog = false;
    public bool _isSketchingLog = false;
    public bool _isSummaryLog = false;
    public float _prepareTime = 3; //in seconds
    public float _stateChangeTime = 10; //in seconds
    public float _betweenSketchesTime = 10; //in seconds


    [Header("For Testing")]
    public bool _isSketchTesting = false;

    bool _startTimer = false;

    [Header("Experiment Objects")]
    public GameObject _puzzleMakerObject;
    public GameObject tabletControllerObj;
    public GameObject oculusControllerObj;
    public GameObject mouseControllerObj;
    private Texture2D screenTexture;

    public GameObject _virtualPadObject;
    public GameObject _gazeObject;
    public GameObject _tabletInstructionObject;
    public GameObject _leftOculusControllerObject;
    public GameObject _rightOculusControllerObject;
    public GameObject _oculusControllerInstructionObject;

    Vector2 gameSize = new Vector2();

    IRemoteController tabletController;
    IRemoteController mouseController;
    IRemoteController oculusController;

    Color[] paintColors = new Color[1];
    Point2D drawnPoint = new Point2D();
    bool hasThingToDraw = false;
    EditMode gameMode;
    Vector3 screenSize = new Vector3();

    PuzzleMaker _puzzleMaker;
    Piece _selectedPiece = null;
    
    bool hasTouchDown = false;
    Vector3 difPosition = Vector2.zero;

    object gestureUpdateLock = new object();
    TouchGesture _currentGesture;
    TouchGesture _latestTouchDown = null;
    List<SimpleGameReceivedEvent> receivedTouchEvents = new List<SimpleGameReceivedEvent>();

    StreamWriter _puzzleMatchingSW;
    StreamWriter _sketchingSW;
    StreamWriter _sketchingSWReverted;
    StreamWriter _summarySW;
    StreamWriter _initSW;

    float _experimentStartTime;
    float _sketchingTime;

    public List<LoggingVariable> _matchingLogList;
    float _matchingStartTime = 0;
    float _matchingPieceStartTime = 0;
    float _matchingActionStartTime = 0;
    int _matchingPuzzleClickDown = 1;
    float _sketchingStartTime = 0;

    bool isSummaryLogForNonReverted = false;

    // Start is called before the first frame update
    void Start()
    {
        //gameCharacterObj.transform.localPosition.Set(0, 0, -0.00001f);
        _isExperimentStarted = false;
        _isExperimentFinished = false;
        _isMatchingLog = false;
        _isSketchingLog = false;
        _isSummaryLog = false;

        //init global game variables to be used in VXSlate for logging the virtual pad state in relation to the game
        GlobalUtilities.globalGameState = new LoggingVariable();
        GlobalUtilities._curParticipantID = _participantID;
        GlobalUtilities.globalGameState.IsTraining = _isTraining ? "TRUE" : "FALSE";
        GlobalUtilities.globalGameState.ExperimentOrder = _experimentOrder.ToString();
        GlobalUtilities.globalGameState.TextureID = _textureID.ToString();
        GlobalUtilities.globalGameState.Stage = "Init";
        if (_usingController)
        {

            GlobalUtilities.curControlDevice = GlobalUtilities.DEVICES.CONTROLER;
        }
        else if(_usingTablet)
        {
            GlobalUtilities.curControlDevice = GlobalUtilities.DEVICES.TABLET;
        }
        else if (_usingMouse)
        {
            GlobalUtilities.curControlDevice = GlobalUtilities.DEVICES.MOUSE;
        }

        if (!_usingController && !_usingMouse && !_usingTablet)
        {
            Debug.LogError("No interaction technique is selected! Please to using Controler or using Tablet or using Mouse.");
        }
        else
        {
            if (_usingController)
            {
                oculusController = oculusControllerObj.GetComponent<IRemoteController>();

                if (oculusController != null)
                {
                    oculusController.setGestureRecognizedCallback(this.handleControlGesture);
                    oculusController.setModeSwitchedCallback(this.handleEditModeChanged);
                }
                else
                {
                    Debug.LogWarning("Missing Oculus Controller Object!", oculusControllerObj);
                }

                if (_virtualPadObject != null)
                    _virtualPadObject.SetActive(false);

                if (_gazeObject != null)
                    _gazeObject.SetActive(false);

                if (_tabletInstructionObject != null)
                    _tabletInstructionObject.SetActive(false);

                if (tabletControllerObj != null)
                    tabletControllerObj.SetActive(false);

                if (mouseControllerObj!= null)
                    mouseControllerObj.SetActive(false);

                _usingTablet = false;
                _usingMouse = false;
            }
            else if (_usingTablet)
            {
                tabletController = tabletControllerObj.GetComponent<IRemoteController>();

                if (tabletController != null)
                {
                    tabletController.setGestureRecognizedCallback(this.handleControlGesture);
                    tabletController.setModeSwitchedCallback(this.handleEditModeChanged);
                }
                else
                {
                    Debug.LogWarning("Missing Tablet Controller Object!", tabletControllerObj);
                }

                if (_leftOculusControllerObject != null)
                    _leftOculusControllerObject.SetActive(false);

                if (_rightOculusControllerObject != null)
                    _rightOculusControllerObject.SetActive(false);

                if (_oculusControllerInstructionObject != null)
                    _oculusControllerInstructionObject.SetActive(false);

                if (oculusControllerObj != null)
                    oculusControllerObj.SetActive(false);

                if (mouseControllerObj != null)
                    mouseControllerObj.SetActive(false);

                _usingController = false;
                _usingMouse = false;
            }
            else
            {
                mouseController = mouseControllerObj.GetComponent<IRemoteController>();

                if (mouseController != null)
                {
                    mouseController.setGestureRecognizedCallback(this.handleControlGesture);
                    mouseController.setModeSwitchedCallback(this.handleEditModeChanged);
                }
                else
                {
                    Debug.LogWarning("Missing Mouse Controller Object!", mouseControllerObj);
                }


                if (_leftOculusControllerObject != null)
                    _leftOculusControllerObject.SetActive(false);

                if (_rightOculusControllerObject != null)
                    _rightOculusControllerObject.SetActive(false);

                if (_oculusControllerInstructionObject != null)
                    _oculusControllerInstructionObject.SetActive(false);

                if (_virtualPadObject != null)
                    _virtualPadObject.SetActive(false);

                if (_gazeObject != null)
                    _gazeObject.SetActive(false);

                if (_tabletInstructionObject != null)
                    _tabletInstructionObject.SetActive(false);

                if (oculusControllerObj != null)
                    oculusControllerObj.SetActive(false);

                if (tabletControllerObj != null)
                    tabletControllerObj.SetActive(false);

                _usingController = false;
                _usingTablet = false;
            }            

            hasTouchDown = false;            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_startTimer && _prepareTime > 0)
        {
            _puzzleMaker._statusObject.GetComponent<Text>().text = "ARE YOU READY?\n" + ((int)_prepareTime + 1).ToString();
            _puzzleMaker._statusObject.GetComponent<Text>().font = _puzzleMaker._statusFont;
            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.blue;
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 30;
            _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            _prepareTime -= Time.deltaTime;
        }
        else if (_prepareTime < 0 && !_isExperimentStarted && _puzzleMaker._isInit)
        {
            _prepareTime = 0;
            _isExperimentStarted = true;
            GlobalUtilities.globalGameState.Stage = "Matching";
            _startTimer = false;

            _puzzleMaker.SetObjectsTransparency(0.5f);

            Debug.Log(">> Experiment Started!");

            _puzzleMaker._statusObject.GetComponent<Text>().text = "Experiment Started!\nPlease start matching...";
            _puzzleMaker._statusObject.GetComponent<Text>().font = _puzzleMaker._statusFont;
            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.black;
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 5;
            _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.LowerCenter;

            string extension = "";
            extension +=  DateTime.Today.Year.ToString()
                          + DateTime.Today.Month.ToString()
                          + DateTime.Today.Day.ToString()
                          + "_"
                          + DateTime.Now.Hour.ToString()
                          + DateTime.Now.Minute.ToString()
                          + DateTime.Now.Second.ToString();

            string matchingFilename = ".\\Assets\\ExperimentResults\\";
            string sketchingFilename = ".\\Assets\\ExperimentResults\\";
            string sketchingRevertedFilename = ".\\Assets\\ExperimentResults\\";
            string summaryFilename = ".\\Assets\\ExperimentResults\\";
            string initFilename = ".\\Assets\\ExperimentResults\\";

            if (_isTraining)
            {
                matchingFilename += "Training\\";
                sketchingFilename += "Training\\";
                sketchingRevertedFilename += "Training\\";
                summaryFilename += "Training\\";
                initFilename += "Training\\";
            }
            else
            {
                matchingFilename += "Experimental\\";
                sketchingFilename += "Experimental\\";
                sketchingRevertedFilename += "Experimental\\";
                summaryFilename += "Experimental\\";
                initFilename += "Experimental\\";
            }

            matchingFilename += "PuzzleMatching\\Matching_" + _participantID + "_" + extension;
            sketchingFilename += "Sketching\\Sketching_" + _participantID + "_" + extension;
            sketchingRevertedFilename += "Sketching\\SketchingReverted_" + _participantID + "_" + extension;
            summaryFilename += "Summary\\Summary_" + _participantID + "_" + extension;
            initFilename += "Init\\Init_" + _participantID + "_" + extension;

            if (_isTraining)
            {
                matchingFilename += "_Training";
                sketchingFilename += "_Training";
                sketchingRevertedFilename += "_Training";
                summaryFilename += "_Training";
                initFilename += "_Training";
            }
            else
            {
                matchingFilename += "_Experimental";
                sketchingFilename += "_Experimental";
                sketchingRevertedFilename += "_Experimental";
                summaryFilename += "_Experimental";
                initFilename += "_Experimental";
            }

            matchingFilename += ".csv";
            sketchingFilename += ".csv";
            sketchingRevertedFilename += ".csv";
            summaryFilename += ".csv";
            initFilename += ".csv";

            _puzzleMatchingSW = new StreamWriter(matchingFilename);
            _sketchingSW = new StreamWriter(sketchingFilename);
            _sketchingSWReverted = new StreamWriter(sketchingRevertedFilename);
            _summarySW = new StreamWriter(summaryFilename);
            _initSW = new StreamWriter(initFilename);

            _puzzleMatchingSW.WriteLine("ParticipantID,IsTraining,ExperimentOrder,TextureID,Stage,IsTablet,IsController,IsMouse,StartTime,EndTime,Duration,MatchingPuzzleTime,PuzzleName,Action,DistanceMoved,ScaledLevel,RotatedAngle,xFromPoint,yFromPoint,_xToPoint,_yToPoint");
            _sketchingSW.WriteLine("ParticipantID,IsTraining,ExperimentOrder,TextureID,Stage,IsTablet,IsController,IsMouse,StartTime,EndTime,Duration,IsTouchPoint,IsOnTrack,SketchingPointID,XSketchingPoint,YSketchingPoint,IsReversed");
            _sketchingSWReverted.WriteLine("ParticipantID,IsTraining,ExperimentOrder,TextureID,Stage,IsTablet,IsController,IsMouse,StartTime,EndTime,Duration,IsTouchPoint,IsOnTrack,SketchingPointID,XSketchingPoint,YSketchingPoint,IsReversed");
            _summarySW.WriteLine("ParticipantID,IsTraining,ExperimentOrder,TextureID,Stage,IsTablet,IsController,IsMouse,StartTime,EndTime,Duration");
            _initSW.WriteLine("ParticipantID,Puzzle,Xpos,Ypos,Zpos,Xrot,Yrot,Zrot,Xscale,Yscale,Zscale");

            for(int i = 1; i <= _puzzleMaker._puzzlePieces.Count; i++)
            {
                var tempTransform = _puzzleMaker._puzzlePieces[i-1].GameObject.GetComponent<Transform>();

                _initSW.WriteLine(_participantID + "," +
                    i.ToString() + "," +
                    tempTransform.position.x.ToString() + "," +
                    tempTransform.position.y.ToString() + "," +
                    tempTransform.position.z.ToString() + "," +
                    tempTransform.rotation.x.ToString() + "," +
                    tempTransform.rotation.y.ToString() + "," +
                    tempTransform.rotation.z.ToString() + "," +
                    tempTransform.localScale.x.ToString() + "," +
                    tempTransform.localScale.y.ToString() + "," +
                    tempTransform.localScale.z.ToString());
            }

            _initSW.Close();

            _experimentStartTime = Time.time;
        }
        else if (_isExperimentStarted && _puzzleMaker._isInit)
        {
            if (!_isExperimentFinished)
            {
                //_puzzleMaker.HighlightGridPiece();
                
                if (!_puzzleMaker.isPuzzledDone && _puzzleMaker.CheckPuzzlesDone())
                {
                    _puzzleMaker.isPuzzledDone = true;

                    _summarySW.WriteLine(_participantID
                                         + ","
                                         + _isTraining.ToString()
                                         + ","
                                         + _experimentOrder.ToString()
                                         + ","
                                         + _textureID.ToString()
                                         + ","
                                         + "MATCHING,"
                                         + _usingTablet.ToString()
                                         + ","
                                         + _usingController.ToString()
                                         + ","
                                         + _usingMouse.ToString()
                                         + ","
                                         + _experimentStartTime.ToString()
                                         + ","
                                         + Time.time.ToString()
                                         + ","
                                         + (Time.time - _experimentStartTime).ToString());

                    _sketchingTime = Time.time;
                }

                if(_isSketchTesting)
                    _puzzleMaker.isPuzzledDone = true;

                if (_puzzleMaker.isPuzzledDone)
                {
                    if (_puzzleMatchingSW != null && !_isMatchingLog && !_isSketchTesting)
                    {
                        foreach (var e in _matchingLogList)
                        {
                            _puzzleMatchingSW.WriteLine(_participantID
                                                        + ","
                                                        + _isTraining.ToString()
                                                        + ","
                                                        + _experimentOrder.ToString()
                                                        + ","
                                                        + _textureID.ToString()
                                                        + ","
                                                        + "MATCHING"
                                                        + ","
                                                        + _usingTablet.ToString()
                                                        + ","
                                                        + _usingController.ToString()
                                                        + ","
                                                        + _usingMouse.ToString()
                                                        + ","
                                                        + e.StartTime
                                                        + ","
                                                        + e.EndTime
                                                        + ","
                                                        + e.Duration
                                                        + ","
                                                        + e.MatchingPuzzleTime
                                                        + ","
                                                        + e.PuzzleName
                                                        + ","
                                                        + e.Action
                                                        + ","
                                                        + e.DistanceMoved
                                                        + ","
                                                        + e.ScaleLevel
                                                        + ","
                                                        + e.RotateAngel
                                                        + ","
                                                        + e.XFromPoint
                                                        + ","
                                                        + e.YFromPoint
                                                        + ","
                                                        + e.XToPoint
                                                        + ","
                                                        + e.YToPoint);
                        }

                        _isMatchingLog = true;
                        _puzzleMatchingSW.Close();
                    }

                    if(_puzzleMaker.isFirstSketchDone && !_puzzleMaker.isTexturePointsReverted)
                    {
                        if(!isSummaryLogForNonReverted)
                        {
                            _summarySW.WriteLine(_participantID
                                             + ","
                                             + _isTraining.ToString()
                                             + ","
                                             + _experimentOrder.ToString()
                                             + ","
                                             + _textureID.ToString()
                                             + ","
                                             + "SKETCHING,"
                                             + _usingTablet.ToString()
                                             + ","
                                             + _usingController.ToString()
                                             + ","
                                             + _usingMouse.ToString()
                                             + ","
                                             + _sketchingStartTime.ToString()
                                             + ","
                                             + Time.time.ToString()
                                             + ","
                                             + (Time.time - _sketchingStartTime).ToString());

                            isSummaryLogForNonReverted = true;
                        }

                        _puzzleMaker.ReverseTexturePoints(_isTraining, _textureID);
                        _puzzleMaker.isTexturePointsReverted = true;
                        _stateChangeTime = _betweenSketchesTime - 0.001f;
                    }
                    else if (!_puzzleMaker.isSketchStarted && _stateChangeTime < 0 && (!_puzzleMaker.isFirstSketchDone || (_puzzleMaker.isFirstSketchDone && _puzzleMaker.isTexturePointsReverted)))
                    {
                        _puzzleMaker.isSketchStarted = true;
                        _sketchingStartTime = Time.time;

                        //For testing
                        _puzzleMaker._puzzleDoneObject.SetActive(true);
                        ////

                        if (_puzzleMaker._sketchedPixels == null)
                            _puzzleMaker._sketchedPixels = new List<Pixel>();

                        _puzzleMaker._statusObject.GetComponent<Text>().color = Color.black;
                        _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 5;
                        _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
                        _puzzleMaker._statusObject.GetComponent<Text>().text = "Please start sketching from the RED point to the BLUE point.";
                    }
                    else if(_stateChangeTime > 0)
                    {
                        //VXSlate: change the virtual pad to not shifting by gaze while there are fingers in sketching
                        if(_usingTablet)
                        {
                            (tabletController as VirtualPadController)._gazeCanShiftWithOneFinger = false;
                        }
                        _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                        _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 7;

                        if (_stateChangeTime < 10)
                        {
                            _puzzleMaker._puzzleDoneObject.SetActive(true);
                            _puzzleMaker.puzzleMasterObject.SetActive(false);
                            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.blue;

                            if (!_puzzleMaker.isFirstSketchDone)
                                _puzzleMaker._statusObject.GetComponent<Text>().text = "FIRST SKETCH\n\n" + ((int)_stateChangeTime + 1).ToString() + "s \n\nPlease start sketching\nfrom the RED point to the BLUE point.";
                            else
                            {
                                _puzzleMaker._statusObject.GetComponent<Text>().text = "SECOND SKETCH\n\n" + ((int)_stateChangeTime + 1).ToString() + "s \n\nPlease start sketching\nfrom the RED point to the BLUE point.";
                                GlobalUtilities.globalGameState.Stage = "Sketching_2";
                            }
                        }
                        else
                        {
                            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.green;
                            _puzzleMaker._statusObject.GetComponent<Text>().text = "PUZZLE MATCHING IS SUCCESSFULLY DONE!\n\nNext task instruction will be presented in\n" + ((int)_stateChangeTime + 1 - 10).ToString() + "s";
                            GlobalUtilities.globalGameState.Stage = "Sketching_1";
                        }

                        _stateChangeTime -= Time.deltaTime;                    
                    }
                }
                   

                if (_puzzleMaker.isSketchDoneSucessfully)
                {
                    //VXSlate: restore to allow shifting virtual pad with gaze while one finger is touching
                    GlobalUtilities.globalGameState.Stage = "Done";
                    if (_usingTablet)
                    {
                        (tabletController as VirtualPadController)._gazeCanShiftWithOneFinger = true;
                    }

                    if(!_isSketchingLog)
                    {
                        _isSketchingLog = true;
                        _sketchingSW.Close();
                        _sketchingSWReverted.Close();
                    }

                    //if (_sketchingSW != null && !_isSketchingLog)
                    //{
                    //    foreach(var e in _puzzleMaker._sketchingLogList)
                    //    {
                    //        _sketchingSW.WriteLine(_participantID
                    //                               + ","
                    //                               + _isTraining.ToString()
                    //                               + ","
                    //                               + _experimentOrder.ToString()
                    //                               + ","
                    //                               + _textureID.ToString()
                    //                               + ","
                    //                               + e.Stage
                    //                               + ","
                    //                               + _usingTablet.ToString()
                    //                               + ","
                    //                               + _usingController.ToString()
                    //                               + ","
                    //                               + _usingMouse.ToString()
                    //                               + ","
                    //                               + e.StartTime
                    //                               + ","
                    //                               + e.EndTime
                    //                               + ","
                    //                               + e.Duration
                    //                               + ","
                    //                               + e.IsTouchPoint
                    //                               + ","
                    //                               + e.IsOnTrack
                    //                               + ","
                    //                               + e.SketchingPointID
                    //                               + ","
                    //                               + e.XSketchPoint
                    //                               + ","
                    //                               + e.YSketchPoint);
                    //    }

                    //    _isSketchingLog = true;
                    //    _sketchingSW.Close();
                    //}

                    if (_summarySW != null && !_isSummaryLog)
                    {
                        _summarySW.WriteLine(_participantID
                                             + ","
                                             + _isTraining.ToString()
                                             + ","
                                             + _experimentOrder.ToString()
                                             + ","
                                             + _textureID.ToString()
                                             + ","
                                             + "SKETCHING_REVERTED,"
                                             + _usingTablet.ToString()
                                             + ","
                                             + _usingController.ToString()
                                             + ","
                                             + _usingMouse.ToString()
                                             + ","
                                             + _sketchingStartTime.ToString()
                                             + ","
                                             + Time.time.ToString()
                                             + ","
                                             + (Time.time - _sketchingStartTime).ToString());

                        _isSummaryLog = true;
                        _summarySW.Close();
                    }

                    _isExperimentFinished = true;
                }
            }
            else
            {
                _puzzleMaker._statusObject.GetComponent<Text>().color = Color.magenta;
                _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 10;

                if(_isTraining)
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "THE TRAINING TASK IS FINISHED!\nPlease take off the HMD.";
                else
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "THE EXPERIMENTAL TASK IS FINISHED!\nPlease take off the HMD.";

                GlobalUtilities.globalGameState.Stage = "Done";
            }
        }

        lock (gestureUpdateLock)
        {
            HandleGameLogic();
        }

        /*if(hasThingToDraw)
        {
            drawOnTexture(screenTexture, drawnPoint.X, drawnPoint.Y);
            hasThingToDraw = false;
            gameObject.GetComponent<Renderer>().material.mainTexture = screenTexture;
        }*/
    }
    void handleEditModeChanged(EditMode mode)
    {
        Debug.Log("Selected mode: " + mode.ToString());
        gameMode = mode;
    }


    int receivedEventCount = 0;
    void handleControlGesture(TouchGesture gesture)
    {
        lock (gestureUpdateLock)
        {

            //_currentGesture = gesture;
            SimpleGameReceivedEvent receivedEvent = new SimpleGameReceivedEvent();
            receivedEvent.ReceivedGesture = gesture;
            receivedEvent.TimeStamp = System.DateTime.Now;
            receivedTouchEvents.Add(receivedEvent);
            
        }

        
    }
    void drawOnTexture(Texture2D tex, int posX, int posY)
    {
        //tex.SetPixels(posX, posY, 5, 5, paintColors);
        tex.SetPixel(posX, posY, Color.red);
    }

    Vector2 ConvertLocalToGlobal(Vector2 local2DPos)
    {
        float scale = 1;
        //if (_puzzleMaker._isLargeScreenPlane)
        //    scale = 10;

        return new Vector2(local2DPos.x * _puzzleMaker._largeScreenObject.transform.localScale.x * scale + _puzzleMaker._largeScreenObject.transform.position.x,
                           local2DPos.y * _puzzleMaker._largeScreenObject.transform.localScale.y * scale + _puzzleMaker._largeScreenObject.transform.position.y);

    }


    int processedEventCount = 0;
    TouchGesture prevProcessedEvent = null;
    double GAME_UPDATE_INTERVAL = 100.0;
    void HandleGameLogic()
    {

        /*if (_latestTouchDown != null)
        {
            backupCurrent = _currentGesture;
            _currentGesture = _latestTouchDown;
            _latestTouchDown = null;
        }*/
        double elapsedMilliS = 0;
        _currentGesture = null;
        lock (gestureUpdateLock)
        {
            if (receivedTouchEvents.Count > 0)
            {
                _currentGesture = receivedTouchEvents[0].ReceivedGesture;
                TimeSpan elapseSpan = System.DateTime.Now - receivedTouchEvents[0].TimeStamp;
                elapsedMilliS = elapseSpan.TotalMilliseconds;
                receivedTouchEvents.RemoveAt(0);
            }
        }
        /*if(_currentGesture != null 
            && _currentGesture.GestureType != GestureType.SINGLE_TOUCH_DOWN
            && _currentGesture.GestureType != GestureType.NONE
            && elapsedMilliS < 1000/GAME_UPDATE_INTERVAL)
        {
            return;
        }*/
        /*if (_currentGesture!= null && (_currentGesture.GestureType == GestureType.OBJECT_ROTATING ||
                    _currentGesture.GestureType == GestureType.OBJECT_SCALING))
        {
            Vector2[] gesture_params = (Vector2[])_currentGesture.MetaData;
            Debug.Log(string.Format("CURRENT: {0} with ({1},{2})", _currentGesture.GestureType, gesture_params[0].x, gesture_params[0].y));
        }*/

        if (_puzzleMaker == null && _puzzleMakerObject != null)
        {
            _puzzleMaker = _puzzleMakerObject.GetComponent<PuzzleMaker>();
            Debug.Log(">> Init Puzzle Maker");
        }
        else if (_puzzleMakerObject == null)
        {
            Debug.LogWarning("Missing Puzzle Maker Object!", _puzzleMakerObject);
        }
        if (_currentGesture != null && _currentGesture.GestureType == GestureType.NONE)
        {
            hasTouchDown = false;

            if (_selectedPiece != null)
            {
                _selectedPiece.IsSelected = false;
                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                _selectedPiece = null;
            }

            _currentGesture = null;
        }
        else
        {
            if (_isExperimentStarted && _currentGesture != null)
            {
                if (!_puzzleMaker.isPuzzledDone)
                {
                    if (!hasTouchDown && _currentGesture != null && _currentGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                    {
                        
                        hasTouchDown = true;

                        Vector2 local2DPos = ConvertLocalToGlobal(((Vector2[])_currentGesture.MetaData)[0]);

                        Vector3 rayPoint = new Vector3(local2DPos.x,
                                                        local2DPos.y,
                                                        _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);

                        Debug.Log(rayPoint);

                        RaycastHit[] hits = Physics.RaycastAll(rayPoint, Vector3.forward);

                        for (int i = 0; i < hits.Length - 1; i++)
                        {
                            for (int j = i + 1; j < hits.Length; j++)
                            {
                                if (hits[i].transform.position.z > hits[j].transform.position.z)
                                {
                                    var tmp = hits[i];
                                    hits[i] = hits[j];
                                    hits[j] = tmp;
                                }
                            }
                        }

                        for (int i = 0; i < hits.Length; i++)
                        {
                            if (hits[i].transform.gameObject.name.Contains("Puzzle Piece"))
                            {
                                for (int j = 0; j < _puzzleMaker._puzzlePieces.Count; j++)
                                {
                                    if (_puzzleMaker._puzzlePieces[j].GameObject.name == hits[i].transform.gameObject.name)
                                    {
                                        //Reset previous puzzle
                                        if (_selectedPiece != null)
                                        {
                                            _selectedPiece.IsSelected = false;
                                            _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                                        }

                                        float z = 0;
                                        z = _puzzleMaker._puzzlePieces[0].GameObject.transform.position.z;

                                        for (int k = 0; k < j; k++)
                                        {
                                            _puzzleMaker._puzzlePieces[k].GameObject.transform.position = new Vector3(_puzzleMaker._puzzlePieces[k].GameObject.transform.position.x,
                                                                                                                        _puzzleMaker._puzzlePieces[k].GameObject.transform.position.y,
                                                                                                                        _puzzleMaker._puzzlePieces[k + 1].GameObject.transform.position.z);
                                        }

                                        _puzzleMaker._puzzlePieces[j].GameObject.transform.position = new Vector3(_puzzleMaker._puzzlePieces[j].GameObject.transform.position.x,
                                                                                                                    _puzzleMaker._puzzlePieces[j].GameObject.transform.position.y,
                                                                                                                    z);

                                        _puzzleMaker.SortPieces();
                                        // Highlight new puzzle
                                        Piece piece = _puzzleMaker._puzzlePieces[0];

                                        piece.IsSelected = true;
                                        piece.GameObject.GetComponent<Renderer>().material.mainTexture = piece.Highlighted;

                                        piece.GameObject.transform.position = new Vector3(piece.GameObject.transform.position.x,
                                                                                            piece.GameObject.transform.position.y,
                                                                                            _puzzleMaker._puzzlePieces[0].GameObject.transform.position.z);

                                        _puzzleMaker._puzzlePieces[0] = piece;
                                        _selectedPiece = piece;

                                        difPosition = rayPoint - _selectedPiece.GameObject.transform.position;

                                        //
                                        //Debug.Log(_puzzleMaker._puzzlePieces[j].GameObject.transform.position);
                                        //Debug.Log(">>>> SELECTED PIECE: " + hits[i].transform.gameObject.name);
                                        //Debug.Log(">>>> SELECTED PIECE: " + piece.GameObject.name);

                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        _currentGesture = null;

                        if (_matchingLogList == null)
                            _matchingLogList = new List<LoggingVariable>();

                        if(_matchingStartTime == 0)
                        {
                            _matchingStartTime = Time.time;
                        }

                        if(_matchingPieceStartTime == 0)
                        {
                            _matchingPieceStartTime = Time.time;
                        }

                        if(_matchingActionStartTime ==0)
                        {
                            _matchingActionStartTime = Time.time;
                        }

                        if(_selectedPiece != null)
                        {
                            LoggingVariable lv = new LoggingVariable(_isTraining.ToString(),
                                                                     _experimentOrder.ToString(),
                                                                     _textureID.ToString(),
                                                                     "MATCHING",
                                                                     _matchingActionStartTime.ToString(),
                                                                     Time.time.ToString(),
                                                                     (Time.time - _matchingActionStartTime).ToString(),
                                                                     _matchingPuzzleClickDown.ToString(),
                                                                     _selectedPiece.GameObject.name,
                                                                     "DOWN",
                                                                     _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                     "0",
                                                                     "0",
                                                                     "0",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "");

                            _matchingLogList.Add(lv);

                            _matchingActionStartTime = Time.time;
                        }

                    }
                    else if (hasTouchDown)
                    {
                        if (_selectedPiece != null && _currentGesture != null && _currentGesture.GestureType == GestureType.SINGLE_TOUCH_MOVE)
                        {
                            Vector2 local2DPos = ConvertLocalToGlobal(((Vector2[])_currentGesture.MetaData)[0]);

                            Vector3 rayPoint = new Vector3(local2DPos.x,
                                                           local2DPos.y,
                                                            _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);


                            Vector3 difV = rayPoint - difPosition;
                            Vector3 moveV = difV - _selectedPiece.GameObject.transform.position;

                            LoggingVariable lv = new LoggingVariable(_isTraining.ToString(),
                                                                     _experimentOrder.ToString(),
                                                                     _textureID.ToString(),
                                                                     "MATCHING",
                                                                     _matchingActionStartTime.ToString(),
                                                                     Time.time.ToString(),
                                                                     (Time.time - _matchingActionStartTime).ToString(),
                                                                     _matchingPuzzleClickDown.ToString(),
                                                                     _selectedPiece.GameObject.name,
                                                                     "MOVE",
                                                                     _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                     difV.x.ToString(),
                                                                     difV.y.ToString(),
                                                                     moveV.magnitude.ToString(),
                                                                     "0",
                                                                     "0",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "");

                            _matchingLogList.Add(lv);

                            _selectedPiece.GameObject.transform.Translate(moveV, Space.World);

                            _currentGesture = null;

                            _matchingActionStartTime = Time.time;
                        }
                        else if (_selectedPiece != null && _currentGesture.GestureType == GestureType.OBJECT_SCALING)
                        {
                            //Vector2 local2DScale = (Vector2)_currentGesture.MetaData;
                            Vector2[] scaleData = (Vector2[])_currentGesture.MetaData;
                            Vector2 local2DScale = scaleData[0];
                            _selectedPiece.GameObject.transform.localScale = new Vector3(_selectedPiece.GameObject.transform.localScale.x * local2DScale.x,
                                                                                          _selectedPiece.GameObject.transform.localScale.y * local2DScale.y,
                                                                                          _selectedPiece.GameObject.transform.localScale.z);
                            Debug.Log("New scale: " + string.Format("({0},{1})", _selectedPiece.GameObject.transform.localScale.x, _selectedPiece.GameObject.transform.localScale.y));

                            LoggingVariable lv = new LoggingVariable(_isTraining.ToString(),
                                                                     _experimentOrder.ToString(),
                                                                     _textureID.ToString(),
                                                                     "MATCHING",
                                                                     _matchingActionStartTime.ToString(),
                                                                     Time.time.ToString(),
                                                                     (Time.time - _matchingActionStartTime).ToString(),
                                                                     _matchingPuzzleClickDown.ToString(),
                                                                     _selectedPiece.GameObject.name,
                                                                     "SCALE",
                                                                     _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                     "0",
                                                                     local2DScale.x.ToString(),
                                                                     "0",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "");

                            _matchingLogList.Add(lv);

                            
                            _currentGesture = null;
                            _matchingActionStartTime = Time.time;
                        }
                        else if (_selectedPiece != null && _currentGesture.GestureType == GestureType.OBJECT_ROTATING)
                        {
                            //Vector2 local2DRotation = (Vector2)_currentGesture.MetaData * -1;
                            Vector2[] rotateData = (Vector2[])_currentGesture.MetaData;
                            Vector2 local2DRotation = rotateData[0]*-1;
                            _selectedPiece.GameObject.transform.RotateAround(_selectedPiece.GameObject.transform.position, _selectedPiece.GameObject.transform.forward, local2DRotation.x);
                            Debug.Log("New rotation");

                            LoggingVariable lv = new LoggingVariable(_isTraining.ToString(),
                                                                     _experimentOrder.ToString(),
                                                                     _textureID.ToString(),
                                                                     "MATCHING",
                                                                     _matchingActionStartTime.ToString(),
                                                                     Time.time.ToString(),
                                                                     (Time.time - _matchingActionStartTime).ToString(),
                                                                     _matchingPuzzleClickDown.ToString(),
                                                                     _selectedPiece.GameObject.name,
                                                                     "ROTATE",
                                                                     _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                     _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                     "0",
                                                                     "0",
                                                                     local2DRotation.x.ToString(),
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "",
                                                                     "");

                            _matchingLogList.Add(lv);

                            
                            _currentGesture = null;
                            _matchingActionStartTime = Time.time;
                        }

                        if (_currentGesture != null && _currentGesture.GestureType == GestureType.NONE)
                        {
                            if (_selectedPiece != null)
                            {
                                _selectedPiece.IsSelected = false;
                                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                                _selectedPiece = null;

                                LoggingVariable lv = new LoggingVariable(_isTraining.ToString(),
                                                                         _experimentOrder.ToString(),
                                                                         _textureID.ToString(),
                                                                         "MATCHING",
                                                                         _matchingActionStartTime.ToString(),
                                                                         Time.time.ToString(),
                                                                         (Time.time - _matchingActionStartTime).ToString(),
                                                                         _matchingPuzzleClickDown.ToString(),
                                                                         _selectedPiece.GameObject.name,
                                                                         "UP",
                                                                         _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                         _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                         _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                         _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                         "0",
                                                                         "0",
                                                                         "0",
                                                                         "",
                                                                         "",
                                                                         "",
                                                                         "",
                                                                         "");

                                _matchingLogList.Add(lv);
                                _matchingActionStartTime = Time.time;
                            }

                            _currentGesture = null;
                            _matchingActionStartTime = Time.time;
                        }
                    }

                    if (_currentGesture != null && _currentGesture.GestureType == GestureType.NONE)
                    {
                        if (_selectedPiece != null)
                        {
                            _selectedPiece.IsSelected = false;
                            _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                            _selectedPiece = null;
                        }

                        _currentGesture = null;
                    }

                }
                else
                {
                    if (_selectedPiece != null)
                    {
                        LoggingVariable lv = new LoggingVariable(_isTraining.ToString(),
                                                                 _experimentOrder.ToString(),
                                                                 _textureID.ToString(),
                                                                 "MATCHING",
                                                                 _matchingActionStartTime.ToString(),
                                                                 Time.time.ToString(),
                                                                 (Time.time - _matchingActionStartTime).ToString(),
                                                                 _matchingPuzzleClickDown.ToString(),
                                                                 _selectedPiece.GameObject.name,
                                                                 "UP",
                                                                 _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                 _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                 _selectedPiece.GameObject.transform.position.x.ToString(),
                                                                 _selectedPiece.GameObject.transform.position.y.ToString(),
                                                                 "0",
                                                                 "0",
                                                                 "0",
                                                                 "",
                                                                 "",
                                                                 "",
                                                                 "",
                                                                 "");

                        _matchingLogList.Add(lv);

                        _selectedPiece.IsSelected = false;
                        _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                        _selectedPiece = null;
                    }

                    if (_puzzleMaker.isSketchStarted && !_puzzleMaker.isSketchDoneSucessfully)
                    {
                        if ((!hasTouchDown && _currentGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN) ||
                            (hasTouchDown && _currentGesture.GestureType == GestureType.SINGLE_TOUCH_MOVE))
                        {

                            hasTouchDown = true;

                            Vector2 local2DPos = ConvertLocalToGlobal(((Vector2[])_currentGesture.MetaData)[0]);

                            Vector3 rayPoint = new Vector3(local2DPos.x,
                                                           local2DPos.y,
                                                           _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);

                            if(_puzzleMaker.isFirstSketchDone)
                                _puzzleMaker.CheckSketch(rayPoint, _sketchingSWReverted, _participantID, _isTraining, _experimentOrder, _textureID, _usingTablet, _usingController, _usingMouse);
                            else
                                _puzzleMaker.CheckSketch(rayPoint, _sketchingSW, _participantID, _isTraining, _experimentOrder, _textureID, _usingTablet, _usingController, _usingMouse);

                            _currentGesture = null;
                        }
                    }
                }
            }
            else
            {
                if (_currentGesture != null && _currentGesture.GestureType == GestureType.SINGLE_TOUCH_DOWN)
                {
                    hasTouchDown = true;

                    Vector2 local2DPos = ConvertLocalToGlobal(((Vector2[])_currentGesture.MetaData)[0]);

                    Vector3 rayPoint = new Vector3(local2DPos.x,
                                                   local2DPos.y,
                                                   _puzzleMaker._largeScreenObject.transform.position.z - 1.5f);

                    RaycastHit[] hits = Physics.RaycastAll(rayPoint, Vector3.forward);

                    foreach (var hitInfo in hits)
                    {
                        if (_puzzleMaker._startButtonObject.gameObject.name == hitInfo.transform.gameObject.name)
                        {
                            _puzzleMaker.Init(_isTraining, _experimentOrder, _textureID, _prepareTime);

                            _puzzleMaker._startButtonObject.SetActive(false);
                            _startTimer = true;

                            Debug.Log(">> Timer started! Counting down...");
                        }
                    }

                    _currentGesture = null;
                }
            }
        }

        //_currentGesture = backupCurrent;
    }

    private void OnApplicationQuit()
    {
        if(_puzzleMatchingSW != null)
            _puzzleMatchingSW.Close();

        if (_sketchingSW != null)
            _sketchingSW.Close();

        if (_sketchingSWReverted != null)
            _sketchingSWReverted.Close();

        if (_summarySW != null)
            _summarySW.Close();
    }
}
