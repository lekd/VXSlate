using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script
{
    public class VirtualPadManager
    {
        public enum VirtualPadState { OBJECT_MANIP, DRAW, MENU_SELECTION}

        VirtualPadState _currentState;

        public VirtualPadState CurrentState
        {
            get
            {
                return _currentState;
            }

            set
            {
                _currentState = value;
            }
        }

        GameObject menuManip;
        GameObject menuDraw;
        Rect menuManipLocalBound;
        Rect menuDrawLocalBound;
        GameObject _virtualPad;
        Material[] menuMaterials = new Material[4];
        public VirtualPadManager(GameObject virtualPad)
        {
            _virtualPad = virtualPad;
            _currentState = VirtualPadState.OBJECT_MANIP;
            menuManip = GameObject.Find("Menu_Manip");
            menuDraw = GameObject.Find("Menu_Draw");
            Bounds menuManipWorldBound = menuManip.GetComponent<Collider>().bounds;
            Vector3 localMin3D = _virtualPad.transform.InverseTransformPoint(menuManipWorldBound.min);
            Vector3 localMax3D = _virtualPad.transform.InverseTransformPoint(menuManipWorldBound.max);
            menuManipLocalBound = new Rect(new Vector2(localMin3D.x, localMin3D.y), new Vector2(localMax3D.x - localMin3D.x, localMax3D.y - localMin3D.y));
            Bounds menuDrawWorldBound = menuDraw.GetComponent<Collider>().bounds;
            localMin3D = _virtualPad.transform.InverseTransformPoint(menuDrawWorldBound.min);
            localMax3D = _virtualPad.transform.InverseTransformPoint(menuDrawWorldBound.max);
            menuDrawLocalBound = new Rect(new Vector2(localMin3D.x, localMin3D.y), new Vector2(localMax3D.x - localMin3D.x, localMax3D.y - localMin3D.y));

            setMenuItemActive(false);
            menuMaterials[0] = Resources.Load("/Materials/ObjManipMenuNormalMat", typeof(Material)) as Material;
            menuMaterials[1] = Resources.Load("/Materials/ObjManipMenuPressedMat", typeof(Material)) as Material;
            menuMaterials[2] = Resources.Load("/Materials/DrawMenuNormalMat", typeof(Material)) as Material;
            menuMaterials[3] = Resources.Load("/Materials/DrawMenuPressedMat", typeof(Material)) as Material;
        }
        void setMenuItemActive(bool isActive)
        {
            menuManip.SetActive(isActive);
            menuDraw.SetActive(isActive);
        }
        public void ReactToTouchGesture(TouchGestureRecognizer.TouchGesture touchGesture)
        {
            if (touchGesture == null)
            {
                return;
            }
            if (_currentState == VirtualPadState.OBJECT_MANIP)
            {
                processGestureInObjectManipMode(touchGesture);
            }
            else if(_currentState == VirtualPadState.DRAW)
            {
                processGestureInDrawMode(touchGesture);
            }
            else
            {
                processGestureInMenuSelection(touchGesture);
            }
        }
        void processGestureInObjectManipMode(TouchGestureRecognizer.TouchGesture touchGesture)
        {
            if(touchGesture.GestureType == TouchGestureRecognizer.TouchGestureType.FIVE_POINTERS)
            {
                setMenuItemActive(true);
                _currentState = VirtualPadState.MENU_SELECTION;
            }
        }
        void processGestureInDrawMode(TouchGestureRecognizer.TouchGesture touchGesture)
        {
            if (touchGesture.GestureType == TouchGestureRecognizer.TouchGestureType.FIVE_POINTERS)
            {
                setMenuItemActive(true);
                _currentState = VirtualPadState.MENU_SELECTION;
            }
        }
        void processGestureInMenuSelection(TouchGestureRecognizer.TouchGesture touchGesture)
        {
            if(touchGesture.GestureType == TouchGestureRecognizer.TouchGestureType.SINGLE_TAP)
            {
                Vector2 rawLocalTapPos = (Vector2)touchGesture.MetaData;
                Vector2 adjustedLocalTapPos = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(rawLocalTapPos);
                if(menuManipLocalBound.Contains(adjustedLocalTapPos))
                {
                    setMenuItemActive(false);
                    _currentState = VirtualPadState.OBJECT_MANIP;
                }
                else if(menuDrawLocalBound.Contains(adjustedLocalTapPos))
                {
                    setMenuItemActive(false);
                    _currentState = VirtualPadState.DRAW;
                }
                
            }
        }
    }
}
