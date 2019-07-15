public class LoggingVariable
{
    string _stage;
    string _startTime;
    string _endTime;
    string _duration;
    string _matchingPuzzleTime;
    string _puzzleName;
    string _action;
    string _xFromPoint;
    string _yFromPoint;
    string _xToPoint;
    string _yToPoint;
    string _distanceMoved;
    string _scaleLevel;
    string _rotateAngel;
    string _isTouchPoint;
    string _isOnTrack;
    string _sketchingPointID;
    string _xSketchPoint;
    string _ySketchPoint;

    public LoggingVariable(string stage, string startTime, string endTime, string duration, string matchingPuzzleTime, string puzzleName, string action, string xFromPoint, string yFromPoint, string xToPoint, string yToPoint, string distanceMoved, string scaleLevel, string rotateAngel, string isTouchPoint, string isOnTrack, string sketchingPointID, string xSketchPoint, string ySketchPoint)
    {
        _stage = stage;
        _startTime = startTime;
        _endTime = endTime;
        _duration = duration;
        _matchingPuzzleTime = matchingPuzzleTime;
        _puzzleName = puzzleName;
        _action = action;
        _xFromPoint = xFromPoint;
        _yFromPoint = yFromPoint;
        _xToPoint = xToPoint;
        _yToPoint = yToPoint;
        _distanceMoved = distanceMoved;
        _scaleLevel = scaleLevel;
        _rotateAngel = rotateAngel;
        _isTouchPoint = isTouchPoint;
        _isOnTrack = isOnTrack;
        _sketchingPointID = sketchingPointID;
        _xSketchPoint = xSketchPoint;
        _ySketchPoint = ySketchPoint;
    }

    public string Stage { get => _stage; set => _stage = value; }
    public string StartTime { get => _startTime; set => _startTime = value; }
    public string EndTime { get => _endTime; set => _endTime = value; }
    public string Duration { get => _duration; set => _duration = value; }
    public string MatchingPuzzleTime { get => _matchingPuzzleTime; set => _matchingPuzzleTime = value; }
    public string PuzzleName { get => _puzzleName; set => _puzzleName = value; }
    public string Action { get => _action; set => _action = value; }
    public string XFromPoint { get => _xFromPoint; set => _xFromPoint = value; }
    public string YFromPoint { get => _yFromPoint; set => _yFromPoint = value; }
    public string XToPoint { get => _xToPoint; set => _xToPoint = value; }
    public string YToPoint { get => _yToPoint; set => _yToPoint = value; }
    public string DistanceMoved { get => _distanceMoved; set => _distanceMoved = value; }
    public string ScaleLevel { get => _scaleLevel; set => _scaleLevel = value; }
    public string RotateAngel { get => _rotateAngel; set => _rotateAngel = value; }
    public string IsTouchPoint { get => _isTouchPoint; set => _isTouchPoint = value; }
    public string IsOnTrack { get => _isOnTrack; set => _isOnTrack = value; }
    public string SketchingPointID { get => _sketchingPointID; set => _sketchingPointID = value; }
    public string XSketchPoint { get => _xSketchPoint; set => _xSketchPoint = value; }
    public string YSketchPoint { get => _ySketchPoint; set => _ySketchPoint = value; }
}