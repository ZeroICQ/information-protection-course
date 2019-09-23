using System;
using System.Linq;
using System.Text;

namespace RSA {
public class LargeUInt {
    // the lower index the higher the position
    public byte[] buf;
    
    public LargeUInt(uint bytesLength) {
        buf = new byte[bytesLength];
    }

    public LargeUInt(LargeUInt other) {
        buf = new byte[other.buf.Length];
        Array.Copy(other.buf, buf, buf.Length);
    }

    public static LargeUInt operator -(LargeUInt lhs, byte rhs) {
        var result = new LargeUInt(lhs);
        var minByte = result.buf[result.buf.Length - 1];

        if (minByte < rhs) {
            // borrow
            int tmp = Byte.MaxValue + minByte;
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