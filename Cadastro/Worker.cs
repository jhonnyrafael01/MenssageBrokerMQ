using FB.Platforme.Business.Interface;
using FB.Platforme.Business;
using FB.Platform.Entity;
using System.Text;
using Newtonsoft.Json;

namespace Cadastro
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBaseEventHandler _eventHandler;
        private readonly ClienteRepository _clienteRepository;

        public Worker(ILogger<Worker> logger, ClienteRepository clienteRepository)
        {
            _logger = logger;
            _clienteRepository = clienteRepository;
            _eventHandler = new ExecuteTRA(_clienteRepository);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            {

                Cliente cliente = new Cliente()
                {
                    Id = 1,
                    Nome = "Jhonny",
                    Idade = 30,
                    CPF = "12345678",
                    Status = (int)StatusCliente.Criado
                };

                var executeTRA = new ExecuteTRA(_clienteRepository);

                var payload = JsonConvert.SerializeObject(cliente);
                var body = Encoding.UTF8.GetBytes(payload);
                var receivedMessage = Encoding.UTF8.GetString(body);
                executeTRA.Handle(receivedMessage).Wait();

                // Consume as filas de erros e atuliza os status do Cliente
                var rabbitMqBus = new RabbitMqBUS(_eventHandler);
                rabbitMqBus.ConsumirFilaFinal(RabbitMqBUS.ErrorQueue, ProcessarMensagemDeErro);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(100, stoppingToken);
            }
        }
        private Task ProcessarMensagemDeErro(string message)
        {
            var propostaCliente = JsonConvert.DeserializeObject<PropostaCliente>(Convert.ToString(message));
            _clienteRepository.Update(propostaCliente.Id);
            return Task.CompletedTask;
        }
    }
}
