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
                security1Ask[i] = Double.MaxValue / 3.0; security1Bid[i] = -1.0;
                security2Ask[i] = Double.MaxValue / 3.0; security2Bid[i] = -1.0;
                security1AskSize[i] = 0.0; security1BidSize[i] = 0.0;
                security2AskSize[i] = 0.0; security2BidSize[i] = 0.0;
            }

        }

        public string getAllValues()
        {
            string ret = "";
            for (int i = 0; i < security1Ask.Length; i++)
            {
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
                ret += "..________________________________________________________________________.. EUR: " + getCurrencyRateName("EUR") + " _ ";
                ret += "..________________________________________________________________________.. SEK: " + getCurrencyRateName("SEK") + " _ ";
                ret += "..________________________________________________________________________.. SEC1_ASK: " + security1Ask[i].ToString() + " _ ";
                ret += "..________________________________________________________________________.. SEC2_ASK: " + security2Ask[i].ToString() + " _ ";
                ret += "..________________________________________________________________________.. SEC1_BID: " + security1Bid[i].ToString() + " _ ";
                ret += "..________________________________________________________________________.. SEC2_BID: " + security2Bid[i].ToString() + " _ ";
                ret += "..________________________________________________________________________.. SEC1_ASK_SIZE: " + security1AskSize[i].ToString() + " _ ";
                ret += "..________________________________________________________________________.. SEC2_ASK_SIZE: " + security2AskSize[i].ToString() + " _ ";
                ret += "..________________________________________________________________________.. SEC1_BID_SIZE: " + security1BidSize[i].ToString() + " _ ";
                ret += "..________________________________________________________________________.. SEC2_BID_SIZE: " + security2BidSize[i].ToString() + " _ ";
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
                ret += " ========================================================================================================";
            }
            return ret;
        }


        public string updateAskSize(string security, double ask)
        {
            int pos;
            if (pos1.TryGetValue(security, out pos))
            {
                security1AskSize[pos] = ask;
                Lgr.Log("INFO", "Updated ASK SIZE of " + security + " to " + ask.ToString());
                return "\nFOUND SECURITY " + security + " - UPDATED ASK SIZE TO " + ask + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2AskSize[pos] = ask;
                Lgr.Log("INFO", "Updated ASK SIZE of " + security + " to " + ask.ToString());
                return "\nFOUND SECURITY " + security + " - UPDATED ASK SIZE TO " + ask + "\n";
            }
            Lgr.Log("INFO", "Did not find security " + security + " did not update ASK SIZE");
            return "\nDID NOT FIND SECURITY " + security + " - DID NOT UPDATED ASK SIZE" + "\n";
        }

        public string updateBidSize(string security, double bid)
        {
            int pos;
            if (pos1.TryGetValue(security, out pos))
            {
                security1BidSize[pos] = bid;
                Lgr.Log("INFO", "Updated BID SIZE of " + security + " to " + bid.ToString());
                return "\nFOUND SECURITY " + security + " - UPDATED BID SIZE TO " + bid + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2BidSize[pos] = bid;
                Lgr.Log("INFO", "Updated BID SIZE of " + security + " to " + bid.ToString());
                return "\nFOUND SECURITY " + security + " - UPDATED BID SIZE TO " + bid + "\n";
            }
            Lgr.Log("INFO", "Did not find security " + security + " did not update BID SIZE");
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

        private double getCurrencyRateName(string currName)
        {
            double rate = -1.0;
            if (!currencyNameToBase.TryGetValue(currName, out rate))
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
                Lgr.Log("INFO", "Unable to find CURNCY RATE for " + security + " did not update ASK PRICE");
                return "\nUNABLE TO FIND CURRENCY RATE FOR " + security + " - DID NOT UPDATE ASK\n";
            }
            if (pos1.TryGetValue(security, out pos))
            {
                security1Ask[pos] = ask * rate;
                Lgr.Log("INFO", "Updated ASK PRICE of " + security + " to " + Math.Round(security1Ask[pos], 2).ToString() + " = " + ask.ToString() + " * (CURNCY rate)" + rate.ToString());
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED ASK TO " + ask + "*" + rate + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2Ask[pos] = ask * rate;
                Lgr.Log("INFO", "Updated ASK PRICE of " + security + " to " + Math.Round(security2Ask[pos], 2).ToString() + " = " + ask.ToString() + " * (CURNCY rate)" + rate.ToString());
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED ASK TO " + ask + "*" + rate + "\n";
            }
            Lgr.Log("INFO", "Did not find security " + security + " did not update ASK PRICE");
            return "\nUNABLE TO FIND SECURITY " + security + " - DID NOT UPDATE ASK";
        }

        public string updateBid(string security, double bid)
        {
            int pos;
            double rate = getCurrencyRate(security);
            if (rate < 0)
            {
                Lgr.Log("INFO", "Unable to find CURNCY RATE for " + security + " did not update BID PRICE");
                return "\nUNABLE TO FIND CURRENCY RATE FOR " + security + " - DID NOT UPDATE BID\n";
            }
            if (pos1.TryGetValue(security, out pos))
            {
                security1Bid[pos] = bid * rate;
                Lgr.Log("INFO", "Updated BID PRICE of " + security + " to " + Math.Round(security1Bid[pos], 2).ToString() + " = " + bid.ToString() + " * (CURNCY rate)" + rate.ToString());
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED BID TO " + bid + "*" + rate + "\n";
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2Bid[pos] = bid * rate;
                Lgr.Log("INFO", "Updated BID PRICE of " + security + " to " + Math.Round(security2Bid[pos], 2).ToString() + " = " + bid.ToString() + " * (CURNCY rate)" + rate.ToString());
                return "\nFOUND CURRENCY RATE FOR " + security + " - UPDATED BID TO " + bid + "*" + rate + "\n";
            }
            Lgr.Log("INFO", "Did not find security " + security + " did not update BID PRICE");
            return "\nUNABLE TO FIND SECURITY " + security + " - DID NOT UPDATE BID";
        }

        public string updateCurrencyRate(string currency, double rate)
        {
            currencyNameToBase[currency] = rate;
            Lgr.Log("INFO", "Updated rate of currency " + currency + " to " + rate.ToString());
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
                    0, 0.0, 0.0, 0.0, (double)profitThresholdEur);
            }
            if (pos1.TryGetValue(security, out pos))
            {
                int amount = 
                    Math.Min((int)security1BidSize[pos], (int)security2AskSize[pos]);
                double arbitrage = 
                    ( security1Bid[pos] / security1Scale[pos] ) * amount -
                    ( security2Ask[pos] / security2Scale[pos] ) * amount;
                //double arbitrage = 101.0;
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
                        arbitrage, (double)profitThresholdEur
                        );
                }
                else
                {
                    return new Transaction(false, "", "", 0.0, 0.0,
                    0, 0.0, 0.0, 0.0, (double)profitThresholdEur);
                }

            }
            else if (pos2.TryGetValue(security, out pos))
            {
                int amount =
                    Math.Min((int)security2BidSize[pos], (int)security1AskSize[pos]);
                double arbitrage =
                    (security2Bid[pos] / security2Scale[pos]) * amount -
                    (security1Ask[pos] / security1Scale[pos]) * amount;
                //double arbitrage = 101.0;
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
                        arbitrage, (double)profitThresholdEur
                        );
                }
                else
                {
                    return new Transaction(false, "", "", 0.0, 0.0,
                    0, 0.0, 0.0, 0.0, (double)profitThresholdEur);
                }
            }
            return new Transaction(false, "", "", 0.0, 0.0,
                    0, 0.0, 0.0, 0.0, (double)profitThresholdEur);
        }
    }
}
