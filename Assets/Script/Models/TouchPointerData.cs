using Assets.Script;
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
        public static TouchPointerData Create(TouchPointerData source)
        {
            TouchPointerData newPointer = new TouchPointerData();
            newPointer.Clone(source);
            return newPointer;
        }
        public string toString()
        {
            string str = string.Format("ID:{0},VelX:{1},VelY:{2},RelVelX:{3},RelVelY:{4}",
                                        _id, _relX, _relY, _relVeloX, _relVeloY);
            return str;
        }
        public const int BYTE_SIZE = 20;
        public static TouchPointerData parseFromBytes(byte[] byteData)
        {
            byte[] buffer = new byte[4];
            TouchPointerData pointer = new TouchPointerData();
            int offset = 0;
            Array.Copy(byteData, offset, buffer, 0, 4);
            pointer.Id = GlobalUtilities.ByteArray2Int(buffer);
            offset += 4;
            Array.Copy(byteData, offset, buffer, 0, 4);
            pointer.RelX = GlobalUtilities.ByteArray2Float(buffer);
            offset += 4;
            Array.Copy(byteData, offset, buffer, 0, 4);
            pointer.RelY = GlobalUtilities.ByteArray2Float(buffer);
            offset += 4;
            Array.Copy(byteData, offset, buffer, 0, 4);
            pointer.RelVeloX = GlobalUtilities.ByteArray2Float(buffer);
            offset += 4;
            Array.Copy(byteData, offset, buffer, 0, 4);
            pointer.RelVeloY = GlobalUtilities.ByteArray2Float(buffer);
            offset += 4;
            return pointer;
        }
    }
}
