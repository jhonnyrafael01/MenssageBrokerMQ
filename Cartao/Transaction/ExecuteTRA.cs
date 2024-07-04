using FB.Platform.Entity;
using FB.Platforme.Business;
using FB.Platforme.Business.Interface;
using Newtonsoft.Json;

namespace Cartao.Transaction;

internal class ExecuteTRA : IBaseEventHandler
{
    private IBaseEventHandler _eventHandler;
    private readonly CartaoRepository _cartaoRepository;

    public ExecuteTRA(CartaoRepository cartaoRepository)
    {
        _cartaoRepository = cartaoRepository;
        _eventHandler = this;
    }

    public Task Handle(object payload)
    {
        try
        {
            var propostaCliente = JsonConvert.DeserializeObject<PropostaCliente>(Convert.ToString(payload));

            Cliente cliente = propostaCliente.ClienteDTO;
            CreditoProposta credito = propostaCliente.CreditoDTO;
            CartaoCredito cartao = new CartaoCredito();
            cartao.Id = cliente.Id;
            cartao.Status = 1;

            ValidateCartao(credito, cartao);

            Save(cartao);

        }
        catch (Exception)
        {
            throw;
        }

        return Task.CompletedTask;
    }

    private static void ValidateCartao(CreditoProposta credito, CartaoCredito cartao)
    {
        if (credito.Value >= 10000M)
        {
            cartao.Count = 2;
            cartao.Value = credito.Value / 2;
        }
        else
        {
            cartao.Count = 1;
            cartao.Value = credito.Value;
        }
    }

    private void Save(CartaoCredito cartao)
    {
        _cartaoRepository.Add(cartao);
    }
}
