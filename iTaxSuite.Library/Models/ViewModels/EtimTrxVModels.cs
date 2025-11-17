using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace iTaxSuite.Library.Models.ViewModels
{
    // Sales Models
    public class TrnsSalesSaveReq : ETIMSBaseReq
    {
        [Required]
        [Newtonsoft.Json.JsonProperty("trdInvcNo")]
        public string TraderInvoiceNo { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DocumentType DocumentType { get; set; }
        [Newtonsoft.Json.JsonProperty("invcNo")]
        public int EtimsInvoiceNum { get; set; }            // Internal
        [Newtonsoft.Json.JsonProperty("orgInvcNo")]
        public int OriginalEtimsInvNum { get; set; }
        [Newtonsoft.Json.JsonProperty("custTin")]
        public string CustomerPIN { get; set; }             // TaxRegistrationNumber1 - Customer
        [Newtonsoft.Json.JsonProperty("custNm")]
        public string CustomerName { get; set; }
        [Newtonsoft.Json.JsonProperty("salesTyCd")]
        public string SalesTypeCode { get; set; }           // 4.8. Transaction Type
        [Newtonsoft.Json.JsonProperty("rcptTyCd")]
        public string ReceiptTypeCode { get; set; }         // 4.9. Sale Receipt Type
        [Newtonsoft.Json.JsonProperty("pmtTyCd")]
        public string PaymentTypeCode { get; set; }         // 4.10. Payment Method
        [Newtonsoft.Json.JsonProperty("salesSttsCd")]
        public string InvoiceStatusCode { get; set; }       // 4.11. Transaction Progress
        [Newtonsoft.Json.JsonProperty("cfmDt")]
        public string ValidateDate { get; set; }
        [Newtonsoft.Json.JsonProperty("salesDt")]
        public string SalesDate { get; set; }
        [Newtonsoft.Json.JsonProperty("stockRlsDt")]
        public string StockReleaseDate { get; set; }
        [Newtonsoft.Json.JsonProperty("cnclReqDt")]         // CreditNote posting date
        public string CancelReqDate { get; set; }
        [Newtonsoft.Json.JsonProperty("cnclDt")]            // for CreditNoteDate
        public string CanceledDate { get; set; }
        [Newtonsoft.Json.JsonProperty("rfdDt")]
        public string CreditNoteDate { get; set; }
        [Newtonsoft.Json.JsonProperty("rfdRsnCd")]
        public string CreditNoteReasonCode { get; set; }    // 4.15. Transaction Progress
        [Newtonsoft.Json.JsonProperty("totItemCnt")]
        public int TotalItemCount { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtA")]
        public decimal TaxableAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtB")]
        public decimal TaxableAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtC")]
        public decimal TaxableAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtD")]
        public decimal TaxableAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtE")]
        public decimal TaxableAmountE { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtA")]
        public decimal TaxRateA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtB")]
        public decimal TaxRateB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtC")]
        public decimal TaxRateC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtD")]
        public decimal TaxRateD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtE")]
        public decimal TaxRateE { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtA")]
        public decimal TaxAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtB")]
        public decimal TaxAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtC")]
        public decimal TaxAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtD")]
        public decimal TaxAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtE")]
        public decimal TaxAmountE { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxblAmt")]
        public decimal TotalTaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxAmt")]
        public decimal TotalTaxAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totAmt")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("prchrAcptcYn")]
        public string PurchaseAccepted { get; set; } = "N";
        [Newtonsoft.Json.JsonProperty("remark")]
        public string Remark { get; set; }
        [Newtonsoft.Json.JsonProperty("regrId")]
        public string RegistrantID { get; set; }
        [Newtonsoft.Json.JsonProperty("regrNm")]
        public string RegistrantName { get; set; }
        [Newtonsoft.Json.JsonProperty("modrId")]
        public string ModifierID { get; set; }              // Modifier ID
        [Newtonsoft.Json.JsonProperty("modrNm")]
        public string ModifierName { get; set; }            // Modifier Name
        [Newtonsoft.Json.JsonProperty("receipt")]
        public EtimsReceipt Receipt { get; set; }
        [Newtonsoft.Json.JsonProperty("itemList")]
        public List<EtimsSaleItem> ItemList { get; set; } = new();

        public TrnsSalesSaveReq()
        {
        }

        public TrnsSalesSaveReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice oeInvoice,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer)
            : this()
        {
            DocumentType = DocumentType.INVOICE;
            PIN = clientBranch.TaxClient.TaxNumber;
            EtimsInvoiceNum = clientBranch.SaleInvoiceSeq;
            TraderInvoiceNo = oeInvoice.InvoiceNumber;
            CustomerName = oeInvoice.BillTo;
            if (oeInvoice.InvoiceDate.HasValue)
            {
                ValidateDate = oeInvoice.InvoiceDate.Value.ToString(ETIMSConst.FMT_DATETIME);
                SalesDate = oeInvoice.InvoiceDate.Value.ToString(ETIMSConst.FMT_DATEONLY);
            }
            if (oeInvoice.ShipmentDate.HasValue)
                StockReleaseDate = oeInvoice.ShipmentDate.Value.ToString(ETIMSConst.FMT_DATETIME);

            // Creator / Modifier
            ModifierID = ModifierName = oeInvoice.EnteredBy;

            SalesTypeCode = ETimsUtils.ConvertTransactionType(DocumentType.INVOICE);
            ReceiptTypeCode = ETimsUtils.ConvertSalesReceiptType(DocumentType.INVOICE);
            PaymentTypeCode = ETimsUtils.ConvertPaymentMethod(oeInvoice.PaymentType);
            InvoiceStatusCode = ETimsUtils.ConvertTransactionStatus(oeInvoice.InvoiceStatus);

            Remark = oeInvoice.Description;
            RegistrantID = oeInvoice.Salesperson1;
            RegistrantName = oeInvoice.SalespersonName1;
            if (string.IsNullOrWhiteSpace(RegistrantID))
            {
                RegistrantID = RegistrantName = oeInvoice.EnteredBy;
            }

            if (customer != null)
            {
                CustomerName = customer.CustomerName.Trim();
                CustomerPIN = customer.TaxRegistrationNumber1.Trim();

                Receipt = new EtimsReceipt(customer);
            }

            foreach (var oeItem in oeInvoice.InvoiceDetails)
            {
                var salesItem = new EtimsSaleItem(oeItem, taxGroup, taxAuthKeys);

                TotalAmount += salesItem.TotalAmount;

                switch (salesItem.TaxTypeCode)
                {
                    case "A":
                        {
                            TaxableAmountA += salesItem.TaxableAmount;
                            TaxAmountA += salesItem.TaxAmount;
                        }
                        break;
                    case "B":
                        {
                            TaxableAmountB += salesItem.TaxableAmount;
                            TaxAmountB += salesItem.TaxAmount;
                        }
                        break;
                    case "C":
                        {
                            TaxableAmountC += salesItem.TaxableAmount;
                            TaxAmountC += salesItem.TaxAmount;
                        }
                        break;
                    case "D":
                        {
                            TaxableAmountD += salesItem.TaxableAmount;
                            TaxAmountD += salesItem.TaxAmount;
                        }
                        break;
                    case "E":
                        {
                            TaxableAmountE += salesItem.TaxableAmount;
                            TaxAmountE += salesItem.TaxAmount;
                        }
                        break;
                }

                ItemList.Add(salesItem);
            }

            TotalItemCount = ItemList.Count;

            TotalTaxAmount = TaxAmountA + TaxAmountB + TaxAmountC + TaxAmountD + TaxAmountE;
            TotalTaxableAmount = TaxableAmountA + TaxableAmountB + TaxableAmountC + TaxableAmountD + TaxableAmountE;
        }

        public TrnsSalesSaveReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote crNote,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer)
            : this()
        {
            DocumentType = DocumentType.INVOICE;
            PIN = clientBranch.TaxClient.TaxNumber;
            TraderInvoiceNo = crNote.CreditDebitNoteNumber;
            CustomerName = crNote.BillTo;
            if (crNote.InvoiceDate.HasValue)
            {
                ValidateDate = crNote.InvoiceDate.Value.ToString(ETIMSConst.FMT_DATETIME);
                SalesDate = crNote.InvoiceDate.Value.ToString(ETIMSConst.FMT_DATEONLY);
            }
            if (crNote.ShipmentDate.HasValue)
                StockReleaseDate = crNote.ShipmentDate.Value.ToString(ETIMSConst.FMT_DATETIME);

            // Creator / Modifier
            ModifierID = ModifierName = crNote.EnteredBy;

            SalesTypeCode = ETimsUtils.ConvertTransactionType(DocumentType.CREDITNOTE);
            ReceiptTypeCode = ETimsUtils.ConvertSalesReceiptType(DocumentType.CREDITNOTE);
            //PaymentTypeCode = ETimsUtils.ConvertPaymentMethod(crNote.PaymentType);
            InvoiceStatusCode = ETimsUtils.ConvertTransactionStatus(crNote.CreditDebitNoteStatus);

            Remark = crNote.Description;
            RegistrantID = crNote.Salesperson1;
            RegistrantName = crNote.SalespersonName1;
            if (string.IsNullOrWhiteSpace(RegistrantID))
            {
                RegistrantID = RegistrantName = crNote.EnteredBy;
            }

            if (customer != null)
            {
                CustomerName = customer.CustomerName.Trim();
                CustomerPIN = customer.TaxRegistrationNumber1.Trim();

                Receipt = new EtimsReceipt(customer);
            }

            foreach (var oeItem in crNote.CreditDebitDetails)
            {
                var salesItem = new EtimsSaleItem(oeItem, taxGroup, taxAuthKeys);

                TotalAmount += salesItem.TotalAmount;

                switch (salesItem.TaxTypeCode)
                {
                    case "A":
                        {
                            TaxableAmountA += salesItem.TaxableAmount;
                            TaxAmountA += salesItem.TaxAmount;
                        }
                        break;
                    case "B":
                        {
                            TaxableAmountB += salesItem.TaxableAmount;
                            TaxAmountB += salesItem.TaxAmount;
                        }
                        break;
                    case "C":
                        {
                            TaxableAmountC += salesItem.TaxableAmount;
                            TaxAmountC += salesItem.TaxAmount;
                        }
                        break;
                    case "D":
                        {
                            TaxableAmountD += salesItem.TaxableAmount;
                            TaxAmountD += salesItem.TaxAmount;
                        }
                        break;
                    case "E":
                        {
                            TaxableAmountE += salesItem.TaxableAmount;
                            TaxAmountE += salesItem.TaxAmount;
                        }
                        break;
                }

                ItemList.Add(salesItem);
            }

            TotalItemCount = ItemList.Count;

            TotalTaxAmount = TaxAmountA + TaxAmountB + TaxAmountC + TaxAmountD + TaxAmountE;
            TotalTaxableAmount = TaxableAmountA + TaxableAmountB + TaxableAmountC + TaxableAmountD + TaxableAmountE;
        }

        public TrnsSalesSaveReq(ClientBranch clientBranch, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice arInvoice,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys,
            Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer)
        {
            PIN = clientBranch.TaxClient.TaxNumber;
            DocumentType = DocumentType.INVOICE;
            TraderInvoiceNo = arInvoice.DocumentNumber;
            // CustomerName = invoice.BillTo;
            if (arInvoice.DocumentDate.HasValue)
            {
                ValidateDate = arInvoice.DocumentDate.Value.ToString(ETIMSConst.FMT_DATETIME);
                SalesDate = arInvoice.DocumentDate.Value.ToString(ETIMSConst.FMT_DATEONLY);
            }
            if (arInvoice.DocumentDate.HasValue)
                StockReleaseDate = arInvoice.DocumentDate.Value.ToString(ETIMSConst.FMT_DATETIME);

            // Creator / Modifier
            ModifierID = ModifierName = arInvoice.EnteredBy;

            SalesTypeCode = ETimsUtils.ConvertTransactionType(DocumentType.INVOICE);
            ReceiptTypeCode = ETimsUtils.ConvertSalesReceiptType(DocumentType.INVOICE);
            PaymentTypeCode = "01"; // ETimsUtils.ConvertPaymentMethod(invoice.PaymentType);
            InvoiceStatusCode = "02"; // ETimsUtils.ConvertTransactionStatus(invoice.InvoiceStatus);

            Remark = arInvoice.InvoiceDescription;
            RegistrantID = arInvoice.Salesperson1;
            //TODO: Get SalesPersonName
            RegistrantName = arInvoice.EnteredBy;

            if (customer != null)
            {
                CustomerName = customer.CustomerName.Trim();
                CustomerPIN = customer.TaxRegistrationNumber1.Trim();

                Receipt = new EtimsReceipt(customer);
            }

            foreach (var arItem in arInvoice.InvoiceDetails)
            {
                var salesItem = new EtimsSaleItem(arInvoice, arItem, taxGroup, taxAuthKeys);

                TotalAmount += salesItem.TotalAmount;

                switch (salesItem.TaxTypeCode)
                {
                    case "A":
                        {
                            TaxableAmountA += salesItem.TaxableAmount;
                            TaxAmountA += salesItem.TaxAmount;
                        }
                        break;
                    case "B":
                        {
                            TaxableAmountB += salesItem.TaxableAmount;
                            TaxAmountB += salesItem.TaxAmount;
                        }
                        break;
                    case "C":
                        {
                            TaxableAmountC += salesItem.TaxableAmount;
                            TaxAmountC += salesItem.TaxAmount;
                        }
                        break;
                    case "D":
                        {
                            TaxableAmountD += salesItem.TaxableAmount;
                            TaxAmountD += salesItem.TaxAmount;
                        }
                        break;
                    case "E":
                        {
                            TaxableAmountE += salesItem.TaxableAmount;
                            TaxAmountE += salesItem.TaxAmount;
                        }
                        break;
                }

                ItemList.Add(salesItem);
            }

            TotalItemCount = ItemList.Count;

            TotalTaxAmount = TaxAmountA + TaxAmountB + TaxAmountC + TaxAmountD + TaxAmountE;
            TotalTaxableAmount = TaxableAmountA + TaxableAmountB + TaxableAmountC + TaxableAmountD + TaxableAmountE;

        }

    }
    public class TrnsSalesSaveResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public SaleCUData Data { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string CUNumber
        {
            get
            {
                if (!IsSuccess || Data == null)
                    return null;
                else
                {
                    return Data.sdcId;
                }
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string QRText
        {
            get
            {
                if (!IsSuccess || Data == null)
                    return null;
                else
                {
                    //TODO : Switch between LIVE and SandBox
                    //TODO : Genereate QRText and QRCode
                    // return $"https://itax.kra.go.ke/KRA-Portal/invoiceChk.htm?actionCode=loadPage&invoiceNo={data.sdcId}";
                    return $"https://tims-test.kra.go.ke/KRA-Portal/invoiceChk.htm?actionCode=loadPage&invoiceNo={Data.sdcId}";
                }
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }

        public bool IsDuplicate => !IsSuccess && Data != null;
    }
    public class SaleCUData
    {
        public int rcptNo { get; set; }
        public string intrlData { get; set; }
        public string rcptSign { get; set; }
        public int totRcptNo { get; set; }
        public string vsdcRcptPbctDate { get; set; }
        public string sdcId { get; set; }
        public string mrcNo { get; set; }
    }

    public class EtimsReceipt
    {
        [Newtonsoft.Json.JsonProperty("custTin")]
        public string CustomerPIN { get; set; }
        [Newtonsoft.Json.JsonProperty("custMblNo")]
        public string CustomerMobile { get; set; }
        [Newtonsoft.Json.JsonProperty("rptNo")]
        public int ReportNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("trdeNm")]
        public string TradeName { get; set; }
        [Newtonsoft.Json.JsonProperty("adrs")]
        public string CustomerAddress { get; set; }
        [Newtonsoft.Json.JsonProperty("topMsg")]
        public string TopMessage { get; set; }
        [Newtonsoft.Json.JsonProperty("btmMsg")]
        public string BottomMessage { get; set; }
        [Newtonsoft.Json.JsonProperty("prchrAcptcYn")]
        public char PurchaseAccepted { get; set; } = 'N';

        public EtimsReceipt()
        {
        }

        public EtimsReceipt(Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Customer customer, string TopMessage = null, string BottomMessage = null)
        {
            CustomerPIN = customer.TaxRegistrationNumber1.Trim();
            CustomerMobile = !string.IsNullOrWhiteSpace(customer.PhoneNumber) ?
                customer.PhoneNumber : customer.ContactsPhone.Trim();
            TradeName = customer.ContactName.Trim();
            CustomerAddress = customer.AddressLine1.Trim();
            ReportNumber = 1;

            this.TopMessage = TopMessage;
            this.BottomMessage = BottomMessage;
        }
    }
    public class EtimsSaleItem : EtimsBaseItemReq
    {
        [Newtonsoft.Json.JsonProperty("dcRt")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("dcAmt")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("isrccCd")]
        public string InsuranceCoCode { get; set; }
        [Newtonsoft.Json.JsonProperty("isrccNm")]
        public string InsuranceCoName { get; set; }
        [Newtonsoft.Json.JsonProperty("isrcRt")]
        public string InsuranceRate { get; set; }
        [Newtonsoft.Json.JsonProperty("isrcAmt")]
        public string InsuranceAmount { get; set; }

        public EtimsSaleItem()
        {
        }
        public EtimsSaleItem(Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.InvoiceDetail item,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
            : this()
        {
            //TODO: Add Error handling while converting            
            ItemSeqNumber = -1; // Internal
            _icItemNumber = item.Item;

            ItemCode = item.Item;
            ItemName = item.Description;
            ItemTypeCode = "2";

            _pkgUnitCode = item.InvoiceUnitOfMeasure;
            Package = item.QuantityShipped;

            _qtyUnitCode = item.PricingUnit;
            Quantity = Package * item.PricingUnitConversion;

            SupplyPrice = item.UnitCost;
            UnitPrice = (item.PricingUnitPrice != 0) ? item.PricingUnitPrice : item.ExtPriceNetOfDiscIncludeTax;
            if (SupplyPrice == 0)
                SupplyPrice = UnitPrice;

            /*
             Match TaxAuthority1 <=> TaxAuthority
             TransactionType <=> Sales/Purchases
             TaxClass1 <=> BuyersClass
             TaxRate1 <=> ItemRate1 = Check After Exempt flag
             */
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority1) && taxAuthKeys.Contains(item.TaxAuthority1))
            {
                TaxAmount += item.TaxAmount1;
                if (string.IsNullOrWhiteSpace(TaxTypeCode))
                {
                    var tClass = taxGroup.Authorities[item.TaxAuthority1].Classes.FirstOrDefault(c => c.ClassKey == item.TaxClass1 && c.TransactionType == Enum.GetName(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass.TransactionTypeEnum.Sales));
                    var tRate = taxGroup.Authorities[item.TaxAuthority1].Rates.FirstOrDefault(r => r.ItemRate1 == item.TaxRate1);
                    TaxTypeCode = ETimsUtils.GetTaxRate(tClass, tRate);
                }
            }
            else
            {
                //TODO: throw error if TaxAuthority1 invalid
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority2) && taxAuthKeys.Contains(item.TaxAuthority2))
            {
                TaxAmount += item.TaxAmount2;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority3) && taxAuthKeys.Contains(item.TaxAuthority3))
            {
                TaxAmount += item.TaxAmount3;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority4) && taxAuthKeys.Contains(item.TaxAuthority4))
            {
                TaxAmount += item.TaxAmount4;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority5) && taxAuthKeys.Contains(item.TaxAuthority5))
            {
                TaxAmount += item.TaxAmount5;
            }

            TaxableAmount = item.TaxBase1;
            //TaxAmount = item.TaxAmount1;
            //TotalAmount = UnitPrice * Quantity;
            TotalAmount = TaxableAmount + TaxAmount;

            DiscountRate = item.DiscountPercent;
            DiscountAmount = item.DiscountedExtendedAmount;
        }

        public EtimsSaleItem(Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitDetail item,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
        {
            //TODO: Add Error handling while converting

            ItemSeqNumber = -1; // Internal

            ItemCode = item.Item;
            ItemName = item.Description;

            _pkgUnitCode = item.InvoiceUnitOfMeasure;
            Package = item.QuantityShipped;

            _qtyUnitCode = item.PricingUnit;
            Quantity = Package * item.PricingUnitConversion;

            SupplyPrice = item.UnitCost;
            if (item.LineType == Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitDetail.LineTypeEnum.Item)
                UnitPrice = item.PricingUnitPrice;
            else
                UnitPrice = (item.ExtendedDiscountedPrice + item.TaxTotal);

            if (!string.IsNullOrWhiteSpace(item.TaxAuthority1) && taxAuthKeys.Contains(item.TaxAuthority1))
            {
                TaxAmount += item.TaxAmount1;
                if (string.IsNullOrWhiteSpace(TaxTypeCode))
                {
                    var tClass = taxGroup.Authorities[item.TaxAuthority1].Classes.FirstOrDefault(c => c.ClassKey == item.TaxClass1 && c.TransactionType == Enum.GetName(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass.TransactionTypeEnum.Sales));
                    var tRate = taxGroup.Authorities[item.TaxAuthority1].Rates.FirstOrDefault(r => r.ItemRate1 == item.TaxRate1);
                    TaxTypeCode = ETimsUtils.GetTaxRate(tClass, tRate);
                }
            }
            else
            {
                //TODO: throw error if TaxAuthority1 invalid
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority2) && taxAuthKeys.Contains(item.TaxAuthority2))
            {
                TaxAmount += item.TaxAmount2;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority3) && taxAuthKeys.Contains(item.TaxAuthority3))
            {
                TaxAmount += item.TaxAmount3;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority4) && taxAuthKeys.Contains(item.TaxAuthority4))
            {
                TaxAmount += item.TaxAmount4;
            }
            if (!string.IsNullOrWhiteSpace(item.TaxAuthority5) && taxAuthKeys.Contains(item.TaxAuthority5))
            {
                TaxAmount += item.TaxAmount5;
            }

            TaxableAmount = item.TaxBase1;
            //TaxAmount = item.TaxAmount1;
            //TotalAmount = UnitPrice * Quantity;
            TotalAmount = TaxableAmount + TaxAmount;

            DiscountRate = item.DiscountPercent;
            DiscountAmount = item.DiscountedExtendedAmount;
        }

        public EtimsSaleItem(Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice arInvoice, Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.InvoiceDetail item,
            S300TaxGroup taxGroup, HashSet<string> taxAuthKeys)
        {
            ItemSeqNumber = -1; // Internal

            ItemCode = item.ItemNumber;
            ItemName = item.Description;

            _pkgUnitCode = item.UnitOfMeasure;
            if (string.IsNullOrWhiteSpace(_pkgUnitCode))
                _pkgUnitCode = "Service";
            _qtyUnitCode = _pkgUnitCode;
            Quantity = Package = (item.Quantity == 0) ? 1 : item.Quantity;

            decimal amtValue = item.Price;
            if (amtValue == 0)
                amtValue = item.ExtendedAmountWithTIP;
            if (amtValue <= 0)
                throw new Exception($"Invalid Amount for {item.Description} :: {amtValue}");

            UnitPrice = SupplyPrice = amtValue;

            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority1) && taxAuthKeys.Contains(arInvoice.TaxAuthority1))
            {
                TaxAmount += item.TaxAmount1;
                if (string.IsNullOrWhiteSpace(TaxTypeCode))
                {
                    var tClass = taxGroup.Authorities[arInvoice.TaxAuthority1].Classes.FirstOrDefault(c => c.ClassKey == arInvoice.TaxClass1 && c.TransactionType == Enum.GetName(Sage.CA.SBS.ERP.Sage300.TX.WebApi.Models.TaxClass.TransactionTypeEnum.Sales));
                    if (tClass is null)
                        throw new Exception($"AR Item {item.ItemNumber} has an invalid TaxClass: {arInvoice.TaxClass1}");
                    var tRate = taxGroup.Authorities[arInvoice.TaxAuthority1].Rates.FirstOrDefault(r => r.ItemRate1 == item.TaxRate1);
                    if (tRate is null)
                        throw new Exception($"AR Item {item.ItemNumber} has an invalid Rate: {item.TaxRate1}");
                    TaxTypeCode = ETimsUtils.GetTaxRate(tClass, tRate);
                }
            }
            else
            {
                //TODO: throw error if TaxAuthority1 invalid
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority2) && taxAuthKeys.Contains(arInvoice.TaxAuthority2))
            {
                TaxAmount += item.TaxAmount2;
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority3) && taxAuthKeys.Contains(arInvoice.TaxAuthority3))
            {
                TaxAmount += item.TaxAmount3;
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority4) && taxAuthKeys.Contains(arInvoice.TaxAuthority4))
            {
                TaxAmount += item.TaxAmount4;
            }
            if (!string.IsNullOrWhiteSpace(arInvoice.TaxAuthority5) && taxAuthKeys.Contains(arInvoice.TaxAuthority5))
            {
                TaxAmount += item.TaxAmount5;
            }

            TaxableAmount = item.TaxBase1;
            //TaxAmount = item.TaxAmount1;
            //TotalAmount = UnitPrice * Quantity;
            TotalAmount = TaxableAmount + TaxAmount;
            if (TotalAmount == 0)
                TotalAmount = TaxableAmount = amtValue;

            DiscountRate = 0;
            DiscountAmount = 0;
        }

    }
    public class SalesFilter : APagedFilter
    {
        public bool IsValid => true;
        public RecordStatusGroup RecordGroup { get; set; } = RecordStatusGroup.ALL;
        [StringLength(32)]
        public string DocNumber { get; set; }
    }
    public class QueueSaveSale
    {
        [StringLength(2)]
        public string BranchCode { get; set; } = "00";
        [StringLength(32)]
        public string DocNumber { get; set; }
    }
    public class SaleTrxKey
    {
        [StringLength(32)]
        public string DocNumber { get; set; }
    }

    // Purchases Models
    public class TrnsPurchaseSalesReq : ETIMSBaseReq
    {
        [Newtonsoft.Json.JsonProperty("lastReqDt")]
        public string LastRequest { get; set; }
    }
    public class TrnsPurchaseSalesResp : ETIMSBaseResp
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public PurchSalesWrapper PurchSalesData { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string RawResponse { get; set; }

        public bool HasData()
        {
            return PurchSalesData != null && PurchSalesData.PurchSalesList != null
                && PurchSalesData.PurchSalesList.Count > 0;
        }
    }
    public class PurchSalesWrapper
    {
        [Newtonsoft.Json.JsonProperty("saleList")]
        public List<PurchaseSale> PurchSalesList { get; set; }
    }
    public class PurchaseSale
    {
        [StringLength(11)]
        [Newtonsoft.Json.JsonProperty("spplrTin")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrTin")]
        public string SupplierPIN { get; set; }
        [StringLength(60)]
        [Newtonsoft.Json.JsonProperty("spplrNm")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrNm")]
        public string SupplierName { get; set; }
        [StringLength(2)]
        [Newtonsoft.Json.JsonProperty("spplrBhfId")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrBhfId")]
        public string SupplierBranchID { get; set; }
        [Newtonsoft.Json.JsonProperty("spplrInvcNo")]
        [System.Text.Json.Serialization.JsonPropertyName("spplrInvcNo")]
        public int SupplierInvoiceNum { get; set; }
        [StringLength(5)]
        [Newtonsoft.Json.JsonProperty("rcptTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("rcptTyCd")]
        public string ReceiptTypeCode { get; set; }         // 4.9. Sale Receipt Type
        [StringLength(5)]
        [Newtonsoft.Json.JsonProperty("pmtTyCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pmtTyCd")]
        public string PaymentTypeCode { get; set; }         // 4.10. Payment Method
        [Newtonsoft.Json.JsonProperty("cfmDt")]
        [System.Text.Json.Serialization.JsonPropertyName("cfmDt")]
        public string ValidateDate { get; set; }
        [Newtonsoft.Json.JsonProperty("salesDt")]
        [System.Text.Json.Serialization.JsonPropertyName("salesDt")]
        public string SalesDate { get; set; }
        [Newtonsoft.Json.JsonProperty("stockRlsDt")]
        [System.Text.Json.Serialization.JsonPropertyName("stockRlsDt")]
        public string StockReleaseDate { get; set; }
        [Newtonsoft.Json.JsonProperty("totItemCnt")]
        [System.Text.Json.Serialization.JsonPropertyName("totItemCnt")]
        public int TotalItemCount { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtA")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtA")]
        public decimal TaxableAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtB")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtB")]
        public decimal TaxableAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtC")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtC")]
        public decimal TaxableAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxblAmtD")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmtD")]
        public decimal TaxableAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtA")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtA")]
        public decimal TaxRateA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtB")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtB")]
        public decimal TaxRateB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtC")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtC")]
        public decimal TaxRateC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxRtD")]
        [System.Text.Json.Serialization.JsonPropertyName("taxRtD")]
        public decimal TaxRateD { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtA")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtA")]
        public decimal TaxAmountA { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtB")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtB")]
        public decimal TaxAmountB { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtC")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtC")]
        public decimal TaxAmountC { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmtD")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmtD")]
        public decimal TaxAmountD { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxblAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totTaxblAmt")]
        public decimal TotalTaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totTaxAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totTaxAmt")]
        public decimal TotalTaxAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totAmt")]
        public decimal TotalAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("remark")]
        [System.Text.Json.Serialization.JsonPropertyName("remark")]
        public string Remark { get; set; }
        [Newtonsoft.Json.JsonProperty("itemList")]
        [System.Text.Json.Serialization.JsonPropertyName("itemList")]
        public List<PurchSaleItem> PurchSalesItems { get; set; } = new();
        public string Reference => $"{SupplierPIN}:{SupplierBranchID}:{SupplierInvoiceNum}";
        public bool HasItems()
        {
            return PurchSalesItems != null && PurchSalesItems.Count > 0;
        }
    }
    public class PurchSaleItem
    {
        [Newtonsoft.Json.JsonProperty("itemSeq")]
        [System.Text.Json.Serialization.JsonPropertyName("itemSeq")]
        public int ItemSeqNumber { get; set; }

        [Newtonsoft.Json.JsonProperty("itemCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemCd")]
        public string ItemCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemClsCd")]
        [System.Text.Json.Serialization.JsonPropertyName("itemClsCd")]
        public string ItemClassCode { get; set; }
        [Newtonsoft.Json.JsonProperty("itemNm")]
        [System.Text.Json.Serialization.JsonPropertyName("itemNm")]
        public string ItemName { get; set; }
        [Newtonsoft.Json.JsonProperty("bcd")]
        [System.Text.Json.Serialization.JsonPropertyName("bcd")]
        public string Barcode { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _pkgUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("pkgUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("pkgUnitCd")]
        public string PkgUnitCode { get; set; }             // 4.5. Packaging Unit
        [Newtonsoft.Json.JsonProperty("pkg")]
        [System.Text.Json.Serialization.JsonPropertyName("pkg")]
        public decimal Package { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string _qtyUnitCode { get; set; }
        [Newtonsoft.Json.JsonProperty("qtyUnitCd")]
        [System.Text.Json.Serialization.JsonPropertyName("qtyUnitCd")]
        public string QtyUnitCode { get; set; }             // 4.6. Unit of Quantity
        [Newtonsoft.Json.JsonProperty("qty")]
        [System.Text.Json.Serialization.JsonPropertyName("qty")]
        public decimal Quantity { get; set; }
        [Newtonsoft.Json.JsonProperty("prc")]
        [System.Text.Json.Serialization.JsonPropertyName("prc")]
        public decimal UnitPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("splyAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("splyAmt")]
        public decimal SupplyPrice { get; set; }
        [Newtonsoft.Json.JsonProperty("dcRt")]
        [System.Text.Json.Serialization.JsonPropertyName("dcRt")]
        public decimal DiscountRate { get; set; }
        [Newtonsoft.Json.JsonProperty("dcAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("dcAmt")]
        public decimal DiscountAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("taxTyCd")]           // Tax-Class / Tax-Rate Convert
        [System.Text.Json.Serialization.JsonPropertyName("taxTyCd")]
        public string TaxTypeCode { get; set; }             // 4.1. Tax Type
        [Newtonsoft.Json.JsonProperty("taxblAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("taxblAmt")]
        public decimal TaxableAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("taxAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("taxAmt")]
        public decimal TaxAmount { get; set; }
        [Newtonsoft.Json.JsonProperty("totAmt")]
        [System.Text.Json.Serialization.JsonPropertyName("totAmt")]
        public decimal TotalAmount { get; set; }
    }

}
