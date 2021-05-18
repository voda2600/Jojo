using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Models
{
    public class State
    {
        public int Id { get; set; }
        public string Name { get; set; }

     
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int StateId { get; set; }
   
    }
}
