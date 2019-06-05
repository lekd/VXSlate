using UnityEngine;
using UnityEditor;

public class Point2D
{
    int _x;
    int _y;

    public int X
    {
        get
        {
            return _x;
        }

        set
        {
            _x = value;
        }
    }

    public int Y
    {
        get
        {
            return _y;
        }

        set
        {
            _y = value;
        }
    }


}