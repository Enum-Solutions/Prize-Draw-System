using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class NewSalaryAccountUser
    {
        public int ID { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public DateTime AccountOpeningDate { get; set; }
        public String AccountOpeningDateStr { get; set; }
        public DateTime LastSalaryTransferDate { get; set; }
        public string LastSalaryTransferDateStr { get; set; }
        public string BranchCode { get; set; }
        public string WinningAmount { get; set; }
    }
}
