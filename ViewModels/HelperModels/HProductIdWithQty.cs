using System.Collections.Generic;

namespace ViewModels.HelperModels
{
    public class HProductIdWithQty
    {
        public HProductIdWithQty(string productId, int qty)
        {
            ProductId = productId;
            Qty = qty;
        }

        public readonly string ProductId;
        public readonly int Qty;
    }
}