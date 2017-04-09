using Microsoft.eShopOnContainers.Services.Ordering.Domain.Seedwork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.SagaData
{
    public interface ISagaEntity 
    {
        int Id { get; set; }
        int CorrelationId { get; set; }
        string Originator { get; set; }
        bool Completed { get; set; }
        bool Cancelled { get; set; }
    }
}
