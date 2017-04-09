using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.SagaData
{
    public class OrderSagaData : ISagaEntity
    {
        public int Id { get; set; }
        public int CorrelationId { get; set; }
        public string Originator { get; set; }
        public bool IsPaymentDone { get; set; }
        public bool IsStockProvided { get; set; }    
        public bool Completed { get; set; }
        public bool Cancelled { get; set; }
    }
}
