using Silky.Rpc.Transport.CachingIntercept;

namespace Silky.Stock.Application.Contracts.Products.Dtos
{
    public class DeductStockInput
    {
        [CacheKey(0)]public long ProductId { get; set; }
        public int Quantity { get; set; }
    }
}