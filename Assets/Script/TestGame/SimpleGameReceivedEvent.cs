using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script.TestGame
{
    public class SimpleGameReceivedEvent
    {
        TouchGesture _receivedGesture;
        DateTime _timeStamp;

        public TouchGesture ReceivedGesture { get => _receivedGesture; set => _receivedGesture = value; }
        public DateTime TimeStamp { get => _timeStamp; set => _timeStamp = value; }

        public SimpleGameReceivedEvent()
        {
            _receivedGesture = null;
        }
    }
}
