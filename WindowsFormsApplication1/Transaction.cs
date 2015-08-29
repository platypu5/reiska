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
        public string securitySell;
        public string securityBuy;
        public double priceSell;
        public double priceBuy;
        public int amount;
        public int amount2;
        public double currencyRateSell;
        public double currencyRateBuy;
        public string description;

        public Transaction(bool feas, string sec1, string sec2, double pr1, double pr2,
            int amt, double rate1, double rate2, string err)
        {
            feasible = feas;
            securitySell = sec1;
            securityBuy = sec2;
            priceSell = pr1;
            priceBuy = pr2;
            amount = amt;
            currencyRateSell = rate1;
            currencyRateBuy = rate2;
            description = err;
        }

        public string toString()
        {
            return "\n" +
                "FEASIBLE: " + feasible.ToString() +
                " SELLING " + 
                amount + 
                " OF " + 
                securitySell + 
                " BUYING " + 
                securityBuy + "\n";
        }


        public bool isFeasible()
        {
            return feasible;
        }
    }
}
