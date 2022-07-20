using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrjInbound
{
    public class ClsAdjustment
    {

        public class Adjustment
        {
            public Header Header { get; set; }
            public List<Body> Body { get; set; }
        }
        public class Header
        {
            public string Date { get; set; }
            public string Employee_Id { get; set; }
            public string Branch_Id { get; set; }
            public string MaturityDate { get; set; }
            public string Currency_Id { get; set; }
            public string ExchangeRate { get; set; }
            public string VAN_Id { get; set; }
            public string sNarration { get; set; }
            public string sChequeNo { get; set; }
            public string CashBankAC { get; set; }
            public string VoucherType { get; set; }

        }
        public class Body
        {
            public string Customer_Id { get; set; }
            public string sRemarks { get; set; }
            public double Amount { get; set; }
            public List<Reference> Reference { get; set; }
        }
        public class Reference
        {
            public string InvoiceNo { get; set; }
            public string Amount { get; set; }
        }
        public class AdjustmentResponse
        {
            public int iStatus { get; set; }
            public string sMessage { get; set; }
            public string sVouchrNo { get; set; }
        }
    }
}
