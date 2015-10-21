using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

using Name = Bloomberglp.Blpapi.Name;
using SessionOptions = Bloomberglp.Blpapi.SessionOptions;
using Session = Bloomberglp.Blpapi.Session;
using Service = Bloomberglp.Blpapi.Service;
using Request = Bloomberglp.Blpapi.Request;
using Element = Bloomberglp.Blpapi.Element;
using CorrelationID = Bloomberglp.Blpapi.CorrelationID;
using Event = Bloomberglp.Blpapi.Event;
using Message = Bloomberglp.Blpapi.Message;
using EventHandler = Bloomberglp.Blpapi.EventHandler;

namespace WindowsFormsApplication1
{
    class TransactionSender
    {
        private static readonly Name ERROR_INFO = new Name("ErrorInfo");
        private static readonly Name CREATE_ORDER_AND_ROUTE_EX = new Name("CreateOrderAndRouteEx");

        private string d_service;
        private string d_host;
        private int d_port;

        private Session session;
        private Service service;

        private List<string> strategyFields = new List<string>();
        private List<string> strategyData = new List<string>();

        public TransactionSender()
        {
            d_service = "//blp/emapisvc_beta";
            d_host = "localhost";
            d_port = 8194;
        }

        public string startSession()
        {
            string retval = "";
            retval += "\nStarting session...\n";
            SessionOptions d_sessionOptions = new SessionOptions();
            d_sessionOptions.ServerHost = d_host;
            d_sessionOptions.ServerPort = d_port;
            session = new Session(d_sessionOptions);
            if (!session.Start())
            {
                retval += "\nError: failed to start session.\n";
                return retval;
            }
            retval += "\nSession started!\n";
            if (!session.OpenService(d_service))
            {
                session.Stop();
                retval += "\nError: failed to open service.\n";
                return retval;
            }
            service = session.GetService(d_service);
            retval += "\nService opened!\n";
            return retval;
        }

        public string stopSession()
        {
            string retval = "";
            retval += "\nStopping session...\n";
            session.Stop();
            retval += "\nSession stopped.\n";
            return retval;
        }

        public string sendTransaction(
            Transaction t
            )
        {
            double tradeValToday = Lgr.GetTradeValToday();
            double maxEurPerDay = Convert.ToDouble(ConfigurationManager.AppSettings["maxEurPerDay"]);
            double maxEurPerTrade = Convert.ToDouble(ConfigurationManager.AppSettings["maxEurPerTrade"]);
            double sellVal = (t.amountSell * t.priceSell);
            double buyVal = (t.amountBuy * t.priceBuy);

            if (tradeValToday + (t.amountBuy * t.priceBuy) + (t.amountSell * t.priceSell) > maxEurPerDay)
            {
                Lgr.Log("INFO", "Trades value today " + tradeValToday.ToString() +
                    " + Sell value " + sellVal.ToString() +
                    " + Buy value " + buyVal.ToString() + 
                    " > maxEurPerDay " + maxEurPerDay.ToString() +
                    " NOT SENDING TRANSACTION");
                return "";
            }
            if ((t.amountBuy * t.priceBuy) + (t.amountSell * t.priceSell) > maxEurPerTrade)
            {
                Lgr.Log("INFO", "Sell value " + sellVal.ToString() +
                    " + Buy value " + buyVal.ToString() +
                    " > maxEurPerTrade " + maxEurPerTrade.ToString() +
                    " NOT SENDING TRANSACTION");
                return "";
            }

            string retstr = "";

            //Request requestBuy = service.CreateRequest("CreateOrderAndRouteEx");
            Request requestBuy = service.CreateRequest("CreateOrder");
            //requestBuy.Set("EMSX_AMOUNT", t.amountBuy);
            //requestBuy.Set("EMSX_LIMIT_PRICE", (t.priceBuy).ToString());
            requestBuy.Set("EMSX_LIMIT_PRICE", 1.0);
            requestBuy.Set("EMSX_AMOUNT", 1);
            requestBuy.Set("EMSX_BROKER", "NORS"); // BMTB
            requestBuy.Set("EMSX_HAND_INSTRUCTION", "DMA");
            requestBuy.Set("EMSX_ORDER_TYPE", "LMT");
            requestBuy.Set("EMSX_SIDE", "BUY");
            requestBuy.Set("EMSX_TICKER", t.securityBuy);
            requestBuy.Set("EMSX_TIF", "DAY");
            requestBuy.Set("EMSX_ACCOUNT", "LAGOTRAD");

            //Request requestSell = service.CreateRequest("CreateOrderAndRouteEx");
            Request requestSell = service.CreateRequest("CreateOrder");
            //requestSell.Set("EMSX_LIMIT_PRICE", (t.priceSell).ToString());
            requestSell.Set("EMSX_LIMIT_PRICE", 1.0);
            //requestSell.Set("EMSX_AMOUNT", t.amountSell);
            requestSell.Set("EMSX_AMOUNT", 1);
            requestSell.Set("EMSX_BROKER", "NORS"); // BMTB
            requestSell.Set("EMSX_HAND_INSTRUCTION", "DMA");
            requestSell.Set("EMSX_ORDER_TYPE", "LMT");
            requestSell.Set("EMSX_SIDE", "SELL");
            requestSell.Set("EMSX_TICKER", t.securitySell);
            requestSell.Set("EMSX_TIF", "DAY");

            CorrelationID requestID = new CorrelationID("-1111");
            session.SendRequest(requestBuy, requestID);

            int timeoutInMilliSeconds = 5000;

            Event evt = session.NextEvent(timeoutInMilliSeconds);
            do
            {

                retstr += "Received Event: " + evt.Type + "\n";

                foreach (Message msg in evt)
                {
                    retstr += "MESSAGE: " + msg.ToString() + "\n";
                    retstr += "CORRELATION ID: " + msg.CorrelationID + "\n";

                    if (evt.Type == Event.EventType.RESPONSE && msg.CorrelationID == requestID)
                    {
                    }
                }

                evt = session.NextEvent(timeoutInMilliSeconds);


            } while (evt.Type != Event.EventType.TIMEOUT);

            Lgr.Log("DEBUG", retstr);

            requestID = new CorrelationID("-2222");
            session.SendRequest(requestSell, requestID);

            Lgr.WriteTrade(sellVal, " ..... SELL: " + t.securitySell +
                " ..... amount " + t.amountSell + " price " + t.priceSell + " EUR" +
                " curr rate " + t.currencyRateSell.ToString());
            Lgr.WriteTrade(buyVal, " ..... BUY: " + t.securityBuy + 
                " ..... amount " + t.amountBuy + " price " + t.priceBuy + " EUR" +
                " curr rate " + t.currencyRateBuy.ToString());

            return "\nSUCCESFULLY SENT: " + t.toString() + "\n" + retstr;
        }
    }
}
