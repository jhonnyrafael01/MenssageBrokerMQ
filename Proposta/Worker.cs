using FB.Platforme.Business.Interface;
using FB.Platforme.Business;
using Proposta.Transaction;

namespace Proposta
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IBaseEventHandler _eventHandler;
        private readonly PropostaCreditoRepository _creditoRepository;

        public Worker(ILogger<Worker> logger, PropostaCreditoRepository creditoRepository)
        {
            _logger = logger;
            _creditoRepository = creditoRepository;
            _eventHandler = new ExecuteTRA(_creditoRepository);
        }
    

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            {
                new RabbitMqBUS(_eventHandler).ConsumirFila(RabbitMqBUS.MainQueue);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
