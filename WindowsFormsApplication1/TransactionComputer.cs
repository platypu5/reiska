using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class TransactionComputer
    {
        double[] security1Ask;
        double[] security2Ask;
        double[] security1Bid;
        double[] security2Bid;
        Dictionary<string, int> pos1;
        Dictionary<string, int> pos2;
        Dictionary<string, string> sec1NameToSec2Name;
        Dictionary<string, string> sec2NameToSec1Name;

        public TransactionComputer(string[] sc1, string[] sc2)
        {
            security1Ask = new double[sc1.Length];
            security1Bid = new double[sc1.Length];
            security2Ask = new double[sc2.Length];
            security2Bid = new double[sc2.Length];
            pos1 = new Dictionary<string, int>();
            pos2 = new Dictionary<string, int>();
            sec1NameToSec2Name = new Dictionary<string, string>();
            sec2NameToSec1Name = new Dictionary<string, string>();
            for(int i=0; i<sc1.Length; i++)
            {
                pos1[sc1[i]] = i;
                pos2[sc2[i]] = i;
                sec1NameToSec2Name[sc1[i]] = sc2[i];
                sec2NameToSec1Name[sc2[i]] = sc1[i];
                security1Ask[i] = -1.0; security1Bid[i] = -1.0;
                security2Ask[i] = -1.0; security2Bid[i] = -1.0;
            }

        }

        public void updateAsk(string security, double ask)
        {
            int pos;
            if (pos1.TryGetValue(security, out pos))
            {
                security1Ask[pos] = ask;
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2Ask[pos] = ask;
            }
        }

        public void updateBid(string security, double bid)
        {
            int pos;
            if (pos1.TryGetValue(security, out pos))
            {
                security1Bid[pos] = bid;
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                security2Bid[pos] = bid;
            }
        }

        public string computeTransaction(string security)
        {
            int pos;
            if (pos1.TryGetValue(security, out pos))
            {
                return security +
                    ", ASK: " + security1Ask[pos].ToString() +
                    ", BID: " + security1Bid[pos].ToString() +
                    "\n" + sec1NameToSec2Name[security] +
                    ", ASK: " + security2Ask[pos].ToString() +
                    ", BID: " + security2Bid[pos].ToString();
            }
            else if (pos2.TryGetValue(security, out pos))
            {
                return security +
                    ", ASK: " + security2Ask[pos].ToString() +
                    ", BID: " + security2Bid[pos].ToString() +
                    "\n" + sec2NameToSec1Name[security] +
                    ", ASK: " + security1Ask[pos].ToString() +
                    ", BID: " + security1Bid[pos].ToString();
            }
            return "";
        }
    }
}
