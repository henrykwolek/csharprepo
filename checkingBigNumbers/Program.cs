//Autor: Henryk Wołek IIC
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace checkingBigNumbers
{
    class Program
    {
        //Metoda która zamienia daną wartość wejściową string na strukturę BigInteger w systemie dziesiętnym
        static string binaryToDecimal(string InValue)
        {
            BigInteger result = 0;
            for (int i = 0; i < InValue.Length; i++)
            {
                char digit = InValue[i];
                result <<= 1;
                result += digit == '1' ? 1 : 0;
            }
            //Zwraca wartość string
            return result.ToString();
        }
        static void Main(string[] args)
        {
            string line;
            //Deklaracja list
            List<string> stringNumbersBeforeConversion = new List<string>();
            List<BigInteger> extremelyLargeNumbers = new List<BigInteger>();
            BigInteger bigNumber = 0;

            //Czytanie pliku liczby.txt
            System.IO.StreamReader fileRead = new System.IO.StreamReader(@"C:\Users\henry\Desktop\liczby.txt");
            while((line = fileRead.ReadLine()) != null)
            {
                //Na początku program dopisuje do listy każdą liczbę z każdego wiersza jako string
                stringNumbersBeforeConversion.Add(line);
            }

            for (int j = 0; j < stringNumbersBeforeConversion.Count; j++)
            {
                //Gdy poprzednia lista jest zapełniona, ta pętla konwertuje elementy na liczby w systemie dziesiętnym
                bigNumber = BigInteger.Parse(binaryToDecimal(stringNumbersBeforeConversion[j]));
                extremelyLargeNumbers.Add(bigNumber);
            }

            //Zapisanie listy jako tablicy
            BigInteger[] bigIntArr = new BigInteger[1000];
            bigIntArr = extremelyLargeNumbers.ToArray();

            for (int k = 0; k < bigIntArr.Length; k++)
            {
                //Jeśli element w tablicy jest równy największemu elementowi z listy, pętla zakończy się
                if (bigIntArr[k] == extremelyLargeNumbers.Max())
                {
                    //Wyświetlenie wiersza na którym znajduje się dana liczba
                    Console.WriteLine("Największa liczba znajduje się na wierszu {0}", (k + 1).ToString());
                    break; 
                }           
            }
        }
    }
}
