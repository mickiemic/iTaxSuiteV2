using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Interfaces;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace iTaxSuite.CLIApp
{
    public class iTaxDriver
    {
        private readonly IDataProtector _dataProtector;
        private readonly ETimsDBContext _dBContext;
        private readonly IMasterDataSvc _masterDataSvc;
        private DecimalFormatConverter decimalFormat = new DecimalFormatConverter();

        public iTaxDriver(IDataProtectionProvider dataProtectionProvider, ETimsDBContext dBContext, IMasterDataSvc masterDataSvc = null)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(SecureConst.DATA_PURPOSE);
            _dBContext = dBContext;
            _masterDataSvc = masterDataSvc;
            _masterDataSvc.InitializeCacheData().GetAwaiter().GetResult();
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
                            "\r\n 6. Select Filter Invoices" +
                            "\r\n 7. Decode QR Code" +
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
                        case "6":
                            {
                                await SelectFilterInvoices();
                                break;
                            }
                        case "7":
                            {
                                await DecodeQRCode();
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
                Console.WriteLine($"{_method_} error: {ex.GetBaseException()}");
            }

        }

        private async Task DecodeQRCode()
        {
            try
            {
                await Task.FromResult(0);
                Console.Write("Enter Document Number: ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Invalid Input! Please retry.");
                    return;
                }
                var salesTransact = await _dBContext.SalesTransact.FirstOrDefaultAsync(x => !x.DocNumber.Equals(input));
                if (salesTransact.QRImage is null)
                {
                    UI.Error($"DecodeQRCode has not QRImage");
                }
                var qrCodeData = FileBinUtils.DecodeQRCode(salesTransact.QRImage);
                Console.WriteLine($"QR Code Data: {qrCodeData}");
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
            await Task.FromResult(0);
        }

        private async Task SelectFilterInvoices()
        {
            string jsonDump = string.Empty;
            await Task.FromResult(0);
            #region stripped payload
            jsonDump = "{\"@odata.context\":\"http://localhost/Sage300WebApi/v1.0/-/111079/OE/$metadata#OEInvoices(InvoiceDate,InvoiceNumber,CustomerNumber,OrderNumber)\",\"value\":[{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000001\",\"CustomerNumber\":\"DR00463\",\"OrderNumber\":\"ORD00001\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000002\",\"CustomerNumber\":\"KE0182932\",\"OrderNumber\":\"ORD00014\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000003\",\"CustomerNumber\":\"KE0012856\",\"OrderNumber\":\"ORD00015\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000004\",\"CustomerNumber\":\"KE0098053\",\"OrderNumber\":\"ORD00018\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000005\",\"CustomerNumber\":\"DR00644\",\"OrderNumber\":\"ORD00025\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000006\",\"CustomerNumber\":\"KE0182929\",\"OrderNumber\":\"ORD00017\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000007\",\"CustomerNumber\":\"PK010\",\"OrderNumber\":\"PK01020260401\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000008\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260401\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000009\",\"CustomerNumber\":\"PK014\",\"OrderNumber\":\"PK01420260401\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000010\",\"CustomerNumber\":\"PK054\",\"OrderNumber\":\"PK05420260401\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000011\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260401\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000012\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260401\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000013\",\"CustomerNumber\":\"PK007\",\"OrderNumber\":\"PK00720260402\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000014\",\"CustomerNumber\":\"PK010\",\"OrderNumber\":\"PK01020260402\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000015\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260402\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000016\",\"CustomerNumber\":\"PK014\",\"OrderNumber\":\"PK01420260402\"},{\"InvoiceDate\":\"2026-04-09T00:00:00Z\",\"InvoiceNumber\":\"IN000017\",\"CustomerNumber\":\"PK96\",\"OrderNumber\":\"PK9620260402\"},{\"InvoiceDate\":\"2026-04-09T00:00:00Z\",\"InvoiceNumber\":\"IN000018\",\"CustomerNumber\":\"PK054\",\"OrderNumber\":\"PK05420260402\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000019\",\"CustomerNumber\":\"PK086\",\"OrderNumber\":\"PK08620260402\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000020\",\"CustomerNumber\":\"PK091\",\"OrderNumber\":\"PK09120260402\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000021\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260402\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000022\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260402\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000023\",\"CustomerNumber\":\"PK007\",\"OrderNumber\":\"PK00720260403\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000024\",\"CustomerNumber\":\"PK010\",\"OrderNumber\":\"PK01020260403\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000025\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260403\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000026\",\"CustomerNumber\":\"PK014\",\"OrderNumber\":\"PK01420260403\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000027\",\"CustomerNumber\":\"PK054\",\"OrderNumber\":\"PK05420260403\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000028\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260403\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000029\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260403\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000030\",\"CustomerNumber\":\"PK96\",\"OrderNumber\":\"PK9620260403\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000031\",\"CustomerNumber\":\"PK007\",\"OrderNumber\":\"PK00720260404\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000032\",\"CustomerNumber\":\"PK010\",\"OrderNumber\":\"PK01020260404\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000033\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260404\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000034\",\"CustomerNumber\":\"PK014\",\"OrderNumber\":\"PK01420260404\"},{\"InvoiceDate\":\"2026-04-09T00:00:00Z\",\"InvoiceNumber\":\"IN000035\",\"CustomerNumber\":\"PK054\",\"OrderNumber\":\"PK05420260404\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000036\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260404\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000037\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260404\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000038\",\"CustomerNumber\":\"PK96\",\"OrderNumber\":\"PK9620260404\"},{\"InvoiceDate\":\"2026-04-05T00:00:00Z\",\"InvoiceNumber\":\"IN000039\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260405\"},{\"InvoiceDate\":\"2026-04-05T00:00:00Z\",\"InvoiceNumber\":\"IN000040\",\"CustomerNumber\":\"PK085\",\"OrderNumber\":\"PK08520260405\"},{\"InvoiceDate\":\"2026-04-05T00:00:00Z\",\"InvoiceNumber\":\"IN000041\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260405\"},{\"InvoiceDate\":\"2026-04-05T00:00:00Z\",\"InvoiceNumber\":\"IN000042\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260405\"},{\"InvoiceDate\":\"2026-04-04T00:00:00Z\",\"InvoiceNumber\":\"IN000043\",\"CustomerNumber\":\"PK010\",\"OrderNumber\":\"PK01020260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000044\",\"CustomerNumber\":\"PK007\",\"OrderNumber\":\"PK00720260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000045\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000046\",\"CustomerNumber\":\"PK014\",\"OrderNumber\":\"PK01420260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000047\",\"CustomerNumber\":\"PK054\",\"OrderNumber\":\"PK05420260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000048\",\"CustomerNumber\":\"PK085\",\"OrderNumber\":\"PK08520260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000049\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000050\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260406\"},{\"InvoiceDate\":\"2026-04-06T00:00:00Z\",\"InvoiceNumber\":\"IN000051\",\"CustomerNumber\":\"PK96\",\"OrderNumber\":\"PK9620260406\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000052\",\"CustomerNumber\":\"DR00433\",\"OrderNumber\":\"ORD00002\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000053\",\"CustomerNumber\":\"KE0012790\",\"OrderNumber\":\"ORD00003\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000054\",\"CustomerNumber\":\"KE0012859\",\"OrderNumber\":\"ORD00004\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000055\",\"CustomerNumber\":\"DR00463\",\"OrderNumber\":\"ORD00006\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000056\",\"CustomerNumber\":\"KE0173410\",\"OrderNumber\":\"ORD00007\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000057\",\"CustomerNumber\":\"KE0186485\",\"OrderNumber\":\"ORD00047\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000058\",\"CustomerNumber\":\"DR00670\",\"OrderNumber\":\"ORD00005\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000059\",\"CustomerNumber\":\"KE0182930\",\"OrderNumber\":\"ORD00010\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000060\",\"CustomerNumber\":\"DR0021\",\"OrderNumber\":\"ORD00040\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000061\",\"CustomerNumber\":\"KE0012808\",\"OrderNumber\":\"ORD00034\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000062\",\"CustomerNumber\":\"KE0139815\",\"OrderNumber\":\"ORD00035\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000063\",\"CustomerNumber\":\"DR00653\",\"OrderNumber\":\"ORD00038\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000065\",\"CustomerNumber\":\"DR00302\",\"OrderNumber\":\"ORD00048\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000064\",\"CustomerNumber\":\"PK007\",\"OrderNumber\":\"PK00720260407\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000066\",\"CustomerNumber\":\"PK010\",\"OrderNumber\":\"PK01020260408\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000067\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260408\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000068\",\"CustomerNumber\":\"PK014\",\"OrderNumber\":\"PK01420260408\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000069\",\"CustomerNumber\":\"PK054\",\"OrderNumber\":\"PK05420260408\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000070\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260408\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000071\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260408\"},{\"InvoiceDate\":\"2026-04-01T00:00:00Z\",\"InvoiceNumber\":\"IN000072\",\"CustomerNumber\":\"DR00670\",\"OrderNumber\":\"ORD00070\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000073\",\"CustomerNumber\":\"PK010\",\"OrderNumber\":\"PK01020260407\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000074\",\"CustomerNumber\":\"PK012\",\"OrderNumber\":\"PK01220260407\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000075\",\"CustomerNumber\":\"PK014\",\"OrderNumber\":\"PK01420260407\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000076\",\"CustomerNumber\":\"PK054\",\"OrderNumber\":\"PK05420260407\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000077\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260407\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000078\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260407\"},{\"InvoiceDate\":\"2026-04-07T00:00:00Z\",\"InvoiceNumber\":\"IN000079\",\"CustomerNumber\":\"PK96\",\"OrderNumber\":\"PK9620260407\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000080\",\"CustomerNumber\":\"PK095\",\"OrderNumber\":\"PK09520260408\"},{\"InvoiceDate\":\"2026-04-08T00:00:00Z\",\"InvoiceNumber\":\"IN000081\",\"CustomerNumber\":\"PK099\",\"OrderNumber\":\"PK09920260408\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000082\",\"CustomerNumber\":\"KE0035798\",\"OrderNumber\":\"ORD00072\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000083\",\"CustomerNumber\":\"KE0012856\",\"OrderNumber\":\"ORD00073\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000084\",\"CustomerNumber\":\"KE0035798\",\"OrderNumber\":\"ORD00009\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000085\",\"CustomerNumber\":\"KE0169538\",\"OrderNumber\":\"ORD00071\"},{\"InvoiceDate\":\"2026-04-10T00:00:00Z\",\"InvoiceNumber\":\"IN000086\",\"CustomerNumber\":\"DR00573\",\"OrderNumber\":\"ORD00074\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000087\",\"CustomerNumber\":\"KE0139815\",\"OrderNumber\":\"ORD00011\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000088\",\"CustomerNumber\":\"DR00481\",\"OrderNumber\":\"ORD00051\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000089\",\"CustomerNumber\":\"KE0152030\",\"OrderNumber\":\"ORD00050\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000090\",\"CustomerNumber\":\"KE0012824\",\"OrderNumber\":\"ORD00075\"},{\"InvoiceDate\":\"2026-04-02T00:00:00Z\",\"InvoiceNumber\":\"IN000091\",\"CustomerNumber\":\"KE0174839\",\"OrderNumber\":\"ORD00049\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000092\",\"CustomerNumber\":\"KE0142692\",\"OrderNumber\":\"ORD00076\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000093\",\"CustomerNumber\":\"KE0012808\",\"OrderNumber\":\"ORD00023\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000094\",\"CustomerNumber\":\"KE0170299\",\"OrderNumber\":\"ORD00008\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000095\",\"CustomerNumber\":\"DR00653\",\"OrderNumber\":\"ORD00024\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000096\",\"CustomerNumber\":\"KE0035798\",\"OrderNumber\":\"ORD00077\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000097\",\"CustomerNumber\":\"KE0142692\",\"OrderNumber\":\"ORD00030\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000098\",\"CustomerNumber\":\"DR00505\",\"OrderNumber\":\"ORD00020\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000099\",\"CustomerNumber\":\"DR00664\",\"OrderNumber\":\"ORD00019\"},{\"InvoiceDate\":\"2026-04-03T00:00:00Z\",\"InvoiceNumber\":\"IN000100\",\"CustomerNumber\":\"KE0167560\",\"OrderNumber\":\"ORD00016\"}],\"@odata.nextLink\":\"http://localhost/Sage300WebApi/v1.0/-/111079/OE/OEInvoices?$skip=100\"}";
            #endregion
            /*var result = await _saleService.SelectFilterInvoices(jsonDump);
            if (result.IsSuccess)
            {
                UI.Info($"SelectFilterInvoices >> {result.GetValue()}");
            }
            else
            {
                UI.Error($"SelectFilterInvoices >> {result.GetError()}");
            }*/
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
