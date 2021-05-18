using System.Threading.Tasks;

namespace B3I_Market.Services
{
    public class PaymentService : IPaymenetService
    {
        public async void Pay()
        {
            await Task.Delay(300);
        }
    }
}
