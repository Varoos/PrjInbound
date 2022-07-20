BaseURl: http://localhost/PrjInbound/FocusService.svc

Note: change the Dbconfig file in publish file -->xmlfiles folder
2. url  localhost place replace with IP address.

Access Token Generation:
=======================

url: http://localhost/PrjInbound/FocusService.svc/Getlogin

Request Json:
-----------------

{"User_Name":"su","Password":"su","Company_Code":"020"}

Response json:
-------------------

{"GetloginResult":{"Auth_Token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjBKMCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTYwODM1MjI1NywibmJmIjoxNjA4MzQ4NjU3fQ.Z-mmNA7urEyLjkho8B_nMZ9XxZOZMB4Vgoy8rZuTEpc","iStatus":1,"obj":null,"sMessage":"Token Generated"}}


SalesInvoice
===========

Url: http://localhost/PrjInbound/FocusService.svc/SalesInvoice?Auth_Token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjA1MCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTYwODI4NTg5NiwibmJmIjoxNjA4MjgyMjk2fQ.mQVcnmsCWV-jnBN_ls6kZbkkDL8bLrx4-WjGKLE51vs


Request:
--------
{
    "data": [
        {
            "Body": [
                {
                    "FOC_Qty": 0.0,
                    "Actual_Qty": 2.0,
                    "Qty": 2.0,
                    "Item_Id": 157,
                    "TaxCode_Code":"SR",
                    "Selling_Price": 200.0,
                    "Add_Charges": 0.0,
                    "Discount_Amt": 0.0,
                    "DiscountPer": 0.0,
                    "Unit_Id": 20,
                    "Description": "Eggs Pack 8X30",
                    "VAT_Value": 20.0
                }
                    ],
            "Header": {
                "Date": "31/12/2021",
                "Helper": 0,
                "VAN_Id": 83,
                "Place_of_supply_Code": "DXB",
                "Driver_Mobile_No": "",
                "Customer_Id": 524,
                "Driver": 0,
                "Employee_Id": 31,
                "Currency_Id": 7,
                "DueDate": "31/12/2021",
                "Branch_Id": 11,
                "LPO_No": "0"
            }
        }
    ]
}


Response:
---------
{"iStatus":1,"sMessage":"SalesInvoice Posted Successfully","sVouchrNo":"1"}

error Response:
--------------
{"iStatus":0,"sMessage":"Token Expired","sVouchrNo":null}



SalesReturn
===========

Url: http://localhost/PrjInbound/FocusService.svc/SalesReturn?Auth_Token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjA1MCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTYwODI4NTg5NiwibmJmIjoxNjA4MjgyMjk2fQ.mQVcnmsCWV-jnBN_ls6kZbkkDL8bLrx4-WjGKLE51vs


Request:
--------
{"Header":{"Document_Number":"1","Date":"20220117","Sales_Account":null,"Customer":"10111111","DueDate":"20220117","Currency":null,"ExchangeRate":null,"Company":null,"Place_of_Supply":null,"Delivery_Site":null,"Warehouse":null},"Body":[{"Item":"100539","Description":"Apple iPhone 11 64GB Purple","Tax_Code":null,"Unit":null,"Quantity":1.0,"Rate":100.0,"Discount_Per":2.0,"InputDiscount_Amount":100.0,"AddCharges":2.0,"VAT":5.0,"Warranty_Charges":100.0,"Commission_Amount":500.0}],"Footer":{"Card":100.0}}

Response:
---------
{"iStatus":1,"sMessage":"SalesReturn Posted Successfully","sVouchrNo":"1"}

error Response:
--------------
{"iStatus":0,"sMessage":"Token Expired","sVouchrNo":null}





Sales Invoice – VAN

Url

http://localhost/HRVST_Integration/FocusService.svc/SalesInvoice?Auth_Token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjBPMCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTY1MDAyNDQ4NywibmJmIjoxNjUwMDIwODg3fQ.ypiHcC8W8OmVLqFp6MosWZJITR6T2ElifEwRYfylbeI

Request  



 { "Header": {
                "Date": "31/12/2021",
                "Helper": 0,
                "VAN_Id": 19,
                "Place_of_supply_Code": "DXB",
                "Driver_Mobile_No": "",
                "Customer_Id": 524,
                "Driver": 0,
                "Employee_Id": 31,
                "Currency_Id": 7,
                "DueDate": "31/12/2021",
                "Branch_Id": 9,
                "LPO_No": "0"
            },
    "Body": [
        {
          "FOC_Qty": 0.0,
                    "Actual_Qty": 2.0,
                    "Qty": 2.0,
                    "Item_Id": 1309,
                    "TaxCode_Code":"SR",
                    "Selling_Price": 200.0,
                    "Add_Charges": 0.0,
                    "Discount_Amt": 0.0,
                    "DiscountPer": 0.0,
                    "Unit_Id": 11,
                    "Description": "Eggs Pack 8X30",
                    "VAT_Value": 20.0

        }
    ]
   
}

Response

{
    "iStatus": 1,
    "sMessage": "SalesInvoice Posted Successfully",
    "sVouchrNo": "SIV-21-0002"
}




Sales Return - VAN

Url

http://localhost/HRVST_Integration/FocusService.svc/SalesReturn?Auth_Token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjBPMCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTY1MDAyNDQ4NywibmJmIjoxNjUwMDIwODg3fQ.ypiHcC8W8OmVLqFp6MosWZJITR6T2ElifEwRYfylbeI

Request

{
    "Header": {
        "Date": "31/12/2021",
        "Helper": 0,
        "VAN_Id": 19,
        "Place_of_supply_Code": "DXB",
        "Driver_Mobile_No": "",
        "Customer_Id": 524,
        "Driver": 0,
        "Employee_Id": 31,
        "Currency_Id": 7,
        "DueDate": "31/12/2021",
        "Branch_Id": 9,
        "LPO_No": "0"
    },
    "Body": [
        {
            "FOC_Qty": 0.0,
            "Actual_Qty": 2.0,
            "Qty": 2.0,
            "Item_Id": 1309,
            "TaxCode_Code": "SR",
            "Selling_Price": 200.0,
            "Add_Charges": 0.0,
            "Discount_Amt": 0.0,
            "DiscountPer": 0.0,
            "Unit_Id": 11,
            "Description": "Eggs Pack 8X30",
            "VAT_Value": 20.0,
            "MfgDate": "NULL",
            "ExpDate": "31/12/2023",
            "Link_Invoice": {
                "VoucherNo": "SIV-21-0002"
            }
        }
    ]
}



Response
{
    "iStatus": 1,
    "sMessage": "SalesReturn Posted Successfully",
    "sVouchrNo": "SRV-21-0001"
}




Cash Payment VAN

URL
http://localhost/HRVST_Integration/FocusService.svc/Adjustment?Auth_Token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjBPMCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTY1MDI2NTI2NCwibmJmIjoxNjUwMjYxNjY0fQ.uh8MLivgvIOeWn1EhN7JT_HGG1mi84D9rIBVyBWQXfg




Request 


{
    "Header": {
        "Date": "18/04/2022",
        "VAN_Id": 19,
        "Employee_Id": 9,
        "Currency_Id": 7,
        "MaturityDate": "18/04/2022",
        "Branch_Id": 9,
        "ExchangeRate": "1",
        "sNarration": "remarks",
        "sChequeNo": "3"
    },
    "Body": [
        {
            "Amount": -2100.0,
            "Customer_Id": 524,
            "sRemarks": "Line Remarks",
            "Reference": [
                {
                    "InvoiceNo": "SIV-21-0002",
                    "Amount": -10.0
                },
                {
                    "InvoiceNo": "SIV-21-0001",
                    "Amount": -20.0
                }
            ]
        }
    }
]
}


Response

{
    "iStatus": 1,
    "sMessage": "Cash Payment VAN Posted Successfully",
    "sVouchrNo": "1"
}



Cash Receipts VAN


URL

http://localhost/HRVST_Integration/FocusService.svc/Adjustment?Auth_Token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjBPMCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTY1MDI2NTI2NCwibmJmIjoxNjUwMjYxNjY0fQ.uh8MLivgvIOeWn1EhN7JT_HGG1mi84D9rIBVyBWQXfg

Request

{
    "Header": {
        "Date": "18/04/2022",
        "VAN_Id": 19,
        "Employee_Id": 9,
        "Currency_Id": 7,
        "MaturityDate": "18/04/2022",
        "Branch_Id": 9,
        "ExchangeRate": "1",
        "sNarration": "remarks",
        "sChequeNo": "3"
    },
    "Body": [
        {
            "Amount": 2100.0,
            "Customer_Id": 524,
            "sRemarks": "Line Remarks",
            "Reference": [
                {
                    "InvoiceNo": "SIV-21-0002",
                    "Amount": 10.0
                },
                {
                    "InvoiceNo": "SIV-21-0001",
                    "Amount": 20.0
                }
            ]
        }
    }
]
}


Response

{
    "iStatus": 1,
    "sMessage": "Cash Receipts VAN Posted Successfully",
    "sVouchrNo": "CRV-22-0002"
}




Post Dated Receipt VAN

URL
http://localhost/HRVST_Integration/FocusService.svc/Adjustment?Auth_Token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6IjBPMCIsIm5hbWVpZCI6IjEiLCJpc3MiOiJGb2N1cyIsImF1ZCI6Imh0dHA6Ly93d3cuRm9jdXNzb2Z0bmV0LmNvbSIsImV4cCI6MTY1MDI2NTI2NCwibmJmIjoxNjUwMjYxNjY0fQ.uh8MLivgvIOeWn1EhN7JT_HGG1mi84D9rIBVyBWQXfg

Request

{
    "Header": {
        "Date": "18/04/2022",
        "VAN_Id": 19,
        "Employee_Id": 9,
        "Currency_Id": 7,
        "MaturityDate": "19/04/2022",
        "Branch_Id": 9,
        "ExchangeRate": "1",
        "sNarration": "remarks",
        "sChequeNo": "3"
    },
    "Body": [
        {
            "Amount": 2100.0,
            "Customer_Id": 524,
            "sRemarks": "Line Remarks",
            "Reference": [
                {
                    "InvoiceNo": "SIV-21-0002",
                    "Amount": 10.0
                },
                {
                    "InvoiceNo": "SIV-21-0001",
                    "Amount": 20.0
                }
            ]
        }
    }
]
}


Response
{
    "iStatus": 1,
    "sMessage": "Post Dated Receipt VAN Posted Successfully",
    "sVouchrNo": "PDRV-22-0001"
}


