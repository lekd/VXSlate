using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Script
{
    public class CriticVar
    {
        object _accessLock;

        public object AccessLock
        {
            get
            {
                return _accessLock;
            }

            set
            {
                _accessLock = value;
            }
        }

        public object CriticData
        {
            get
            {
                return _criticData;
            }

            set
            {
                _criticData = value;
            }
        }

        object _criticData;

        public CriticVar()
        {
            _accessLock = new object();
            _criticData = null;
        }
    }
}
