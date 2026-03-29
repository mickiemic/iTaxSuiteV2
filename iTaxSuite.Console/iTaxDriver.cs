using Azure;
using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace iTaxSuite.CLIApp
{
    public class iTaxDriver
    {
        private readonly IDataProtector _dataProtector;
        private readonly ETimsDBContext _dBContext;
        private DecimalFormatConverter decimalFormat = new DecimalFormatConverter();

        public iTaxDriver(IDataProtectionProvider dataProtectionProvider, ETimsDBContext dBContext = null)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(SecureConst.DATA_PURPOSE);
            _dBContext = dBContext;
        }

        public async void RunConsoleApp()
        {
            string _method_ = "RunConsoleApp";
            try
            {
                bool loop = true;
                do
                {
                    await Console.Out.WriteAsync("========== CONSOLE ACTIONS =============" +
                            "\r\n 1. Test Data Protection" +
                            "\r\n 2. Test Data UnProtect" +
                            "\r\n 3. Update ProductData" +
                            "\r\n 4. Update CNotes Data" +
                            "\r\n 5. Update Invoices Data" +
                            "\r\n 0. Exit\r\nSelect an Option: ");
                    string _input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(_input))
                    {
                        Console.WriteLine("Invalid Input [" + _input + "]! Please retry.");
                    }
                    string _choice = _input ?? string.Empty.Trim();

                    switch (_choice)
                    {
                        case "1":
                            {
                                await TestDataProtection();
                                break;
                            }
                        case "2":
                            {
                                await TestDataUnProtect();
                                break;
                            }
                        case "3":
                            {
                                await UpdateProductData();
                                break;
                            }
                        case "4":
                            {
                                await UpdateCNotesTrxData();
                                break;
                            }
                        case "5":
                            {
                                await UpdateInvoicesTrxData();
                                break;
                            }
                        case "0":
                            loop = false;
                            break;
                    }

                } while(loop);

            }
            catch (Exception ex)
            {
                //UI.Error(ex, ex.GetBaseException().ToString());
                Console.WriteLine(ex.GetBaseException());
            }

        }

        private async Task UpdateCNotesTrxData()
        {
            var dataList = await _dBContext.SalesTrxData.Where(x => x.RequestPayload.Contains("return_date")).ToListAsync();
            foreach (var data in dataList) 
            {
                if (data.SourcePayload.Contains("BatchNumber")) // AR Items
                {
                    var arCRNote = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice>(data.SourcePayload);
                    var dTaxSaveCNoteReq = JsonConvert.DeserializeObject<DTaxSaveCNoteReq>(data.RequestPayload);
                    if (dTaxSaveCNoteReq.TraderInvoiceNo != arCRNote.DocumentNumber)
                    {
                        dTaxSaveCNoteReq.TraderInvoiceNo = arCRNote.DocumentNumber;
                        data.RequestPayload = JsonConvert.SerializeObject(dTaxSaveCNoteReq);
                        await _dBContext.SaveChangesAsync();
                    }
                }
                else
                {
                    var oeCRNote = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.CreditDebitNote>(data.SourcePayload);
                    var dTaxSaveCNoteReq = JsonConvert.DeserializeObject<DTaxSaveCNoteReq>(data.RequestPayload);
                    if (dTaxSaveCNoteReq.TraderInvoiceNo != oeCRNote.CreditDebitNoteNumber)
                    {
                        dTaxSaveCNoteReq.TraderInvoiceNo = oeCRNote.CreditDebitNoteNumber;
                        data.RequestPayload = JsonConvert.SerializeObject(dTaxSaveCNoteReq, decimalFormat);
                        data.UpdatedOn = DateTime.Now;
                        data.UpdatedBy = "SYS-ADMIN";
                        await _dBContext.SaveChangesAsync();
                    }
                }
                
            }
        }
        private async Task UpdateInvoicesTrxData()
        {
            var completeStatii = new List<RecordStatus>() { RecordStatus.POST_OK, RecordStatus.POST_DUPL };
            var dataList = await _dBContext.SalesTransact.Include(x => x.SalesTrxData).Include(x => x.SalesItems)
                .Where(x => !!completeStatii.Contains(x.RecordStatus) && x.SalesTrxData.RequestPayload.Contains("sale_date"))
                .ToListAsync();
            foreach (var data in dataList)
            {
                int changes = 0;
                DTaxSaveSaleReq dTaxSaveSaleReq = null;
                if (data.SalesTrxData.SourcePayload.Contains("BatchNumber")) // AR Items
                {
                    var arInvoice = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.AR.WebApi.Models.Invoice>(data.SalesTrxData.SourcePayload);
                    dTaxSaveSaleReq = JsonConvert.DeserializeObject<DTaxSaveSaleReq>(data.SalesTrxData.RequestPayload);
                    /*if (dTaxSaveSaleReq.TraderInvoiceNo != arInvoice.DocumentNumber)
                    {
                        dTaxSaveSaleReq.TraderInvoiceNo = arInvoice.DocumentNumber;
                        changes++;
                    }*/
                }
                else
                {
                    var oeInvoice = JsonConvert.DeserializeObject<Sage.CA.SBS.ERP.Sage300.OE.WebApi.Models.Invoice>(data.SalesTrxData.SourcePayload);
                    dTaxSaveSaleReq = JsonConvert.DeserializeObject<DTaxSaveSaleReq>(data.SalesTrxData.RequestPayload);
                    /*if (dTaxSaveSaleReq.TraderInvoiceNo != oeInvoice.InvoiceNumber)
                    {
                        dTaxSaveSaleReq.TraderInvoiceNo = oeInvoice.InvoiceNumber;
                        changes++;
                    }*/
                }

                if (dTaxSaveSaleReq is not null && dTaxSaveSaleReq.ItemList.Count > 0)
                {
                    var itemMap = new Dictionary<string, int>();
                    foreach(var item in dTaxSaveSaleReq.ItemList.Where(x => x.ID is not null))
                    {
                        if (itemMap.ContainsKey(item.ID))
                        {
                            itemMap[item.ID]++;
                        }
                        else
                        {
                            itemMap.Add(item.ID, 1);
                        }
                    }
                    foreach(var kv in itemMap.Where(x => x.Value > 1))
                    {
                        UI.Info($"Summarizing SaleTrxID: {data.SalesTrxID} >> ID:{kv.Key}, Count:{kv.Value}");
                        var _amtSum = dTaxSaveSaleReq.ItemList.Where(x => x.ID == kv.Key).Sum(x => x.TotalAmount);
                        var _unitSum = dTaxSaveSaleReq.ItemList.Where(x => x.ID == kv.Key).Sum(x => x.UnitPrice * x.Quantity);
                        var _qtySum = dTaxSaveSaleReq.ItemList.Where(x => x.ID == kv.Key).Sum(x => x.Quantity);
                        var _avgUnitPrice = _amtSum / _qtySum;
                        var finalItem = dTaxSaveSaleReq.ItemList.First(x => x.ID == kv.Key);
                        finalItem.UnitPrice = _avgUnitPrice;
                        finalItem.Quantity = finalItem.Package = _qtySum;
                        finalItem.TotalAmount = _amtSum;

                        dTaxSaveSaleReq.ItemList.RemoveAll(x => x.ID == kv.Key);
                        dTaxSaveSaleReq.ItemList.Add(finalItem);
                        changes++;
                    }
                }
                if (dTaxSaveSaleReq is not null && changes > 0)
                {
                    data.SalesTrxData.RequestPayload = JsonConvert.SerializeObject(dTaxSaveSaleReq, decimalFormat);
                    data.UpdatedOn = DateTime.Now;
                    data.UpdatedBy = "SYS-ADMIN";
                    await _dBContext.SaveChangesAsync();
                }

            }
        }

        private async Task UpdateProductData()
        {
            var dataList = await _dBContext.ProductData.Where(x => !x.RequestPayload.Contains("itemsync")).ToListAsync();
            foreach (var data in dataList)
            {
                var dTaxCreateItem = JsonConvert.DeserializeObject<DTaxCreateItemReq>(data.RequestPayload);
                dTaxCreateItem.CallbackURL = "https://furzy-omari-subvocal.ngrok-free.dev/hook/digitax/business_01KJWFECFF1VN3EHGV4XE2D423/itemsync";
                data.RequestPayload = JsonConvert.SerializeObject(dTaxCreateItem, decimalFormat);
                await _dBContext.SaveChangesAsync();
            }
        }

        private async Task TestDataUnProtect()
        {
            string _method_ = "TestDataProtection";
            try
            {
                await Task.FromResult(0);
                Console.Write("Enter input: ");
                string input = Console.ReadLine();

                // unprotect the payload
                string unprotectedPayload = _dataProtector.Unprotect(input);
                Console.WriteLine($"{_method_} : Unprotect returned: {unprotectedPayload}");
            }
            catch (Exception ex)
            {
                //UI.Error(ex, ex.GetBaseException().ToString());
                Console.WriteLine(ex.GetBaseException());
            }
        }

        private async Task TestDataProtection()
        {
            string _method_ = "TestDataProtection";
            try
            {
                await Task.FromResult(0);
                Console.Write("Enter input: ");
                string input = Console.ReadLine();

                // protect the payload
                string protectedPayload = _dataProtector.Protect(input);
                Console.WriteLine($"{_method_} : Protect returned: {protectedPayload}");

                // unprotect the payload
                string unprotectedPayload = _dataProtector.Unprotect(protectedPayload);
                Console.WriteLine($"{_method_} : Unprotect returned: {unprotectedPayload}");
            }
            catch (Exception ex)
            {
                //UI.Error(ex, ex.GetBaseException().ToString());
                Console.WriteLine(ex.GetBaseException());
            }
        }
    }
}
