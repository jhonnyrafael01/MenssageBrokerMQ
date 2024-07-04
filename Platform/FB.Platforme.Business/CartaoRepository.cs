using FB.Platform.Entity;
using MongoDB.Driver;

namespace FB.Platforme.Business
{
    public class CartaoRepository
    {
        private readonly IMongoCollection<CartaoCredito> _cartao;

        public CartaoRepository(MongoDbContext context)
        {
            _cartao = context.CartaoCredito;
        }
        public void Add(CartaoCredito cartao)
        {
            try
            {
                _cartao.InsertOne(cartao);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Update(CartaoCredito cartao)
        {
            try
            {
                var filter = Builders<CartaoCredito>.Filter.Eq(x => x.Id, cartao.Id);
                var update = Builders<CartaoCredito>.Update.Set(x => x.Status, cartao.Status);

                _cartao.UpdateOne(filter, update);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
