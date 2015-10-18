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
        public int amountBuy;
        public int amountSell;
        public double currencyRateSell;
        public double currencyRateBuy;
        public string description;
        public double arbitrage;
        public double threshold;

        public Transaction(bool feas, string sec1, string sec2, double pr1, double pr2,
            int amt, double rate1, double rate2, double arbi, double thres)
        {
            feasible = feas;
            securitySell = sec1;
            securityBuy = sec2;
            priceSell = pr1;
            priceBuy = pr2;
            amountBuy = amt;
            amountSell = amt;
            currencyRateSell = rate1;
            currencyRateBuy = rate2;
            arbitrage = arbi;
            threshold = thres;
        }

        public string toString()
        {
            return "FEASIBLE: " + feasible.ToString() +
                " SELLING " +
                amountSell.ToString() +
                " OF " +
                securitySell +
                " FOR " +
                priceSell.ToString() +
                ", BUYING " +
                amountBuy.ToString() +
                " OF " +
                securityBuy +
                " FOR " +
                priceBuy.ToString() +
                " ARBITRAGE " +
                arbitrage.ToString() +
                " THRESHOLD " +
                threshold.ToString();
        }


        public bool isFeasible()
        {
            return feasible;
        }
    }
}
