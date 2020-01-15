using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRefferal
{
    public class Refferal
    {
        public string Code { get; set; }
        public int ActivatedAccounts { get; set; }

        public Refferal()
        {

        }

        public Refferal(string code, int activatedAccounts)
        {
            Code = code;
            ActivatedAccounts = activatedAccounts;
        }
    }
}
