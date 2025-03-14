using System.Text;

namespace MHiRepacker
{
    public class HexHelper
    {

        public static byte[] CopyByteArr(byte[] src)
        {
            byte[] target = new byte[src.Length];
            //加载数据
            target = new byte[src.Length];
            for (int i = 0; i < src.Length; i++)
                target[i] = src[i];
            return target;
        }

        /// <summary>
        /// 读取byte[]数据
        /// </summary>
        /// <param name="src"></param>
        /// <param name="lenght"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static byte[] ReadBytes(byte[] src, int lenght, int offset = 0)
        {
            byte[] data = new byte[lenght];
            for (int i = 0; i < lenght; i++)
            {
                data[i] = src[offset + i];
            }
            return data;
        }

        /**  
        * byte[]转换int byte高位在前
        */
        public static int bytesToInt(byte[] src,int lenght, int offset = 0)
        {
            if (lenght == 1)
                return src[offset + 0];

            byte[] data = new byte[lenght];
            for (int i = 0; i < lenght; i++)
            {
                data[i] = src[offset + i];
            }

            if(lenght == 2)
                return BitConverter.ToInt16(data, 0);
            else //if (lenght == 4)
                return BitConverter.ToInt32(data, 0);
        }

        /**  
        * byte[]转换int byte高位在前
        */
        public static uint bytesToUInt(byte[] src, int lenght, int offset = 0)
        {
            if (lenght == 1)
                return src[offset + 0];

            byte[] data = new byte[lenght];
            for (int i = 0; i < lenght; i++)
            {
                data[i] = src[offset + i];
            }

            if (lenght == 2)
                return BitConverter.ToUInt16(data, 0);
            else //if (lenght == 4)
                return BitConverter.ToUInt32(data, 0);
        }

        /**  
        * int 转 byte[] byte高位在前
        */
        public static byte[] intToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }
        /**  
        * int 转 byte[] byte高位在前
        */
        public static byte[] uintToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        /**  
        * 从字节读取字符串
        */
        public static string ReadBytesToString(byte[] src, int Start, Encoding encoding = null)
        {
            List<byte> bytes = new List<byte>();

            int index = 0;
            while (true)
            {
                bytes.Add(src[Start + index]);

                if (src[Start + index + 1] == 0x00)
                    break;

                index++;
            }
            if (encoding == null)
                encoding = Encoding.GetEncoding("Shift-JIS");
            string str = encoding.GetString(bytes.ToArray());
            return str;
        }


        /**  
        * 从字节读取字符串
        */
        public static string ReadBytesToString(byte[] src, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.GetEncoding("Shift-JIS");
            string str = encoding.GetString(src.ToArray());
            return str;
        }


        /**  
        * 写入int到byte[] byte高位在前
        */
        public static void ModifyIntHexToBytes(byte[] srcdata, int targetvalue,int startoffset, int srclenght)
        {
            byte[] targetVal = intToBytes(targetvalue);

            //抹去数据
            for (int i = 0; i < srclenght; i++)
                srcdata[startoffset + i] = 0x00;

            for (int i = 0; i < targetVal.Length && i < srclenght; i++)
                srcdata[startoffset + i] = targetVal[i];
        }


        /**  
        * 写入byte[]到byte[] byte高位在前
        */
        public static void ModifyDataToBytes(byte[] srcdata, byte[] targetVal, int startoffset)
        {
            //抹去数据
            for (int i = 0; i < targetVal.Length; i++)
                srcdata[startoffset + i] = 0x00;

            for (int i = 0; i < targetVal.Length && i < targetVal.Length; i++)
                srcdata[startoffset + i] = targetVal[i];
        }



        /**  
        * 对比数据
        */
        public static bool CheckDataEquals(byte[] srcdata, byte[] targetVal, int startoffset)
        {
            byte[] temp = new byte[targetVal.Length];
            for (int i = 0; i < targetVal.Length && i < targetVal.Length; i++)
                temp[i] = srcdata[startoffset + i];

            return Array.Equals(targetVal, temp);
        }

        /// <summary>
        /// 另一种16进制转10进制的处理方式，Multiplier参与*16的循环很巧妙，对Multiplier的处理很推荐，逻辑统一
        /// </summary>
        /// <param name="HexaDecimalString"></param>
        /// <returns></returns>
        public static int HexaToDecimal(string HexaDecimalString)
        {
            int Decimal = 0;
            int Multiplier = 1;

            for (int i = HexaDecimalString.Length - 1; i >= 0; i--)
            {
                Decimal += HexaToDecimal(HexaDecimalString[i]) * Multiplier;
                Multiplier *= 16;
            }
            return Decimal;
        }

        static int HexaToDecimal(char c)
        {
            switch (c)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                case 'A':
                case 'a':
                    return 10;
                case 'B':
                case 'b':
                    return 11;
                case 'C':
                case 'c':
                    return 12;
                case 'D':
                case 'd':
                    return 13;
                case 'E':
                case 'e':
                    return 14;
                case 'F':
                case 'f':
                    return 15;
            }
            return -1;
        }
    }
}
