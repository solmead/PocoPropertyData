using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.PocoProperties;

namespace ConsoleAppFramework
{
    class Program
    {
        static void Main(string[] args)
        {

            var t = new TestMe();
            t.a = "Test";
            t.b = "Me";

            var c = t.Clone();

            Console.WriteLine(c.a);
            Console.WriteLine(c.b);

        }
    }
}
