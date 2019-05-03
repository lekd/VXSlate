using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Assets
{
    [Serializable]
    public class TouchPointerData
    {
        
        int _id;
        public int Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
            }
        }
        
        float _relX;
        public float RelX
        {
            get
            {
                return _relX;
            }

            set
            {
                _relX = value;
            }
        }

        
        float _relY;
        public float RelY
        {
            get
            {
                return _relY;
            }

            set
            {
                _relY = value;
            }
        }

        
        float _relVeloX;
        public float RelVeloX
        {
            get
            {
                return _relVeloX;
            }

            set
            {
                _relVeloX = value;
            }
        }

        float _relVeloY;
        public float RelVeloY
        {
            get
            {
                return _relVeloY;
            }

            set
            {
                _relVeloY = value;
            }
        }
        public void Clone(TouchPointerData source)
        {
            this.Id = source.Id;
            this.RelX = source.RelX;
            this.RelY = source.RelY;
            this.RelVeloX = source.RelVeloX;
            this.RelVeloY = source.RelVeloY;
        }
    }
}
