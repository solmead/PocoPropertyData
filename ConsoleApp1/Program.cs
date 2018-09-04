using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PocoPropertyData;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {


            var t = new TestClass1
            {
                Test = "1",
                Test2 = "2",
                Test3 = "3"
            };


            var v = t.GetValue("My Test");

            //t.SetPropertyOn("TC.Name", "Test Sub Class");


            t.SetPropertyOn("TCName", "Test Sub Class 2", new Dictionary<string, string>
            {
                {"TCName","TC.Name" }
            });

            Debug.WriteLine(v);

        }
    }
}
