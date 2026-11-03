namespace iTaxSuite.WinForms.Clients
{
    partial class ETIMSClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            grpRequest = new GroupBox();
            reqEditor = new ScintillaNET.Scintilla();
            grpResponse = new GroupBox();
            respEditor = new ScintillaNET.Scintilla();
            btnSubmitRequest = new Button();
            btnCanceRequest = new Button();
            txtReqAddress = new TextBox();
            tabMPurchase = new TabPage();
            tabSetup = new TabPage();
            grpSetup = new GroupBox();
            lytTableSetup = new TableLayoutPanel();
            btnGetNotices = new Button();
            btnSetupTaxes = new Button();
            btnViewQueue = new Button();
            btnSelectCodes = new Button();
            tabSales = new TabPage();
            grpSales = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnClearSales = new Button();
            btnGetOEInvoice = new Button();
            btnGetOECRNote = new Button();
            btnGetARInvoice = new Button();
            btnGetARCRNote = new Button();
            tabStock = new TabPage();
            grpStock = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            btnClearStock = new Button();
            tabPurchases = new TabPage();
            grpPurchases = new GroupBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            btnGetPurch = new Button();
            btnClearPurch = new Button();
            btnPOInvoice = new Button();
            tabItems = new TabPage();
            grpItems = new GroupBox();
            lytTableItems = new TableLayoutPanel();
            btnSaveItem = new Button();
            btnSelectImports = new Button();
            btnSaveCompose = new Button();
            btnSelectItem = new Button();
            tabControlEtims = new TabControl();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            grpRequest.SuspendLayout();
            grpResponse.SuspendLayout();
            tabSetup.SuspendLayout();
            grpSetup.SuspendLayout();
            lytTableSetup.SuspendLayout();
            tabSales.SuspendLayout();
            grpSales.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tabStock.SuspendLayout();
            grpStock.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tabPurchases.SuspendLayout();
            grpPurchases.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tabItems.SuspendLayout();
            grpItems.SuspendLayout();
            lytTableItems.SuspendLayout();
            tabControlEtims.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(19, 174);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(grpRequest);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(grpResponse);
            splitContainer1.Size = new Size(1675, 946);
            splitContainer1.SplitterDistance = 827;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // grpRequest
            // 
            grpRequest.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpRequest.Controls.Add(reqEditor);
            grpRequest.Location = new Point(0, 4);
            grpRequest.Margin = new Padding(4);
            grpRequest.Name = "grpRequest";
            grpRequest.Padding = new Padding(4);
            grpRequest.Size = new Size(823, 938);
            grpRequest.TabIndex = 1;
            grpRequest.TabStop = false;
            grpRequest.Text = "Request Data";
            // 
            // reqEditor
            // 
            reqEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            reqEditor.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 212);
            reqEditor.LexerName = null;
            reqEditor.Location = new Point(7, 31);
            reqEditor.Margin = new Padding(2);
            reqEditor.Name = "reqEditor";
            reqEditor.ScrollWidth = 86;
            reqEditor.Size = new Size(809, 900);
            reqEditor.TabIndex = 0;
            // 
            // grpResponse
            // 
            grpResponse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpResponse.Controls.Add(respEditor);
            grpResponse.Location = new Point(4, 0);
            grpResponse.Margin = new Padding(4);
            grpResponse.Name = "grpResponse";
            grpResponse.Padding = new Padding(4);
            grpResponse.Size = new Size(828, 946);
            grpResponse.TabIndex = 1;
            grpResponse.TabStop = false;
            grpResponse.Text = "Response Data";
            // 
            // respEditor
            // 
            respEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            respEditor.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 212);
            respEditor.LexerName = null;
            respEditor.Location = new Point(7, 35);
            respEditor.Margin = new Padding(2);
            respEditor.Name = "respEditor";
            respEditor.ReadOnly = true;
            respEditor.ScrollWidth = 86;
            respEditor.Size = new Size(814, 900);
            respEditor.TabIndex = 0;
            // 
            // btnSubmitRequest
            // 
            btnSubmitRequest.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmitRequest.Location = new Point(1482, 1127);
            btnSubmitRequest.Margin = new Padding(10, 4, 4, 4);
            btnSubmitRequest.Name = "btnSubmitRequest";
            btnSubmitRequest.Size = new Size(212, 41);
            btnSubmitRequest.TabIndex = 9;
            btnSubmitRequest.Text = "Submit Request";
            btnSubmitRequest.UseVisualStyleBackColor = true;
            // 
            // btnCanceRequest
            // 
            btnCanceRequest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCanceRequest.Location = new Point(14, 1127);
            btnCanceRequest.Margin = new Padding(10, 4, 4, 4);
            btnCanceRequest.Name = "btnCanceRequest";
            btnCanceRequest.Size = new Size(212, 41);
            btnCanceRequest.TabIndex = 10;
            btnCanceRequest.Text = "Cancel && Reset";
            btnCanceRequest.UseVisualStyleBackColor = true;
            // 
            // txtReqAddress
            // 
            txtReqAddress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtReqAddress.Location = new Point(246, 1127);
            txtReqAddress.Margin = new Padding(4);
            txtReqAddress.Name = "txtReqAddress";
            txtReqAddress.Size = new Size(1188, 35);
            txtReqAddress.TabIndex = 11;
            // 
            // tabMPurchase
            // 
            tabMPurchase.Location = new Point(4, 39);
            tabMPurchase.Name = "tabMPurchase";
            tabMPurchase.Size = new Size(1672, 115);
            tabMPurchase.TabIndex = 5;
            tabMPurchase.Text = "Map Purchase";
            tabMPurchase.UseVisualStyleBackColor = true;
            // 
            // tabSetup
            // 
            tabSetup.Controls.Add(grpSetup);
            tabSetup.Location = new Point(4, 39);
            tabSetup.Margin = new Padding(4);
            tabSetup.Name = "tabSetup";
            tabSetup.Size = new Size(1672, 115);
            tabSetup.TabIndex = 4;
            tabSetup.Text = "Basic Setup";
            tabSetup.UseVisualStyleBackColor = true;
            // 
            // grpSetup
            // 
            grpSetup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSetup.Controls.Add(lytTableSetup);
            grpSetup.Location = new Point(7, 4);
            grpSetup.Margin = new Padding(4);
            grpSetup.Name = "grpSetup";
            grpSetup.Padding = new Padding(4);
            grpSetup.Size = new Size(1655, 106);
            grpSetup.TabIndex = 0;
            grpSetup.TabStop = false;
            grpSetup.Text = "Basic Setup";
            // 
            // lytTableSetup
            // 
            lytTableSetup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lytTableSetup.ColumnCount = 5;
            lytTableSetup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableSetup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableSetup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableSetup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableSetup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableSetup.Controls.Add(btnSelectCodes, 0, 0);
            lytTableSetup.Controls.Add(btnViewQueue, 2, 0);
            lytTableSetup.Controls.Add(btnSetupTaxes, 4, 0);
            lytTableSetup.Controls.Add(btnGetNotices, 3, 0);
            lytTableSetup.Location = new Point(7, 36);
            lytTableSetup.Margin = new Padding(4);
            lytTableSetup.Name = "lytTableSetup";
            lytTableSetup.RowCount = 1;
            lytTableSetup.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lytTableSetup.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            lytTableSetup.Size = new Size(1640, 62);
            lytTableSetup.TabIndex = 0;
            // 
            // btnGetNotices
            // 
            btnGetNotices.Anchor = AnchorStyles.None;
            btnGetNotices.Location = new Point(1030, 10);
            btnGetNotices.Margin = new Padding(4);
            btnGetNotices.Name = "btnGetNotices";
            btnGetNotices.Padding = new Padding(0, 0, 12, 0);
            btnGetNotices.Size = new Size(236, 41);
            btnGetNotices.TabIndex = 14;
            btnGetNotices.Text = "Get Notices";
            btnGetNotices.UseVisualStyleBackColor = true;
            btnGetNotices.Click += btnGetNotices_Click;
            // 
            // btnSetupTaxes
            // 
            btnSetupTaxes.Anchor = AnchorStyles.Right;
            btnSetupTaxes.Location = new Point(1468, 10);
            btnSetupTaxes.Margin = new Padding(4);
            btnSetupTaxes.Name = "btnSetupTaxes";
            btnSetupTaxes.Padding = new Padding(0, 0, 12, 0);
            btnSetupTaxes.Size = new Size(168, 41);
            btnSetupTaxes.TabIndex = 7;
            btnSetupTaxes.Text = "Setup Taxes";
            btnSetupTaxes.UseVisualStyleBackColor = true;
            btnSetupTaxes.Click += btnSetupTaxes_Click;
            // 
            // btnViewQueue
            // 
            btnViewQueue.Anchor = AnchorStyles.None;
            btnViewQueue.Location = new Point(742, 10);
            btnViewQueue.Margin = new Padding(4);
            btnViewQueue.Name = "btnViewQueue";
            btnViewQueue.Size = new Size(155, 41);
            btnViewQueue.TabIndex = 6;
            btnViewQueue.Text = "View Queue";
            btnViewQueue.UseVisualStyleBackColor = true;
            // 
            // btnSelectCodes
            // 
            btnSelectCodes.Anchor = AnchorStyles.Left;
            btnSelectCodes.Location = new Point(10, 10);
            btnSelectCodes.Margin = new Padding(10, 4, 4, 4);
            btnSelectCodes.Name = "btnSelectCodes";
            btnSelectCodes.Size = new Size(212, 41);
            btnSelectCodes.TabIndex = 15;
            btnSelectCodes.Text = "Select Codes";
            btnSelectCodes.UseVisualStyleBackColor = true;
            btnSelectCodes.Click += btnSelectCodes_Click;
            // 
            // tabSales
            // 
            tabSales.Controls.Add(grpSales);
            tabSales.Location = new Point(4, 39);
            tabSales.Margin = new Padding(4);
            tabSales.Name = "tabSales";
            tabSales.Size = new Size(1672, 115);
            tabSales.TabIndex = 3;
            tabSales.Text = "Manage Sales";
            tabSales.UseVisualStyleBackColor = true;
            // 
            // grpSales
            // 
            grpSales.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSales.Controls.Add(tableLayoutPanel1);
            grpSales.Location = new Point(7, 4);
            grpSales.Margin = new Padding(4);
            grpSales.Name = "grpSales";
            grpSales.Padding = new Padding(4);
            grpSales.Size = new Size(1660, 106);
            grpSales.TabIndex = 0;
            grpSales.TabStop = false;
            grpSales.Text = "Manage Sales";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Controls.Add(btnGetARCRNote, 4, 0);
            tableLayoutPanel1.Controls.Add(btnGetARInvoice, 3, 0);
            tableLayoutPanel1.Controls.Add(btnGetOECRNote, 1, 0);
            tableLayoutPanel1.Controls.Add(btnGetOEInvoice, 0, 0);
            tableLayoutPanel1.Controls.Add(btnClearSales, 2, 0);
            tableLayoutPanel1.Location = new Point(0, 36);
            tableLayoutPanel1.Margin = new Padding(4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1652, 62);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnClearSales
            // 
            btnClearSales.Anchor = AnchorStyles.None;
            btnClearSales.Location = new Point(758, 10);
            btnClearSales.Margin = new Padding(4);
            btnClearSales.Name = "btnClearSales";
            btnClearSales.Size = new Size(134, 41);
            btnClearSales.TabIndex = 7;
            btnClearSales.Text = "Clear";
            btnClearSales.UseVisualStyleBackColor = true;
            btnClearSales.Click += btnClearSales_Click;
            // 
            // btnGetOEInvoice
            // 
            btnGetOEInvoice.Anchor = AnchorStyles.Left;
            btnGetOEInvoice.Location = new Point(10, 10);
            btnGetOEInvoice.Margin = new Padding(10, 4, 4, 4);
            btnGetOEInvoice.Name = "btnGetOEInvoice";
            btnGetOEInvoice.Size = new Size(212, 41);
            btnGetOEInvoice.TabIndex = 8;
            btnGetOEInvoice.Text = "Select OE Invoice";
            btnGetOEInvoice.UseVisualStyleBackColor = true;
            btnGetOEInvoice.Click += btnGetOEInvoice_Click;
            // 
            // btnGetOECRNote
            // 
            btnGetOECRNote.Anchor = AnchorStyles.None;
            btnGetOECRNote.Location = new Point(398, 10);
            btnGetOECRNote.Margin = new Padding(4);
            btnGetOECRNote.Name = "btnGetOECRNote";
            btnGetOECRNote.Size = new Size(194, 41);
            btnGetOECRNote.TabIndex = 9;
            btnGetOECRNote.Text = "Select OE CRNote";
            btnGetOECRNote.UseVisualStyleBackColor = true;
            btnGetOECRNote.Click += btnGetOECRNote_Click;
            // 
            // btnGetARInvoice
            // 
            btnGetARInvoice.Anchor = AnchorStyles.None;
            btnGetARInvoice.Location = new Point(1037, 10);
            btnGetARInvoice.Margin = new Padding(4);
            btnGetARInvoice.Name = "btnGetARInvoice";
            btnGetARInvoice.Padding = new Padding(0, 0, 12, 0);
            btnGetARInvoice.Size = new Size(236, 41);
            btnGetARInvoice.TabIndex = 13;
            btnGetARInvoice.Text = "Select AR Invoice";
            btnGetARInvoice.UseVisualStyleBackColor = true;
            btnGetARInvoice.Click += btnGetARInvoice_Click;
            // 
            // btnGetARCRNote
            // 
            btnGetARCRNote.Anchor = AnchorStyles.Right;
            btnGetARCRNote.Location = new Point(1412, 10);
            btnGetARCRNote.Margin = new Padding(4);
            btnGetARCRNote.Name = "btnGetARCRNote";
            btnGetARCRNote.Padding = new Padding(0, 0, 12, 0);
            btnGetARCRNote.Size = new Size(236, 41);
            btnGetARCRNote.TabIndex = 14;
            btnGetARCRNote.Text = "Select AR CRNote";
            btnGetARCRNote.UseVisualStyleBackColor = true;
            btnGetARCRNote.Click += btnGetARCRNote_Click;
            // 
            // tabStock
            // 
            tabStock.Controls.Add(grpStock);
            tabStock.Location = new Point(4, 39);
            tabStock.Margin = new Padding(4);
            tabStock.Name = "tabStock";
            tabStock.Size = new Size(1672, 115);
            tabStock.TabIndex = 2;
            tabStock.Text = "Manage Stock";
            tabStock.UseVisualStyleBackColor = true;
            // 
            // grpStock
            // 
            grpStock.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpStock.Controls.Add(tableLayoutPanel2);
            grpStock.Location = new Point(7, 4);
            grpStock.Margin = new Padding(4);
            grpStock.Name = "grpStock";
            grpStock.Padding = new Padding(4);
            grpStock.Size = new Size(1660, 106);
            grpStock.TabIndex = 0;
            grpStock.TabStop = false;
            grpStock.Text = "Manage Stock";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 5;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.Controls.Add(btnClearStock, 2, 0);
            tableLayoutPanel2.Location = new Point(0, 36);
            tableLayoutPanel2.Margin = new Padding(4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(1655, 70);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // btnClearStock
            // 
            btnClearStock.Anchor = AnchorStyles.None;
            btnClearStock.Location = new Point(760, 14);
            btnClearStock.Margin = new Padding(4);
            btnClearStock.Name = "btnClearStock";
            btnClearStock.Size = new Size(134, 41);
            btnClearStock.TabIndex = 7;
            btnClearStock.Text = "Clear";
            btnClearStock.UseVisualStyleBackColor = true;
            // 
            // tabPurchases
            // 
            tabPurchases.Controls.Add(grpPurchases);
            tabPurchases.Location = new Point(4, 39);
            tabPurchases.Margin = new Padding(4);
            tabPurchases.Name = "tabPurchases";
            tabPurchases.Padding = new Padding(4);
            tabPurchases.Size = new Size(1672, 115);
            tabPurchases.TabIndex = 1;
            tabPurchases.Text = "Manage Purchases";
            tabPurchases.UseVisualStyleBackColor = true;
            // 
            // grpPurchases
            // 
            grpPurchases.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpPurchases.Controls.Add(tableLayoutPanel3);
            grpPurchases.Location = new Point(4, 4);
            grpPurchases.Margin = new Padding(4);
            grpPurchases.Name = "grpPurchases";
            grpPurchases.Padding = new Padding(4);
            grpPurchases.Size = new Size(1663, 106);
            grpPurchases.TabIndex = 0;
            grpPurchases.TabStop = false;
            grpPurchases.Text = "Manage Purchases";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel3.ColumnCount = 5;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.Controls.Add(btnPOInvoice, 0, 0);
            tableLayoutPanel3.Controls.Add(btnClearPurch, 2, 0);
            tableLayoutPanel3.Controls.Add(btnGetPurch, 4, 0);
            tableLayoutPanel3.Location = new Point(7, 36);
            tableLayoutPanel3.Margin = new Padding(4);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(1651, 70);
            tableLayoutPanel3.TabIndex = 3;
            // 
            // btnGetPurch
            // 
            btnGetPurch.Anchor = AnchorStyles.Right;
            btnGetPurch.Location = new Point(1420, 14);
            btnGetPurch.Margin = new Padding(4);
            btnGetPurch.Name = "btnGetPurch";
            btnGetPurch.Padding = new Padding(0, 0, 12, 0);
            btnGetPurch.Size = new Size(227, 41);
            btnGetPurch.TabIndex = 9;
            btnGetPurch.Text = "Select Purchases";
            btnGetPurch.UseVisualStyleBackColor = true;
            btnGetPurch.Click += btnGetPurch_Click;
            // 
            // btnClearPurch
            // 
            btnClearPurch.Anchor = AnchorStyles.None;
            btnClearPurch.Location = new Point(758, 14);
            btnClearPurch.Margin = new Padding(4);
            btnClearPurch.Name = "btnClearPurch";
            btnClearPurch.Size = new Size(134, 41);
            btnClearPurch.TabIndex = 7;
            btnClearPurch.Text = "Clear";
            btnClearPurch.UseVisualStyleBackColor = true;
            btnClearPurch.Click += btnClearPurch_Click;
            // 
            // btnPOInvoice
            // 
            btnPOInvoice.Anchor = AnchorStyles.Left;
            btnPOInvoice.Location = new Point(10, 14);
            btnPOInvoice.Margin = new Padding(10, 4, 4, 4);
            btnPOInvoice.Name = "btnPOInvoice";
            btnPOInvoice.Size = new Size(212, 41);
            btnPOInvoice.TabIndex = 8;
            btnPOInvoice.Text = "Select PO Invoice";
            btnPOInvoice.UseVisualStyleBackColor = true;
            // 
            // tabItems
            // 
            tabItems.Controls.Add(grpItems);
            tabItems.Location = new Point(4, 39);
            tabItems.Margin = new Padding(4);
            tabItems.Name = "tabItems";
            tabItems.Padding = new Padding(4);
            tabItems.Size = new Size(1672, 115);
            tabItems.TabIndex = 0;
            tabItems.Text = "Manage Item";
            tabItems.UseVisualStyleBackColor = true;
            // 
            // grpItems
            // 
            grpItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpItems.Controls.Add(lytTableItems);
            grpItems.Location = new Point(7, 7);
            grpItems.Margin = new Padding(4);
            grpItems.Name = "grpItems";
            grpItems.Padding = new Padding(4);
            grpItems.Size = new Size(1655, 98);
            grpItems.TabIndex = 0;
            grpItems.TabStop = false;
            grpItems.Text = "Item Operations";
            // 
            // lytTableItems
            // 
            lytTableItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lytTableItems.ColumnCount = 5;
            lytTableItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableItems.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            lytTableItems.Controls.Add(btnSelectItem, 0, 0);
            lytTableItems.Controls.Add(btnSaveCompose, 4, 0);
            lytTableItems.Controls.Add(btnSelectImports, 1, 0);
            lytTableItems.Controls.Add(btnSaveItem, 3, 0);
            lytTableItems.Location = new Point(0, 36);
            lytTableItems.Margin = new Padding(4);
            lytTableItems.Name = "lytTableItems";
            lytTableItems.RowCount = 1;
            lytTableItems.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lytTableItems.Size = new Size(1648, 55);
            lytTableItems.TabIndex = 5;
            // 
            // btnSaveItem
            // 
            btnSaveItem.Anchor = AnchorStyles.None;
            btnSaveItem.Location = new Point(1070, 7);
            btnSaveItem.Margin = new Padding(4);
            btnSaveItem.Name = "btnSaveItem";
            btnSaveItem.Size = new Size(163, 41);
            btnSaveItem.TabIndex = 2;
            btnSaveItem.Text = "Save Item";
            btnSaveItem.UseVisualStyleBackColor = true;
            btnSaveItem.Click += btnSaveItem_Click;
            // 
            // btnSelectImports
            // 
            btnSelectImports.Anchor = AnchorStyles.None;
            btnSelectImports.Location = new Point(396, 7);
            btnSelectImports.Margin = new Padding(4);
            btnSelectImports.Name = "btnSelectImports";
            btnSelectImports.Size = new Size(194, 41);
            btnSelectImports.TabIndex = 4;
            btnSelectImports.Text = "Select Imports";
            btnSelectImports.UseVisualStyleBackColor = true;
            // 
            // btnSaveCompose
            // 
            btnSaveCompose.Anchor = AnchorStyles.Right;
            btnSaveCompose.Location = new Point(1388, 7);
            btnSaveCompose.Margin = new Padding(4, 4, 10, 4);
            btnSaveCompose.Name = "btnSaveCompose";
            btnSaveCompose.Size = new Size(250, 41);
            btnSaveCompose.TabIndex = 3;
            btnSaveCompose.Text = "Save Composition";
            btnSaveCompose.UseVisualStyleBackColor = true;
            // 
            // btnSelectItem
            // 
            btnSelectItem.Anchor = AnchorStyles.Left;
            btnSelectItem.Location = new Point(10, 7);
            btnSelectItem.Margin = new Padding(10, 4, 4, 4);
            btnSelectItem.Name = "btnSelectItem";
            btnSelectItem.Size = new Size(212, 41);
            btnSelectItem.TabIndex = 1;
            btnSelectItem.Text = "Select Item";
            btnSelectItem.UseVisualStyleBackColor = true;
            btnSelectItem.Click += btnSelectItem_Click;
            // 
            // tabControlEtims
            // 
            tabControlEtims.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tabControlEtims.Controls.Add(tabItems);
            tabControlEtims.Controls.Add(tabPurchases);
            tabControlEtims.Controls.Add(tabStock);
            tabControlEtims.Controls.Add(tabSales);
            tabControlEtims.Controls.Add(tabSetup);
            tabControlEtims.Controls.Add(tabMPurchase);
            tabControlEtims.Location = new Point(14, 13);
            tabControlEtims.Margin = new Padding(4);
            tabControlEtims.Name = "tabControlEtims";
            tabControlEtims.SelectedIndex = 0;
            tabControlEtims.Size = new Size(1680, 158);
            tabControlEtims.TabIndex = 0;
            // 
            // ETIMSClient
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1708, 1172);
            Controls.Add(txtReqAddress);
            Controls.Add(btnCanceRequest);
            Controls.Add(btnSubmitRequest);
            Controls.Add(splitContainer1);
            Controls.Add(tabControlEtims);
            KeyPreview = true;
            Margin = new Padding(4);
            Name = "ETIMSClient";
            Text = "e-TIMS Client";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            grpRequest.ResumeLayout(false);
            grpResponse.ResumeLayout(false);
            tabSetup.ResumeLayout(false);
            grpSetup.ResumeLayout(false);
            lytTableSetup.ResumeLayout(false);
            tabSales.ResumeLayout(false);
            grpSales.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tabStock.ResumeLayout(false);
            grpStock.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tabPurchases.ResumeLayout(false);
            grpPurchases.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tabItems.ResumeLayout(false);
            grpItems.ResumeLayout(false);
            lytTableItems.ResumeLayout(false);
            tabControlEtims.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private SplitContainer splitContainer1;
        private GroupBox grpRequest;
        private GroupBox grpResponse;
        private ScintillaNET.Scintilla reqEditor;
        private ScintillaNET.Scintilla respEditor;
        private Button btnSubmitRequest;
        private Button btnCanceRequest;
        private TextBox txtReqAddress;
        private TabPage tabMPurchase;
        private TabPage tabSetup;
        private GroupBox grpSetup;
        private TableLayoutPanel lytTableSetup;
        private Button btnSelectCodes;
        private Button btnViewQueue;
        private Button btnSetupTaxes;
        private Button btnGetNotices;
        private TabPage tabSales;
        private GroupBox grpSales;
        private TableLayoutPanel tableLayoutPanel1;
        private Button btnGetARCRNote;
        private Button btnGetARInvoice;
        private Button btnGetOECRNote;
        private Button btnGetOEInvoice;
        private Button btnClearSales;
        private TabPage tabStock;
        private GroupBox grpStock;
        private TableLayoutPanel tableLayoutPanel2;
        private Button btnClearStock;
        private TabPage tabPurchases;
        private GroupBox grpPurchases;
        private TableLayoutPanel tableLayoutPanel3;
        private Button btnPOInvoice;
        private Button btnClearPurch;
        private Button btnGetPurch;
        private TabPage tabItems;
        private GroupBox grpItems;
        private TableLayoutPanel lytTableItems;
        private Button btnSelectItem;
        private Button btnSaveCompose;
        private Button btnSelectImports;
        private Button btnSaveItem;
        private TabControl tabControlEtims;
    }
}