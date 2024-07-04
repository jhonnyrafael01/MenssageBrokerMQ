using FB.Platform.Entity;
using FB.Platforme.Business;
using FB.Platforme.Business.Interface;
using Newtonsoft.Json;

namespace Proposta.Transaction;


internal class ExecuteTRA : IBaseEventHandler
{
    private IBaseEventHandler _eventHandler;
    private readonly PropostaCreditoRepository _creditoRepository;

    public ExecuteTRA(PropostaCreditoRepository creditoRepository)
    {
        _creditoRepository = creditoRepository;
        _eventHandler = this;
    }

    public Task Handle(object payload)
    {
        try
        {
            CreditoProposta creditoProposta = new CreditoProposta();

            var cliente = JsonConvert.DeserializeObject<Cliente>(Convert.ToString(payload));

            ValidateCredito(creditoProposta, cliente);

            var propostaCliente = new PropostaCliente { Id = cliente.Id, Status = 1, ClienteDTO = cliente, CreditoDTO = creditoProposta };
            _creditoRepository.Add(propostaCliente);

            var message = JsonConvert.SerializeObject(propostaCliente);

            if (cliente.Status == (int)StatusCliente.CreditoAprovado)
                new RabbitMqBUS(_eventHandler).PublicarFila(message, RabbitMqBUS.CreditoQueue);
            else
                new RabbitMqBUS(_eventHandler).PublicarFila(message, RabbitMqBUS.ErrorQueue);
        }
        catch (Exception)
        {
            throw;
        }

        return Task.CompletedTask;
    }

    private static void ValidateCredito(CreditoProposta creditoProposta, Cliente? cliente)
    {
        if (cliente.Idade >= 18 && cliente.Idade <= 21)
            creditoProposta.Value = 1000M;
        else if (cliente.Idade > 21 && cliente.Idade < 30)
            creditoProposta.Value = 5000M;
        else if (cliente.Idade >= 30 && cliente.Idade < 40)
            creditoProposta.Value = 10000M;
        else if (cliente.Idade >= 40)
            creditoProposta.Value = 15000M;
        else
        {
            creditoProposta.Status = "2";

            cliente.Status = (int)StatusCliente.NegadoCredito;
            creditoProposta.Status = "Negado";
            return;
        }
        creditoProposta.Status = "1";
        cliente.Status = (int)StatusCliente.CreditoAprovado;
    }
}
