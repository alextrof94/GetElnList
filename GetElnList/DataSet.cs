using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetElnList
{
    class DataSet
    {
        public string regNum { get; set; }
        public string lnCode { get; set; }
        public string snils { get; set; }

        public DataSet(string regNum, string lnCode, string snils)
        {
            this.regNum = regNum;
            this.lnCode = lnCode;
            this.snils = snils;
        }

        public override string ToString()
        {
            return regNum + " " + lnCode + " " + snils;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DataSet))
                return false;
            if ((obj as DataSet).regNum == regNum
                    && (obj as DataSet).lnCode == lnCode
                    && (obj as DataSet).snils == snils)
                return true;
            return false;
        }
    }
}
