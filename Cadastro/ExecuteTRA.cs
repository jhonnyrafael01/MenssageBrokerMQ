using FB.Platform.Entity;
using FB.Platforme.Business;
using FB.Platforme.Business.Interface;
using FB.Platforme.Business.Transaction;
using Newtonsoft.Json;

namespace Cadastro;


internal class ExecuteTRA : IBaseEventHandler
{
    private readonly IBaseEventHandler _eventHandler;
    private readonly ClienteRepository _clienteRepository;

    public ExecuteTRA(ClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
        _eventHandler = this;
    }

    public Task Handle(object payload)
    {
        try
        {
            var cliente = JsonConvert.DeserializeObject<Cliente>(Convert.ToString(payload));

            if (cliente != null)
            {
                _clienteRepository.Add(cliente); // Salva o cliente no MongoDB.
                new ProcessClienteTRA().ProcessCliente(cliente);
            }

            var message = JsonConvert.SerializeObject(cliente);
            new RabbitMqBUS(_eventHandler).PublicarFila(message, RabbitMqBUS.MainQueue);
        }
        catch (Exception)
        {
            throw;
        }

        return Task.CompletedTask;
    }
}
