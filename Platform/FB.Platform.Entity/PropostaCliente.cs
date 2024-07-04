using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FB.Platform.Entity
{
    public class PropostaCliente
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public Cliente ClienteDTO { get; set; }
        public CreditoProposta CreditoDTO { get; set; }
    }
}
