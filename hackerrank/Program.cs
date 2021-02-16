using System;

namespace hackerrank
{
    class result
    {
        public static void fizzBuzz(int n)
        {
            for (int i = 1; i < n + 1; i++)
            {
                if (i % 3 == 0 && i % 5 == 0)
                {
                    Console.WriteLine("FizzBuzz");
                }
                else if (i % 3 == 0 && i % 5 != 0)
                {
                    Console.WriteLine("Fizz");
                }
                else if (i % 3 != 0 && i % 5 == 0)
                {
                    Console.WriteLine("Buzz");
                }
                else
                {
                    Console.WriteLine(i.ToString());
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int n = Convert.ToInt32(Console.ReadLine().Trim());

            result.fizzBuzz(n);
        }
    }
}
