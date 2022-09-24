using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordsFinder
{
    internal class VlvConverter
    {
        internal static string DecToHex(int decNum)
        {
            return decNum.ToString("X").Length < 2 ? "0" + decNum.ToString("X") : decNum.ToString("X");
        }

        internal static int HexToDec(string hexNum)
        {
            return Convert.ToInt32(hexNum, 16);
        }

        internal static string DecToBin(int decNum)
        {
            /*int largestSmallerPower = 0;
            while (Math.Pow(2, largestSmallerPower + 1) < decNum)
            {
                largestSmallerPower++;
            }

            int[] binary = new int[largestSmallerPower + 1];
            for (int i = 0; decNum > 0; i++)
            {
                binary[i] = decNum % 2;
                decNum = (int)Math.Floor((double)decNum / 2.0);
            }
            Array.Reverse(binary);*/

            return Convert.ToString(decNum, 2);
        }

        internal static int BinToDec(string binNum)
        {
            return Convert.ToInt32(binNum, 2);
        }

        internal static string BinTo7Bin(string binNum)
        {
            int missing0s = (binNum.Length % 7 == 0) ? 0 : 7 - binNum.Length % 7;
            string toBeAppended0s = "";
            for (int i = 0; i < missing0s; i++)
            {
                toBeAppended0s += "0";
            }
            return toBeAppended0s + binNum;
        }

        internal static string[] BinToBinVlv(string binNum)
        {
            string[] binVlv = new string[(int)Math.Floor(binNum.Length / 8.0) + 1];
            char[] charArray = binNum.ToCharArray();
            Array.Reverse(charArray);
            binVlv = charArray.Chunk(size: 7).Select(x => new string(x)).ToArray();
            for (int i = 0; i < binVlv.Length; i++)
            {
                charArray = binVlv[i].ToCharArray();
                Array.Reverse(charArray);
                binVlv[i] = String.Join("" ,charArray);
            }
            Array.Reverse(binVlv);
            for (int i = 0; i < binVlv.Length; i++)
            {
                binVlv[i] = BinTo7Bin(binVlv[i]);
                if (i == binVlv.Length - 1)
                    binVlv[i] = "0" + binVlv[i];
                else
                    binVlv[i] = "1" + binVlv[i];
            }
            return binVlv;
        }

        internal static string BinVlvToHexVlv(string[] binNumArr)
        {
            string hexVlv = "";
            for (int i = 0; i < binNumArr.Length; i++)
            {
                hexVlv += DecToHex(BinToDec(binNumArr[i])) + " ";
            }

            hexVlv = hexVlv.Substring(0, hexVlv.Length - 1);

            return hexVlv;
        }

        internal static string DecToHexVlv(int decNum)
        {
            return BinVlvToHexVlv(BinToBinVlv(DecToBin(decNum)));
        }
    }
}
