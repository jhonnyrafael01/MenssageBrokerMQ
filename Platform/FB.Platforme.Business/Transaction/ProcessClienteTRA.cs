using FB.Platform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FB.Platforme.Business.Transaction
{
    public class ProcessClienteTRA
    {
        public void ProcessCliente(Cliente cliente)
        {
            Console.WriteLine($"Cliente processado com sucesso!\nNome:{cliente.Nome}, Idade: {cliente.Idade}, CPF: {cliente.CPF}");
        }
    }
}
