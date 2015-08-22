using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class Transaction
    {
        public bool feasible;
        public string security1;
        public string security2;
        public double price1;
        public double price2;
        public int amount;
        public double currencyRate1;
        public double currencyRate2;
        public string description;

        public Transaction(bool feas, string sec1, string sec2, double pr1, double pr2,
            int amt, double rate1, double rate2, string err)
        {
            feasible = feas;
            security1 = sec1;
            security2 = sec2;
            price1 = pr1;
            price2 = pr2;
            amount = amt;
            currencyRate1 = rate1;
            currencyRate2 = rate2;
            error = err;
        }

        public bool isFeasible()
        {
            return feasible;
        }
    }
}
