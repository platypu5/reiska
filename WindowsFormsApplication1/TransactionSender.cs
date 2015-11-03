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

        Dictionary<int, Request> unfilledOrders = new Dictionary<int, Request>();
        Dictionary<int, Transaction> unfilledTransactions = new Dictionary<int, Transaction>();

        Random rnd = new Random();

        private string d_service;
        private string d_host;
        private int d_port;

        private Session session;
        private Service service;

        private List<string> strategyFields = new List<string>();
        private List<string> strategyData = new List<string>();

        public TransactionSender()
        {
            d_service = "//blp/emapisvc";
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

        private int checkOneFill(int key)
        {
            Request request = service.CreateRequest("OrderInfo");
            request.Set("EMSX_SEQUENCE", key);

            CorrelationID requestID = new CorrelationID(1);

            session.SendRequest(request, requestID);

            int timeoutInMilliSeconds = 5000;

            Event evt = session.NextEvent(timeoutInMilliSeconds);
            do
            {
                System.Console.WriteLine("Received Event: " + evt.Type);

                foreach (Bloomberglp.Blpapi.Message msg in evt.GetMessages())
                {
                    System.Console.WriteLine(msg.ToString());

                    if (evt.Type == Event.EventType.RESPONSE && msg.CorrelationID == requestID)
                    {

                        System.Console.WriteLine("Message Type: " + msg.MessageType);
                        if (msg.MessageType.Equals(ERROR_INFO))
                        {
                            int errorCode = msg.GetElementAsInt32("ERROR_CODE");
                            string errorMessage = msg.GetElementAsString("ERROR_MESSAGE");
                            System.Console.WriteLine("ERROR CODE: " + errorCode + "\tERROR MESSAGE: " + errorMessage);
                        }
                        else if (msg.MessageType.Equals("ORDER_INFO"))
                        {
                            int emsx_amount = msg.GetElementAsInt32("EMSX_AMOUNT");
                            double emsx_avg_price = msg.GetElementAsFloat64("EMSX_AVG_PRICE");
                            string emsx_basket_name = msg.GetElementAsString("EMSX_BASKET_NAME");
                            string emsx_broker = msg.GetElementAsString("EMSX_BROKER");
                            string emsx_exchange = msg.GetElementAsString("EMSX_EXCHANGE");
                            int emsx_filled = msg.GetElementAsInt32("EMSX_FILLED");
                            int emsx_flag = msg.GetElementAsInt32("EMSX_FLAG");
                            int emsx_idle_amount = msg.GetElementAsInt32("EMSX_IDLE_AMOUNT");
                            double emsx_limit_price = msg.GetElementAsFloat64("EMSX_LIMIT_PRICE");
                            string emsx_notes = msg.GetElementAsString("EMSX_NOTES");
                            string emsx_order_create_date = msg.GetElementAsString("EMSX_ORDER_CREATE_DATE");
                            string emsx_order_create_time = msg.GetElementAsString("EMSX_ORDER_CREATE_TIME");
                            string emsx_order_type = msg.GetElementAsString("EMSX_ORDER_TYPE");
                            string emsx_port_mgr = msg.GetElementAsString("EMSX_PORT_MGR");
                            string emsx_position = msg.GetElementAsString("EMSX_POSITION");
                            string emsx_side = msg.GetElementAsString("EMSX_SIDE");
                            string emsx_step_out_broker = msg.GetElementAsString("EMSX_STEP_OUT_BROKER");
                            int emsx_sub_flag = msg.GetElementAsInt32("EMSX_SUB_FLAG");
                            string emsx_ticker = msg.GetElementAsString("EMSX_TICKER");
                            string emsx_tif = msg.GetElementAsString("EMSX_TIF");
                            string emsx_trader = msg.GetElementAsString("EMSX_TRADER");
                            long emsx_trader_uuid = msg.GetElementAsInt64("EMSX_TRADER_UUID");
                            long emsx_ts_ordnum = msg.GetElementAsInt64("EMSX_TS_ORDNUM");
                            int emsx_working = msg.GetElementAsInt32("EMSX_WORKING");
                            string emsx_yellow_key = msg.GetElementAsString("EMSX_YELLOW_KEY");

                            System.Console.WriteLine("EMSX_AMOUNT: " + emsx_amount);
                            System.Console.WriteLine("EMSX_AVG_PRICE: " + emsx_avg_price);
                            System.Console.WriteLine("EMSX_BASKET_NAME: " + emsx_basket_name);
                            System.Console.WriteLine("EMSX_BROKER: " + emsx_broker);
                            System.Console.WriteLine("EMSX_EXCHANGE: " + emsx_exchange);
                            System.Console.WriteLine("EMSX_FILLED: " + emsx_filled);
                            System.Console.WriteLine("EMSX_FLAG: " + emsx_flag);
                            System.Console.WriteLine("EMSX_IDLE_AMOUNT: " + emsx_idle_amount);
                            System.Console.WriteLine("EMSX_LIMIT_PRICE: " + emsx_limit_price);
                            System.Console.WriteLine("EMSX_NOTES: " + emsx_notes);
                            System.Console.WriteLine("EMSX_ORDER_CREATE_DATE: " + emsx_order_create_date);
                            System.Console.WriteLine("EMSX_ORDER_CREATE_TIME: " + emsx_order_create_time);
                            System.Console.WriteLine("EMSX_ORDER_TYPE: " + emsx_order_type);
                            System.Console.WriteLine("EMSX_PORT_MGR: " + emsx_port_mgr);
                            System.Console.WriteLine("EMSX_POSITION: " + emsx_position);
                            System.Console.WriteLine("EMSX_SIDE: " + emsx_side);
                            System.Console.WriteLine("EMSX_STEP_OUT_BROKER: " + emsx_step_out_broker);
                            System.Console.WriteLine("EMSX_SUB_FLAG: " + emsx_sub_flag);
                            System.Console.WriteLine("EMSX_TICKER: " + emsx_ticker);
                            System.Console.WriteLine("EMSX_TIF: " + emsx_tif);
                            System.Console.WriteLine("EMSX_TRADER: " + emsx_trader);
                            System.Console.WriteLine("EMSX_TRADER_UUID: " + emsx_trader_uuid);
                            System.Console.WriteLine("EMSX_TS_ORDNUM: " + emsx_ts_ordnum);
                            System.Console.WriteLine("EMSX_WORKING: " + emsx_working);
                            System.Console.WriteLine("EMSX_YELLOW_KEY: " + emsx_yellow_key);

                            return emsx_amount - emsx_filled; // FIXME: No idea how we should deal with partial fill
                        }
                        
                    }
                }

                evt = session.NextEvent(timeoutInMilliSeconds);

            }
            while (evt.Type != Event.EventType.TIMEOUT);

            return 1;
        }
            

        public string checkFill()
        {
            string ret = "NO (UN)FILLED TRANSACTIONS PENDING";
            foreach (KeyValuePair<int, Request> entry in unfilledOrders)
            {
                int unfilled = checkOneFill(entry.Key);
                if (unfilled == 0)
                {
                    ret += "FILLED! -> " + entry.Value.ToString() + " ... ";
                    Lgr.WriteFilledTrade(entry.Value);
                    if ("BUY".Equals(((entry.Value).GetElement("EMSX_SIDE")).ToString()))
                    {
                        sendSellTransaction(this.unfilledTransactions[entry.Key]);
                    }
                    unfilledTransactions.Remove(entry.Key);
                    unfilledOrders.Remove(entry.Key);
                }
                else
                {
                    ret += " UNFILLED -> " + entry.Value.ToString() + " ... ";
                }
            }
            return ret;
        }

        private int GetTimestamp(DateTime value)
        {
            string now = DateTime.Now.ToString("HHmmss");
            now = "1" + now;
            now += Convert.ToString(rnd.Next(10));
            now += Convert.ToString(rnd.Next(10));
            return Convert.ToInt32(now);
        }

        private void sendSellTransaction(
            Transaction t
            )
        {
            double sellVal = (t.amountSell * t.priceSell);

            Request requestSell = service.CreateRequest("CreateOrderAndRouteEx");
            //Request requestSell = service.CreateRequest("CreateOrder");
            requestSell.Set("EMSX_LIMIT_PRICE", (t.priceSell).ToString());
            //requestSell.Set("EMSX_LIMIT_PRICE", 1.0);
            //requestSell.Set("EMSX_AMOUNT", t.amountSell);
            requestSell.Set("EMSX_AMOUNT", 1);
            requestSell.Set("EMSX_BROKER", "NORS"); // BMTB
            requestSell.Set("EMSX_HAND_INSTRUCTION", "DMA");
            requestSell.Set("EMSX_ORDER_TYPE", "LMT");
            requestSell.Set("EMSX_SIDE", "SELL");
            requestSell.Set("EMSX_TICKER", t.securitySell);
            requestSell.Set("EMSX_TIF", "DAY");
            requestSell.Set("EMSX_ACCOUNT", "LAGOTRAD");

            int sellStamp = GetTimestamp(DateTime.Now);
            requestSell.Set("EMSX_SEQUENCE", sellStamp);
            this.unfilledOrders[sellStamp] = requestSell;
            this.unfilledTransactions[sellStamp] = t;

            CorrelationID requestID = new CorrelationID("-2222");
            session.SendRequest(requestSell, requestID);

            Lgr.WriteTrade(sellVal, " ..... SELL: " + t.securitySell +
                " ..... amount " + t.amountSell + " price " + t.priceSell + " EUR" +
                " curr rate " + t.currencyRateSell.ToString());
        }

        public string sendBuyTransaction(
            Transaction t
            )
        {
            double tradeValToday = Lgr.GetTradeValToday();
            double maxEurPerDay = Convert.ToDouble(ConfigurationManager.AppSettings["maxEurPerDay"]);
            double maxEurPerTrade = Convert.ToDouble(ConfigurationManager.AppSettings["maxEurPerTrade"]);
            double buyVal = (t.amountBuy * t.priceBuy);
            double sellVal = (t.amountSell * t.priceSell);

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
            requestBuy.Set("EMSX_AMOUNT", t.amountBuy);
            //double price = t.priceBuy * t.currencyRateBuy;
            requestBuy.Set("EMSX_LIMIT_PRICE", (t.priceBuy).ToString());
            //requestBuy.Set("EMSX_LIMIT_PRICE", 1.0);
            //requestBuy.Set("EMSX_AMOUNT", 1);
            requestBuy.Set("EMSX_BROKER", "NORS"); // BMTB
            requestBuy.Set("EMSX_HAND_INSTRUCTION", "DMA");
            requestBuy.Set("EMSX_ORDER_TYPE", "LMT");
            requestBuy.Set("EMSX_SIDE", "BUY");
            requestBuy.Set("EMSX_TICKER", t.securityBuy);
            requestBuy.Set("EMSX_TIF", "DAY");
            requestBuy.Set("EMSX_ACCOUNT", "LAGOTRAD");

            int buyStamp = GetTimestamp(DateTime.Now);
            requestBuy.Set("EMSX_SEQUENCE", buyStamp);
            this.unfilledOrders[buyStamp] = requestBuy;

            CorrelationID requestID = new CorrelationID("-1111");
            session.SendRequest(requestBuy, requestID);
           
            //Request requestInfo = service.CreateRequest("OrderInfo");
            //requestInfo.Set("EMSX_SEQUENCE", requestBuy.EMSX_SEQUENCE);
            //CorrelationID infoID = new CorrelationID("-3333");
            //session.SendRequest(requestInfo, infoID);
            //use this to get buy order info and if order status == FILLED -> sell

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

            Lgr.WriteTrade(buyVal, " ..... BUY: " + t.securityBuy + 
                " ..... amount " + t.amountBuy + " price " + t.priceBuy + " EUR" +
                " curr rate " + t.currencyRateBuy.ToString());

            return "\nSUCCESFULLY SENT: " + t.toString() + "\n" + retstr;
        }
    }
}
