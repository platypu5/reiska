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

        private string stopSession()
        {
            string retval = "";
            retval += "\nStopping session...\n";
            session.Stop();
            retval += "\nSession stopped.\n";
            return retval;
        }

        public void sendTransaction(
            Transaction t
            )
        {
            Request request = service.CreateRequest("CreateOrderAndRouteEx");
            request.Set("EMSX_AMOUNT", emsx_amount);
            request.Set("EMSX_BROKER", emsx_broker);
            request.Set("EMSX_HAND_INSTRUCTION", emsx_hand_instruction);
            request.Set("EMSX_ORDER_TYPE", emsx_order_type);
            request.Set("EMSX_SIDE", emsx_side);
            request.Set("EMSX_TICKER", emsx_ticker);
            request.Set("EMSX_TIF", emsx_tif);

            session.SendRequest(request, new CorrelationID(-9998));
        }
    }
}
