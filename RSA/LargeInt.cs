using System;
using System.Linq;
using System.Text;

namespace RSA {
public class LargeUInt {
    // the lower index the higher the position
    public byte[] buf;
    public bool isEven => (buf.Last() & 1) != 1;
    
    public LargeUInt(int bytesLength) {
        buf = new byte[bytesLength];
    }
    

    public LargeUInt(LargeUInt other) {
        buf = new byte[other.buf.Length];
        Array.Copy(other.buf, buf, buf.Length);
    }

    public static LargeUInt FromInt(int i) {
        var res = new LargeUInt(sizeof(int));

        for (var j = 0; j < sizeof(int); j++) {
            res.buf[res.buf.Length -  1 - j]  = (byte)(i >> (j*8) & 0xff); 
        }
//        var lolo = (byte)(i & 0xff);
//        var hilo = (byte)((i >> 8) & 0xff);
//        var lohi = (byte)((i >> 16) & 0xff);
//        var hihi = (byte)(i >> 24);
        return res;
    }

    public static LargeUInt operator -(LargeUInt lhs, byte rhs) {
        var result = new LargeUInt(lhs);
        var minByteVal = result.buf[result.buf.Length - 1];

        if (minByteVal < rhs) {
            // borrow
            int tmp = Byte.MaxValue + 1 + minByteVal;
            result.buf[result.buf.Length - 1] = (byte)(tmp - rhs);

            int curIndex = result.buf.Length - 2;
            bool hasToBorrow = true;
            
            while (hasToBorrow) {
                if (result.buf[curIndex] >= 1) {
                    result.buf[curIndex] -= 1;
                    hasToBorrow = false;
                    continue;
                }
                result.buf[curIndex] = Byte.MaxValue;
                curIndex--;
            }
            
        }
        else {
            result.buf[result.buf.Length - 1] -= rhs;
        }

        return result;
    }
    
    public static LargeUInt DivideBy2(LargeUInt number) {
        var result = new LargeUInt(number.buf.Length);
        var curIndex = result.buf.Length - 1;
        
        while (curIndex >= 0) {
            result.buf[curIndex] = (byte)(number.buf[curIndex] >> 1);
            if (curIndex == 0)
                break;

            var higherBit = (byte)(number.buf[curIndex - 1] & 1);
            higherBit = (byte) (higherBit << 7);
            result.buf[curIndex] &= 127;
            result.buf[curIndex] |= higherBit;
            curIndex--;
        }

        return result;
    }

    public void IncreaseLengthTo(int bufLength) {
        var newBuf = new byte[bufLength];
        Array.Copy(buf, 0, newBuf, buf.Length - 1, buf.Length);
        buf = newBuf;
    }

    public string ToString() {
        var strBld = new StringBuilder();
        strBld.EnsureCapacity(2 * buf.Length - 1);
        
        for (int i = 0; i < buf.Length; i++) {
            strBld.Append(buf[i]);
            
            if (i != buf.Length - 1)
                strBld.Append(":");
        }
        return strBld.ToString();
    }
}
}