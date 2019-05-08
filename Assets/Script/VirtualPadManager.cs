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

        GameObject _menuManip;
        GameObject _menuDraw;
        Rect menuManipLocalBound;
        Rect menuDrawLocalBound;
        GameObject _virtualPad;
        Material[] menuMaterials = new Material[4];
        public VirtualPadManager(GameObject virtualPad,GameObject menuManip,GameObject menuDraw)
        {
            _virtualPad = virtualPad;
            _currentState = VirtualPadState.OBJECT_MANIP;
            _menuManip = menuManip;
            _menuDraw = menuDraw;
            Bounds menuManipWorldBound = menuManip.GetComponent<Collider>().bounds;
            Vector3 localMin3D = _virtualPad.transform.InverseTransformPoint(menuManipWorldBound.min);
            Vector3 localMax3D = _virtualPad.transform.InverseTransformPoint(menuManipWorldBound.max);
            menuManipLocalBound = new Rect(new Vector2(localMin3D.x, localMin3D.y), new Vector2(localMax3D.x - localMin3D.x, localMax3D.y - localMin3D.y));
            Bounds menuDrawWorldBound = menuDraw.GetComponent<Collider>().bounds;
            localMin3D = _virtualPad.transform.InverseTransformPoint(menuDrawWorldBound.min);
            localMax3D = _virtualPad.transform.InverseTransformPoint(menuDrawWorldBound.max);
            menuDrawLocalBound = new Rect(new Vector2(localMin3D.x, localMin3D.y), new Vector2(localMax3D.x - localMin3D.x, localMax3D.y - localMin3D.y));
            
            setMenuItemActive(false);
        }
        public void setMenuMaterials(Material mManipNormal,Material mManipPressed,Material mDrawNormal,Material mDrawPressed)
        {
            menuMaterials[0] = mManipNormal;
            menuMaterials[1] = mManipPressed;
            menuMaterials[2] = mDrawNormal;
            menuMaterials[3] = mDrawPressed;
        }
        void setMenuItemActive(bool isActive)
        {
            /*
            if(menuManip != null)
            {
                menuManip.SetActive(isActive);
            }
            if(menuDraw != null)
            {
                menuDraw.SetActive(isActive);
            }
            */
            try
            {
                _menuManip.GetComponent<MeshRenderer>().material = menuMaterials[0];
                _menuDraw.GetComponent<MeshRenderer>().material = menuMaterials[2];
                _menuManip.SetActive(isActive);
                _menuDraw.SetActive(isActive);
            }
            catch (Exception ex)
            {
                Debug.Log("Error setting menu activeness: " + ex.Message);
            }
            //menuManip.SetActive(isActive);
            //menuDraw.SetActive(isActive);
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
            if(touchGesture.GestureType == TouchGestureRecognizer.TouchGestureType.SINGLE_TOUCH_DOWN)
            {
                TouchPointerData touchData = (TouchPointerData)touchGesture.MetaData;
                Vector2 rawLocalTapPos = new Vector2(touchData.RelX, touchData.RelY);
                Vector2 adjustedLocalTapPos = GlobalUtilities.ConvertMobileRelPosToUnityRelPos(rawLocalTapPos);
                if (menuManipLocalBound.Contains(adjustedLocalTapPos))
                {
                    _menuManip.GetComponent<MeshRenderer>().material = menuMaterials[1];
                }
                else if (menuDrawLocalBound.Contains(adjustedLocalTapPos))
                {
                    _menuDraw.GetComponent<MeshRenderer>().material = menuMaterials[3];
                }
            }
            else if(touchGesture.GestureType == TouchGestureRecognizer.TouchGestureType.SINGLE_TAP)
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
