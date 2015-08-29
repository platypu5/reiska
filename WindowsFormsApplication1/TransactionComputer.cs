using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class TransactionComputer
    {
        double[] security1Ask; // in base currency (EUR)
        double[] security2Ask; // in base currency (EUR)
        double[] security1Bid; // in base currency (EUR)
        double[] security2Bid; // in base currency (EUR)
        double[] security1AskSize;
        double[] security2AskSize;
        double[] security1BidSize;
        double[] security2BidSize;
        double[] security1Scale;
        double[] security2Scale;
        Dictionary<string, int> pos1;
        Dictionary<string, int> pos2;
        Dictionary<string, string> sec1NameToSec2Name;
        Dictionary<string, string> sec2NameToSec1Name;
        Dictionary<string, string> secNameToCurrencyName;
        Dictionary<string, double> currencyNameToBase;
        int profitThresholdEur;


        public TransactionComputer
            (string[] sc1, string[] sc2,
            string[] cur1, string[] cur2,
            string[] scale1, string[] scale2,
            string baseCur, int profitThreshold)
        {
            profitThresholdEur = profitThreshold;
            security1Ask = new double[sc1.Length];
            security1Bid = new double[sc1.Length];
            security2Ask = new double[sc2.Length];
            security2Bid = new double[sc2.Length];
            security1AskSize = new double[sc1.Length];
            security1BidSize = new double[sc1.Length];
            security2AskSize = new double[sc2.Length];
            security1Scale = new double[sc1.Length];
            security2Scale = new double[sc2.Length];
            security2BidSize = new double[sc2.Length];
            pos1 = new Dictionary<string, int>();
            pos2 = new Dictionary<string, int>();
            secNameToCurrencyName = new Dictionary<string, string>();
            currencyNameToBase = new Dictionary<string, double>();
            sec1NameToSec2Name = new Dictionary<string, string>();
            sec2NameToSec1Name = new Dictionary<string, string>();
            for (int i = 0; i < cur1.Length; i++)
            {
                secNameToCurrencyName[sc1[i]] = cur1[i];
            }
            for (int i = 0; i < cur2.Length; i++)
            {
                secNameToCurrencyName[sc2[i]] = cur2[i];
            }
            currencyNameToBase[baseCur] = 1.0;
            for (int i = 0; i < sc1.Length; i++)
            {
                security1Scale[i] = 1.0; // FIXME
                security2Scale[i] = 1.0; // FIXME
            }
            for (int i=0; i<sc1.Length; i++)
            {
                pos1[sc1[i]] = i;
                pos2[sc2[i]] = i;
                sec1NameToSec2Name[sc1[i]] = sc2[i];
                sec2NameToSec1Name[sc2[i]] = sc1[i];
                security1Ask[i] = -1.0; security1Bid[i] = -1.0;
                security2Ask[i] = -1.0; security2Bid[i] = -1.0;
                security1AskSize[i] = 0.0; security1BidSize[i] = 0.0;
                security2AskSize[i] = 0.0; security2BidSize[i] = 0.0;
            }

        }

        public string updateAskSize(string security, double ask)
        {
            int pos;
            if (pos1.TryGetValue(security, out pos))
            {
                security1AskSize[pos] = ask;
                return "\nFOUND SECURITY " + security + " - UPDATED ASK SIZE TO " + ask + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2AskSize[pos] = ask;
                return "\nFOUND SECURITY " + security + " - UPDATED ASK SIZE TO " + ask + "\n";
            }
            return "\nDID NOT FIND SECURITY " + security + " - DID NOT UPDATED ASK SIZE" + "\n";
        }

        public string updateBidSize(string security, double bid)
        {
            int pos;
            if (pos1.TryGetValue(security, out pos))
            {
                security1BidSize[pos] = bid;
                return "\nFOUND SECURITY " + security + " - UPDATED BID SIZE TO " + bid + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2BidSize[pos] = bid;
                return "\nFOUND SECURITY " + security + " - UPDATED BID SIZE TO " + bid + "\n";
            }
            return "\nDID NOT FIND SECURITY " + security + " - DID NOT UPDATED BID SIZE" + "\n";
        }

        private double getCurrencyRate(string security)
        {
            double rate = -1.0;
            string currName;
            if (! secNameToCurrencyName.TryGetValue(security, out currName))
            {
                return rate;
            }
            if (! currencyNameToBase.TryGetValue(currName, out rate))
            {
                return rate;
            }
            return rate;
        }

        public string updateAsk(string security, double ask)
        {
            int pos;
            double rate = getCurrencyRate(security);
            if (rate < 0)
            {
                return "\nUNABLE TO FIND CURRENCY RATE FOR " + security + " - DID NOT UPDATE ASK\n";
            }
            if (pos1.TryGetValue(security, out pos))
            {
                security1Ask[pos] = ask * rate;
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED ASK TO " + ask + "*" + rate + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2Ask[pos] = ask * rate;
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED ASK TO " + ask + "*" + rate + "\n";
            }
            return "\nUNABLE TO FIND SECURITY " + security + " - DID NOT UPDATE ASK";
        }

        public string updateBid(string security, double bid)
        {
            int pos;
            double rate = getCurrencyRate(security);
            if (rate < 0)
            {
                return "\nUNABLE TO FIND CURRENCY RATE FOR " + security + " - DID NOT UPDATE BID\n";
            }
            if (pos1.TryGetValue(security, out pos))
            {
                security1Bid[pos] = bid * rate;
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED BID TO " + bid + "*" + rate + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2Bid[pos] = bid * rate;
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED BID TO " + bid + "*" + rate + "\n";
            }
            return "\nUNABLE TO FIND SECURITY " + security + " - DID NOT UPDATE BID";
        }

        public string updateCurrencyRate(string currency, double rate)
        {
            currencyNameToBase[currency] = rate;
            return "\nUPDATED RATE OF CURRENCY " + currency + " TO " + rate + "\n";

        }

        private bool ratesExist(string security)
        {
            int pos;
            string security2;
            double rate1;
            double rate2;
            if (pos1.TryGetValue(security, out pos))
            {
                security2 = sec1NameToSec2Name[security];
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2 = sec2NameToSec1Name[security];
            }
            else
            {
                return false; // incorrect security name
            }
            if (
                currencyNameToBase.TryGetValue(secNameToCurrencyName[security], out rate1)
                &&
                currencyNameToBase.TryGetValue(secNameToCurrencyName[security2], out rate2)
                )
            {
                return true; // found currency rates for both securities
            }
            return false; // did not find currency rate for one or both of the securities
        }

        public Transaction computeTransaction(string security)
        {
            int pos;
            if (!ratesExist(security))
            {
                return new Transaction(false, "", "", 0.0, 0.0,
                    0, 0.0, 0.0, "\nNO CURRENCY RATE FOUND FOR ONE OF THE SECURITIES\n");
            }
            if (pos1.TryGetValue(security, out pos))
            {
                int amount = 
                    Math.Min((int)security1BidSize[pos], (int)security2AskSize[pos]);
                //double arbitrage = 
                //    ( security1Bid[pos] / security1Scale[pos] ) * amount -
                //    ( security2Ask[pos] / security2Scale[pos] ) * amount;
                double arbitrage = 101.0;
                if ( arbitrage > (double)(profitThresholdEur) )
                {
                    return new Transaction(
                        true,
                        security,
                        sec1NameToSec2Name[security],
                        security1Bid[pos],
                        security2Ask[pos],
                        amount,
                        currencyNameToBase[secNameToCurrencyName[security]],
                        currencyNameToBase[secNameToCurrencyName[sec1NameToSec2Name[security]]],
                        "FEASIBLE TRANSACTION:\n" +
                        security +
                        ", BID: " + security1Bid[pos].ToString() +
                        ", BIDSIZE: " + security1BidSize[pos].ToString() +
                        "\n-exceeds threshold by arbitrage- " + arbitrage + "\n" +
                        sec1NameToSec2Name[security] +
                        ", ASK: " + security2Ask[pos].ToString() +
                        ", ASKSIZE: " + security2AskSize[pos].ToString()
                        );
                }
                else
                {
                    return new Transaction(false, "", "", 0.0, 0.0,
                    0, 0.0, 0.0, "\nNO FEASIBLE TRANSACTION FOR: " +
                        security + " <-> " + sec1NameToSec2Name[security] + "\n");
                }

            }
            else if (pos2.TryGetValue(security, out pos))
            {
                int amount =
                    Math.Min((int)security2BidSize[pos], (int)security1AskSize[pos]);
                //double arbitrage =
                //    (security2Bid[pos] / security2Scale[pos]) * amount -
                //    (security1Ask[pos] / security1Scale[pos]) * amount;
                double arbitrage = 101.0;
                if ( arbitrage > (double)(profitThresholdEur) )
                {
                    return new Transaction(
                        true,
                        security,
                        sec2NameToSec1Name[security],
                        security2Bid[pos],
                        security1Ask[pos],
                        amount,
                        currencyNameToBase[secNameToCurrencyName[security]],
                        currencyNameToBase[secNameToCurrencyName[sec2NameToSec1Name[security]]],
                        "FEASIBLE TRANSACTION:\n" +
                        security +
                        ", BID: " + security2Bid[pos].ToString() +
                        ", BIDSIZE: " + security2BidSize[pos].ToString() +
                        "\n-exceeds threshold by arbitrage- " + arbitrage + "\n" +
                        sec2NameToSec1Name[security] +
                        ", ASK: " + security1Ask[pos].ToString() +
                        ", ASKSIZE: " + security1AskSize[pos].ToString()
                        );
                }
                else
                {
                    return new Transaction(false, "", "", 0.0, 0.0,
                    0, 0.0, 0.0, "\nNO FEASIBLE TRANSACTION FOR: " +
                        security + " <-> " + sec2NameToSec1Name[security] + "\n");
                }
            }
            return new Transaction(false, "", "", 0.0, 0.0,
                    0, 0.0, 0.0, "\nNO TRANSACTION\n");
        }
    }
}
