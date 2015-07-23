using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class TransactionComputer
    {
        string[] securityList1;
        string[] securityList2;

        public TransactionComputer(string[] sc1, string[] sc2)
        {
            securityList1 = sc1;
            securityList2 = sc2;
        }

        public string computeTransaction(string security, float ask, float bid)
        {
            return "";
        }
    }
}
