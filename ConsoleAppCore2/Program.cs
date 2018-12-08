using PocoPropertyData;
using System;

namespace ConsoleAppCore2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            var t = new TestMe();
            t.a = "Test";
            t.b = "Me";

            var c = t.Clone();

            Console.WriteLine(c.a);
            Console.WriteLine(c.b);
        }
    }
}
