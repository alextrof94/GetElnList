using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetElnList
{
    public class ResultData
    {
        public string STATUS { get; set; }
        public string SNILS { get; set; }
        public string SURNAME { get; set; }
        public string NAME { get; set; }
        public string PATRONIMIC { get; set; }
        public string BOZ_FLAG { get; set; }
        public string LPU_EMPLOYER { get; set; }
        public string LPU_EMPL_FLAG { get; set; }
        public string LN_CODE { get; set; }
        public string PRIMARY_FLAG { get; set; }
        public string DUPLICATE_FLAG { get; set; }
        public string LN_DATE { get; set; }
        public string LPU_NAME { get; set; }
        public string LPU_ADDRESS { get; set; }
        public string LPU_OGRN { get; set; }
        public string BIRTHDAY { get; set; }
        public string GENDER { get; set; }
        public string REASON1 { get; set; }
        public string DATE1 { get; set; }
        public string DATE2 { get; set; }
        public string SERV1_AGE { get; set; }
        public string SERV1_MM { get; set; }
        public string SERV1_RELATION_CODE { get; set; }
        public string SERV1_FIO { get; set; }
        public string SERV2_AGE { get; set; }
        public string SERV2_MM { get; set; }
        public string PREGN12W_FLAG { get; set; }
        public string HOSPITAL_DT1 { get; set; }
        public string HOSPITAL_DT2 { get; set; }
        public string MSE_DT1 { get; set; }
        public string MSE_DT2 { get; set; }
        public string MSE_DT3 { get; set; }
        public string MSE_INVALID_GROUP { get; set; }
        public string LN_STATE { get; set; }
        public string EMPL_FLAG { get; set; }
        public string INSUR_YY { get; set; }
        public string INSUR_MM { get; set; }
        public string NOT_INSUR_YY { get; set; }
        public string NOT_INSUR_MM { get; set; }
        public string FORM1_DT { get; set; }
        public string RETURN_DATE_EMPL { get; set; }
        public string DT1_LN { get; set; }
        public string DT2_LN { get; set; }
        public string LN_HASH { get; set; }
        public List<TreatFullPeriod> treatPeriods { get; set; }
        public List<LnResult> lnResult { get; set; }

        public ResultData()
        {
            treatPeriods = new List<TreatFullPeriod>();
            lnResult = new List<LnResult>();
        }
    }
}
