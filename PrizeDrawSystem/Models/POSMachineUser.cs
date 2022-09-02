using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class POSMachineUser
    {
        public int ID { get; set; }
        public string CreditCardNumber { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public double POSTransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string BranchCode { get; set; }
        public string WinningAmount { get; set; }
        public int Chances { get; set; }
    }
}
