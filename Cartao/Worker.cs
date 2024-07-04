using Cartao.Transaction;
using FB.Platforme.Business;
using FB.Platforme.Business.Interface;

namespace Cartao
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IBaseEventHandler _eventHandler;
        private readonly CartaoRepository _cartaoRepository;

        public Worker(ILogger<Worker> logger, CartaoRepository cartaoRepository)
        {
            _logger = logger;
            _cartaoRepository = cartaoRepository;
            _eventHandler = new ExecuteTRA(_cartaoRepository);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            {
                new RabbitMqBUS(_eventHandler).ConsumirFila(RabbitMqBUS.CreditoQueue);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
