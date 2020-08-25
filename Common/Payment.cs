using System;

namespace Common
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Payee { get; set; }
        public string AccountNumber { get; set; }
    }
}
