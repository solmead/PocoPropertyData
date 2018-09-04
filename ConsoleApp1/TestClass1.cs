using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class TestClass1
    {
        public string Name { get; set; }

        [UIHint("MultilineText")]
        public string Test { get; set; }


        [Display(Name="My Test")]
        public string Test2 { get; set; }


        [Column("MyTest3")]
        public string Test3 { get; set; }


        public TestClass2 TC { get; set; }

    }
}
