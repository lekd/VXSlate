using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script
{
    public interface IRemoteController
    {
        string getControllerName();
        void setGestureRecognizedCallback(GestureRecognizedEventCallback gestureRecognizedListener);
        void setModeSwitchedCallback(MenuItemListener.EditModeSelectedCallBack modeChangeListener);
    }
}
