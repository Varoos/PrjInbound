using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrjInbound
{
    public class ClsSalesReturn
    {
        public class SalesReturn
        {
            public Header Header { get; set; }
            public List<Body> Body { get; set; }
        }

        public class Header
        {
            public string Date { get; set; }
            public string Customer_Id { get; set; }
            public string DueDate { get; set; }
            public string Currency_Id { get; set; }
            public string Helper { get; set; }
            public string Place_of_supply_Code { get; set; }
            public string VAN_Id { get; set; }
            public string Driver_Mobile_No { get; set; }
            public string Driver { get; set; }
            public string Employee_Id { get; set; }
            public string Branch_Id { get; set; }
            public string LPO_No { get; set; }
            public string InvoiceType { get; set; }
        }
        public class Body
        {
            public string Item_Id { get; set; }
            public string Description { get; set; }
            public string TaxCode_Code { get; set; }
            public string Unit_Id { get; set; }
            public string Batch { get; set; }
            public string SalesAc { get; set; }
            public double Qty { get; set; }
            public double FOC_Qty { get; set; }
            public double Actual_Qty { get; set; }
            public double Selling_Price { get; set; }
            public double Add_Charges { get; set; }
            public double VAT_Value { get; set; }
            public double Discount_Amt { get; set; }
            public Link_Invoice Link_Invoice { get; set; }
            public string MfgDate { get; set; }
            public string ExpDate { get; set; }
            public double DiscountPer { get; set; }
        }

        public class Reference
        {
            public string CustomerId { get; set; }
            public string Amount { get; set; }
            public string BillReference { get; set; }
        }
        public class Link_Invoice
        {
            public string VoucherNo { get; set; }
        }
      

        public class SalesResponse
        {
            public int iStatus { get; set; }
            public string sMessage { get; set; }
            public string sVouchrNo { get; set; }
        }

    }
}