using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script
{
    public delegate void GestureRecognizedEventCallback(TouchGestureRecognizer.TouchGesture recognizedGesture);
    public delegate void PointerReceivedEventCallback(TouchEventData touchEvent);
    public interface IGeneralPointerEventListener
    {
        void SetGestureRecognizedListener(GestureRecognizedEventCallback eventRecognizedListener);
        CriticVar getCurrentAvaiPointers();
    }
}
