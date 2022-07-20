using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;
using System.Security.Principal;
using System.Data;
using System.ServiceModel.Channels;
using System.Collections;

namespace PrjInbound
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class FocusService : IFocusService
    {
        int ccode = 0;
        Clsdata cls = new Clsdata();
        string strerror = "";
        string sessionId = "";
        string serverip = WebConfigurationManager.AppSettings["Server_Ip"];
        string username = WebConfigurationManager.AppSettings["User_Name"];
        string Password = WebConfigurationManager.AppSettings["Password"];
        string companycode = WebConfigurationManager.AppSettings["companyCode"];
        int writelog = Convert.ToInt32(WebConfigurationManager.AppSettings["WriteLog"]);
        string AccessToken = "";
        string doctype = "";

        #region GetLogin               
        public ClsProperties.LogingResult Getlogin(string User_Name, string Password, string Company_Code)
        {
            ClsProperties.LogingResult obj = new ClsProperties.LogingResult();
            try
            {

                string companyid = Company_Code;
                ccode = cls.GetCompanyId(companyid);
                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Getlogin", writelog);
                if (companyid == "")
                {
                    obj.iStatus = 0; obj.sMessage = "Company code should not be blank"; obj.Auth_Token = "";
                }
                if (User_Name == "" || Password == "")
                {
                    obj.iStatus = 0; obj.sMessage = "User_Name or Passwrod should not be blank"; obj.Auth_Token = "";
                }

                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + companyid, writelog);

                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + User_Name, writelog);
                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + Password, writelog);

                string sSessionId = "";

                ClsProperties.Datum datanum = new ClsProperties.Datum();
                datanum.CompanyCode = Company_Code;
                datanum.Username = User_Name;
                datanum.password = Password;
                List<ClsProperties.Datum> lstd = new List<ClsProperties.Datum>();
                lstd.Add(datanum);
                ClsProperties.Lolgin lngdata = new ClsProperties.Lolgin();
                lngdata.data = lstd;
                string sContent = JsonConvert.SerializeObject(lngdata);
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("Content-Type", "application/json");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    client.Timeout = 10 * 60 * 1000;
                    var arrResponse = client.UploadString("http://" + serverip + "/focus8API/Login", sContent);
                    //returnObject = new clsDeserialize().Deserialize<RootObject>(arrResponse);
                    ClsProperties.Resultlogin lng = JsonConvert.DeserializeObject<ClsProperties.Resultlogin>(arrResponse);


                    sSessionId = lng.data[0].fSessionId;
                    if (lng.data[0].fSessionId == null || lng.data[0].fSessionId == "" || lng.data[0].fSessionId == "-1")
                    {
                        obj.iStatus = 0; obj.sMessage = "User_Name or Password is mismatch"; obj.Auth_Token = "";
                        return obj;

                    }
                    else

                    {
                        // bool flg = logout(sSessionId, serverip);
                        client.Headers.Add("fSessionId", sSessionId);
                        //client.Timeout = 10 * 60 * 1000;
                        arrResponse = client.DownloadString("http://" + serverip + "/focus8API/Logout");
                    }
                }

                int iloginhandle = 1;
                if (iloginhandle <= 0)
                {
                    obj.iStatus = 0; obj.sMessage = "User_Name or Password is mismatch"; obj.Auth_Token = "";
                }
                else
                {
                    Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Token is generating", writelog);
                    //int iloginhandle = 1;
                    AuthenticationModule objAuth = new AuthenticationModule();
                    string authtoken = objAuth.GenerateTokenForUser(companyid, iloginhandle);
                    Clsdata.LogFile("7HRVSTLogin", DateTime.Now + authtoken, writelog);
                    if (authtoken != "")
                    {
                        obj.iStatus = 1; obj.sMessage = "Token Generated"; obj.Auth_Token = authtoken;

                        Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Token is Generated", writelog);
                    }
                    else
                    {
                        obj.iStatus = 0; obj.sMessage = "User_Name or Password is mismatch"; obj.Auth_Token = "";
                    }
                }
            }
            catch (Exception ex)
            {
                obj.iStatus = 0; obj.sMessage = ex.Message; obj.Auth_Token = "";

                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Getlogin excetpion:" + ex.Message, writelog);
            }
            return obj;
        }
        #endregion

        #region Invoice
        public ClsSalesInvoice.SalesResponse SalesInvoice(ClsSalesInvoice.SalesInvoice objfocus)
        {
            doctype = "SalesInvoice";
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }
                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }
                ccode = cls.GetCompanyId(companycode);
                Clsdata.LogFile(doctype, "CompanyCode = " + ccode.ToString(), writelog);
                Clsdata.LogFile(doctype, "Go To Create Invoice", writelog);
                objreturn = CreateInvoice(objfocus, ccode);
                //int voucherytpe = 0;
                //DataSet ds = Clsdata.GetData("select ivouchertype from ccore_vouchers_0 where sName='Sales Invoice - VAN'", ccode);
                //if (ds.Tables[0].Rows.Count > 0)
                //{
                //    voucherytpe = Convert.ToInt32(ds.Tables[0].Rows[0]["ivouchertype"]);
                //    Clsdata.LogFile(doctype, "voucherytpe = " + voucherytpe.ToString(), writelog);
                //}

                //Clsdata.LogFile(doctype, "Document_Number = " + objfocus.Header.Document_Number, writelog);
                //ds = Clsdata.GetData("select sVoucherNo from tCore_Header_0 where  sVoucherNo='" + objfocus.Header.Document_Number + "' and iVoucherType = " + voucherytpe, ccode);
                //if (ds.Tables[0].Rows.Count == 0)
                //{
                //    Clsdata.LogFile(doctype, "Go To Create Invoice", writelog);
                //    objreturn = CreateInvoice(objfocus, ccode);
                //}
                //else
                //{
                //    objreturn.iStatus = 0;
                //    objreturn.sMessage = "Document No Already Exist in Focus";
                //}
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }





            return objreturn;
        }
        public ClsSalesInvoice.SalesResponse CreateInvoice(ClsSalesInvoice.SalesInvoice objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreateInvoice", writelog);
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            Clsdata.LogFile(doctype, "Entered SalesResponse", writelog);

            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Header.Date.Substring(6, 4)), Convert.ToInt32(objfocus.Header.Date.Substring(3, 2)), Convert.ToInt32(objfocus.Header.Date.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Header.DueDate.Substring(6, 4)), Convert.ToInt32(objfocus.Header.DueDate.Substring(3, 2)), Convert.ToInt32(objfocus.Header.DueDate.Substring(0, 2)));
                int duedate = cls.GetDateToInt(dt);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + duedate.ToString(), writelog);
                if (objfocus.Header.Customer_Id == "")
                {
                    Clsdata.LogFile(doctype, "Customer should not be empty" + objfocus.Header.Customer_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Customer Account should not be empty" + objfocus.Header.Customer_Id;
                    return objreturn;
                }
                else
                {
                    if (!IsExist(ccode, "mcore_Account", objfocus.Header.Customer_Id))
                    {
                        Clsdata.LogFile(doctype, "Customer is not exist in focus" + objfocus.Header.Customer_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Customer Account is not exist in focus" + objfocus.Header.Customer_Id;
                        return objreturn;
                    }
                }
                Clsdata.LogFile(doctype, "objfocus.Header.Customer" + objfocus.Header.Customer_Id.ToString(), writelog);
                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    Clsdata.LogFile(doctype, "InvTag" + InvTag, writelog);
                    FinTag = Tags.Split(',')[1];
                    Clsdata.LogFile(doctype, "FinTag" + FinTag, writelog);
                }

                string FaTag_id = objfocus.Header.Branch_Id;
                string InvTag_id = objfocus.Header.VAN_Id;

                if (objfocus.Header.Place_of_supply_Code == "")
                {
                    Clsdata.LogFile(doctype, "Place_of_Supply should not be empty" + objfocus.Header.Place_of_supply_Code, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Place_of_Supply should not be empty" + objfocus.Header.Place_of_supply_Code;
                    return objreturn;
                }
                else
                {
                    if (Getid(objfocus.Header.Place_of_supply_Code, ccode, "mcore_placeofsupply", 1) <= 0)
                    {
                        Clsdata.LogFile(doctype, "Place_of_Supply is not exist in focus" + objfocus.Header.Place_of_supply_Code, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Place_of_Supply is not exist in focus" + objfocus.Header.Place_of_supply_Code;
                        return objreturn;
                    }
                }

                if (objfocus.Header.VAN_Id == "")
                {
                    Clsdata.LogFile(doctype, InvTag + " should not be empty" + objfocus.Header.VAN_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = InvTag + " should not be empty" + objfocus.Header.VAN_Id;
                    return objreturn;
                }
                else
                {
                    string InvType = InvTag.Trim().ToLower() == "outlet".Trim().ToLower() ? "mPos" : "mcore";
                    if (!IsExist(ccode, InvType + "_" + InvTag, InvTag_id))
                    {
                        Clsdata.LogFile(doctype, InvTag + " is not exist in focus" + objfocus.Header.VAN_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = InvTag + " is not exist in focus" + objfocus.Header.VAN_Id;
                        return objreturn;
                    }
                }
                string Judictionid = "";
                if (objfocus.Header.Branch_Id == "")
                {
                    Clsdata.LogFile(doctype, FinTag + " should not be empty" + objfocus.Header.Branch_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = FinTag + " should not be empty" + objfocus.Header.Branch_Id;
                    return objreturn;
                }
                else
                {
                    if (!IsExist(ccode, "mcore_" + FinTag, FaTag_id))
                    {
                        Clsdata.LogFile(doctype, FinTag + " is not exist in focus" + objfocus.Header.Branch_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = FinTag + " is not exist in focus" + objfocus.Header.Branch_Id;
                        return objreturn;
                    }
                    else
                    {
                        Judictionid = GetExtraFeild("Jurisdiction", ccode, "muCore_" + FinTag, FaTag_id);
                        if (Judictionid == "" || Judictionid == "0")
                        {
                            Clsdata.LogFile(doctype, "Jurisdiction is not mapped with " + FinTag + " in focus  ", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Jurisdiction is not mapped with " + FinTag + " in focus  ";
                            return objreturn;
                        }

                    }
                }


                Hashtable header = new Hashtable
                {

                    { "Date",docdate  },
                    //{ "DocNo",objfocus.Header.Document_Number },
                    { "CustomerAC__Id",objfocus.Header.Customer_Id},
                    { "DueDate",duedate },
                    { FinTag+"__Id",FaTag_id },
                    {"Place of supply__Code",objfocus.Header.Place_of_supply_Code },
                    {"Jurisdiction__Id",Judictionid },
                    {InvTag+"__Id", InvTag_id},
                    {"Employee",objfocus.Header.Employee_Id },
                    { "Helper", objfocus.Header.Helper },
                    { "Driver_Mobile_No", objfocus.Header.Driver_Mobile_No },
                    { "Driver", objfocus.Header.Driver },
                    { "LPO_No", objfocus.Header.LPO_No },
                    { "Currency",objfocus.Header.Currency_Id },
                    { "InvoiceType__Code",objfocus.Header.InvoiceType },
            };

                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };
                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    if (objfocus.Body[i].Item_Id == "")
                    {
                        Clsdata.LogFile(doctype, "Item master should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Item master should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsExist(ccode, "mCore_Product", objfocus.Body[i].Item_Id))
                        {
                            Clsdata.LogFile(doctype, "Item is not exist in focus  " + objfocus.Body[i].Item_Id, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Item is not exist in focus" + objfocus.Body[i].Item_Id;
                            return objreturn;
                        }
                        else
                        {
                            objfocus.Body[i].SalesAc = getSalesAc(objfocus.Body[i].Item_Id);
                            if (objfocus.Body[i].SalesAc == "")
                            {
                                Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + objfocus.Body[i].SalesAc, writelog);
                                objreturn.iStatus = 0;
                                objreturn.sMessage = "Sales Account is not mapped with Item in focus" + objfocus.Body[i].SalesAc;
                                return objreturn;
                            }
                        }
                    }
                    if (objfocus.Body[i].TaxCode_Code == "")
                    {
                        Clsdata.LogFile(doctype, "Tax_Code should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Tax_Code should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (Getid(objfocus.Body[i].TaxCode_Code, ccode, "mCore_TaxCode", 1) <= 0)
                        {
                            Clsdata.LogFile(doctype, "Tax Code is not exist in focus  " + objfocus.Body[i].TaxCode_Code, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Tax Code is not exist in focus" + objfocus.Body[i].TaxCode_Code;
                            return objreturn;
                        }
                    }


                    row = new Hashtable
                    {
                        {"Item",objfocus.Body[i].Item_Id },
                        { "Unit__Id", objfocus.Body[i].Unit_Id },
                        {"TaxCode__Code",objfocus.Body[i].TaxCode_Code },
                        { "Description", objfocus.Body[i].Description },
                        { "Quantity", objfocus.Body[i].Qty },
                        { "Selling Price", objfocus.Body[i].Selling_Price },
                        { "Input Discount Amt", objfocus.Body[i].Discount_Amt },
                        { "Add Charges", objfocus.Body[i].Add_Charges },
                        { "VAT", objfocus.Body[i].VAT_Value },
                        { "Actual Quantity", objfocus.Body[i].Actual_Qty },
                        { "FOC Quantity", objfocus.Body[i].FOC_Qty },
                        { "Batch", objfocus.Body[i].Batch },
                        { "SalesAC__Id", objfocus.Body[i].SalesAc },
                        { "Discount %", objfocus.Body[i].DiscountPer },
                    };
                    body.Add(row);
                }


                var postingData = new ClsProperties.PostingData();
                //postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, " SalesInvoice JSon:" + sContent, writelog);
                string err = "";
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/Sales Invoice - VAN";
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, " SalesInvoice response :" + response, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = "SalesInvoice Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region Return
        public ClsSalesReturn.SalesResponse SalesReturn(ClsSalesReturn.SalesReturn objfocus)
        {
            doctype = "SalesReturn";
            ClsSalesReturn.SalesResponse objreturn = new ClsSalesReturn.SalesResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }

                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }

                ccode = cls.GetCompanyId(companycode);
                objreturn = CreateReturn(objfocus, ccode);
                //int voucherytpe = 0;
                //DataSet ds = Clsdata.GetData("select ivouchertype from ccore_vouchers_0 where sName='Sales Return - VAN'", ccode);
                //if (ds.Tables[0].Rows.Count > 0)
                //{
                //    voucherytpe = Convert.ToInt32(ds.Tables[0].Rows[0]["ivouchertype"]);
                //}
                //ds = Clsdata.GetData("select sVoucherNo from tCore_Header_0 where  sVoucherNo='" + objfocus.Header.Document_Number + "' and iVoucherType = " + voucherytpe, ccode);
                //if (ds.Tables[0].Rows.Count == 0)
                //{

                //    objreturn = CreateReturn(objfocus, ccode);

                //}
                //else
                //{
                //    objreturn.iStatus = 0;
                //    objreturn.sMessage = "Document No Already Exist in Focus";
                //}
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }
            return objreturn;
        }
        public ClsSalesReturn.SalesResponse CreateReturn(ClsSalesReturn.SalesReturn objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreateReturn", writelog);
            ClsSalesReturn.SalesResponse objreturn = new ClsSalesReturn.SalesResponse();
            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Header.Date.Substring(6, 4)), Convert.ToInt32(objfocus.Header.Date.Substring(3, 2)), Convert.ToInt32(objfocus.Header.Date.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Header.DueDate.Substring(6, 4)), Convert.ToInt32(objfocus.Header.DueDate.Substring(3, 2)), Convert.ToInt32(objfocus.Header.DueDate.Substring(0, 2)));
                int duedate = cls.GetDateToInt(dt);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + duedate.ToString(), writelog);
                if (objfocus.Header.Customer_Id == "")
                {
                    Clsdata.LogFile(doctype, "Customer should not be empty" + objfocus.Header.Customer_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Customer Account should not be empty" + objfocus.Header.Customer_Id;
                    return objreturn;
                }
                else
                {
                    if (!IsExist(ccode, "mcore_Account", objfocus.Header.Customer_Id))
                    {
                        Clsdata.LogFile(doctype, "Customer is not exist in focus" + objfocus.Header.Customer_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Customer Account is not exist in focus" + objfocus.Header.Customer_Id;
                        return objreturn;
                    }
                }

                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    FinTag = Tags.Split(',')[1];
                }

                string FaTag_id = objfocus.Header.Branch_Id;
                string InvTag_id = objfocus.Header.VAN_Id;

                if (objfocus.Header.Place_of_supply_Code == "")
                {
                    Clsdata.LogFile(doctype, "Place_of_Supply should not be empty" + objfocus.Header.Place_of_supply_Code, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Place_of_Supply should not be empty" + objfocus.Header.Place_of_supply_Code;
                    return objreturn;
                }
                else
                {
                    if (Getid(objfocus.Header.Place_of_supply_Code, ccode, "mcore_placeofsupply", 1) <= 0)
                    {
                        Clsdata.LogFile(doctype, "Place_of_Supply is not exist in focus" + objfocus.Header.Place_of_supply_Code, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Place_of_Supply is not exist in focus" + objfocus.Header.Place_of_supply_Code;
                        return objreturn;
                    }
                }

                if (objfocus.Header.VAN_Id == "")
                {
                    Clsdata.LogFile(doctype, InvTag + " should not be empty" + objfocus.Header.VAN_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = InvTag + " should not be empty" + objfocus.Header.VAN_Id;
                    return objreturn;
                }
                else
                {
                    string InvType = InvTag.Trim().ToLower() == "outlet".Trim().ToLower() ? "mPos" : "mcore";
                    if (!IsExist(ccode, InvType + "_" + InvTag, InvTag_id))
                    {
                        Clsdata.LogFile(doctype, InvTag + " is not exist in focus" + objfocus.Header.VAN_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = InvTag + " is not exist in focus" + objfocus.Header.VAN_Id;
                        return objreturn;
                    }
                }
                string Judictionid = "";
                if (objfocus.Header.Branch_Id == "")
                {
                    Clsdata.LogFile(doctype, FinTag + " should not be empty" + objfocus.Header.Branch_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = FinTag + " should not be empty" + objfocus.Header.Branch_Id;
                    return objreturn;
                }
                else
                {
                    if (!IsExist(ccode, "mcore_" + FinTag, FaTag_id))
                    {
                        Clsdata.LogFile(doctype, FinTag + " is not exist in focus" + objfocus.Header.Branch_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = FinTag + " is not exist in focus" + objfocus.Header.Branch_Id;
                        return objreturn;
                    }
                    else
                    {
                        Judictionid = GetExtraFeild("Jurisdiction", ccode, "muCore_" + FinTag, FaTag_id);
                        if (Judictionid == "" || Judictionid == "0")
                        {
                            Clsdata.LogFile(doctype, "Jurisdiction is not mapped with " + FinTag + " in focus  ", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Jurisdiction is not mapped with " + FinTag + " in focus  ";
                            return objreturn;
                        }

                    }
                }


                Hashtable header = new Hashtable
                {
                    { "Date",docdate  },
                    //{ "DocNo",objfocus.Header.Document_Number },
                    { "CustomerAC__Id",objfocus.Header.Customer_Id},
                    { "DueDate",duedate },
                    { FinTag+"__Id",FaTag_id },
                    {"Place of supply__Code",objfocus.Header.Place_of_supply_Code },
                    {"Jurisdiction__Id",Judictionid },
                    {InvTag+"__Id", InvTag_id},
                    {"Employee",objfocus.Header.Employee_Id },
                    { "Helper", objfocus.Header.Helper },
                    { "Driver_Mobile_No", objfocus.Header.Driver_Mobile_No },
                    { "Driver", objfocus.Header.Driver },
                    { "LPO_No", objfocus.Header.LPO_No },
                    { "Currency",objfocus.Header.Currency_Id },
                    { "InvoiceType__Code",objfocus.Header.InvoiceType },
            };


                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };
                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    if (objfocus.Body[i].Item_Id == "")
                    {
                        Clsdata.LogFile(doctype, "Item master should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Item master should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsExist(ccode, "mCore_Product", objfocus.Body[i].Item_Id))
                        {
                            Clsdata.LogFile(doctype, "Item is not exist in focus  " + objfocus.Body[i].Item_Id, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Item is not exist in focus" + objfocus.Body[i].Item_Id;
                            return objreturn;
                        }
                        else
                        {
                            objfocus.Body[i].SalesAc = getSalesAc(objfocus.Body[i].Item_Id);
                            if (objfocus.Body[i].SalesAc == "")
                            {
                                Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + objfocus.Body[i].SalesAc, writelog);
                                objreturn.iStatus = 0;
                                objreturn.sMessage = "Sales Account is not mapped with Item in focus" + objfocus.Body[i].SalesAc;
                                return objreturn;
                            }
                        }
                    }
                    if (objfocus.Body[i].TaxCode_Code == "")
                    {
                        Clsdata.LogFile(doctype, "Tax_Code should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Tax_Code should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (Getid(objfocus.Body[i].TaxCode_Code, ccode, "mCore_TaxCode", 1) <= 0)
                        {
                            Clsdata.LogFile(doctype, "Tax Code is not exist in focus  " + objfocus.Body[i].TaxCode_Code, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Tax Code is not exist in focus" + objfocus.Body[i].TaxCode_Code;
                            return objreturn;
                        }
                    }

                    int MfgDate = 0;
                    int ExpDate = 0;
                    if (objfocus.Body[i].MfgDate == null)
                    {
                        MfgDate = 0;
                    }
                    else if (objfocus.Body[i].MfgDate.ToString() == "01-01-1900 00:00:00" || objfocus.Body[i].MfgDate.ToString().ToUpper() == "NULL" || objfocus.Body[i].MfgDate.ToString() == "")
                    {
                        MfgDate = 0;
                    }
                    else
                    {
                        DateTime mdt = new DateTime(Convert.ToInt32(objfocus.Body[i].MfgDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[i].MfgDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[i].MfgDate.Substring(0, 2)));
                        Clsdata.LogFile(doctype, "MfgDate" + mdt.ToString(), writelog);
                        MfgDate = cls.GetDateToInt(mdt);
                    }

                    if (objfocus.Body[i].ExpDate == null)
                    {
                        ExpDate = 0;
                    }
                    else if (objfocus.Body[i].ExpDate.ToString() == "01-01-1900 00:00:00" || objfocus.Body[i].ExpDate.ToString().ToUpper() == "NULL" || objfocus.Body[i].ExpDate.ToString() == "")
                    {
                        ExpDate = 0;
                    }
                    else
                    {
                        DateTime edt = new DateTime(Convert.ToInt32(objfocus.Body[i].ExpDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[i].ExpDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[i].ExpDate.Substring(0, 2)));
                        Clsdata.LogFile(doctype, "MfgExpDateDate" + edt.ToString(), writelog);
                        ExpDate = cls.GetDateToInt(edt);
                    }

                    List<Hashtable> LinkList = new List<Hashtable>();
                    List<Hashtable> RefList = new List<Hashtable>();
                    if (objfocus.Body[i].Link_Invoice.VoucherNo != null && objfocus.Body[i].Link_Invoice.VoucherNo != "")
                    {
                        int TransID = 0, TransQty = 0;

                        DataSet TransDs = GetReturnDSTranID(objfocus.Body[i].Link_Invoice.VoucherNo, Convert.ToInt32(objfocus.Body[i].Item_Id), objfocus.Body[i].Batch, 1);
                        if (TransDs.Tables[0].Rows.Count > 0)
                        {
                            TransID = Convert.ToInt32(TransDs.Tables[0].Rows[0]["ibodyid"]);
                            int LinkID = Convert.ToInt32(TransDs.Tables[0].Rows[0]["iLinkId"]);
                            decimal UsedValue = Convert.ToDecimal(TransDs.Tables[0].Rows[0]["penvalue"]);
                            Clsdata.LogFile(doctype, "TransID = " + TransID.ToString() + " Linkid = " + LinkID.ToString() + " UsedValue = " + UsedValue.ToString(), writelog);
                            Hashtable LinkRef = new Hashtable
                         {
                             { "BaseTransactionId", TransID},
                             { "VoucherType", "3336"},
                             { "VoucherNo", "SINVV:" + TransDs.Tables[0].Rows[0]["svoucherno"].ToString()},
                             { "UsedValue", UsedValue},
                             { "LinkId",  LinkID},
                             { "RefId", TransID }
                         };

                            LinkList.Add(LinkRef);
                        }
                        else
                        {
                            Clsdata.LogFile(doctype, "Link Reference Not Found", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Link Reference Not Found";
                            return objreturn;
                        }

                        int iref = 0;
                        Clsdata.LogFile(doctype, "BillReference = " + objfocus.Body[i].Link_Invoice.VoucherNo, writelog);
                        DataSet DsRef = GetIRefDs(objfocus.Body[i].Link_Invoice.VoucherNo);
                        if (DsRef.Tables[0].Rows.Count > 0)
                        {
                            iref = Convert.ToInt32(DsRef.Tables[0].Rows[0]["RefId"]);
                            Clsdata.LogFile(doctype, "iref = " + iref.ToString(), writelog);
                            #region BillRef
                            if (iref != 0)
                            {
                                string reference = DsRef.Tables[0].Rows[0]["VNo"] + ":" + DsRef.Tables[0].Rows[0]["Date"].ToString();
                                decimal qty = Convert.ToDecimal(objfocus.Body[i].Qty);
                                decimal RefQuantity = Convert.ToDecimal(DsRef.Tables[0].Rows[0]["Quantity"]);
                                decimal CalcValue = Convert.ToDecimal(DsRef.Tables[0].Rows[0]["fOrigNet"]) / RefQuantity;
                                decimal Amount = CalcValue * qty;

                                Hashtable billRef = new Hashtable();
                                billRef.Add("aptag", 0);
                                billRef.Add("CustomerId", objfocus.Header.Customer_Id);
                                billRef.Add("Amount", Amount);
                                billRef.Add("BillNo", "");
                                billRef.Add("reftype", 2);
                                billRef.Add("mastertypeid", 0);
                                billRef.Add("Reference", reference);
                                billRef.Add("artag", 0);
                                billRef.Add("ref", iref);
                                billRef.Add("tag", 0);
                                RefList.Add(billRef);
                            }
                            #endregion
                        }
                    }
                    row = new Hashtable
                    {
                        {"Item",objfocus.Body[i].Item_Id },
                        { "Unit__Id", objfocus.Body[i].Unit_Id },
                        {"TaxCode__Code",objfocus.Body[i].TaxCode_Code },
                        { "Description", objfocus.Body[i].Description },
                        { "Quantity", objfocus.Body[i].Qty },
                        { "Selling Price", objfocus.Body[i].Selling_Price },
                        { "Input Discount Amt", objfocus.Body[i].Discount_Amt },
                        { "Add Charges", objfocus.Body[i].Add_Charges },
                        { "VAT", objfocus.Body[i].VAT_Value },
                        { "Actual Quantity", objfocus.Body[i].Actual_Qty },
                        { "FOC Quantity", objfocus.Body[i].FOC_Qty },
                        { "Batch", objfocus.Body[i].Batch },
                        { "SalesAC__Id", objfocus.Body[i].SalesAc },
                        { "L-Sales Invoice - VAN", LinkList },
                        { "Reference", RefList },
                        { "MfgDate",MfgDate },
                        { "ExpDate", ExpDate },
                        { "Discount %", objfocus.Body[i].DiscountPer },
                    };
                    body.Add(row);
                }

                var postingData = new ClsProperties.PostingData();
                //postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, " SalesInvoice JSon:" + sContent, writelog);
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/Sales Return - VAN";
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, " SalesReturn response :" + response, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = "SalesRetrun Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region Adjustment
        public ClsAdjustment.AdjustmentResponse Adjustment(ClsAdjustment.Adjustment objfocus)
        {
            doctype = "Adjustments";
            ClsAdjustment.AdjustmentResponse objreturn = new ClsAdjustment.AdjustmentResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }

                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }

                ccode = cls.GetCompanyId(companycode);

                int voucherytpe = 0;
                string VoucherTypeName = "";
                if (objfocus.Body[0].Amount < 0)
                {
                    VoucherTypeName = "Cash Payment VAN";
                }
                else
                {
                    DateTime dt = new DateTime(Convert.ToInt32(objfocus.Header.MaturityDate.Substring(6, 4)), Convert.ToInt32(objfocus.Header.MaturityDate.Substring(3, 2)), Convert.ToInt32(objfocus.Header.MaturityDate.Substring(0, 2)));
                    Clsdata.LogFile(doctype, "MaturityDate" + dt.ToString(), writelog);
                    int docdate = cls.GetDateToInt(dt);

                    DateTime dt2 = new DateTime(Convert.ToInt32(objfocus.Header.Date.Substring(6, 4)), Convert.ToInt32(objfocus.Header.Date.Substring(3, 2)), Convert.ToInt32(objfocus.Header.Date.Substring(0, 2)));
                    Clsdata.LogFile(doctype, "date" + dt2.ToString(), writelog);


                    if (dt == dt2 && objfocus.Header.sChequeNo == null)
                    {
                        VoucherTypeName = "Cash Receipts VAN";
                    }
                    else
                    {
                        VoucherTypeName = "Post Dated Receipt VAN";
                    }
                }
                objfocus.Header.VoucherType = VoucherTypeName;
                Clsdata.LogFile(doctype, " VoucherTypeName = " + VoucherTypeName, writelog);
                objreturn = CreateAdjustment(objfocus, ccode);
                //DataSet ds = Clsdata.GetData("select ivouchertype from ccore_vouchers_0 where sName='"+ VoucherTypeName + "'", ccode);
                //if (ds.Tables[0].Rows.Count > 0)
                //{
                //    voucherytpe = Convert.ToInt32(ds.Tables[0].Rows[0]["ivouchertype"]);
                //}

                //ds = Clsdata.GetData("select sVoucherNo from tCore_Header_0 where  sVoucherNo='" + objfocus.Header.Document_Number + "' and iVoucherType = " + voucherytpe, ccode);
                //if (ds.Tables[0].Rows.Count == 0)
                //{
                //    objreturn = CreateAdjustment(objfocus, ccode);
                //}
                //else
                //{
                //    objreturn.iStatus = 0;
                //    objreturn.sMessage = "Document No Already Exist in Focus";
                //}
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }
            return objreturn;
        }
        public ClsAdjustment.AdjustmentResponse CreateAdjustment(ClsAdjustment.Adjustment objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreateAdjustment", writelog);
            ClsAdjustment.AdjustmentResponse objreturn = new ClsAdjustment.AdjustmentResponse();
            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Header.Date.Substring(6, 4)), Convert.ToInt32(objfocus.Header.Date.Substring(3, 2)), Convert.ToInt32(objfocus.Header.Date.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                DateTime dt2 = new DateTime(Convert.ToInt32(objfocus.Header.MaturityDate.Substring(6, 4)), Convert.ToInt32(objfocus.Header.MaturityDate.Substring(3, 2)), Convert.ToInt32(objfocus.Header.MaturityDate.Substring(0, 2)));
                int duedate = cls.GetDateToInt(dt2);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + duedate.ToString(), writelog);
                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    FinTag = Tags.Split(',')[1];
                }

                string FaTag_id = objfocus.Header.Branch_Id;
                string InvTag_id = objfocus.Header.VAN_Id;


                if (objfocus.Header.VAN_Id == "")
                {
                    Clsdata.LogFile(doctype, InvTag + " should not be empty" + objfocus.Header.VAN_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = InvTag + " should not be empty" + objfocus.Header.VAN_Id;
                    return objreturn;
                }
                else
                {
                    string InvType = InvTag.Trim().ToLower() == "outlet".Trim().ToLower() ? "mPos" : "mcore";
                    if (!IsExist(ccode, InvType + "_" + InvTag, InvTag_id))
                    {
                        Clsdata.LogFile(doctype, InvTag + " is not exist in focus" + objfocus.Header.VAN_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = InvTag + " is not exist in focus" + objfocus.Header.VAN_Id;
                        return objreturn;
                    }
                }
                if (objfocus.Header.Branch_Id == "")
                {
                    Clsdata.LogFile(doctype, FinTag + " should not be empty", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = FinTag + " should not be empty";
                    return objreturn;
                }
                else
                {
                    if (!IsExist(ccode, "mcore_" + FinTag, FaTag_id))
                    {
                        Clsdata.LogFile(doctype, FinTag + " is not exist in focus" + objfocus.Header.Branch_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = FinTag + " is not exist in focus" + objfocus.Header.Branch_Id;
                        return objreturn;
                    }
                }
                {
                    if (objfocus.Header.Employee_Id == "")
                    {
                        Clsdata.LogFile(doctype, "Employee should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Employee should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsExist(ccode, "mPay_Employee", objfocus.Header.Employee_Id))
                        {
                            Clsdata.LogFile(doctype, "Employee is not exist in focus " + objfocus.Header.Employee_Id, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Employee is not exist in focus " + objfocus.Header.Employee_Id;
                            return objreturn;
                        }
                        else
                        {
                            if (objfocus.Header.CashBankAC == "" || objfocus.Header.CashBankAC == null || objfocus.Header.CashBankAC.ToLower() == "null")
                            {
                                objfocus.Header.CashBankAC = GetExtraFeild("CashAccountVAN", ccode, "vmPay_Employee", objfocus.Header.Employee_Id);
                                if (objfocus.Header.CashBankAC == "" || objfocus.Header.CashBankAC == "-1" || objfocus.Header.CashBankAC == "0" || objfocus.Header.CashBankAC == null || objfocus.Header.CashBankAC.ToLower() == "null")
                                {
                                    Clsdata.LogFile(doctype, "Cash/Bank Account is not mapped with Employee " + objfocus.Header.Employee_Id + " in focus", writelog);
                                    objreturn.iStatus = 0;
                                    objreturn.sMessage = "Cash/Bank Account is not mapped with Employee " + objfocus.Header.Employee_Id + " in focus";
                                    return objreturn;
                                }
                            }
                        }
                    }

                }
                if (objfocus.Header.VoucherType.ToUpper() == "Post Dated Receipt VAN".ToUpper())
                {
                    if (objfocus.Header.sChequeNo == "" || objfocus.Header.sChequeNo == null)
                    {
                        Clsdata.LogFile(doctype, "Cheque No is Mandatory", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Cheque No is Mandatory";
                        return objreturn;
                    }
                }
                Hashtable header = new Hashtable
                {

                    { "Date",docdate  },
                    { "CashBankAC",objfocus.Header.CashBankAC},
                    { "MaturityDate",duedate },
                    { FinTag+"__Id",FaTag_id },
                    {InvTag+"__Id", InvTag_id},
                    {"Employee",objfocus.Header.Employee_Id },
                    { "Currency",objfocus.Header.Currency_Id },
                    { "sChequeNo",objfocus.Header.sChequeNo },
            };

                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };
                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    if (objfocus.Body[i].Customer_Id == "")
                    {
                        Clsdata.LogFile(doctype, "Customer should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Customer Account should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsExist(ccode, "mcore_Account", objfocus.Body[i].Customer_Id))
                        {
                            Clsdata.LogFile(doctype, "Customer is not exist in focus" + objfocus.Body[i].Customer_Id, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Customer Account is not exist in focus" + objfocus.Body[i].Customer_Id;
                            return objreturn;
                        }
                    }
                    List<Hashtable> RefList = new List<Hashtable>();
                    foreach (var objref in objfocus.Body[i].Reference)
                    {
                        string reference = "";
                        int _ref = 0;
                        Clsdata.LogFile(doctype, " InvoiceNo :" + objref.InvoiceNo, writelog);
                        if (objref.InvoiceNo != null && objref.InvoiceNo != "")
                        {
                            DataSet DsRef = GetIRefDs(objref.InvoiceNo.ToString());
                            if (DsRef.Tables[0].Rows.Count > 0)
                            {
                                if (Convert.ToInt32(DsRef.Tables[0].Rows[0]["RefId"]) != 0)
                                {
                                    reference = DsRef.Tables[0].Rows[0]["VNo"] + ":" + DsRef.Tables[0].Rows[0]["Date"].ToString();
                                    _ref = Convert.ToInt32(DsRef.Tables[0].Rows[0]["RefId"]);
                                    Hashtable billRef = new Hashtable();
                                    billRef.Add("aptag", 0);
                                    billRef.Add("CustomerId", objfocus.Body[i].Customer_Id);
                                    billRef.Add("Amount", Math.Abs(Convert.ToDecimal(objref.Amount)));
                                    billRef.Add("BillNo", "");
                                    billRef.Add("reftype", 2);
                                    billRef.Add("mastertypeid", 0);
                                    billRef.Add("Reference", reference);
                                    billRef.Add("artag", 0);
                                    billRef.Add("ref", _ref);
                                    billRef.Add("tag", 0);
                                    RefList.Add(billRef);
                                }
                                else
                                {
                                    row = new Hashtable
                                    {
                                        { "Account", objfocus.Body[i].Customer_Id},
                                        { "Amount",Math.Abs(objfocus.Body[i].Amount)},
                                        //{ "Reference",RefList},
                                        { "sRemarks",objfocus.Body[i].sRemarks}

                                    };
                                    body.Add(row);
                                    //Clsdata.LogFile(doctype, "Refernece Not Found for " + objref.InvoiceNo.ToString(), writelog);
                                    //objreturn.iStatus = 0;
                                    //objreturn.sMessage = "Refernece Not Found for " + objref.InvoiceNo.ToString();
                                    //return objreturn;
                                }
                            }
                            else
                            {
                                row = new Hashtable
                                    {
                                        { "Account", objfocus.Body[i].Customer_Id},
                                        { "Amount",Math.Abs(objfocus.Body[i].Amount)},
                                        //{ "Reference",RefList},
                                        { "sRemarks",objfocus.Body[i].sRemarks}

                                    };
                                body.Add(row);
                            }
                        }
                        else
                        {
                            row = new Hashtable
                                    {
                                        { "Account", objfocus.Body[i].Customer_Id},
                                        { "Amount",Math.Abs(objfocus.Body[i].Amount)},
                                        //{ "Reference",RefList},
                                        { "sRemarks",objfocus.Body[i].sRemarks}

                                    };
                            body.Add(row);
                        }
                    }
                }


                var postingData = new ClsProperties.PostingData();
                //postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, objfocus.Header.VoucherType + " JSon:" + sContent, writelog);
                string err = "";
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/" + objfocus.Header.VoucherType;
                Clsdata.LogFile(doctype, " url :" + url, writelog);
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, objfocus.Header.VoucherType + " response :" + response, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = objfocus.Header.VoucherType + " Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region Order
        public ClsSalesInvoice.SalesResponse SalesOrder(ClsSalesInvoice.SalesInvoice objfocus)
        {
            doctype = "SalesOrder";
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }
                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }
                ccode = cls.GetCompanyId(companycode);
                Clsdata.LogFile(doctype, "CompanyCode = " + ccode.ToString(), writelog);
                Clsdata.LogFile(doctype, "Go To Create Order", writelog);
                objreturn = CreateOrder(objfocus, ccode);
               
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }





            return objreturn;
        }
        public ClsSalesInvoice.SalesResponse CreateOrder(ClsSalesInvoice.SalesInvoice objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreateOrder", writelog);
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            Clsdata.LogFile(doctype, "Entered SalesResponse", writelog);

            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Header.Date.Substring(6, 4)), Convert.ToInt32(objfocus.Header.Date.Substring(3, 2)), Convert.ToInt32(objfocus.Header.Date.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Header.DueDate.Substring(6, 4)), Convert.ToInt32(objfocus.Header.DueDate.Substring(3, 2)), Convert.ToInt32(objfocus.Header.DueDate.Substring(0, 2)));
                int duedate = cls.GetDateToInt(dt);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + duedate.ToString(), writelog);
                if (objfocus.Header.Customer_Id == "")
                {
                    Clsdata.LogFile(doctype, "Customer should not be empty" + objfocus.Header.Customer_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Customer Account should not be empty" + objfocus.Header.Customer_Id;
                    return objreturn;
                }
                else
                {
                    if (!IsExist(ccode, "mcore_Account", objfocus.Header.Customer_Id))
                    {
                        Clsdata.LogFile(doctype, "Customer is not exist in focus" + objfocus.Header.Customer_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Customer Account is not exist in focus" + objfocus.Header.Customer_Id;
                        return objreturn;
                    }
                }
                Clsdata.LogFile(doctype, "objfocus.Header.Customer" + objfocus.Header.Customer_Id.ToString(), writelog);
                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    Clsdata.LogFile(doctype, "InvTag" + InvTag, writelog);
                    FinTag = Tags.Split(',')[1];
                    Clsdata.LogFile(doctype, "FinTag" + FinTag, writelog);
                }

                string FaTag_id = objfocus.Header.Branch_Id;
                string InvTag_id = "";
                string SalesOrderVan = WebConfigurationManager.AppSettings["SalesOrderVan"];
                if (SalesOrderVan == "1")
                {
                    InvTag_id = objfocus.Header.VAN_Id;
                    if (objfocus.Header.VAN_Id == "")
                    {
                        Clsdata.LogFile(doctype, InvTag + " should not be empty" + objfocus.Header.VAN_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = InvTag + " should not be empty" + objfocus.Header.VAN_Id;
                        return objreturn;
                    }
                    else
                    {
                        string InvType = InvTag.Trim().ToLower() == "outlet".Trim().ToLower() ? "mPos" : "mcore";
                        if (!IsExist(ccode, InvType + "_" + InvTag, InvTag_id))
                        {
                            Clsdata.LogFile(doctype, InvTag + " is not exist in focus" + objfocus.Header.VAN_Id, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = InvTag + " is not exist in focus" + objfocus.Header.VAN_Id;
                            return objreturn;
                        }
                    }
                }
                
                if (objfocus.Header.Place_of_supply_Code == "")
                {
                    Clsdata.LogFile(doctype, "Place_of_Supply should not be empty" + objfocus.Header.Place_of_supply_Code, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Place_of_Supply should not be empty" + objfocus.Header.Place_of_supply_Code;
                    return objreturn;
                }
                else
                {
                    if (Getid(objfocus.Header.Place_of_supply_Code, ccode, "mcore_placeofsupply", 1) <= 0)
                    {
                        Clsdata.LogFile(doctype, "Place_of_Supply is not exist in focus" + objfocus.Header.Place_of_supply_Code, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Place_of_Supply is not exist in focus" + objfocus.Header.Place_of_supply_Code;
                        return objreturn;
                    }
                }

                
                string Judictionid = "";
                if (objfocus.Header.Branch_Id == "")
                {
                    Clsdata.LogFile(doctype, FinTag + " should not be empty" + objfocus.Header.Branch_Id, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = FinTag + " should not be empty" + objfocus.Header.Branch_Id;
                    return objreturn;
                }
                else
                {
                    if (!IsExist(ccode, "mcore_" + FinTag, FaTag_id))
                    {
                        Clsdata.LogFile(doctype, FinTag + " is not exist in focus" + objfocus.Header.Branch_Id, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = FinTag + " is not exist in focus" + objfocus.Header.Branch_Id;
                        return objreturn;
                    }
                    else
                    {
                        Judictionid = GetExtraFeild("Jurisdiction", ccode, "muCore_" + FinTag, FaTag_id);
                        if (Judictionid == "" || Judictionid == "0")
                        {
                            Clsdata.LogFile(doctype, "Jurisdiction is not mapped with " + FinTag + " in focus  ", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Jurisdiction is not mapped with " + FinTag + " in focus  ";
                            return objreturn;
                        }

                    }
                }


                Hashtable header = new Hashtable
                {

                    { "Date",docdate  },
                    //{ "DocNo",objfocus.Header.Document_Number },
                    { "CustomerAC__Id",objfocus.Header.Customer_Id},
                    { "DueDate",duedate },
                    { FinTag+"__Id",FaTag_id },
                    {"Place of supply__Code",objfocus.Header.Place_of_supply_Code },
                    {"Jurisdiction__Id",Judictionid },
                    {InvTag+"__Id", InvTag_id},
                    {"Employee",objfocus.Header.Employee_Id },
                    { "Helper", objfocus.Header.Helper },
                    { "Driver_Mobile_No", objfocus.Header.Driver_Mobile_No },
                    { "Driver", objfocus.Header.Driver },
                    { "LPO_No", objfocus.Header.LPO_No },
                    { "Currency",objfocus.Header.Currency_Id },
            };

                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };
                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    if (objfocus.Body[i].Item_Id == "")
                    {
                        Clsdata.LogFile(doctype, "Item master should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Item master should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsExist(ccode, "mCore_Product", objfocus.Body[i].Item_Id))
                        {
                            Clsdata.LogFile(doctype, "Item is not exist in focus  " + objfocus.Body[i].Item_Id, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Item is not exist in focus" + objfocus.Body[i].Item_Id;
                            return objreturn;
                        }
                        else
                        {
                            objfocus.Body[i].SalesAc = getSalesAc(objfocus.Body[i].Item_Id);
                            if (objfocus.Body[i].SalesAc == "")
                            {
                                Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + objfocus.Body[i].SalesAc, writelog);
                                objreturn.iStatus = 0;
                                objreturn.sMessage = "Sales Account is not mapped with Item in focus" + objfocus.Body[i].SalesAc;
                                return objreturn;
                            }
                        }
                    }
                    if (objfocus.Body[i].TaxCode_Code == "")
                    {
                        Clsdata.LogFile(doctype, "Tax_Code should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Tax_Code should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (Getid(objfocus.Body[i].TaxCode_Code, ccode, "mCore_TaxCode", 1) <= 0)
                        {
                            Clsdata.LogFile(doctype, "Tax Code is not exist in focus  " + objfocus.Body[i].TaxCode_Code, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Tax Code is not exist in focus" + objfocus.Body[i].TaxCode_Code;
                            return objreturn;
                        }
                    }


                    row = new Hashtable
                    {
                        {"Item",objfocus.Body[i].Item_Id },
                        { "Unit__Id", objfocus.Body[i].Unit_Id },
                        {"TaxCode__Code",objfocus.Body[i].TaxCode_Code },
                        { "Description", objfocus.Body[i].Description },
                        { "Quantity", objfocus.Body[i].Qty },
                        { "Selling Price", objfocus.Body[i].Selling_Price },
                        { "Input Discount Amt", objfocus.Body[i].Discount_Amt },
                        { "Add Charges", objfocus.Body[i].Add_Charges },
                        { "VAT", objfocus.Body[i].VAT_Value },
                        { "Actual Quantity", objfocus.Body[i].Actual_Qty },
                        { "FOC Quantity", objfocus.Body[i].FOC_Qty },
                        { "Batch", objfocus.Body[i].Batch },
                        { "SalesAC__Id", objfocus.Body[i].SalesAc },
                        { "Discount %", objfocus.Body[i].DiscountPer },
                    };
                    body.Add(row);
                }


                var postingData = new ClsProperties.PostingData();
                //postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, " SalesOrder JSon:" + sContent, writelog);
                string err = "";
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/Sales Order - VAN";
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, " SalesOrder response :" + response, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = "SalesOrder Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region Common Methods
        public static string Post(string url, string data, string sessionId, ref string err)
        {
            try
            {
                using (var client = new WebClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    client.Encoding = Encoding.UTF8;
                    client.Timeout = 30 * 60 * 1000;
                    client.Headers.Add("fSessionId", sessionId);
                    client.Headers.Add("Content-Type", "application/json");
                    var response = client.UploadString(url, data);
                    return response;
                }
            }
            catch (Exception e)
            {
                err = e.Message;
                return null;
            }

        }
        public bool logout(string sessionid, string serverip)
        {
            bool flg = false;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("fsessionid", sessionid);
                client.Timeout = 10 * 60 * 1000;
                client.Headers.Add("Content-Type", "application/json");
                var arrResponse = client.DownloadString("http://" + serverip + "/focus8API/Logout");
                flg = true;
            }
            return flg;
        }
        public string getsessionid(string usrename, string password, string companycode)
        {
            string sid = "";
            ClsProperties.Datum datanum = new ClsProperties.Datum();
            datanum.CompanyCode = companycode;
            datanum.Username = usrename;
            datanum.password = password;
            List<ClsProperties.Datum> lstd = new List<ClsProperties.Datum>();
            lstd.Add(datanum);
            ClsProperties.Lolgin lngdata = new ClsProperties.Lolgin();
            lngdata.data = lstd;
            string sContent = JsonConvert.SerializeObject(lngdata);
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Timeout = 10 * 60 * 1000;
                var arrResponse = client.UploadString("http://" + serverip + "/focus8API/Login", sContent);
                //returnObject = new clsDeserialize().Deserialize<RootObject>(arrResponse);
                ClsProperties.Resultlogin lng = JsonConvert.DeserializeObject<ClsProperties.Resultlogin>(arrResponse);

                sid = lng.data[0].fSessionId;


            }

            return sid;
        }
        public int Getid(string name, int ccode, string tablename, int type)
        {
            int id = 0;
            DataSet dss = new DataSet();
            string qry = "";
            if (type == 0)
            {
                qry = "select imasterid from  " + tablename + "   where sname ='" + name + "' and istatus<>5 and bgroup=0  ";
                dss = Clsdata.GetData(qry, ccode);

            }
            else
            {
                qry = "select imasterid from  " + tablename + "  where scode ='" + name + "' and istatus<>5 and bgroup=0  ";
                dss = Clsdata.GetData(qry, ccode);
            }
            Clsdata.LogFile(doctype, qry, writelog);
            if (dss.Tables[0].Rows.Count > 0)
            {
                id = Convert.ToInt32(dss.Tables[0].Rows[0]["imasterid"]);
            }

            return id;
        }
        public string GetTagName(int ccode)
        {
            string TagName = "";
            try
            {
                string qry = "select(SELECT sMasterName  FROM cCore_MasterDef WHERE iMasterTypeId = (SELECT iValue FROM cCore_PreferenceVal_0 WHERE iCategory = 0 and iFieldId = 1)) Invtag,(SELECT sMasterName FROM cCore_MasterDef WHERE iMasterTypeId = (SELECT iValue FROM cCore_PreferenceVal_0 WHERE iCategory = 0 and iFieldId = 0)) FinTag";
                Clsdata.LogFile(doctype, qry, writelog);
                DataSet dss = Clsdata.GetData(qry, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    TagName = dss.Tables[0].Rows[0]["Invtag"].ToString() + "," + dss.Tables[0].Rows[0]["FinTag"].ToString();
                }
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(doctype, ex.Message, writelog);
            }
            return TagName;
        }
        public string GetExtraFeild(string TagName, int ccode, string TableName, string masterid)
        {
            string TagValue = "";
            try
            {
                string sql = "select " + TagName + " from " + TableName + " where iMasterId = " + masterid;
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    TagValue = dss.Tables[0].Rows[0][0].ToString();
                    Clsdata.LogFile(doctype, "TagValue = " + TagValue, writelog);
                }
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(doctype, ex.Message, writelog);
            }
            return TagValue;
        }
        public bool IsExist(int ccode, string TableName, string val)
        {
            bool IsExists = false;
            try
            {
                string sql = "select 1 from " + TableName + " where iMasterId = " + val;
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    IsExists = true;
                }
                else
                {
                    IsExists = false;
                }
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(doctype, ex.Message, writelog);
            }
            return IsExists;
        }
        public DataSet GetReturnDSTranID(string DocNo, int ID, string BatchNo, int Type)
        {
            #region Default Acc

            int LinkPathId = 0;
            int InvVchr = 0;
            string sql = "";

            string Lsql = $@"declare @inv int;
                            declare @ret int;
                            set @inv = (select iVoucherType from cCore_Vouchers_0 where sName = 'Sales Invoice - VAN');
                            set @ret = (select iVoucherType from cCore_Vouchers_0 where sName = 'Sales Return - VAN');
                            select ilinkpathid,@inv invtype,@ret rettype from vmCore_Links_0 with (ReadUnCommitted)  where BaseVoucherId=@inv and LinkVoucherId=@ret group by ilinkpathid,Basevoucherid";
            DataSet lds = Clsdata.GetData(Lsql, ccode);
            for (int i = 0; i < lds.Tables[0].Rows.Count; i++)
            {
                LinkPathId = Convert.ToInt32(lds.Tables[0].Rows[i]["ilinkpathid"]);
                InvVchr = Convert.ToInt32(lds.Tables[0].Rows[i]["invtype"]);
            }

            sql = $@"select svoucherno,iLinkId, (fvalue-linkvalue)penvalue,iproduct,ibodyid from  (select h.svoucherno,iLinkId,fvalue,
                    i.iproduct,i.ibodyid, (select isnull(sum(fvalue),0) from tcore_links_0 tl1 where  tl1.bbase=0 
                    and tl1.ilinkid=tl.ilinkid and tl1.irefid=tl.itransactionid)linkvalue from tcore_header_0 h with (ReadUnCommitted) 
                    join tcore_data_0 d with (ReadUnCommitted) on d.iheaderid=h.iheaderid  join tcore_indta_0 i with (ReadUnCommitted) on i.ibodyid=d.ibodyid
                    join tcore_links_0 tl with (ReadUnCommitted) on tl.itransactionid=d.itransactionid  
                    join tcore_headerdata{InvVchr}_0 uh with (ReadUnCommitted) on uh.iheaderid=d.iheaderid 
                    join tcore_data{InvVchr}_0 ub with (ReadUnCommitted) on ub.ibodyid=d.ibodyid where tl.ilinkid={LinkPathId}
                    and tl.bbase=1 and h.bsuspended=0  and h.sVoucherNo='{DocNo}' and iProduct={ID}
                    )a where (fvalue-linkvalue)>0";
            Clsdata.LogFile(doctype, "sql = " + sql, writelog);
            DataSet ds = Clsdata.GetData(sql, ccode);
            return ds;
            #endregion
        }
        public DataSet GetIRefDs(string RefNo)
        {
            #region Default Acc

            string sql = $@"EXEC Proc_LN_Vnd_Blk_Pnd '{RefNo}'";
            Clsdata.LogFile(doctype, "sql = " + sql, writelog);
            DataSet ds = Clsdata.GetData(sql, ccode);
            return ds;
            #endregion
        }
        public string getSalesAc(string masterid)
        {
            try
            {
                string sql = "select dbo.GetProductSalesAc(" + Convert.ToInt32(masterid) + ")";
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    return dss.Tables[0].Rows[0][0].ToString();
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion


        #region AuthToken
        public string getToken(string Vtype, string Token)
        {
            string AuthToken = "";

            try
            {
                string username = Token.Split(',')[0].Trim().Split(':')[1].Trim();
                Clsdata.LogFile(Vtype, "username = " + username, writelog);
                string Pwd = Token.Split(',')[1].Trim().Split(':')[1].Trim();
                Clsdata.LogFile(Vtype, "Pwd = " + Pwd, writelog);
                string CompCode = Token.Split(',')[2].Trim().Split(':')[1].Trim();
                Clsdata.LogFile(Vtype, "CompCode = " + CompCode, writelog);
                ClsProperties.LogingResult _login = new ClsProperties.LogingResult();
                _login = Getlogin(username, Pwd, CompCode);
                AuthToken = _login.Auth_Token;
                Clsdata.LogFile(Vtype, "AuthToken = " + AuthToken, writelog);
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(Vtype, ex.Message, writelog);
            }
            return AuthToken;
        }
        #endregion
    }
    #region JWT Token Generation
    public class AuthenticationModule
    {
        private const string communicationKey = "##%%12R8*O34*S89**M5687HRVST*INSevenHarvest****%%##";
        System.IdentityModel.Tokens.SecurityKey signingKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(communicationKey));


        // The Method is used to generate token for user
        public string GenerateTokenForUser(string userName, int userId)
        {
            var signingKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(communicationKey));
            var now = DateTime.Now;
            var signingCredentials = new System.IdentityModel.Tokens.SigningCredentials(signingKey,
               System.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature, System.IdentityModel.Tokens.SecurityAlgorithms.Sha256Digest);

            var claimsIdentity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            }, "Custom");

            var securityTokenDescriptor = new System.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                AppliesToAddress = "http://www.Focussoftnet.com",
                TokenIssuerName = "Focus",
                Subject = claimsIdentity,
                SigningCredentials = signingCredentials,
                Lifetime = new Lifetime(now, now.AddMinutes(60)),
            };


            var tokenHandler = new JwtSecurityTokenHandler();

            var plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
            var signedAndEncodedToken = tokenHandler.WriteToken(plainToken);

            return signedAndEncodedToken;

        }

        /// Using the same key used for signing token, user payload is generated back
        public JwtSecurityToken GenerateUserClaimFromJWT(string authToken)
        {

            var tokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters()
            {
                ValidAudiences = new string[]
                      {
                    "http://www.Focussoftnet.com",
                      },

                ValidIssuers = new string[]
                  {
                      "Focus",
                  },
                IssuerSigningKey = signingKey
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            System.IdentityModel.Tokens.SecurityToken validatedToken;

            try
            {

                tokenHandler.ValidateToken(authToken, tokenValidationParameters, out validatedToken);
            }
            catch (Exception ex)
            {

                return null;

            }

            return validatedToken as JwtSecurityToken;

        }

        public JWTAuthenticationIdentity PopulateUserIdentity(JwtSecurityToken userPayloadToken)
        {
            string name = ((userPayloadToken)).Claims.FirstOrDefault(m => m.Type == "unique_name").Value;
            string userId = ((userPayloadToken)).Claims.FirstOrDefault(m => m.Type == "nameid").Value;
            return new JWTAuthenticationIdentity(name) { UserId = Convert.ToInt32(userId), UserName = name };

        }

        public class JWTAuthenticationIdentity : GenericIdentity
        {

            public string UserName { get; set; }
            public int UserId { get; set; }
            public JWTAuthenticationIdentity(string userName)
                : base(userName)
            {
                UserName = userName;
            }
        }
    }
    #endregion

    
    public class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; }
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest lWebRequest = base.GetWebRequest(uri);
            lWebRequest.Timeout = Timeout;
            ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
            return lWebRequest;
        }
    }
}
