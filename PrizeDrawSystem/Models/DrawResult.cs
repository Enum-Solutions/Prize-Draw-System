using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class DrawResult
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public int TotalWinners { get; set; }
        public string DrawType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string CreatedStr { get; set; }
    }
}
