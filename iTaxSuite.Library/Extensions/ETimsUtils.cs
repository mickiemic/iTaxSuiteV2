using iTaxSuite.Library.Models.ViewModels;

namespace iTaxSuite.Library.Extensions
{
    public class ETimsUtils
    {
        // 4.1. Tax Type
        public static string GetTaxRate(S300TaxClass tClass, S300TaxRate tRate)
        {
            string result = null;
            if (tRate.ItemRate1 == 0)
            {
                result = (tClass.Exempt) ? "A" : "C";
            }
            else
            {
                result = (tRate.ItemRate1 == 16) ? "B" : "E";
            }
            return result;
        }

        // 4.2. Taxpayer Status

        // 4.3. Product Type

        // 4.4. Countries Code

        // 4.7. Currency

        // 4.8. Transaction Type
        public static string ConvertTransactionType(DocumentType documentType)
        {
            if (documentType == DocumentType.INVOICE)
            {
                return "N";
            }
            else if (documentType == DocumentType.CREDITNOTE)
            {
                return "N";
            }
            throw new ArgumentException($"Invalid Transaction Type {documentType}");
        }

        // 4.9. Sale Receipt Type
        public static string ConvertSalesReceiptType(DocumentType documentType)
        {
            if (documentType == DocumentType.INVOICE)
            {
                return "S";
            }
            else if (documentType == DocumentType.CREDITNOTE)
            {
                return "R";
            }
            throw new ArgumentException($"Invalid Sale Receipt Type {documentType}");
        }

        // 4.10. Payment Method
        public static string ConvertPaymentMethod(Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.PaymentTypeEnum paymentType)
        {
            string result = null;
            switch (paymentType)
            {
                case Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.PaymentTypeEnum.Cash:
                    result = "01";
                    break;
                case Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.PaymentTypeEnum.Check:
                    result = "04";
                    break;
                case Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.PaymentTypeEnum.CreditCard:
                case Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.PaymentTypeEnum.SPSCreditCard:
                    result = "05";
                    break;
                default:
                    result = "07";
                    break;
            }
            return result;
        }
        public static string ConvertPaymentMethod(long payments)
        {
            return "01";
        }

        // 4.11. Transaction Progress
        public static string ConvertTransactionStatus(Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.InvoiceStatusEnum invoiceStatus)
        {
            if (invoiceStatus == Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice.InvoiceStatusEnum.Documentcosted)
            {
                return "02";
            }
            else
            {
                return "01";
            }
        }
        public static string ConvertTransactionStatus(Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote.CreditDebitNoteStatusEnum crDrNoteStatus)
        {
            if (crDrNoteStatus == Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote.CreditDebitNoteStatusEnum.Documentcosted)
            {
                return "02";
            }
            else
            {
                return "01";
            }
        }

        // 4.12. Registration Type
        public static string GetRegistrationType(bool IsAutomatic = true)
        {
            return IsAutomatic ? "A" : "M";
        }

        // 4.13. Purchase Receipt Type
        public static string GetPurchaseReceiptType(bool IsCreditNote = false)
        {
            return IsCreditNote ? "R" : "P";
        }

        // 4.14. API Response Code
        public static string GetAPIRespMessage(string ResultCode)
        {
            if (string.IsNullOrWhiteSpace(ResultCode))
                return ResultCode;

            switch (ResultCode)
            {
                case "000":
                    return "It is succeeded";
                case "001":
                    return "There is no search result";
                case "801": 
                    return "There is no data to retransmit.";
                case "802": 
                    return "There is data that has not been transferred. After transfer is possible.";
                case "803": 
                    return "This is a report that transfer is complete.";
                case "804": 
                    return "There is no data to send for the report.";
                case "805": 
                    return "Corresponding retransmission data exists.";
                case "834": 
                    return "SalesType and ReceiptType must be NS-NR-ND-TS-TR-TD-CS-CR-CD-PS check your inputs";
                case "836": 
                    return "Your Sequences have been altered, Connect to ZRA API to get Sequences.";
                case "838": 
                    return "Connection to API is not established: check connection.";
                case "884": 
                    return "Invalid customer TPIN was provided";
                case "891": 
                    return "An error occurred while Request URL is created.";
                case "892": 
                    return "An error occurred while Request Header data is created.";
                case "893": 
                    return "An error occurred while Request Body data is created.";
                case "894": 
                    return "An error regarding server communication occurred.";
                case "895": 
                    return "An error regarding unallowed Request Method occurred.";
                case "896": 
                    return "An error regarding Request Status occurred.";
                case "899": 
                    return "An error regarding Client occurred.";
                case "900":
                    return "There is no Header information";
                case "901":
                    return "It is not valid device";
                case "902":
                    return "This device is installed";
                case "903":
                    return "Only VSCU device can be verified.";
                case "910":
                    return "Request parameter error";
                case "911":
                    return "There is no request full text";
                case "912":
                    return "There is a request Method error.";
                case "913":
                    return "Code value error among request";
                case "921":
                    return "Sales or sales invoice data which is declared cannot be received.";
                case "922":
                    return "Sales invoice data can be received after receiving the sales data.";
                case "924":
                    return "Invoice number already exists.";
                case "930":
                    return "The specified invoice could not be found. Please verify [orgInvcNo] and try again";
                case "931":
                    return "The credit note amount exceeds the original invoice amount for item:";
                case "932":
                    return "The item specified in the credit note does not exist on the original invoice. [itemCd]";
                case "934":
                    return "The quantity specified in the credit note exceeds the quantity in the original invoice.";
                case "935":
                    return "The credit note contains information that does not match the original invoice data";
                case "990":
                    return "The maximum number of views are exceeded";
                case "991":
                    return "There is an error during registration";
                case "992":
                    return "There is an error during modification";
                case "993":
                    return "There is an error during deletion";
                case "994":
                    return "There is an overlapped Data";
                case "995": 
                    return "There is no downloaded file";
                case "999": 
                    return "There is an unknown error. Please ask it administrator";
                default:
                    return ResultCode;
            }
        }

        // 4.15. Transaction Progress

        // 4.16. Credit Note Reason Code
        public static string ConvertCreditNoteReasonCode(DocumentType documentType)
        {
            if (documentType == DocumentType.CREDITNOTE)
                return "03";
            else
                return null;
        }

        // 4.17. Item Code

        // 4.18. Import Item Status
        public static string GetImportedItemStatus()
        {
            return string.Empty;
        }

    }
}
