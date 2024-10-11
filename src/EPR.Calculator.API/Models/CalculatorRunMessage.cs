using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.API.Models
{
    public class CalculatorRunMessage
    {
        public required int CalculatorRunId { get; set; }

        public required string FinancialYear { get; set; }

        public required string CreatedBy { get; set; }
    }
}