using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FB.Platform.Entity
{
    public class CartaoCredito
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public decimal Value { get; set; }
        public int Status { get; set; }
    }
}
