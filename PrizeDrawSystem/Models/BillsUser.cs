using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class BillsUser
    {
        public int ID { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public String TransactionDateStr { get; set; }
        public string TransactionCategory { get; set; }
        public double TransactionAmount { get; set; }
        public string Customer { get; set; }
        public string WinningAmount { get; set; }
    }
}
