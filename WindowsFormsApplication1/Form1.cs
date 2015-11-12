﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

using CsvHelper;
using CsvHelper.Configuration;

using Bloomberglp.Blpapi;

//This declaration specifies that EventHandler refers to Bloomberglp.Blpapi.EventHandler and not System.EventHandler.  The Bloomberg API named this ambiguously.
using EventHandler = Bloomberglp.Blpapi.EventHandler;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        string[] securityList1;
        string[] securityList2;
        string[] currencyList1;
        string[] currencyList2;
        string[] securityScale1;
        string[] securityScale2;
        string baseCurrency;
        bool sendTransactions;
        int profitThresholdEur;
        TransactionComputer tc;
        TransactionSender ts;
        Dictionary<string, double> pendingAsk = new Dictionary<string, double>();
        Dictionary<string, double> pendingBid = new Dictionary<string, double>();
        private Session sessionSec;
        private Session sessionCurr;
        private bool running;

        public Form1()
        {
            InitializeComponent();
        }

        private void checkFill()
        {
            Result result = ts.startSession();
            bool succ = result.success;
            string retval = result.retval;
            if(!succ)
            {
                Lgr.Log("ERROR", retval);
                return;
            }
            Invoke(new Action(() =>
                          richTextBox1.AppendText(retval)));

            retval = ts.checkFill();
            Invoke(new Action(() =>
              richTextBox1.AppendText(retval)));

            retval = ts.stopSession();
            Invoke(new Action(() =>
                          richTextBox1.AppendText(retval)));
        }

        private void sendTransaction(Transaction t)
        {
            Result result = ts.startSession();
            bool succ = result.success;
            string retval = result.retval;
            if (!succ)
            {
                Lgr.Log("ERROR", retval);
                return;
            }

            Invoke(new Action(() =>
                          richTextBox1.AppendText(retval)));

            retval = ts.sendBuyTransaction(t);
            Invoke(new Action(() =>
              richTextBox1.AppendText(retval)));

            retval = ts.stopSession();
            Invoke(new Action(() =>
                          richTextBox1.AppendText(retval)));
        }

        private void updatePendingAskBid()
        {
            foreach (KeyValuePair<string, double> entry in pendingAsk)
            {
                string ret = tc.updateAsk(entry.Key, entry.Value);
                if(!ret.Equals("NO CURRENCY"))
                {
                    pendingAsk.Remove(entry.Key);
                }
            }
            foreach (KeyValuePair<string, double> entry in pendingBid)
            {
                string ret = tc.updateBid(entry.Key, entry.Value);
                if (!ret.Equals("NO CURRENCY"))
                {
                    pendingBid.Remove(entry.Key);
                }
            }
        }

        private void ProcessEvent(Event evt, List<String> fields)
        {
            string allVals = tc.getAllValues();
            Invoke(new Action(() =>
                richTextBox1.AppendText 
                (string.Format("{0}",
                allVals))));

            Invoke(new Action(() => richTextBox1.AppendText("\n\nGOT NEW MESSAGE --> ProcessEvent(Event, List<String>)\n")));

            const bool excludeNullElements = false;

            foreach (Bloomberglp.Blpapi.Message message in evt.GetMessages())
            {
                /*
                Invoke(new Action(() =>
                    richTextBox1.AppendText
                    (string.Format("MESSAGE: {0}",
                    message.ToString()))));
                    */

                string security = message.TopicName;

                Invoke(new Action(() =>
                          richTextBox1.AppendText
                          (string.Format("GOT SECURITY: {0}",
                            security))));

                string elmFields = "";
                foreach (var field in fields)
                {
                    //This ignores the extraneous fields in the response
                    if (message.HasElement(field, excludeNullElements)) //be careful, excludeNullElements is false by default
                    {
                        Element elmField = message[field];
                        elmFields += security + ": " + elmField.ToString().Trim() + "; ";
                        string retval = "";
                        if (security.Contains("Curncy"))
                        {
                            Invoke(new Action(() =>
                                richTextBox1.AppendText
                                (string.Format("\nGOT CURNCY FIELD {0}\n",
                                field))));

                            if (field.Equals("LAST_PRICE"))
                            {
                                Invoke(new Action(() =>
                                    richTextBox1.AppendText
                                    ("\nGOT CURNCY LAST_PRICE\n")));   
                                retval =
                                    tc.updateCurrencyRate
                                    (security.Substring(0, 3), elmField.GetValueAsFloat64());
                            }
                        }
                        else if (field.Equals("BEST_ASK"))
                        {
                            retval = tc.updateAsk(security, elmField.GetValueAsFloat64());
                            if (retval.Equals("NO CURRENCY"))
                            {
                                this.pendingAsk[security] = elmField.GetValueAsFloat64();
                            }
                        }
                        else if (field.Equals("BEST_BID"))
                        {
                            retval = tc.updateBid(security, elmField.GetValueAsFloat64());
                            if (retval.Equals("NO CURRENCY"))
                            {
                                this.pendingBid[security] = elmField.GetValueAsFloat64();
                            }
                        }
                        else if (field.Equals("BEST_ASK1_SZ"))
                        {
                            retval = tc.updateAskSize(security, elmField.GetValueAsFloat64());
                        }
                        else if (field.Equals("BEST_BID1_SZ"))
                        {
                            retval = tc.updateBidSize(security, elmField.GetValueAsFloat64());
                        }
                        if (!retval.Equals(""))
                        {
                            Invoke(new Action(() =>
                                richTextBox1.AppendText
                                (string.Format("\n -> EXECUTED ACTION: {0}",
                                retval))));
                        }

                        if (security.Contains("Curncy"))
                        {
                            continue;
                        } 
                    }
                }

                Invoke(new Action(() =>
                    richTextBox1.AppendText
                    (string.Format
                    ("-\n{0:HH:mm:ss}\nELEMENT FIELDS (FROM API):\n{1}\n-\n",
                     DateTime.Now,
                     elmFields.Trim()))));

                Transaction transaction = tc.computeTransaction(security);
                handleTransaction(transaction, elmFields);
            }
            Invoke(new Action(() => richTextBox1.AppendText("\n")));
        }

        private void handleTransaction(Transaction transaction, string elmFields)
        {
            Lgr.Log("INFO", "Got element fields " + elmFields + " from API.");
            if (sendTransactions)
            {
                Invoke(new Action(() =>
                  richTextBox1.AppendText("\nsendTransactions=true\n")));
                if (transaction.isFeasible())
                {
                    Lgr.Log("INFO", "SEND TRANSACTIONS = TRUE, FEASIBLE TRANSACTION!!!");
                    Lgr.Log("INFO", transaction.toString());
                    Invoke(new Action(() =>
                        richTextBox1.AppendText("\nFEASIBLE TRANSACTION!\n")));
                    sendTransaction(transaction);
                }
                else
                {
                    Invoke(new Action(() =>
                        richTextBox1.AppendText("\nno feasible transaction\n")));
                }
            }
            else
            {
                if (transaction.isFeasible())
                {
                    Lgr.Log("INFO", "SEND TRANSACTIONS = FALSE, FEASIBLE TRANSACTION");
                    Lgr.Log("INFO", transaction.toString());
                }
                Invoke(new Action(() =>
                    richTextBox1.AppendText("\nsendTransactions=false\n")));
            }
        }

        private void ProcessEventCurr(Event evt, Session session)
        {
            ProcessEvent(evt, session, "currency");
        }

        private void ProcessEventSec(Event evt, Session session)
        {
            ProcessEvent(evt, session, "security");
        }


        private void ProcessEvent(Event evt, Session session, string type)
        {
            Invoke(new Action(() => richTextBox1.AppendText("\n\nProcessEvent(Event, Session)\n")));
            List<string> _fields;
            _fields = new List<String>();
            if (type.Equals("currency"))
            {
                _fields.Add("LAST_PRICE");
            }
            else
            {
                _fields.Add("BEST_BID");
                _fields.Add("BEST_ASK");
                _fields.Add("BEST_BID1_SZ");
                _fields.Add("BEST_ASK1_SZ");
            }

            switch (evt.Type)
            {
                case Event.EventType.SESSION_STATUS: //use this to open the service
                    foreach (Bloomberglp.Blpapi.Message message in evt.GetMessages())
                    {
                        if (message.MessageType.Equals("SessionStarted"))
                        {
                            try
                            {
                                session.OpenServiceAsync("//blp/mktdata", new CorrelationID(-9999));
                                Invoke(new Action(() => richTextBox1.AppendText("Session opened\n")));
                            }
                            catch (Exception)
                            {
                                Invoke(new Action(() => richTextBox1.AppendText("Could not open //blp/mktdata for async\n")));
                            }
                        }
                    }
                    break;

                case Event.EventType.SERVICE_STATUS: //use this to subscribe to ticker feeds
                    List<Subscription> slist = new List<Subscription>();

                    string interval = ConfigurationManager.AppSettings["interval"];
                    List<string> options = new string[] { "interval=" + interval }.ToList();

                    //uncomment the following line to see what a request for a nonexistent security looks like
                    //slist.Add(new Subscription("ZYZZ US EQUITY", MarketDataRequest._fields, options));
                    //  My code treats all securities that start with a 'Z' as a nonexistent security

                    if (type.Equals("security"))
                    {
                        foreach (string security in securityList1)
                        {
                            Invoke(new Action(() =>
                                richTextBox1.AppendText
                                (string.Format("ADDING {0} TO SUBSCRIPTIONS\n",
                                security))));
                            slist.Add(new Subscription(security, _fields, options));
                        }
                        foreach (string security in securityList2)
                        {
                            Invoke(new Action(() =>
                                richTextBox1.AppendText
                                (string.Format("ADDING {0} TO SUBSCRIPTIONS\n",
                                security))));
                            slist.Add(new Subscription(security, _fields, options));
                        }
                        Invoke(new Action(() => richTextBox1.AppendText("added securities\n")));
                    }
                    else
                    {
                        foreach (string currency in currencyList1)
                        {
                            if (currency == baseCurrency) { continue; }
                            string currencyID = currency + baseCurrency + " Curncy";
                            Invoke(new Action(() =>
                                richTextBox1.AppendText
                                (string.Format("ADDING {0} TO SUBSCRIPTIONS\n",
                                currencyID))));
                            slist.Add(new Subscription(currencyID, _fields, options));
                        }
                        foreach (string currency in currencyList2)
                        {
                            if (currency == baseCurrency) { continue; }
                            string currencyID = currency + baseCurrency + " Curncy";
                            Invoke(new Action(() =>
                                richTextBox1.AppendText
                                (string.Format("ADDING {0} TO SUBSCRIPTIONS\n",
                                currencyID))));
                            slist.Add(new Subscription(currencyID, _fields, options));
                        }
                        Invoke(new Action(() => richTextBox1.AppendText("added currencies\n")));
                    }

                    //slist.Add(new Subscription("SPY US EQUITY", _fields, options));
                    //slist.Add(new Subscription("AAPL 150117C00600000 EQUITY", _fields, options));

                    session.Subscribe(slist);
                    break;

                case Event.EventType.SUBSCRIPTION_DATA:
                case Event.EventType.RESPONSE:
                case Event.EventType.PARTIAL_RESPONSE:
                    ProcessEvent(evt, _fields);
                    break;

                case Event.EventType.SUBSCRIPTION_STATUS:
                    foreach (var msg in evt.GetMessages())
                    {
                        bool fieldExceptionsExist = msg.MessageType.ToString() == "SubscriptionStarted" && msg.HasElement("exceptions", true);
                        bool securityError = msg.MessageType.ToString() == "SubscriptionFailure" && msg.HasElement("reason", true);

                        if (fieldExceptionsExist)
                        {
                            Element elmExceptions = msg["exceptions"];
                            for (int i = 0; i < elmExceptions.NumValues; i++)
                            {
                                Element elmException = elmExceptions.GetValueAsElement(i);
                                string fieldId = elmException.GetElementAsString("fieldId");
                                Element elmReason = elmException["reason"];
                                string source = elmReason.GetElementAsString("source");
                                //int errorCode = elmReason.GetElementAsInt32("errorCode");
                                string category = elmReason.GetElementAsString("category");
                                string description = elmReason.GetElementAsString("description");
                                Invoke(new Action(() => richTextBox1.AppendText("field error: ")));
                                Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tfieldId = {0}", fieldId))));
                                Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tsource = {0}", source))));
                                //Invoke(new Action(() => richTextBox1.AppendText(string.Format("\terrorCode = {0}", errorCode))));
                                Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tcategory = {0}", category))));
                                Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tdescription = {0}", description))));
                            }
                        }
                        else if (securityError)
                        {
                            string security = msg.TopicName;
                            Element elmReason = msg["reason"];
                            string source = elmReason.GetElementAsString("source");
                            int errorCode = elmReason.GetElementAsInt32("errorCode");
                            string category = elmReason.GetElementAsString("category");
                            string description = elmReason.GetElementAsString("description");
                            Invoke(new Action(() => richTextBox1.AppendText("security not found: ")));
                            Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tsecurity = {0}", security))));
                            Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tsource = {0}", source))));
                            Invoke(new Action(() => richTextBox1.AppendText(string.Format("\terrorCode = {0}", errorCode))));
                            Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tcategory = {0}", category))));
                            Invoke(new Action(() => richTextBox1.AppendText(string.Format("\tdescription = {0}", description))));
                        }
                    }
                    break;

                default:
                    foreach (var msg in evt.GetMessages())
                    {
                        Invoke(new Action(() => richTextBox1.AppendText(msg.ToString())));
                    }
                    break;
            }
        }

        private void pullInitial(SessionOptions sessionOptions)
        {
            Session session = new Session(sessionOptions);
            if (!session.Start())
            {
                Invoke(new Action(() => richTextBox1.AppendText("Could not start session 2.")));
            }
            if (!session.OpenService("//blp/refdata"))
            {
                Invoke(new Action(() => richTextBox1.AppendText("Could not open service " + "//blp/refdata")));
            }
            CorrelationID requestID = new CorrelationID(1);

            Service refDataSvc = session.GetService("//blp/refdata");

            Request request =
                refDataSvc.CreateRequest("ReferenceDataRequest");

            foreach (string security in securityList1)
            {
                request.Append("securities", security);
            }
            foreach (string security in securityList2)
            {
                request.Append("securities", security);
            }
            foreach (string currency in currencyList1)
            {
                if (currency == baseCurrency) { continue; }
                string currencyID = currency + baseCurrency + " Curncy";
                request.Append("securities", currencyID);
            }
            foreach (string currency in currencyList2)
            {
                if (currency == baseCurrency) { continue; }
                string currencyID = currency + baseCurrency + " Curncy";
                request.Append("securities", currencyID);
            }
            Invoke(new Action(() => richTextBox1.AppendText("added securities\n")));

            List<string> _fields;
            _fields = new List<String>();
            _fields.Add("LAST_PRICE");
            _fields.Add("BEST_BID");
            _fields.Add("BEST_ASK");
            _fields.Add("BEST_BID1_SZ");
            _fields.Add("BEST_ASK1_SZ");

            request.Append("fields", "LAST_PRICE");
            request.Append("fields", "BEST_BID");
            request.Append("fields", "BEST_ASK");
            request.Append("fields", "BEST_BID1_SZ");
            request.Append("fields", "BEST_ASK1_SZ");
            
            session.SendRequest(request, requestID);

            bool continueToLoop = true;

            while (continueToLoop)
            {
                Bloomberglp.Blpapi.Event evt = session.NextEvent();

                switch (evt.Type)
                {
                    case Event.EventType.PARTIAL_RESPONSE:
                        Invoke(new Action(() => richTextBox1.AppendText("partial response\n")));
                        ProcessEvent(evt, _fields);
                        break;
                    case Event.EventType.RESPONSE: // final event
                        Invoke(new Action(() => richTextBox1.AppendText("final response\n")));
                        continueToLoop = false; // fall through
                        break;
                }
            }
            Invoke(new Action(() => richTextBox1.AppendText("-------------- FINISHED INITIAL PULL -------------")));
            Invoke(new Action(() => richTextBox1.AppendText("-------------- FINISHED INITIAL PULL -------------")));
            Invoke(new Action(() => richTextBox1.AppendText("-------------- FINISHED INITIAL PULL -------------")));
        }

        public class DataRecord
        {
            public string security1 { get; set; }
            public string security2 { get; set; }
            public string currency1 { get; set; }
            public string currency2 { get; set; }
            public string security1scale { get; set; }
            public string security2scale { get; set; }
            public string arbidir { get; set; }
        }

        private int get_Securities()
        { 
            // Input handling and validation.
            List<string> _securityList1 = new List<string>();
            List<string> _securityList2 = new List<string>();
            List<string> _securityScale1 = new List<string>();
            List<string> _securityScale2 = new List<string>();
            List<string> _currencyList1 = new List<string>();
            List<string> _currencyList2 = new List<string>();
            // FIXME: per row or global?
            List<string> _arbidir = new List<string>();
            // FIXME: missing expensesEUR, expensesperc (found in appconfig), needed?

            // FIXME: configurable filename?
            using (var sr = new StreamReader(@"securities.csv"))
            {
                var reader = new CsvReader(sr);
                IEnumerable<DataRecord> records = reader.GetRecords<DataRecord>();
                int i = 1;
                bool error;

                foreach (DataRecord record in records)
                {
                    string dirvalue = "";
                    error = false;
                    i++;
                    foreach (var prop in record.GetType().GetProperties())
                    {
                        string value = String.Format("{0}", prop.GetValue(record));
                        if (String.IsNullOrEmpty(value))
                        {
                            Invoke(new Action(() => richTextBox1.AppendText(String.Format("Error in securities.csv (line {0}): no value for column {1}\n", i, prop.Name)))); //, prop.GetValue(record)))));
                            error = true;
                            continue;
                        }
                        if (prop.Name == "security1scale" || prop.Name == "security2scale")
                        {
                            try
                            {
                                float k = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch (System.FormatException)
                            {
                                Invoke(new Action(() => richTextBox1.AppendText(String.Format("Bad {0} (line {1}): {2}\n", prop.Name, i, value))));
                                error = true;
                            }
                        }
                        else if (prop.Name == "arbidir")
                        {
                            dirvalue = value.ToLower();
                            switch (dirvalue)
                            {
                                case "->":
                                case "to":
                                    dirvalue = "->";
                                    break;
                                case "<-":
                                case "from":
                                    dirvalue = "<-";
                                    break;
                                case "both":
                                case "<->":
                                    dirvalue = "<->";
                                    break;
                                default:
                                    Invoke(new Action(() => richTextBox1.AppendText(String.Format("Bad direction (line {0}): {1}\n", i, value))));
                                    error = true;
                                    break;
                            }
                        }
                    }
                    if (!error)
                    {
                        _securityList1.Add(record.security1);
                        _securityList2.Add(record.security2);
                        _securityScale1.Add(record.security1scale);
                        _securityScale1.Add(record.security2scale);
                        _currencyList1.Add(record.currency1);
                        _currencyList2.Add(record.currency2); 
                        _arbidir.Add(dirvalue);
                        Invoke(new Action(() => richTextBox1.AppendText(String.Format(
                            "New security: {0} ({2}, {4}) {6} {1} ({3}, {5})\n",
                            record.security1, record.security2,
                            record.security1scale, record.security2scale,
                            record.currency1, record.currency2, dirvalue))));
                    }
                }                    
            }
            securityList1 = _securityList1.ToArray();
            securityList2 = _securityList2.ToArray();
            currencyList1 = _currencyList1.ToArray();
            currencyList2 = _currencyList2.ToArray();
            securityScale1 = _securityScale1.ToArray();
            securityScale2 = _securityScale2.ToArray();
            return _securityList1.Count;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Lgr.Start();
            int _securities;

            try
            {
                _securities = get_Securities();
            }
            catch (CsvHelper.CsvMissingFieldException)
            {
                Invoke(new Action(() => richTextBox1.AppendText("Malformed securities.csv.\nSee that the header line is security1,security2,currency1,currency2,security1scale,security2scale,arbidir\n")));
                return;
            }
            catch (System.IO.FileNotFoundException)
            {
                Invoke(new Action(() => richTextBox1.AppendText("Could not find securities.csv\n")));
                return;
            }

            Invoke(new Action(() => richTextBox1.AppendText(String.Format("Found {0} securities\n", _securities))));
            if (_securities == 0)
            {
                Invoke(new Action(() => richTextBox1.AppendText("No valid securities found. Not starting.\n")));
                return;
            }

            baseCurrency = ConfigurationManager.AppSettings["baseCurrency"];
            profitThresholdEur = Convert.ToInt32(ConfigurationManager.AppSettings["profitThresholdEur"]);
            sendTransactions = Convert.ToBoolean(ConfigurationManager.AppSettings["sendTransaction"]);
            tc = new TransactionComputer
                (securityList1, securityList2,
                 currencyList1, currencyList2,
                 securityScale1, securityScale2,
                 baseCurrency, profitThresholdEur);
            ts = new TransactionSender();
            if (running == false)
            {
                running = true;

                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.ServerHost = "localhost";
                sessionOptions.ServerPort = 8194;

                //pullInitial(sessionOptions);

                sessionCurr = new Session(sessionOptions, new EventHandler(ProcessEventCurr));
                sessionCurr.StartAsync();

                sessionSec = new Session(sessionOptions, new EventHandler(ProcessEventSec));
                sessionSec.StartAsync();

                Invoke(new Action(() => richTextBox1.AppendText("started\n")));
                this.button1.Text = "Stop";
            }
            else
            {
                // FIXME: Any danger in stopping, eg. to transactions?
                running = false;
                sessionCurr.Stop();
                sessionSec.Stop();
                Invoke(new Action(() => richTextBox1.AppendText("stopped\n")));
                this.button1.Text = "Start";
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
