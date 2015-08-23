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
            Request requestSell = service.CreateRequest("CreateOrderAndRouteEx");
            requestSell.Set("EMSX_AMOUNT", t.amount);
            requestSell.Set("EMSX_BROKER", "API");
            requestSell.Set("EMSX_HAND_INSTRUCTION", "ANY");
            requestSell.Set("EMSX_ORDER_TYPE", "MKT");
            requestSell.Set("EMSX_SIDE", "SELL");
            requestSell.Set("EMSX_TICKER", t.securitySell);
            requestSell.Set("EMSX_TIF", "DAY");

            Request requestBuy = service.CreateRequest("CreateOrderAndRouteEx");
            requestSell.Set("EMSX_AMOUNT", t.amount);
            requestSell.Set("EMSX_BROKER", "API");
            requestSell.Set("EMSX_HAND_INSTRUCTION", "ANY");
            requestSell.Set("EMSX_ORDER_TYPE", "MKT");
            requestSell.Set("EMSX_SIDE", "BUY");
            requestSell.Set("EMSX_TICKER", t.securityBuy);
            requestSell.Set("EMSX_TIF", "DAY");

            session.SendRequest(requestSell, new CorrelationID(-2222));
            session.SendRequest(requestBuy, new CorrelationID(-1111));

            return "\n!!! SELLING " + t.amount + " OF " + t.securitySell + " BUYING " + t.securityBuy + "!!!\n";
        }
    }
}
