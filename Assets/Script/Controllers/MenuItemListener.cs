using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItemListener : MonoBehaviour
{
    public delegate void EditModeSelectedCallBack(EditMode selectedMode);
    // Start is called before the first frame update
    public GameObject parentObject;
    public Material normalStateMat;
    public Material pressedStateMat;
    private Bounds localBound;
    private Rect itemFlatLocalBound;
    private bool _isBeingHit = false;
    private EditMode _correspondingMode;

    public EditMode CorrespondingMode
    {
        get
        {
            return _correspondingMode;
        }

        set
        {
            _correspondingMode = value;
        }
    }

    

    public event EditModeSelectedCallBack edidModeSelectedListener = null;
    string menuItemName = "";
    void Start()
    {
        menuItemName = gameObject.name;
        Bounds itemWorldBound = gameObject.GetComponent<Collider>().bounds;
        localBound = new Bounds();
        localBound.min = parentObject.transform.InverseTransformPoint(itemWorldBound.min);
        localBound.max = parentObject.transform.InverseTransformPoint(itemWorldBound.max);
        itemFlatLocalBound = new Rect(new Vector2(localBound.min.x, localBound.min.y), new Vector2(localBound.size.x, localBound.size.y));
    }

    // Update is called once per frame
    void Update()
    {
        //gameObject.SetActive(_isActive);
        if(_isBeingHit)
        {
            gameObject.GetComponent<MeshRenderer>().material = pressedStateMat;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = normalStateMat;
        }
    }
    public void HandlePointerGesture(TouchGesture gesture)
    {
        if(gesture.GestureType == GestureType.NONE)
        {
            _isBeingHit = false;
        }
        else if(gesture.GestureType == GestureType.SINGLE_TOUCH_DOWN
            || gesture.GestureType == GestureType.SINGLE_TAP)
        {
            Vector2 flatTouchPoint = (Vector2)gesture.MetaData;
            if(itemFlatLocalBound.Contains(flatTouchPoint))
            {
                _isBeingHit = true;
                if (gesture.GestureType == GestureType.SINGLE_TAP)
                {
                    if (edidModeSelectedListener != null)
                    {
                        edidModeSelectedListener(_correspondingMode);
                    }
                }
            }
            else
            {
                _isBeingHit = false;
            }
        }
    }
}
