using Assets.Script.TestGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    public GameObject _puzzleMakerObject;

    [Header("Experiment log")]
    public string _participantID;
    public int _textureID = 1;
    public bool _isExperimentStarted = false;
    public bool _isExperimentFinished = false;
    public float _prepareTime = 3; //in seconds

    bool _startTimer = false;

    bool isMouseDown = false;

    PuzzleMaker _puzzleMaker;
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
            //test, skip showing Start button
            _puzzleMaker.Init(_textureID, _prepareTime);

            _puzzleMaker._startButtonObject.SetActive(false);
            _startTimer = true;
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
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 28;
            _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            _prepareTime -= Time.deltaTime;
        }

        if (_prepareTime < 0 && !_isExperimentStarted && _puzzleMaker._isInit)
        {
            _prepareTime = 0;
            _isExperimentStarted = true;
            _startTimer = false;

            _puzzleMaker.SetObjectsActive(true);

            Debug.Log("Experiment Started!");

            _puzzleMaker._statusObject.GetComponent<Text>().text = "Experiment Started!";
            _puzzleMaker._statusObject.GetComponent<Text>().font = _puzzleMaker._statusFont;
            _puzzleMaker._statusObject.GetComponent<Text>().color = Color.black;
            _puzzleMaker._statusObject.GetComponent<Text>().fontSize = 4;
            _puzzleMaker._statusObject.GetComponent<Text>().alignment = TextAnchor.LowerCenter;
        }
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

                if (_puzzleMaker.isSketchStarted && !_puzzleMaker.isSketchDoneSucessfully && isMouseDown)
                    _puzzleMaker.CheckSketch();

                if (_puzzleMaker.isSketchDoneSucessfully)
                    _isExperimentFinished = true;
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
}
