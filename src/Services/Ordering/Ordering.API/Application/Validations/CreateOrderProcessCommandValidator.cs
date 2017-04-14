using FluentValidation;
using Ordering.API.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ordering.API.Application.Commands.StartOrderProcessCommand;

namespace Ordering.API.Application.Validations
{
    public class CreateOrderProcessCommandValidator : AbstractValidator<StartOrderProcessCommand>
    {
        public CreateOrderProcessCommandValidator()
        {
            RuleFor(order => order.OrderNumber).NotEmpty();
            RuleFor(order => order.OrderItems).Must(ContainOrderItems).WithMessage("No order items found");
        }

        private bool ContainOrderItems(IEnumerable<OrderItemDTO> orderItems)
        {
            return orderItems.Any();
        }
    }
}
