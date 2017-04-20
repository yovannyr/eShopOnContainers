using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMVC.ViewModels
{
    public class OrderProcessAction
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        
        public static OrderProcessAction CheckStock = new OrderProcessAction(nameof(CheckStock).ToLowerInvariant(), "Check Stock");
        public static OrderProcessAction RecordPayment = new OrderProcessAction(nameof(RecordPayment).ToLowerInvariant(), "Record Payment");
        public static OrderProcessAction Ship = new OrderProcessAction(nameof(Ship).ToLowerInvariant(), "Ship");
        public static OrderProcessAction Refund = new OrderProcessAction(nameof(Refund).ToLowerInvariant(), "Refund");
        public static OrderProcessAction Cancel = new OrderProcessAction(nameof(Cancel).ToLowerInvariant(), "Cancel");
        public static OrderProcessAction Complete = new OrderProcessAction(nameof(Complete).ToLowerInvariant(), "Complete");

        protected OrderProcessAction()
        {
        }

        public OrderProcessAction(string code, string name)
        {
            Code = code;
            Name = name;
        }       
    }
}
