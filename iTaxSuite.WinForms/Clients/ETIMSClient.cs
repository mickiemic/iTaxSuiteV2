using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using iTaxSuite.WinForms.Extensions;
using iTaxSuite.WinForms.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace iTaxSuite.WinForms.Clients
{
    public partial class ETIMSClient : BaseForm
    {
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly VSCUConfig _vscuConfig;
        private readonly ETimsDBContext _dbContext;

        private readonly IEnumerable<IS300ProductSvc> _s300ProductSvcs;
        private IS300ProductSvc _s300ProductSvc;

        private readonly IEnumerable<IS300SaleService> _s300SaleServices;
        private IS300SaleService _saleService;

        private readonly IEtimsService _etimsService;
        private readonly IDigiTaxService _digiTaxService;
        private ClientBranch _clientBranch;

        private TestData _testData;
        public ETIMSClient(IMasterDataSvc masterDataSvc, IEnumerable<IS300SaleService> s300SaleServices, VSCUConfig vscuConfig,
            ETimsDBContext dbContext, IEtimsService etimsService, IEnumerable<IS300ProductSvc> s300ProductSvcs, IDigiTaxService digiTaxService)
        {
            _masterDataSvc = masterDataSvc;
            _dbContext = dbContext;
            _vscuConfig = vscuConfig;
            _etimsService = etimsService;
            _digiTaxService = digiTaxService;

            _s300ProductSvcs = s300ProductSvcs;
            _s300SaleServices = s300SaleServices;

            InitializeComponent();
            Load += MFormLoad;
            FormClosing += MFormClosing;
            KeyDown += OnKeyDown;

            EditorHelper.initSyntaxColoring(reqEditor);
            EditorHelper.initCodeFolding(reqEditor);
            EditorHelper.initSyntaxColoring(respEditor);
            EditorHelper.initCodeFolding(respEditor);
        }

        private async void MFormLoad(object? sender, EventArgs e)
        {
            _clientBranch = await _masterDataSvc.GetBranchAsync();
            _s300ProductSvc = _s300ProductSvcs.Single(x => x.GetDeviceType() == _clientBranch.TaxClient.DeviceType);
            _saleService = _s300SaleServices.Single(x => x.GetDeviceType() == _clientBranch.TaxClient.DeviceType);

            StartupSetup();
        }

        private async void StartupSetup()
        {
            if (_vscuConfig.ClientCode == "TSCLTD")
            {
                _testData = new TestData()
                {
                    ICItem = "SPA001",
                    OEInvoice = "IN008422",
                    OECreditNote = "CN000299",
                    ARInvBatch = "9070",
                    ARInvoice = "",
                    ARCreditNote = "",
                    POReceipt = "ZO01"
                };
            }
            else if (_vscuConfig.ClientCode == "CARLTD")
            {
                _testData = new TestData()
                {
                    ICItem = "BX12024",
                    OEInvoice = "INV00003",
                    OECreditNote = "CN000001",
                    ARInvBatch = "20",
                    ARInvoice = "IN000110",
                    ARCNBatch = "27",
                    ARCreditNote = "CN000004",
                    POReceipt = ""
                };
            }
            else if (_vscuConfig.ClientCode == "LFKDB")
            {
                _testData = new TestData()
                {
                    ICItem = "BK001",
                    OEInvoice = "INV20242476",
                    OECreditNote = "CN20230069",
                    ARInvBatch = "22",
                    ARInvoice = "INV2022/179",
                    ARCreditNote = "",
                    POReceipt = "VINV2133"
                };
            }
            else
            {
                _testData = new TestData();
            }
            
            ShowLoadingScreen(this, "Setting Up Tax Metadata");
            await _masterDataSvc.InitiateTaxSetup();
            await _masterDataSvc.InitializeCacheData();
            if (_clientBranch.TaxClient.DeviceType == TaxDeviceType.DIGITAX)
                await _digiTaxService.GetBranchCount();
            else
                await _etimsService.GetBranchCount();
            HideLoadingScreen();
        }

        public int GetCurrenttab()
        {
            return tabControlEtims.SelectedIndex;
        }
        public void SetCurrentTab(int tabIndex)
        {
            tabControlEtims.SelectedIndex = tabIndex;
        }
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control && e.KeyCode == Keys.Space)
            {
                MessageBox.Show("Posting Request...");
            }
        }

        private void MFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private async void btnSetupTaxes_Click(object sender, EventArgs e)
        {
            ShowLoadingScreen(this, "Setting Up Tax Metadata");
            var result = await _masterDataSvc.InitiateTaxSetup();
            //await Task.Delay(TimeSpan.FromSeconds(3));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Tax Setup");
                return;
            }
        }

        private async void btnGetOEInvoice_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetOEInvoice_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strInput = Interaction.InputBox("Enter Invoice Number", "Select OE Invoice", _testData.OEInvoice);
                if (string.IsNullOrWhiteSpace(strInput))
                {
                    MessageBox.Show($"Invalid Request {strInput}", "Select OE Invoice");
                    return;
                }

                #region payload
                string srcPayload = "{\"InvoiceUniquifier\":961,\"OrderNumber\":\"ORD00029\",\"ICDayEndTransactionNumber\":1176,\"CustomerNumber\":\"AR0001\",\"BillTo\":\"BASE GARDEN\",\"BillToAddress1\":\"1\",\"BillToAddress2\":\"1\",\"BillToAddress3\":\"1\",\"BillToAddress4\":\"\",\"BillToCity\":\"\",\"BillToState\":\"\",\"BillToZipCode\":\"\",\"BillToCountry\":\"\",\"BillToPhone\":\"\",\"BillToFax\":\"\",\"BillToContact\":\"\",\"ShipToAddressCode\":\"\",\"ShipTo\":\"BASE GARDEN\",\"ShipToAddress1\":\"1\",\"ShipToAddress2\":\"1\",\"ShipToAddress3\":\"1\",\"ShipToAddress4\":\"\",\"ShipToCity\":\"\",\"ShipToState\":\"\",\"ShipToZipCode\":\"\",\"ShipToCountry\":\"\",\"ShipToPhone\":\"\",\"ShipToFax\":\"\",\"ShipToContact\":\"\",\"CustomerDiscountLevel\":\"A\",\"PriceListCode\":\"PK002\",\"PurchaseOrderNumber\":\"\",\"Territory\":\"\",\"TermsCode\":\"COD\",\"TotalTermsAmountDue\":132800,\"TermsRateOverride\":false,\"Reference\":\"\",\"OrderDate\":\"2026-04-21T00:00:00Z\",\"ShipViaCode\":\"\",\"ShipViaCodeDescription\":\"\",\"FreeOnBoardPoint\":\"\",\"TemplateCode\":\"\",\"Location\":\"100718\",\"Description\":\"\",\"Comment\":\"\",\"ShipmentDate\":\"2026-04-21T00:00:00Z\",\"InvoiceDate\":\"2026-04-21T00:00:00Z\",\"InvoiceFiscalYear\":\"2026\",\"InvoiceFiscalPeriod\":\"Num4\",\"NumberOfLinesInInvoice\":1,\"NumberOfLabels\":1,\"NumberOfTermsPayments\":1,\"TermsPaymentsAsOfDate\":\"2026-04-21T00:00:00Z\",\"InvoiceTotalEstimatedWeight\":0.0000,\"NextDetailNumber\":2,\"InvoiceStatus\":\"Documentcosted\",\"InvoicePrinted\":false,\"InvoiceDiscOnMiscellaneousCharges\":false,\"PostingDate\":\"2026-04-21T00:00:00Z\",\"CompletionDate\":\"2026-04-21T00:00:00Z\",\"RequiresShippingLabels\":false,\"ShippingLabelsPrinted\":false,\"InvoiceTotalBeforeTax\":114482.76,\"InvoiceIncludedTaxTotAmount\":18317.24,\"InvoiceItemTotalAmount\":132800,\"InvoiceDiscountBase\":132800,\"InvoiceDiscountPercentage\":0.00000,\"InvoiceDiscountAmount\":0.000,\"InvoiceTotalMiscellaneousCharges\":0.000,\"InvoiceSubtotalAmount\":132800,\"InvoiceTotalWithInvoiceDisc\":132800,\"InvoiceExcludedTaxTotAmount\":0.000,\"InvoiceTotalWithTax\":132800,\"InvoiceHomeCurrency\":\"KES\",\"InvoiceRateType\":\"SP\",\"InvoiceSourceCurrency\":\"KES\",\"InvoiceRateDate\":\"2026-04-21T00:00:00Z\",\"InvoiceRate\":1,\"InvoiceSpread\":0.0000000,\"InvoiceRateDateMatching\":3,\"InvoiceRateOperator\":1,\"InvoiceRateOverrideFlag\":false,\"Salesperson1\":\"\",\"Salesperson2\":\"\",\"Salesperson3\":\"\",\"Salesperson4\":\"\",\"Salesperson5\":\"\",\"SalesPercentage1\":0.00000,\"SalesPercentage2\":0.00000,\"SalesPercentage3\":0.00000,\"SalesPercentage4\":0.00000,\"SalesPercentage5\":0.00000,\"TaxOverridden\":false,\"TaxGroup\":\"VAT\",\"TaxAuthority1\":\"VAT\",\"TaxAuthority2\":\"\",\"TaxAuthority3\":\"\",\"TaxAuthority4\":\"\",\"TaxAuthority5\":\"\",\"TaxClass1\":1,\"TaxClass2\":0,\"TaxClass3\":0,\"TaxClass4\":0,\"TaxClass5\":0,\"TaxBase1\":114482.76,\"TaxBase2\":0.000,\"TaxBase3\":0.000,\"TaxBase4\":0.000,\"TaxBase5\":0.000,\"ExcludedTaxAmount1\":0.000,\"ExcludedTaxAmount2\":0.000,\"ExcludedTaxAmount3\":0.000,\"ExcludedTaxAmount4\":0.000,\"ExcludedTaxAmount5\":0.000,\"IncludedTaxAmount1\":18317.24,\"IncludedTaxAmount2\":0.000,\"IncludedTaxAmount3\":0.000,\"IncludedTaxAmount4\":0.000,\"IncludedTaxAmount5\":0.000,\"Registration1\":\"\",\"Registration2\":\"\",\"Registration3\":\"\",\"Registration4\":\"\",\"Registration5\":\"\",\"PriceListCodeDescription\":\"STOCKIST\",\"TermsCodeDescription\":\"Cash On Delivery\",\"TaxGroupCodeDescription\":\"Value Added Tax - KES\",\"LocationCodeDescription\":\"Main Warehouse\",\"SalespersonName1\":\"\",\"SalespersonName2\":\"\",\"SalespersonName3\":\"\",\"SalespersonName4\":\"\",\"SalespersonName5\":\"\",\"TaxAuthority1Description\":\"16% Value Added Tax-Kes\",\"TaxAuthority2Description\":\"\",\"TaxAuthority3Description\":\"\",\"TaxAuthority4Description\":\"\",\"TaxAuthority5Description\":\"\",\"TaxClass1Description\":\"Taxable\",\"TaxClass2Description\":\"\",\"TaxClass3Description\":\"\",\"TaxClass4Description\":\"\",\"TaxClass5Description\":\"\",\"InvoiceSourceCurrencyDescription\":\"Kenya Shilling\",\"InvoiceHomeCurrencyDescription\":\"Kenya Shilling\",\"InvoiceRateTypeDescription\":\"Daily spot rate\",\"PaymentSourceCurrencyDescription\":\"\",\"PaymentHomeCurrencyDescription\":\"\",\"PaymentRateTypeDescription\":\"\",\"TotalTaxAmount1\":18317.24,\"TotalTaxAmount2\":0.000,\"TotalTaxAmount3\":0.000,\"TotalTaxAmount4\":0.000,\"TotalTaxAmount5\":0.000,\"TotalTaxAmount\":18317.24,\"InvoicePaymntInCustomerCurrency\":0.000,\"InvoicePaymentDiscount\":0.000,\"InvoiceAmountDue\":132800,\"AutoTaxCalculationStatus\":true,\"OrderPaymentsTotal\":0.000,\"BillToEmail\":\"\",\"BillToContactPhone\":\"\",\"BillToContactFax\":\"\",\"BillToContactEmail\":\"\",\"ShipToEmail\":\"\",\"ShipToContactPhone\":\"\",\"ShipToContactFax\":\"\",\"ShipToContactEmail\":\"\",\"RecalculateTax\":false,\"DiscountAvailable\":0.000,\"ShipmentHomeCurrency\":\"KES\",\"ShipmentRateType\":\"SP\",\"ShipmentSourceCurrency\":\"KES\",\"ShipmentRateDate\":\"2026-04-21T00:00:00Z\",\"ShipmentRate\":1,\"ShipmentSpread\":0.0000000,\"ShipmentRateDateMatching\":3,\"ShipmentRateOperator\":1,\"ShipmentRateOverrideFlag\":false,\"ShipmentNumber\":\"SH000002\",\"GenerateFromMultipleShipments\":false,\"FromHowManyShipments\":0,\"InvoiceNumber\":\"IN000002\",\"PrepaymentBatchNumber\":0,\"PrepaymentBankCode\":\"\",\"PrepaymentReceiptType\":\"\",\"PrepaymentCheckDate\":\"2026-04-22T00:00:00Z\",\"PrepaymentFiscalYear\":\"2026\",\"PrepaymentFiscalPeriod\":4,\"PrepaymentCheckNumber\":\"\",\"PrepaymentApplyTo\":\"InvoiceNumber\",\"PrepaymentInBankCurrency\":0.000,\"PrepaymentHomeCurrency\":\"KES\",\"PrepaymentRateType\":\"SP\",\"PrepaymentSourceCurrency\":\"KES\",\"PrepaymentRateDate\":\"2026-04-22T00:00:00Z\",\"PrepaymentRate\":1,\"PrepaymentSpread\":0.0000000,\"PrepaymentDateMatch\":3,\"PrepaymentRateOperator\":1,\"GOCALCTAX\":false,\"PerformCreditLimitCheck\":false,\"ShipAll\":false,\"DosConvert\":false,\"ForceTaxCalculation\":false,\"DistributeManualTax\":false,\"TaxCalculationInProgress\":false,\"InvoiceRunningTotal\":132800,\"DisplayRateWarning\":false,\"CustomerExists\":true,\"RecalcMultiPaymentDates\":false,\"GenerateInvoiceFromSingleShip\":false,\"GenerateInvoiceFromMultShips\":false,\"ShipmentRateTypeDescription\":\"Daily spot rate\",\"ShipmentTrackingNumber\":\"\",\"Allowpartialshipments\":\"Yes\",\"OverCreditLimit\":false,\"ApprovedLimit\":132800,\"AuthorizingUserID\":\"\",\"AuthorizingUserPassword\":\"\",\"UserCanApproveCreditLift\":false,\"NumberOfOptionalFields\":0,\"ShipmentUniquifier\":5633,\"ProcessOIPCommand\":\"NothingToProcess\",\"DocumentDiscountBaseWithTax\":132800,\"DocumentDiscountBaseWithoutTax\":114482.76,\"ProcessOECommand\":\"NoAction\",\"UserEnteredApprovalAmount\":0.000,\"CheckingCustomerCreditLimit\":false,\"CheckingCustomerAgingLimit\":false,\"CheckingNatAccountCreditLimit\":false,\"CheckingNatAccountAgingLimit\":false,\"CustomerIsOverCreditLimit\":false,\"CustomerIsOverAgingLimit\":false,\"NatAccountIsOverCreditLimit\":false,\"NatAccountIsOverAgingLimit\":false,\"CustomerCreditLimit\":0.000,\"CustomerBalancePosted\":0.000,\"CustomerDaysOverdue\":0,\"CustomerOverdueLimit\":0.000,\"CustomerBalanceOverdue\":0.000,\"NatAccountCreditLimit\":0.000,\"NatAccountBalance\":0.000,\"NatAccountDaysOverdue\":0,\"NatAccountOverdueLimit\":0.000,\"NatAccountBalanceOverdue\":0.000,\"ARPendingTransactionIncluded\":false,\"OEPendingTransactionIncluded\":false,\"OtherPendingTransactionIncluded\":false,\"ARPendingBalance\":0.000,\"OEPendingBalance\":0.000,\"OtherPendingBalance\":0.000,\"CustomerTotalOutstanding\":0.000,\"NatAccountTotalOutstanding\":0.000,\"CustomerLimitLeft\":0.000,\"NatAccountLimitLeft\":0.000,\"CustomerLimitExceeded\":0.000,\"NatAccountLimitExceeded\":0.000,\"LastInvoiceAmount\":0.000,\"LastInvoiceDate\":null,\"LastPaymentAmount\":0.000,\"LastPaymentDate\":null,\"DrivenbyUI\":false,\"ItemDetailDiscountTotal\":0.000,\"MiscellaneousChargeDetailDiscountTot\":0.000,\"DetailDiscountTotal\":0.000,\"DetailDiscountPercentage\":0.00000,\"DocumentNetOfDetailDisc\":132800,\"AutoCalculationTaxReportingAmounts\":1,\"TaxReportingTRCurrency\":\"KES\",\"TRRateType\":\"SP\",\"TRRateDate\":\"2026-04-21T00:00:00Z\",\"TRRate\":1,\"TRSpread\":0.0000000,\"TRRateDateMatching\":1,\"TRRateOperator\":1,\"TRRateOverrideFlag\":false,\"TRExcludedTaxAmount1\":0.000,\"TRExcludedTaxAmount2\":0.000,\"TRExcludedTaxAmount3\":0.000,\"TRExcludedTaxAmount4\":0.000,\"TRExcludedTaxAmount5\":0.000,\"TRIncludedTaxAmount1\":18317.24,\"TRIncludedTaxAmount2\":0.000,\"TRIncludedTaxAmount3\":0.000,\"TRIncludedTaxAmount4\":0.000,\"TRIncludedTaxAmount5\":0.000,\"TRTaxAmount1\":18317.24,\"TRTaxAmount2\":0.000,\"TRTaxAmount3\":0.000,\"TRTaxAmount4\":0.000,\"TRTaxAmount5\":0.000,\"TRExcludedTaxTotal\":0.000,\"TRIncludedTaxTotal\":18317.24,\"TRTaxTotal\":18317.24,\"TaxReportingShipmentTRCurr\":\"KES\",\"TRShipmentRateType\":\"SP\",\"TRShipmentRateDate\":\"2026-04-21T00:00:00Z\",\"TRShipmentRate\":1,\"TRShipmentSpread\":0.0000000,\"TRShipmentRateDateMatching\":1,\"TRShipmentRateOperator\":1,\"TRShipmentRateOverrideFlag\":false,\"TRShipmentCurrencyDescription\":\"Kenya Shilling\",\"TRInvoiceCurrencyDescription\":\"Kenya Shilling\",\"TRShipmentRateTypeDescriptio\":\"Daily spot rate\",\"TRInvoiceRateTypeDescription\":\"Daily spot rate\",\"TaxVersion\":1,\"PaymentType\":\"None\",\"InvoiceDiscountAmountOverride\":false,\"JobRelated\":false,\"JobRelatedDetailLines\":0,\"HasRetainage\":false,\"RetainageTerms\":\"COD\",\"RetainageAmount\":0.000,\"RetainagePercent\":0.00000,\"RetainageExchangeRate\":\"UseOriginalDocumentExchangeRate\",\"RetainageTaxBase1\":0.000,\"RetainageTaxBase2\":0.000,\"RetainageTaxBase3\":0.000,\"RetainageTaxBase4\":0.000,\"RetainageTaxBase5\":0.000,\"RetainageTaxAmount1\":0.000,\"RetainageTaxAmount2\":0.000,\"RetainageTaxAmount3\":0.000,\"RetainageTaxAmount4\":0.000,\"RetainageTaxAmount5\":0.000,\"RetainageTermsDescription\":\"Cash On Delivery\",\"CustomerAccountSet\":\"134\",\"CustomerAccountSetDescription\":\"On Trade\",\"EnteredBy\":\"ADMIN\",\"DATEBUS\":\"2026-04-21T00:00:00Z\",\"ShipmentPaymentsTotal\":0.000,\"PrepaymentDistributedAmount\":0.000,\"PrepaymentUnappliedAmount\":0.000,\"TotalRetainageTaxAmount\":0.000,\"SageCRMOpportunityLines\":0,\"PreAuthExistsForInvoice\":false,\"ExportDeclarationNumber\":\"\",\"InvoiceDetails\":[{\"InvoiceUniquifier\":961,\"LineUniquifier\":32,\"LineType\":\"Item\",\"Item\":\"207134\",\"MiscellaneousChargesCode\":\"\",\"Description\":\"Dalwhinnie 75cl*\",\"ItemAccountSet\":\"2C\",\"UserSpecifiedCostingMethod\":false,\"PriceList\":\"PK002\",\"Category\":\"2C\",\"Location\":\"111079\",\"PickingSequence\":\"\",\"ShipmentDate\":\"2026-04-21T00:00:00Z\",\"StockItem\":true,\"CurrentQuantityOutstanding\":20,\"QuantityShipped\":20,\"QuantityBackordered\":0.0000,\"InvoiceUnitOfMeasure\":\"CAS\",\"UnitConversion\":6,\"UnitPrice\":6640,\"PriceOverride\":false,\"UnitCost\":36403.98,\"MostRecentUnitCost\":36403.98,\"StandardUnitCost\":0.000000,\"AlternateUnitCost1\":0.000000,\"AlternateUnitCost2\":0.000000,\"UnitPriceNumberOfDecimals\":2,\"PricingUnit\":\"CAS\",\"PricingUnitPrice\":6640,\"PricingUnitConversion\":6,\"PriceDiscountPercentage\":0.00000,\"PriceDiscountAmount\":5540,\"PricingBaseUnit\":\"CAS\",\"PricingBaseUnitPrice\":6916.99999,\"PricingBaseUnitConversion\":6,\"CostingUnit\":\"BTL\",\"CostingUnitCost\":6067.33,\"CostingUnitConversion\":1,\"ExtendedDetailCost\":728079.6,\"ExtendedShippedPriceMiscellaneousCha\":132800,\"InvoiceDiscountAmount\":0.000,\"ExtendedAmountOverride\":false,\"UnitWeight\":0.0000,\"ExtendedWeight\":0.0000,\"TaxAuthority1\":\"VAT\",\"TaxAuthority2\":\"\",\"TaxAuthority3\":\"\",\"TaxAuthority4\":\"\",\"TaxAuthority5\":\"\",\"TaxClass1\":1,\"TaxClass2\":0,\"TaxClass3\":0,\"TaxClass4\":0,\"TaxClass5\":0,\"TaxIncluded1\":true,\"TaxIncluded2\":false,\"TaxIncluded3\":false,\"TaxIncluded4\":false,\"TaxIncluded5\":false,\"TaxBase1\":114482.76,\"TaxBase2\":0.000,\"TaxBase3\":0.000,\"TaxBase4\":0.000,\"TaxBase5\":0.000,\"TaxAmount1\":18317.24,\"TaxAmount2\":0.000,\"TaxAmount3\":0.000,\"TaxAmount4\":0.000,\"TaxAmount5\":0.000,\"TaxRate1\":16,\"TaxRate2\":0.00000,\"TaxRate3\":0.00000,\"TaxRate4\":0.00000,\"TaxRate5\":0.00000,\"DetailNumber\":1,\"HaveCommentsInstructions\":false,\"PriceListDescription\":\"STOCKIST\",\"CategoryDescription\":\"SPIRITS-Whisky - Single Malt Scotch\",\"LocationDescription\":\"Main Warehouse\",\"TaxAuthority1Description\":\"16% Value Added Tax-Kes\",\"TaxAuthority2Description\":\"\",\"TaxAuthority3Description\":\"\",\"TaxAuthority4Description\":\"\",\"TaxAuthority5Description\":\"\",\"TaxClass1Description\":\"Taxable\",\"TaxClass2Description\":\"\",\"TaxClass3Description\":\"\",\"TaxClass4Description\":\"\",\"TaxClass5Description\":\"\",\"NonstockClearingAccount\":\"\",\"NonstockClearingAccountDescription\":\"\",\"AverageUnitCost\":36403.98,\"LastUnitCost\":36403.98,\"ShipmentNumber\":\"SH000002\",\"ShipmentDetailLineNumber\":1,\"TotalMostRecentCost\":728079.6,\"TotalStandardCost\":0.000,\"TotalAlternateCost1\":0.000,\"TotalAlternateCost2\":0.000,\"TotalAverageCost\":728079.6,\"TotalLastCost\":728079.6,\"ShipmentTrackingNumber\":\"\",\"ShipViaCode\":\"\",\"ShipViaCodeDescription\":\"\",\"DiscountPercent\":0.00000,\"ExtendedDiscountedPrice\":132800,\"OrderQuantityOrdered\":20,\"OrderQuantityBackordered\":20,\"OrderQuantityCommitted\":0.0000,\"OrderQuantityTrueCommitted\":0.0000,\"OrderQuantityShippedtodate\":0.0000,\"OrderUnitOfMeasure\":\"CAS\",\"OrderUnitConversion\":6,\"ManufacturersItemNumber\":\"\",\"CustomerItemNumber\":\"\",\"QuantityCommitted\":0.0000,\"QuantityTrueCommitted\":0.0000,\"OrderNumber\":\"ORD00029\",\"OrderDetailNumber\":1,\"RefreshOrderQuantityatUpdate\":false,\"OriginalQuantityshipped\":20,\"DrivenbyUI\":false,\"FoundNegativeInventory\":false,\"Action\":0,\"ShipmentUniquifier\":5633,\"OrderDate\":\"2026-04-21T00:00:00Z\",\"SHIDATE\":\"2026-04-21T00:00:00Z\",\"NumberOfOptionalFields\":0,\"KittingBOM\":\"None\",\"KitBOMNumber\":\"\",\"BOMBuildQuantity\":0.0000,\"BOMBuildUnit\":\"\",\"BOMBuildUnitConversion\":0.000000,\"UnformattedItemNumber\":\"207134\",\"ShipmentLineNumber\":32,\"ProcessCommand\":\"NothingToProcess\",\"ePOSPromotionID\":0,\"SubjectToPaymentDiscount\":\"Yes\",\"PaymentDiscountBaseWithTax\":132800,\"PaymentDiscountBaseWithoutTa\":114482.76,\"PricingBaseWeightUnit\":\"\",\"WeightUnitOfMeasure\":\"\",\"WeightConversionFactor\":1,\"PricingWeightUOM\":\"\",\"PricingWeightConversionFactor\":1,\"PricingBaseWeightConvFactor\":1,\"DefWeightUOMUnitWeight\":0.0000,\"DefWeightUOMExtUnitWeight\":0.0000,\"PriceBy\":\"Quantity\",\"PriceCheckPending\":false,\"PriceApprovedBy\":\"\",\"ApprovingUsersPassword\":\"\",\"PriceApprovalNeeded\":false,\"WeightUOMDescription\":\"\",\"HeaderDiscount\":0.000,\"TRTaxAmount1\":18317.24,\"TRTaxAmount2\":0.000,\"TRTaxAmount3\":0.000,\"TRTaxAmount4\":0.000,\"TRTaxAmount5\":0.000,\"ExtendedAmountNetOfTax\":114482.76,\"DiscountedExtendedAmount\":132800,\"TaxTotal\":18317.24,\"TRTaxTotal\":18317.24,\"CostOfGoods\":0.000,\"RecordCosted\":false,\"JobRelated\":false,\"ContractCode\":\"\",\"ProjectCode\":\"\",\"CategoryCode\":\"\",\"CostClass\":\"None\",\"ProjectStyle\":\"None\",\"ProjectType\":\"None\",\"AccountingMethod\":\"None\",\"BillingType\":\"None\",\"RevenueBillingAccount\":\"\",\"COGSWIPAccount\":\"\",\"RetainageAmount\":0.000,\"RetainagePercent\":0.00000,\"RetainageDays\":0,\"RetainageDueDate\":\"2026-04-21T00:00:00Z\",\"RetainageDueDateOverride\":false,\"RetainageAmountOverride\":false,\"RetainageTaxBase1\":0.000,\"RetainageTaxBase2\":0.000,\"RetainageTaxBase3\":0.000,\"RetainageTaxBase4\":0.000,\"RetainageTaxBase5\":0.000,\"RetainageTaxAmount1\":0.000,\"RetainageTaxAmount2\":0.000,\"RetainageTaxAmount3\":0.000,\"RetainageTaxAmount4\":0.000,\"RetainageTaxAmount5\":0.000,\"DefaultOEPrice\":\"None\",\"Level1Name\":\"\",\"Level2Name\":\"\",\"Level3Name\":\"\",\"UnformattedContractCode\":\"\",\"PrepaymentDistributed\":0.000,\"ExtPriceNetOfDiscIncludeTax\":132800,\"DetailAmountDue\":132800,\"SerialQuantity\":0,\"LotQuantity\":0.0000,\"SerialLotQuantityToProcess\":0.0000,\"NumberOfLotsToGenerate\":0.0000,\"QuantityperLot\":0.0000,\"AllocateFromSerial\":\"\",\"AllocateFromLot\":\"\",\"ItemSerializedLotted\":\"None\",\"SerialLotWindowHandle\":0,\"SageCRMCompanyID\":0,\"SageCRMOpportunityID\":0,\"NoninteractivePriceApproval\":false,\"ExportDeclarationNumber\":\"\",\"InvoiceDetailOptionalFields\":[],\"InvoiceBOMDetails\":[],\"InvoiceKittingDetails\":[],\"InvoiceDetailSerialNumbers\":[],\"InvoiceDetailLotNumbers\":[],\"UpdateOperation\":\"Unspecified\"}],\"InvoiceCommentsInstructions\":[],\"InvoicePaymentSchedules\":[{\"InvoiceUniquifier\":961,\"PaymentNumber\":32,\"DiscountBase\":132800,\"DiscountDate\":\"2026-04-21T00:00:00Z\",\"DiscountPercentage\":0.00000,\"DiscountAmount\":0.000,\"DueAmountBase\":132800,\"DueDate\":\"2026-04-21T00:00:00Z\",\"PercentageDue\":100,\"AmountDue\":132800,\"UpdateOperation\":\"Unspecified\"}],\"MultipleShipmentsToInvoice\":[],\"InvoiceOptionalFields\":[],\"UpdateOperation\":\"Unspecified\"}";
                srcPayload = string.Empty;
                #endregion

                ShowLoadingScreen(this, $"Loading OE Invoice Number {strInput}");
                var convertRes = await _saleService.GetConvertOEInvoice(new SaleTrxKey { DocNumber = strInput }, srcPayload);
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    respEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
                    if (salesView.DTaxSaveSale != null)
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.DTaxSaveSale, decimalFormat));
                    else
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select OE Invoice", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGetOECRNote_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetOECRNote_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strInput = Interaction.InputBox("Enter OE CRNote Number", "Select OE CRNote", _testData.OECreditNote);
                if (string.IsNullOrWhiteSpace(strInput))
                {
                    MessageBox.Show($"Invalid Request {strInput}", "Select OE CRNote");
                    return;
                }

                ShowLoadingScreen(this, $"Loading OE CRNote Number {strInput}");
                var convertRes = await _saleService.GetConvertOECRNote(new SaleTrxKey { DocNumber = strInput });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    respEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
                    if (salesView.DTaxSaveCNoteReq != null)
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.DTaxSaveCNoteReq, decimalFormat));
                    else
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select OE CRNote", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE CRNote Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGetARInvoice_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetARInvoice_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strBatch = Interaction.InputBox("Enter Batch Number", "Enter AR Batch Number", _testData.ARInvBatch);
                if (string.IsNullOrWhiteSpace(strBatch))
                {
                    MessageBox.Show($"Invalid Request {strBatch}", "Select Item");
                    return;
                }
                string strInvoice = Interaction.InputBox("Enter Invoice Number", "Enter AR Invoice Number", _testData.ARInvoice);
                if (string.IsNullOrWhiteSpace(strInvoice))
                {
                    strInvoice = string.Empty;
                }

                UI.Info($"{_method_} running..");
                ShowLoadingScreen(this, $"Loading AR Batch Number {strBatch}, Invoice: {strInvoice}");
                var convertRes = await _saleService.GetConvertARInvoice(new SaleBatchTrxKey { BatchNumber = strBatch, DocNumber = strInvoice });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    respEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
                    if (salesView.DTaxSaveSale != null)
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.DTaxSaveSale, decimalFormat));
                    else
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select AR Invoice", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "AR Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoadingScreen();
            }
        }

        private async void btnGetARCRNote_Click(object sender, EventArgs e)
        {
            string _method_ = "btnGetARCRNote_Click";
            var decimalFormat = new DecimalFormatConverter();
            try
            {
                string strBatch = Interaction.InputBox("Enter Batch Number", "Enter AR Batch Number", _testData.ARCNBatch);
                if (string.IsNullOrWhiteSpace(strBatch))
                {
                    MessageBox.Show($"Invalid Request {strBatch}", "Select Item");
                    return;
                }
                string strInvoice = Interaction.InputBox("Enter Credit Note Number", "Enter AR Credit Note Number", _testData.ARCreditNote);
                if (string.IsNullOrWhiteSpace(strInvoice))
                {
                    strInvoice = string.Empty;
                }

                UI.Info($"{_method_} running..");
                ShowLoadingScreen(this, $"Loading AR Batch Number {strBatch}, Credit Note: {strInvoice}");
                var convertRes = await _saleService.GetConvertARCRNote(new SaleBatchTrxKey { BatchNumber = strBatch, DocNumber = strInvoice });
                HideLoadingScreen();

                EtimsSalesView salesView;
                if (convertRes.IsSuccess)
                {
                    salesView = convertRes.GetValue();
                    respEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesTransact, decimalFormat));
                    if (salesView.DTaxSaveCNoteReq != null)
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.DTaxSaveCNoteReq, decimalFormat));
                    else
                        reqEditor.setEditorText(JsonConvert.SerializeObject(salesView.SalesSaveReq, decimalFormat));
                }
                else
                {
                    MessageBox.Show($"{convertRes.GetError()}", "Select AR Credit Note", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "AR Credit Note Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSaveItem_Click(object sender, EventArgs e)
        {
            string _method_ = "btnSaveItem_Click";
            string _strError = string.Empty;
            try
            {
                string strInput = Interaction.InputBox("Enter Item Code", "Create Item", _testData.ICItem);
                if (string.IsNullOrWhiteSpace(strInput))
                {
                    MessageBox.Show($"Invalid Request {strInput}", "Create Item");
                    return;
                }

                var stockItem = await _dbContext.StockItems.Include(e => e.Product).Include(e => e.Product.ProductData)
                    .Where(e => e.ProductCode == strInput).OrderBy(e => e.CreatedOn)
                    .AsNoTracking().FirstOrDefaultAsync();
                if (stockItem is null)
                {
                    MessageBox.Show($"Invalid Request No ProductCode {strInput} found.", "Create Item");
                    return;
                }
                var etimsRequest = stockItem.Product.ProductData.GetEtimsRequest();
                if (stockItem != null)
                {
                    respEditor.ClearAll();
                    respEditor.setEditorText(etimsRequest);

                    if (stockItem.RecordStatus == RecordStatus.POST_OK || stockItem.RecordStatus == RecordStatus.POST_DUPL)
                    {
                        if (MessageBox.Show($"{stockItem.Product} Already Registeded Fully Successfully. Do you want to update it?", "Update Item",
                        MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (MessageBox.Show($"{stockItem.Product} Already exists but failed. Do you want to update it?", "Update Item",
                        MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Item does not exist!", "Create Item", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ShowLoadingScreen(this, $"Creating Product Code {strInput}");
                var eTimsResp = await _etimsService.CreateEtimsItem(etimsRequest);
                HideLoadingScreen();
                using (var _dbTrans = await _dbContext.Database.BeginTransactionAsync())
                {
                    var tStamp = DateTime.Now;
                    var recordStatus = RecordStatus.POST_FAIL;

                    if (eTimsResp.IsError)
                    {
                        _strError = eTimsResp.GetError();
                        UI.Error($"Saving Stock Item:{stockItem.CacheKey} failed: {eTimsResp.GetError()}");

                        await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                                .SetProperty(x => x.ResponsePayload, _strError)
                                .SetProperty(x => x.ResponseTime, tStamp)
                                .SetProperty(x => x.UpdatedOn, tStamp)
                                .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                            );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _strError)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, _strError)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );

                        await _dbTrans.CommitAsync();

                        MessageBox.Show(_strError, "Create Item", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        SaveItemResp saveItemResp = eTimsResp.GetValue();
                        _strError = saveItemResp.RawResponse;
                        recordStatus = RecordStatus.POST_OK;

                        await _dbContext.ProductData.Where(e => e.ProductCode == stockItem.ProductCode).ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.ResponsePayload, saveItemResp.RawResponse)
                            .SetProperty(x => x.ResponseTime, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.StockItems.Where(e => e.ProductCode == stockItem.ProductCode && e.BranchCode == stockItem.BranchCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );
                        await _dbContext.Products.Where(e => e.ProductCode == stockItem.ProductCode)
                            .ExecuteUpdateAsync(x => x
                            .SetProperty(x => x.Remark, saveItemResp.ResultMsg)
                            .SetProperty(x => x.RecordStatus, recordStatus)
                            .SetProperty(x => x.Tries, x => x.Tries + 1)
                            .SetProperty(x => x.LastTry, tStamp)
                            .SetProperty(x => x.UpdatedOn, tStamp)
                            .SetProperty(x => x.UpdatedBy, "SYS-ADMIN")
                        );

                        await _dbTrans.CommitAsync();

                        MessageBox.Show(_strError, "Create Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error: {ex.GetBaseException().Message}");
                MessageBox.Show($"{_method_} error: {ex.GetBaseException().Message}", "OE Invoice Processing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSelectCodes_Click(object sender, EventArgs e)
        {
            string strInput = Interaction.InputBox("Enter Last Request Date", "Select Codes", "20191130000000");
            if (string.IsNullOrWhiteSpace(strInput))
            {
                MessageBox.Show($"Invalid Request {strInput}", "Select Item");
                return;
            }

            ShowLoadingScreen(this, $"Loading Codes Request DT:{strInput}");
            var result = await Task.Run(() => _etimsService.SelectCodes(strInput));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Select Codes");
                return;
            }

            respEditor.ClearAll();
            respEditor.setEditorText(JsonConvert.SerializeObject(result.GetValue()));
        }

        private async void btnSelectItem_Click(object sender, EventArgs e)
        {
            /*string strInput = Interaction.InputBox("Enter Last Request Date", "Select Codes", "20191130000000");
            if (string.IsNullOrWhiteSpace(strInput))
            {
                MessageBox.Show($"Invalid Request {strInput}", "Select Item");
                return;
            }

            ShowLoadingScreen(this, $"Loading Items Request DT:{strInput}");
            var result = await Task.Run(() => _etimsService.SelectItems(strInput));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Select Items");
                return;
            }

            respEditor.ClearAll();
            respEditor.setEditorText(JsonConvert.SerializeObject(result.GetValue()));*/
            string strInput = Interaction.InputBox("Enter Product Code", "ReFetch Product", "AR:TPA001");
            if (string.IsNullOrWhiteSpace(strInput))
            {
                MessageBox.Show($"Invalid Request {strInput}", "ReFetch Product");
                return;
            }

            ShowLoadingScreen(this, $"ReFetching Product {strInput}");
            var result = await Task.Run(() => _s300ProductSvc.ReFetchProduct(new ProductKey{ ProductCode = strInput }));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "ReFetch Product");
                return;
            }

            var respObj = result.GetValue();
            respEditor.ClearAll();
            respEditor.setEditorText(JsonConvert.SerializeObject(respObj));
            if (respObj.ProductData is not null)
            {
                reqEditor.setEditorText(respObj.ProductData.RequestPayload);
            }

        }

        private async void btnClearSales_Click(object sender, EventArgs e)
        {
            //await GenSalesItems();

            /*ShowLoadingScreen(this, $"Query SaleTransact");
            var result = await _saleService.QuerySaleTransact(new SaleTrxKey { DocNumber = "INV00005" });
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Query SaleTransact");
            }
            else
            {
                MessageBox.Show($"SaleTransact status is OK", "Query SaleTransact");
            }*/

            /*ShowLoadingScreen(this, "Querying Products");
            var glProducts = await _s300ProductSvc.FetchGLProducts();
            var strJson1 = JsonConvert.SerializeObject(glProducts);
            var arProducts = await _s300ProductSvc.FetchARProducts();
            var strJson2 = JsonConvert.SerializeObject(arProducts);
            HideLoadingScreen();
            Console.WriteLine($"GLProducts: {glProducts?.Count}, ARProducts:{arProducts?.Count}");*/

            await ConvertDigiTaxData();
        }

        private async void btnGetPurch_Click(object sender, EventArgs e)
        {
            int pageSize = 0;
            string before = string.Empty, after = string.Empty;

            string strInput = Interaction.InputBox("Enter Page Count", "Select Purchases", "");
            if (!string.IsNullOrWhiteSpace(strInput))
            {
                if (int.TryParse(strInput, out int tempSize))
                {
                    pageSize = tempSize;
                }
            }
            strInput = Interaction.InputBox("Enter Previous Item", "Select Purchases", "");
            if (!string.IsNullOrWhiteSpace(strInput))
            {
                before = strInput.Trim();
            }
            strInput = Interaction.InputBox("Enter Next Item", "Select Purchases", "");
            if (!string.IsNullOrWhiteSpace(strInput))
            {
                after = strInput.Trim();
            }

            ShowLoadingScreen(this, $"Loading Items Request DT:{strInput}");
            var result = await Task.Run(() => _digiTaxService.GetDTaxPurchases(pageSize, before, after));
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Select Purchases");
                return;
            }

            respEditor.ClearAll();
            respEditor.setEditorText(JsonConvert.SerializeObject(result.GetValue()));
        }

        private async void btnClearPurch_Click(object sender, EventArgs e)
        {
            ShowLoadingScreen(this, $"Loading Notices");
            var result = await Task.Run(() => _digiTaxService.SelectNotices());
            HideLoadingScreen();
            if (result.IsError)
            {
                MessageBox.Show($"{result.GetError()}", "Select Notices");
                return;
            }

            respEditor.ClearAll();
            respEditor.setEditorText(JsonConvert.SerializeObject(result.GetValue()));
        }
    }

}
