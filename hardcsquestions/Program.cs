using System;
using System.Collections.Generic;

namespace hardcsquestions
{
    class Program
    {
        internal static void reverseString(string inStr)
        {
            char[] inStrArr = inStr.ToCharArray();
            for (int i = 0, j = inStrArr.Length - 1; i < j; i++, j--)
            {
                inStrArr[i] = inStrArr[j];
                inStrArr[j] = inStrArr[i];
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Give me a string\n");
            string userInputStr = Console.ReadLine();
            Console.WriteLine(reverseString(userInputStr));
        }
    }
}
