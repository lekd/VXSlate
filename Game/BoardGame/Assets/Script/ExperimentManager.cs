using BoardGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    [Header("Objects Mapping")]
    public GameObject _puzzleMakerObject;
    public GameObject _eventListennerObject;

    [Header("Experiment log")]
    public string _participantID;
    public int _textureID = 1;
    public bool _isExperimentStarted = false;
    public bool _isExperimentFinished = false;
    public float _prepareTime = 3; //in seconds
      
    bool _startTimer = false;

    bool isMouseDown = false;

    PuzzleMaker _puzzleMaker;
    EventListenner _eventListenner;
    Piece _selectedPiece = null;
    // Start is called before the first frame update
    void Start()
    {
        if (_puzzleMakerObject == null)
        {
            Debug.LogWarning("Missing Puzzle Maker Object!", _puzzleMakerObject);
        }
        else
        {
            _puzzleMaker = _puzzleMakerObject.GetComponent<PuzzleMaker>();
        }

        if (_eventListennerObject == null)
        {
            Debug.LogWarning("Missing Puzzle Maker Object!", _puzzleMakerObject);
        }
        else
        {
            _eventListenner = _eventListennerObject.GetComponent<EventListenner>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_startTimer && _prepareTime > 0)
        {
            _puzzleMaker._statusObject.GetComponent<Text>().text = "ARE YOU READY?\n" + ((int)_prepareTime + 1).ToString();
            _puzzleMaker._statusObject.GetComponent<Text>().font = _puzzleMaker._statusFont;
            _puzzleMaker._statusObject.GetComponent<Text>().color =Color.blue;
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 28;
            _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            _prepareTime -= Time.deltaTime;
        }

        if (_prepareTime < 0 && !_isExperimentStarted && _puzzleMaker._isInit)
        {
            _prepareTime = 0;
            _isExperimentStarted = true;
            _startTimer = false;

            _puzzleMaker.SetObjectsTransparency(0.5f);

            Debug.Log("Experiment Started!");

            _puzzleMaker._statusObject.GetComponent<Text>().text = "Experiment Started!";
            _puzzleMaker._statusObject.GetComponent<Text>().font = _puzzleMaker._statusFont;
            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.black;
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 4;
            _puzzleMaker. _statusObject.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
        }

        //OnMouseDown();
        //OnMouseUp();

        CheckEvents();

        if (_isExperimentStarted && _puzzleMaker._isInit)
        {
            

            if (!_isExperimentFinished)
            {
                if (isMouseDown && _selectedPiece != null)
                {
                    _puzzleMaker.UpdatePiecePosition(_selectedPiece);
                }

                _puzzleMaker.HighlightGridPiece();

                if (!_puzzleMaker.isPuzzledDone && _puzzleMaker.CheckPuzzlesDone())
                {
                    _puzzleMaker.isPuzzledDone = true;
                    _puzzleMaker._puzzleDoneObject.SetActive(true);
                }

                if (_puzzleMaker.isPuzzledDone && !_puzzleMaker.isSketchStarted)
                {
                    _puzzleMaker.isSketchStarted = true;

                    if (_puzzleMaker._sketchedPixels == null)
                        _puzzleMaker._sketchedPixels = new List<Pixel>();

                    _puzzleMaker._statusObject.GetComponent<Text>().color = Color.green;
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle grid is done!\nPlease start sketching from RED to BLUE point.";
                }

                if(_puzzleMaker.isSketchStarted && !_puzzleMaker.isSketchDoneSucessfully && isMouseDown)
                    _puzzleMaker.CheckSketch();

                if(_puzzleMaker.isSketchDoneSucessfully)
                    _isExperimentFinished = true;

                if (Input.GetKeyDown(KeyCode.DownArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceScaleDown(_selectedPiece);

                    Debug.Log("Puzzle piece is scaled down!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is scaled down!";
                }

                if (Input.GetKeyUp(KeyCode.UpArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceScaleUp(_selectedPiece);

                    Debug.Log("Puzzle piece is scaled up!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is scaled up!";
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceRotateLeft(_selectedPiece);

                    Debug.Log("Puzzle piece is rotated left!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is rotated left!";
                }

                if (Input.GetKeyUp(KeyCode.RightArrow) && _selectedPiece != null)
                {
                    _selectedPiece = _puzzleMaker.PuzzlePieceRotateRight(_selectedPiece);

                    Debug.Log("Puzzle piece is rotated right!");
                    _puzzleMaker._statusObject.GetComponent<Text>().text = "Puzzle piece is rotated right!";
                }
            }
            else
            {
                _puzzleMaker._statusObject.GetComponent<Text>().color = Color.blue;
                _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 10;
                _puzzleMaker._statusObject.GetComponent<Text>().text = "THE TASK IS FINISHED!\nPlease take off the HMD.";
            }
        }
    }

    private void CheckEvents()
    {
        if(_eventListenner.CLICKDOWN)
        {
            if (_isExperimentStarted)
            {
                if (Input.GetMouseButton(0))
                {
                    //For testing
                    //_puzzleMaker.isPuzzledDone = true;

                    RaycastHit hitInfo1 = new RaycastHit();

                    bool hit1 = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo1);

                    //if (InStartPoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                    //    Debug.Log("IN START POINTS");

                    //if (InLinePoints(ConvertPositionToPixelPosition(new Vector2(hitInfo1.point.x, hitInfo1.point.y))))
                    //    Debug.Log("IN LINE POINTS");

                    if (!isMouseDown)
                    {
                        Debug.Log("OnMouseDown");

                        if (!_puzzleMaker.isPuzzledDone)
                        {
                            RaycastHit hitInfo = new RaycastHit();

                            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                            if (hit)
                            {
                                if (_selectedPiece != null && _selectedPiece.GameObject.name == hitInfo.transform.gameObject.name)
                                {
                                    _selectedPiece.IsSelected = false;
                                    _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;

                                    _selectedPiece = null;
                                }
                                else
                                {
                                    for (int i = 0; i < _puzzleMaker._puzzlePieces.Count; i++)
                                    {
                                        if (_puzzleMaker._puzzlePieces[i].GameObject.name == hitInfo.transform.gameObject.name)
                                        {
                                            float z = 0;

                                            Piece piece = _puzzleMaker._puzzlePieces[i];

                                            //Reset previous puzzle
                                            if (_selectedPiece != null)
                                            {
                                                _selectedPiece.IsSelected = false;
                                                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                                            }

                                            z = _puzzleMaker._puzzlePieces[0].GameObject.transform.position.z;

                                            for (int j = 0; j < i; j++)
                                            {
                                                _puzzleMaker._puzzlePieces[j].GameObject.transform.position = new Vector3(_puzzleMaker._puzzlePieces[j].GameObject.transform.position.x,
                                                                                                                         _puzzleMaker._puzzlePieces[j].GameObject.transform.position.y,
                                                                                                                         _puzzleMaker._puzzlePieces[j + 1].GameObject.transform.position.z);
                                            }

                                            // Highlight new puzzle
                                            piece.IsSelected = true;
                                            piece.GameObject.GetComponent<Renderer>().material.mainTexture = piece.Highlighted;

                                            if (z != 0)
                                                piece.GameObject.transform.position = new Vector3(piece.GameObject.transform.position.x,
                                                                                                  piece.GameObject.transform.position.y,
                                                                                                  z);

                                            _selectedPiece = piece;

                                            _puzzleMaker._puzzlePieces = _puzzleMaker.SortPieces(_puzzleMaker._puzzlePieces);

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (_selectedPiece != null)
                            {
                                _selectedPiece.IsSelected = false;
                                _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                            }


                        }

                        isMouseDown = true;
                    }
                }
            }
            else
            {
                if(_eventListenner._isMouse)
                { 
                    RaycastHit hitInfo = new RaycastHit();

                    bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                    if (hit && _puzzleMaker._startButtonObject.gameObject.name == hitInfo.transform.gameObject.name)
                    {
                        _puzzleMaker.Init(_textureID, _prepareTime);

                        _puzzleMaker._startButtonObject.SetActive(false);
                        _startTimer = true;

                        Debug.Log("Timer started! Counting down...");
                    }
                }

                if(_eventListenner._isTablet)
                {
                    Vector2 startButtonV2 = new Vector2(_puzzleMaker._startButtonObject.gameObject.transform.position.x,
                                                        _puzzleMaker._startButtonObject.gameObject.transform.position.y);

                    if ((startButtonV2 - _eventListenner.touchPosition).magnitude
                        < _puzzleMaker._startButtonObject.gameObject.transform.localScale.x)
                    {
                        _puzzleMaker.Init(_textureID, _prepareTime);

                        _puzzleMaker._startButtonObject.SetActive(false);
                        _startTimer = true;

                        Debug.Log("Timer started! Counting down...");
                    }
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if (!Input.GetMouseButton(0) && isMouseDown == true)
        {
            isMouseDown = false;

            //if(isSketchStarted && !isSketchDoneSucessfully)
            //{
            //    ResetTexture();
            //}

            Debug.Log("OnMouseUp");
        }
    }

    private void OnMouseDrag()
    {
        if (_isExperimentStarted && _puzzleMaker._isInit)
        {
            Debug.Log("OnMouseDrag");

            RaycastHit hitInfo = new RaycastHit();

            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (hit)
            {
                if (_selectedPiece != null)
                {
                    _selectedPiece.IsSelected = false;
                    _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;
                }

                foreach (var piece in _puzzleMaker._puzzlePieces)
                {
                    if (piece.GameObject.name == hitInfo.transform.gameObject.name)
                    {
                        if (_selectedPiece != null && _selectedPiece.GameObject.name == hitInfo.transform.gameObject.name)
                        {
                            _selectedPiece.IsSelected = false;
                            _selectedPiece.GameObject.GetComponent<Renderer>().material.mainTexture = _selectedPiece.Original;

                            _selectedPiece = null;
                        }
                        else
                        {
                            piece.IsSelected = true;
                            piece.GameObject.GetComponent<Renderer>().material.mainTexture = piece.Highlighted;

                            _selectedPiece = piece;
                        }
                    }
                }
            }
            else
            {
                if (_selectedPiece != null)
                {
                    _selectedPiece.IsSelected = false;
                    _selectedPiece = null;
                }
            }
        }
    }

    Vector2 ConvertTabletTo3D(Vector2 v)
    {
        Vector2 ret = new Vector2();

        ret.x = v.x * _puzzleMaker._largeScreenObject.transform.localScale.x;
        ret.y = v.y * _puzzleMaker._largeScreenObject.transform.localScale.y;

        return ret;
    }
}
