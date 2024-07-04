using FB.Platform.Entity;
using MongoDB.Driver;

namespace FB.Platforme.Business
{
    public class PropostaCreditoRepository
    {
        private readonly IMongoCollection<PropostaCliente> _propostaCliente;

        public PropostaCreditoRepository(MongoDbContext context)
        {
            _propostaCliente = context.PropostaCliente;
        }
        public void Add(PropostaCliente propostaCliente)
        {
            try
            {
                _propostaCliente.InsertOne(propostaCliente);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Update(PropostaCliente propostaCliente)
        {
            try
            {
                var filter = Builders<PropostaCliente>.Filter.Eq(x => x.Id, propostaCliente.Id);
                var update = Builders<PropostaCliente>.Update.Set(x => x.Status, propostaCliente.Status);

                _propostaCliente.UpdateOne(filter, update);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
