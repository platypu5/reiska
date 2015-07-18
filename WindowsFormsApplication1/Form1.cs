using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Bloomberglp.Blpapi;

//This declaration specifies that EventHandler refers to Bloomberglp.Blpapi.EventHandler and not System.EventHandler.  The Bloomberg API named this ambiguously.
using EventHandler = Bloomberglp.Blpapi.EventHandler;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ProcessEvent(Event evt, List<String> fields)
        {
            Invoke(new Action(() => richTextBox1.AppendText("ProcessEvent(Event, List<String>)\n")));
            const bool excludeNullElements = true;
            foreach (Bloomberglp.Blpapi.Message message in evt.GetMessages())
            {
                string security = message.TopicName;
                foreach (var field in fields)
                {
                    //This ignores the extraneous fields in the response
                    if (message.HasElement(field, excludeNullElements)) //be careful, excludeNullElements is false by default
                    {
                        Element elmField = message[field];

                        Invoke(new Action(() => richTextBox1.AppendText(string.Format("{0:HH:mm:ss}: {1}, {2}",
                            DateTime.Now,
                            security,
                            elmField.ToString().Trim()))));
                    }
                }
            }
            Invoke(new Action(() => richTextBox1.AppendText("\n")));
        }

        private void ProcessEvent(Event evt, Session session)
        {
            Invoke(new Action(() => richTextBox1.AppendText("ProcessEvent(Event, Session)\n")));
            List<string> _fields;
            _fields = new List<String>();
            _fields.Add("BID");
            _fields.Add("ASK");

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

                //Conflate the data to show every two seconds.
                //  Please note that the Bloomberg API Emulator code does not treat this exactly correct: individual subscriptions should each have their own interval setting.
                //  I have not coded that in the emulator.
                List<string> options = new string[] { "interval=2", "start_time=16:22", "end_time=16:23" }.ToList(); //2 seconds.  //Comment this line to receive a subscription data event whenever it happens in the market.

                //uncomment the following line to see what a request for a nonexistent security looks like
                //slist.Add(new Subscription("ZYZZ US EQUITY", MarketDataRequest._fields, options));
                //  My code treats all securities that start with a 'Z' as a nonexistent security

                slist.Add(new Subscription("SPY US EQUITY", _fields, options));
                slist.Add(new Subscription("AAPL 150117C00600000 EQUITY", _fields, options));

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


        private void button1_Click(object sender, EventArgs e)
        {
            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.ServerHost = "localhost";
            sessionOptions.ServerPort = 8194;
            Session session = new Session(sessionOptions, new EventHandler(ProcessEvent));
            session.StartAsync();

            //Invoke(new Action(() => richTextBox1.AppendText("moi\n");
            Invoke(new Action(() => richTextBox1.AppendText("end\n")));
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
