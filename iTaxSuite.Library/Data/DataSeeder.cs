using iTaxSuite.Library.Constants;
using iTaxSuite.Library.Extensions;
using iTaxSuite.Library.Models;
using iTaxSuite.Library.Models.Entities;

namespace iTaxSuite.Library.Data
{
    public class DataSeeder
    {
        public static void InitializeDB(ETimsDBContext context)
        {
            string _method_ = "DataSeeder-InitializeDB";
            try
            {
                if (!context.SyncChannels.Any())
                {
                    var channels = new List<SyncChannel>()
                    {
                        // Master Data
                        new SyncChannel{ ChannelId=GeneralConst.CUSTOMER_SYNC, ChannelName="Customer Sync Channel",
                            KeyCol="CustomerNumber", DateCol=null},

                        new SyncChannel{ ChannelId=GeneralConst.PRODUCT_SYNC, ChannelName="Item Sync Channel",
                            KeyCol="ItemNumber", DateCol=null},

                        // Stock Control
                        new SyncChannel{ ChannelId=GeneralConst.PO_INVOICE_SYNC, ChannelName="POReceipt Sync Channel",
                            KeyCol="ReceiptNumber", DateCol="ReceiptDate"},

                        // Transactions
					    new SyncChannel{ ChannelId=GeneralConst.OE_INVOICE_SYNC, ChannelName="Invoice Sync Channel",
                            KeyCol="InvoiceNumber", DateCol="InvoiceDate"},

                        new SyncChannel{ ChannelId=GeneralConst.OE_CRDRNOTE_SYNC, ChannelName="CreditNotes Sync Channel",
                            KeyCol="CreditDebitNoteNumber", DateCol="CreditDebitNoteDate"}
                    };
                    context.SyncChannels.AddRange(channels);
                    if (context.SaveChanges() < 1)
                    {
                        UI.Error($"{_method_} SyncSchedules Error: Failed Seeding SyncChannels");
                    }
                }
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"{_method_} error : {ex.GetBaseException().Message}");
                throw;
            }
        }
    }

}
