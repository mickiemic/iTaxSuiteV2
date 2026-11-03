using Dapper;
using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Configs;
using iTaxSuite.Library.Models.Entities;
using iTaxSuite.Library.Models.ViewModels;
using iTaxSuite.Library.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace iTaxSuite.WinForms.Clients
{
    public partial class VSCUStock : BaseForm
    {
        private readonly IEtimsService _etimsService;
        private readonly IMasterDataSvc _masterDataSvc;
        private readonly ETimsDBContext _dbContext;
        private readonly DatabaseOptions _databaseOptions;
        
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExtSystConfig _extSystConfig;
        private readonly JsonSerializerSettings _serializerSettings, _sageEdmSerializer;

        private ClientBranch _clientBranch;
        private List<VendorModel> _vendorMap = new();
        private Dictionary<string, string> _locationMap = new();
        private Dictionary<string, ItemModel> _prodMap;
        private Dictionary<string, string> _itemMap = new();

        private MPOReceipt currentReceipt = null;
        public VSCUStock(IEtimsService etimsService, IMasterDataSvc masterDataSvc, ETimsDBContext dbContext, DatabaseOptions databaseOptions, IHttpClientFactory httpClientFactory, ExtSystConfig extSystConfig)
        {
            _etimsService = etimsService;
            _masterDataSvc = masterDataSvc;
            _dbContext = dbContext;
            _databaseOptions = databaseOptions;
            _httpClientFactory = httpClientFactory;
            _extSystConfig = extSystConfig;

            _serializerSettings = new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            };
            _sageEdmSerializer = new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter(), new EdmDateTimeOffsetConverter() }
            };

            InitializeComponent();
            Load += MFormLoad;
            FormClosing += MFormClosing;
            KeyDown += OnKeyDown;

            dataGridPurchHead.SelectionChanged += DataGridPurchHead_SelectionChanged;
            dataGridPurchHead.CellBeginEdit += DataGridPurchHead_CellBeginEdit;
            dataGridPurchHead.CellEndEdit += DataGridPurchHead_CellEndEdit;

            dataGridPurchHead.ReadOnly = false;
            dataGridPurchHead.DataBindingComplete += DataGridPurchHead_DataBindingComplete;

            dataGridPurchDet.ReadOnly = false;
            dataGridPurchDet.CellBeginEdit += DataGridPurchDet_CellBeginEdit;
            dataGridPurchDet.CellEndEdit += DataGridPurchDet_CellEndEdit;
            dataGridPurchDet.DataBindingComplete += DataGridPurchDet_DataBindingComplete;
        }

        private void DataGridPurchDet_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridPurchDet.CurrentCell.OwningColumn.Name == "ItemNumber")
                {
                    var textCell = new DataGridViewTextBoxCell();
                    textCell.Value = dataGridPurchDet.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    dataGridPurchDet.Rows[e.RowIndex].Cells[e.ColumnIndex] = textCell;
                    dataGridImpDet.EndEdit();
                    string itemCode = (string?)textCell.Value;
                    if (!string.IsNullOrWhiteSpace(itemCode))
                    {
                        var stockItem = _prodMap[itemCode];
                        if (stockItem != null)
                        {
                            dataGridPurchDet.Rows[e.RowIndex].Cells["ItemDescription"].Value = stockItem.Description;
                            dataGridPurchDet.Rows[e.RowIndex].Cells["RCPUNIT"].Value = stockItem.UOM;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"DataGridPurchDet_CellEndEdit error: {ex.GetBaseException().Message}");
            }
        }

        private void DataGridPurchDet_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                if (dataGridPurchDet.CurrentCell.OwningColumn.Name == "ItemNumber")
                {
                    var comboCell = new DataGridViewComboBoxCell();
                    comboCell.DataSource = _itemMap.ToArray();
                    comboCell.DisplayMember = "Value";
                    comboCell.ValueMember = "Key";
                    comboCell.AutoComplete = true;
                    comboCell.Value = dataGridPurchDet.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    dataGridPurchDet.Rows[e.RowIndex].Cells[e.ColumnIndex] = comboCell;
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"DataGridPurchDet_CellBeginEdit error: {ex.GetBaseException().Message}");
            }
        }

        private void DataGridPurchHead_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (dataGridPurchHead.CurrentCell.OwningColumn.Name == "Vendor"
                || dataGridPurchHead.CurrentCell.OwningColumn.Name == "Location")
            {
                var textCell = new DataGridViewTextBoxCell();
                textCell.Value = dataGridPurchHead.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                dataGridPurchHead.Rows[e.RowIndex].Cells[e.ColumnIndex] = textCell;
            }
        }

        private void DataGridPurchHead_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (dataGridPurchHead.CurrentCell.OwningColumn.Name == "Vendor")
            {
                var comboCell = new DataGridViewComboBoxCell();
                // comboCell.Items.AddRange("Option1", "Option2", "Option3");
                comboCell.DataSource = _vendorMap.ToArray();
                comboCell.DisplayMember = "VendorName";
                comboCell.ValueMember = "VendorID";
                comboCell.AutoComplete = true;
                comboCell.Value = dataGridPurchHead.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                dataGridPurchHead.Rows[e.RowIndex].Cells[e.ColumnIndex] = comboCell;
            }
            else if (dataGridPurchHead.CurrentCell.OwningColumn.Name == "Location")
            {
                var comboCell = new DataGridViewComboBoxCell();
                //comboCell.Items.AddRange("100718", "OptionX1", "OptionX2", "OptionX3");
                comboCell.DataSource = _locationMap.ToArray();
                comboCell.DisplayMember = "Value";
                comboCell.ValueMember = "Key";
                comboCell.AutoComplete = true;
                comboCell.Value = dataGridPurchHead.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                dataGridPurchHead.Rows[e.RowIndex].Cells[e.ColumnIndex] = comboCell;
            }
        }

        //private DataTable customers;
        private List<PurchTransact> purchases;
        private List<MPOReceipt> poReceipts;
        private async void LoadSampleData()
        {
            using var connection = new SqlConnection(_databaseOptions.iTaxDBConnString);
            _vendorMap = connection.Query<VendorModel>("select [VENDORID], [VENDNAME] as [VendorName], [SHORTNAME] as [VendorPIN]" +
                "from [vw_Vendors] order by [VENDNAME]").ToList();
            _locationMap = connection.Query<KeyValuePair<string, string>>("select [LOCATION] as [Key], [DESC] as [Value] from [vw_Locations] " +
                "order by [DESC]").ToDictionary(x => x.Key, x => x.Value);
            var prodList = connection.Query<ItemModel>("select ProductCode, [Description], [JSONStockUOM] [UOM] from [vw_Products] " +
                "where [SourceApp]='IC'").ToList();
            _prodMap = new();
            foreach (var prod in prodList)
            {
                _prodMap.Add(prod.ProdRawCode, prod);
            }
            if (_prodMap.Count > 0)
            {
                _itemMap = _prodMap.ToDictionary(x => x.Key, x => x.Value.Description);
            }

            poReceipts = new List<MPOReceipt>();
            purchases = _dbContext.PurchTransact.Include(x => x.PurchTrxData).ToList();

            foreach (var purchase in purchases)
            {
                var poReceipt = new MPOReceipt()
                {
                    ReceiptDate = purchase.DocStamp,
                    ReceiptNumber = purchase.Reference,
                    //Vendor = purchase.VendorNumber,
                    //Location = "100",
                    Reference = purchase.Reference,
                    Description = purchase.Reference,
                    TotalAmount = purchase.HomeTotAmtWTax
                };
                var vendorNumber = _vendorMap.FirstOrDefault(x => x.VendorPIN == purchase.VendorTaxNumber);
                if (!string.IsNullOrWhiteSpace(vendorNumber?.VendorID))
                {
                    poReceipt.Vendor = vendorNumber.VendorID;
                }

                var trnPurchReq = JsonConvert.DeserializeObject<TrnsPurchaseSaveReq>(purchase.PurchTrxData.RequestPayload);
                foreach (var item in trnPurchReq.ItemList)
                {
                    var poItem = new MReceiptLine2()
                    {
                        //ItemNumber = string.IsNullOrWhiteSpace(item.Barcode) ? string.Empty : item.Barcode,
                        ItemDescription = item.ItemName,
                        QuantityReceived = (double)item.Quantity,
                        UnitCost = (item.UnitPrice != 0) ? item.UnitPrice : item.TaxableAmount,
                        TotalTax = item.TaxAmount,
                        Location = poReceipt.Location
                    };

                    poReceipt.ReceiptLine2s.Add(poItem);
                }

                poReceipts.Add(poReceipt);
            }

            // Parent table
            /*customers = new DataTable("Customers");
            customers.Columns.Add("CustomerID", typeof(int));
            customers.Columns.Add("Name", typeof(string));
            customers.Columns.Add("City", typeof(string));

            customers.Rows.Add(1, "Alice", "New York");
            customers.Rows.Add(2, "Bob", "Chicago");
            customers.Rows.Add(3, "Carol", "Boston");

            // Child table
            var orders = new DataTable("Orders");
            orders.Columns.Add("OrderID", typeof(int));
            orders.Columns.Add("CustomerID", typeof(int));
            orders.Columns.Add("Product", typeof(string));
            orders.Columns.Add("Amount", typeof(decimal));

            orders.Rows.Add(101, 1, "Widget", 9.99m);
            orders.Rows.Add(102, 1, "Gadget", 19.99m);
            orders.Rows.Add(103, 2, "Sprocket", 4.50m);
            orders.Rows.Add(104, 3, "Widget", 9.99m);
            orders.Rows.Add(105, 3, "Cog", 2.75m);

            // Assemble into DataSet
            var ds = new DataSet("Sales");
            ds.Tables.Add(customers);
            ds.Tables.Add(orders);

            // Define the parent→child relationship
            ds.Relations.Add("CustomerOrders",
                customers.Columns["CustomerID"],   // parent key
                orders.Columns["CustomerID"]       // child key
            );*/

            // dataGridPurchHead.DataSource = ds.Tables[0];
            dataGridPurchHead.DataSource = poReceipts;
        }

        private void DataGridPurchHead_SelectionChanged(object? sender, EventArgs e)
        {
            if (dataGridPurchHead.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridPurchHead.SelectedRows[0];
                var receiptNumber = (string)selectedRow.Cells["ReceiptNumber"].Value;
                currentReceipt = poReceipts.FirstOrDefault(x => x.ReceiptNumber == receiptNumber);
                dataGridPurchDet.DataSource = currentReceipt.ReceiptLine2s;
                if (currentReceipt != null)
                {
                    btnSavePurchase.Enabled = true;
                }
            }
            else
            {
                dataGridPurchDet.DataSource = null;
                btnSavePurchase.Enabled = false;
                currentReceipt = null;
            }

        }

        private void DataGridPurchDet_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn column in dataGridPurchDet.Columns)
            {
                if (column.Name == "ItemNumber")
                {
                    column.ReadOnly = false;
                }
                else
                {
                    column.ReadOnly = true;
                }
            }
        }

        private void DataGridPurchHead_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewColumn column in dataGridPurchHead.Columns)
            {
                if (column.Name == "Vendor" || column.Name == "Location")
                {
                    column.ReadOnly = false;
                }
                else
                {
                    column.ReadOnly = true;
                }
            }
        }

        private async void btnSavePurchase_Click(object sender, EventArgs e)
        {
            if (currentReceipt == null)
            {
                MessageBox.Show("Invalid Selection", "Save Purchase", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var strErrors = currentReceipt.GetErrors();
            if (!string.IsNullOrWhiteSpace(strErrors))
            {
                MessageBox.Show(strErrors, "Save Purchase", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Are you sure you want to complete purchase mapping?", "Complete Purchase",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            //currentReceipt.ReceiptNumber = GeneralConst.SAGE_NEW_ID;
            var jsonRequest = JsonConvert.SerializeObject(currentReceipt, _sageEdmSerializer);
            UI.Info($"{currentReceipt.Reference} >> {jsonRequest}");

            var _dbEntry = await _dbContext.PurchTransact.AsNoTracking().FirstOrDefaultAsync(e => e.Reference == currentReceipt.Reference);
            // update RequestPayload with current
            var tStamp = DateTime.Now;
            await _dbContext.PurchTransact.Where(e => e.PurchaseID == _dbEntry.PurchaseID).ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.VendorNumber, currentReceipt.Vendor)
                    .SetProperty(x => x.Location, currentReceipt.Location)
                    .SetProperty(x => x.UpdatedOn, tStamp)
                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                );
            await _dbContext.PurchTrxData.Where(e => e.PurchaseID == _dbEntry.PurchaseID).ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.RequestPayload, jsonRequest)
                    .SetProperty(x => x.UpdatedOn, tStamp)
                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                );

            // Post to Sage
            var client = _httpClientFactory.CreateClient(GeneralConst.HTTP_CLIENT_UNSAFE);
            string _reqUrl = string.Format($"{_extSystConfig.ApiAddress}/PO/POReceipts");
            currentReceipt.PrepareRequest();

            string _strResponse;
            var apiResult = await client.PostSage300ERPAsync<POReceipt>(_reqUrl, _extSystConfig.Username, _extSystConfig.Password, jsonRequest);
            if (apiResult.IsError)
            {
                UI.Error($"{currentReceipt.Reference} >> {apiResult.GetError().Message}");
                MessageBox.Show(apiResult.GetError().Message, "Save Purchase", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var poReceiptResp = apiResult.GetValue();
            string jsonResp = JsonConvert.SerializeObject(poReceiptResp);
            UI.Info($"{currentReceipt.Reference} >> {jsonResp}");

            tStamp = DateTime.Now;
            /*await _dbContext.PurchTransact.Where(e => e.PurchaseID == _dbEntry.PurchaseID).ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.DocNumber, poReceiptResp.)
                    .SetProperty(x => x.UpdatedOn, tStamp)
                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                );*/
            await _dbContext.PurchTrxData.Where(e => e.PurchaseID == _dbEntry.PurchaseID).ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.ResponsePayload, jsonResp)
                    .SetProperty(x => x.UpdatedOn, tStamp)
                    .SetProperty(x => x.UpdatedBy, GeneralConst.APPLICATION_NAME)
                );

            //MessageBox.Show("Function Disabled by the current configuration", "Save Purchase", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
        }

        private void MFormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private async void MFormLoad(object? sender, EventArgs e)
        {
            _clientBranch = await _masterDataSvc.GetBranchAsync();

            LoadSampleData();
        }

        class VendorModel
        {
            public string VendorID { get; set; }
            public string VendorName { get; set; }
            public string VendorPIN { get; set; }
        }
        class ItemModel
        {
            public string ProductCode { get; set; }
            public string ProdRawCode => string.IsNullOrWhiteSpace(ProductCode) ? null : ProductCode.Split(':').ElementAt(1);
            public string Description { get; set; }
            public string UOM { get; set; }
        }
    }
}
