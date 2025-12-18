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
            tabControlEtims = new TabControl();
            tabItems = new TabPage();
            grpItems = new GroupBox();
            lytTableItems = new TableLayoutPanel();
            btnSelectItem = new Button();
            btnSaveCompose = new Button();
            btnSelectImports = new Button();
            btnSaveItem = new Button();
            tabPurchases = new TabPage();
            grpPurchases = new GroupBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            btnPOInvoice = new Button();
            btnClearPurch = new Button();
            btnGetPurch = new Button();
            tabStock = new TabPage();
            grpStock = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            btnClearStock = new Button();
            tabSales = new TabPage();
            grpSales = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnGetARCRNote = new Button();
            btnGetARInvoice = new Button();
            btnGetOECRNote = new Button();
            btnGetOEInvoice = new Button();
            btnClearSales = new Button();
            tabSetup = new TabPage();
            grpSetup = new GroupBox();
            lytTableSetup = new TableLayoutPanel();
            btnViewQueue = new Button();
            btnSetupTaxes = new Button();
            btnGetNotices = new Button();
            splitContainer1 = new SplitContainer();
            grpRequest = new GroupBox();
            reqEditor = new ScintillaNET.Scintilla();
            grpResponse = new GroupBox();
            respEditor = new ScintillaNET.Scintilla();
            btnSubmitRequest = new Button();
            btnCanceRequest = new Button();
            txtReqAddress = new TextBox();
            tabControlEtims.SuspendLayout();
            tabItems.SuspendLayout();
            grpItems.SuspendLayout();
            lytTableItems.SuspendLayout();
            tabPurchases.SuspendLayout();
            grpPurchases.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tabStock.SuspendLayout();
            grpStock.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tabSales.SuspendLayout();
            grpSales.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tabSetup.SuspendLayout();
            grpSetup.SuspendLayout();
            lytTableSetup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            grpRequest.SuspendLayout();
            grpResponse.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlEtims
            // 
            tabControlEtims.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tabControlEtims.Controls.Add(tabItems);
            tabControlEtims.Controls.Add(tabPurchases);
            tabControlEtims.Controls.Add(tabStock);
            tabControlEtims.Controls.Add(tabSales);
            tabControlEtims.Controls.Add(tabSetup);
            tabControlEtims.Location = new Point(12, 11);
            tabControlEtims.Name = "tabControlEtims";
            tabControlEtims.SelectedIndex = 0;
            tabControlEtims.Size = new Size(1400, 132);
            tabControlEtims.TabIndex = 0;
            // 
            // tabItems
            // 
            tabItems.Controls.Add(grpItems);
            tabItems.Location = new Point(4, 34);
            tabItems.Name = "tabItems";
            tabItems.Padding = new Padding(3);
            tabItems.Size = new Size(1392, 94);
            tabItems.TabIndex = 0;
            tabItems.Text = "Manage Item";
            tabItems.UseVisualStyleBackColor = true;
            // 
            // grpItems
            // 
            grpItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpItems.Controls.Add(lytTableItems);
            grpItems.Location = new Point(6, 6);
            grpItems.Name = "grpItems";
            grpItems.Size = new Size(1379, 82);
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
            lytTableItems.Location = new Point(0, 30);
            lytTableItems.Name = "lytTableItems";
            lytTableItems.RowCount = 1;
            lytTableItems.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lytTableItems.Size = new Size(1373, 46);
            lytTableItems.TabIndex = 5;
            // 
            // btnSelectItem
            // 
            btnSelectItem.Anchor = AnchorStyles.Left;
            btnSelectItem.Location = new Point(8, 6);
            btnSelectItem.Margin = new Padding(8, 3, 3, 3);
            btnSelectItem.Name = "btnSelectItem";
            btnSelectItem.Size = new Size(177, 34);
            btnSelectItem.TabIndex = 1;
            btnSelectItem.Text = "Select Item";
            btnSelectItem.UseVisualStyleBackColor = true;
            // 
            // btnSaveCompose
            // 
            btnSaveCompose.Anchor = AnchorStyles.Right;
            btnSaveCompose.Location = new Point(1157, 6);
            btnSaveCompose.Margin = new Padding(3, 3, 8, 3);
            btnSaveCompose.Name = "btnSaveCompose";
            btnSaveCompose.Size = new Size(208, 34);
            btnSaveCompose.TabIndex = 3;
            btnSaveCompose.Text = "Save Composition";
            btnSaveCompose.UseVisualStyleBackColor = true;
            // 
            // btnSelectImports
            // 
            btnSelectImports.Anchor = AnchorStyles.None;
            btnSelectImports.Location = new Point(330, 6);
            btnSelectImports.Name = "btnSelectImports";
            btnSelectImports.Size = new Size(162, 34);
            btnSelectImports.TabIndex = 4;
            btnSelectImports.Text = "Select Imports";
            btnSelectImports.UseVisualStyleBackColor = true;
            // 
            // btnSaveItem
            // 
            btnSaveItem.Anchor = AnchorStyles.None;
            btnSaveItem.Location = new Point(891, 6);
            btnSaveItem.Name = "btnSaveItem";
            btnSaveItem.Size = new Size(136, 34);
            btnSaveItem.TabIndex = 2;
            btnSaveItem.Text = "Save Item";
            btnSaveItem.UseVisualStyleBackColor = true;
            // 
            // tabPurchases
            // 
            tabPurchases.Controls.Add(grpPurchases);
            tabPurchases.Location = new Point(4, 34);
            tabPurchases.Name = "tabPurchases";
            tabPurchases.Padding = new Padding(3);
            tabPurchases.Size = new Size(1392, 94);
            tabPurchases.TabIndex = 1;
            tabPurchases.Text = "Manage Purchases";
            tabPurchases.UseVisualStyleBackColor = true;
            // 
            // grpPurchases
            // 
            grpPurchases.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpPurchases.Controls.Add(tableLayoutPanel3);
            grpPurchases.Location = new Point(3, 3);
            grpPurchases.Name = "grpPurchases";
            grpPurchases.Size = new Size(1386, 88);
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
            tableLayoutPanel3.Location = new Point(6, 30);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new Size(1376, 58);
            tableLayoutPanel3.TabIndex = 3;
            // 
            // btnPOInvoice
            // 
            btnPOInvoice.Anchor = AnchorStyles.Left;
            btnPOInvoice.Location = new Point(8, 12);
            btnPOInvoice.Margin = new Padding(8, 3, 3, 3);
            btnPOInvoice.Name = "btnPOInvoice";
            btnPOInvoice.Size = new Size(177, 34);
            btnPOInvoice.TabIndex = 8;
            btnPOInvoice.Text = "Select PO Invoice";
            btnPOInvoice.UseVisualStyleBackColor = true;
            // 
            // btnClearPurch
            // 
            btnClearPurch.Anchor = AnchorStyles.None;
            btnClearPurch.Location = new Point(631, 12);
            btnClearPurch.Name = "btnClearPurch";
            btnClearPurch.Size = new Size(112, 34);
            btnClearPurch.TabIndex = 7;
            btnClearPurch.Text = "Clear";
            btnClearPurch.UseVisualStyleBackColor = true;
            // 
            // btnGetPurch
            // 
            btnGetPurch.Anchor = AnchorStyles.Right;
            btnGetPurch.Location = new Point(1184, 12);
            btnGetPurch.Name = "btnGetPurch";
            btnGetPurch.Padding = new Padding(0, 0, 10, 0);
            btnGetPurch.Size = new Size(189, 34);
            btnGetPurch.TabIndex = 9;
            btnGetPurch.Text = "Select Purchases";
            btnGetPurch.UseVisualStyleBackColor = true;
            // 
            // tabStock
            // 
            tabStock.Controls.Add(grpStock);
            tabStock.Location = new Point(4, 34);
            tabStock.Name = "tabStock";
            tabStock.Size = new Size(1392, 94);
            tabStock.TabIndex = 2;
            tabStock.Text = "Manage Stock";
            tabStock.UseVisualStyleBackColor = true;
            // 
            // grpStock
            // 
            grpStock.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpStock.Controls.Add(tableLayoutPanel2);
            grpStock.Location = new Point(6, 3);
            grpStock.Name = "grpStock";
            grpStock.Size = new Size(1383, 88);
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
            tableLayoutPanel2.Location = new Point(0, 30);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(1379, 58);
            tableLayoutPanel2.TabIndex = 2;
            // 
            // btnClearStock
            // 
            btnClearStock.Anchor = AnchorStyles.None;
            btnClearStock.Location = new Point(631, 12);
            btnClearStock.Name = "btnClearStock";
            btnClearStock.Size = new Size(112, 34);
            btnClearStock.TabIndex = 7;
            btnClearStock.Text = "Clear";
            btnClearStock.UseVisualStyleBackColor = true;
            // 
            // tabSales
            // 
            tabSales.Controls.Add(grpSales);
            tabSales.Location = new Point(4, 34);
            tabSales.Name = "tabSales";
            tabSales.Size = new Size(1392, 94);
            tabSales.TabIndex = 3;
            tabSales.Text = "Manage Sales";
            tabSales.UseVisualStyleBackColor = true;
            // 
            // grpSales
            // 
            grpSales.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSales.Controls.Add(tableLayoutPanel1);
            grpSales.Location = new Point(6, 3);
            grpSales.Name = "grpSales";
            grpSales.Size = new Size(1383, 88);
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
            tableLayoutPanel1.Location = new Point(0, 30);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1377, 52);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnGetARCRNote
            // 
            btnGetARCRNote.Anchor = AnchorStyles.Right;
            btnGetARCRNote.Location = new Point(1177, 9);
            btnGetARCRNote.Name = "btnGetARCRNote";
            btnGetARCRNote.Padding = new Padding(0, 0, 10, 0);
            btnGetARCRNote.Size = new Size(197, 34);
            btnGetARCRNote.TabIndex = 14;
            btnGetARCRNote.Text = "Select AR CRNote";
            btnGetARCRNote.UseVisualStyleBackColor = true;
            // 
            // btnGetARInvoice
            // 
            btnGetARInvoice.Anchor = AnchorStyles.None;
            btnGetARInvoice.Location = new Point(864, 9);
            btnGetARInvoice.Name = "btnGetARInvoice";
            btnGetARInvoice.Padding = new Padding(0, 0, 10, 0);
            btnGetARInvoice.Size = new Size(197, 34);
            btnGetARInvoice.TabIndex = 13;
            btnGetARInvoice.Text = "Select AR Invoice";
            btnGetARInvoice.UseVisualStyleBackColor = true;
            // 
            // btnGetOECRNote
            // 
            btnGetOECRNote.Anchor = AnchorStyles.None;
            btnGetOECRNote.Location = new Point(331, 9);
            btnGetOECRNote.Name = "btnGetOECRNote";
            btnGetOECRNote.Size = new Size(162, 34);
            btnGetOECRNote.TabIndex = 9;
            btnGetOECRNote.Text = "Select OE CRNote";
            btnGetOECRNote.UseVisualStyleBackColor = true;
            // 
            // btnGetOEInvoice
            // 
            btnGetOEInvoice.Anchor = AnchorStyles.Left;
            btnGetOEInvoice.Location = new Point(8, 9);
            btnGetOEInvoice.Margin = new Padding(8, 3, 3, 3);
            btnGetOEInvoice.Name = "btnGetOEInvoice";
            btnGetOEInvoice.Size = new Size(177, 34);
            btnGetOEInvoice.TabIndex = 8;
            btnGetOEInvoice.Text = "Select OE Invoice";
            btnGetOEInvoice.UseVisualStyleBackColor = true;
            // 
            // btnClearSales
            // 
            btnClearSales.Anchor = AnchorStyles.None;
            btnClearSales.Location = new Point(631, 9);
            btnClearSales.Name = "btnClearSales";
            btnClearSales.Size = new Size(112, 34);
            btnClearSales.TabIndex = 7;
            btnClearSales.Text = "Clear";
            btnClearSales.UseVisualStyleBackColor = true;
            // 
            // tabSetup
            // 
            tabSetup.Controls.Add(grpSetup);
            tabSetup.Location = new Point(4, 34);
            tabSetup.Name = "tabSetup";
            tabSetup.Size = new Size(1392, 94);
            tabSetup.TabIndex = 4;
            tabSetup.Text = "Basic Setup";
            tabSetup.UseVisualStyleBackColor = true;
            // 
            // grpSetup
            // 
            grpSetup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSetup.Controls.Add(lytTableSetup);
            grpSetup.Location = new Point(6, 3);
            grpSetup.Name = "grpSetup";
            grpSetup.Size = new Size(1379, 88);
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
            lytTableSetup.Controls.Add(btnViewQueue, 2, 0);
            lytTableSetup.Controls.Add(btnSetupTaxes, 4, 0);
            lytTableSetup.Controls.Add(btnGetNotices, 3, 0);
            lytTableSetup.Location = new Point(6, 30);
            lytTableSetup.Name = "lytTableSetup";
            lytTableSetup.RowCount = 1;
            lytTableSetup.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lytTableSetup.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            lytTableSetup.Size = new Size(1367, 52);
            lytTableSetup.TabIndex = 0;
            // 
            // btnViewQueue
            // 
            btnViewQueue.Anchor = AnchorStyles.None;
            btnViewQueue.Location = new Point(618, 9);
            btnViewQueue.Name = "btnViewQueue";
            btnViewQueue.Size = new Size(129, 34);
            btnViewQueue.TabIndex = 6;
            btnViewQueue.Text = "View Queue";
            btnViewQueue.UseVisualStyleBackColor = true;
            // 
            // btnSetupTaxes
            // 
            btnSetupTaxes.Anchor = AnchorStyles.Right;
            btnSetupTaxes.Location = new Point(1224, 9);
            btnSetupTaxes.Name = "btnSetupTaxes";
            btnSetupTaxes.Padding = new Padding(0, 0, 10, 0);
            btnSetupTaxes.Size = new Size(140, 34);
            btnSetupTaxes.TabIndex = 7;
            btnSetupTaxes.Text = "Setup Taxes";
            btnSetupTaxes.UseVisualStyleBackColor = true;
            // 
            // btnGetNotices
            // 
            btnGetNotices.Anchor = AnchorStyles.None;
            btnGetNotices.Location = new Point(857, 9);
            btnGetNotices.Name = "btnGetNotices";
            btnGetNotices.Padding = new Padding(0, 0, 10, 0);
            btnGetNotices.Size = new Size(197, 34);
            btnGetNotices.TabIndex = 14;
            btnGetNotices.Text = "Get Notices";
            btnGetNotices.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(16, 145);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(grpRequest);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(grpResponse);
            splitContainer1.Size = new Size(1396, 788);
            splitContainer1.SplitterDistance = 692;
            splitContainer1.TabIndex = 1;
            // 
            // grpRequest
            // 
            grpRequest.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpRequest.Controls.Add(reqEditor);
            grpRequest.Location = new Point(0, 3);
            grpRequest.Name = "grpRequest";
            grpRequest.Size = new Size(689, 782);
            grpRequest.TabIndex = 1;
            grpRequest.TabStop = false;
            grpRequest.Text = "Request Data";
            // 
            // reqEditor
            // 
            reqEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            reqEditor.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            reqEditor.LexerName = null;
            reqEditor.Location = new Point(6, 26);
            reqEditor.Margin = new Padding(2);
            reqEditor.Name = "reqEditor";
            reqEditor.ScrollWidth = 86;
            reqEditor.Size = new Size(678, 751);
            reqEditor.TabIndex = 0;
            // 
            // grpResponse
            // 
            grpResponse.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpResponse.Controls.Add(respEditor);
            grpResponse.Location = new Point(3, 0);
            grpResponse.Name = "grpResponse";
            grpResponse.Size = new Size(689, 788);
            grpResponse.TabIndex = 1;
            grpResponse.TabStop = false;
            grpResponse.Text = "Response Data";
            // 
            // respEditor
            // 
            respEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            respEditor.AutocompleteListSelectedBackColor = Color.FromArgb(0, 120, 215);
            respEditor.LexerName = null;
            respEditor.Location = new Point(6, 29);
            respEditor.Margin = new Padding(2);
            respEditor.Name = "respEditor";
            respEditor.ReadOnly = true;
            respEditor.ScrollWidth = 86;
            respEditor.Size = new Size(678, 751);
            respEditor.TabIndex = 0;
            // 
            // btnSubmitRequest
            // 
            btnSubmitRequest.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSubmitRequest.Location = new Point(1235, 939);
            btnSubmitRequest.Margin = new Padding(8, 3, 3, 3);
            btnSubmitRequest.Name = "btnSubmitRequest";
            btnSubmitRequest.Size = new Size(177, 34);
            btnSubmitRequest.TabIndex = 9;
            btnSubmitRequest.Text = "Submit Request";
            btnSubmitRequest.UseVisualStyleBackColor = true;
            // 
            // btnCanceRequest
            // 
            btnCanceRequest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCanceRequest.Location = new Point(12, 939);
            btnCanceRequest.Margin = new Padding(8, 3, 3, 3);
            btnCanceRequest.Name = "btnCanceRequest";
            btnCanceRequest.Size = new Size(177, 34);
            btnCanceRequest.TabIndex = 10;
            btnCanceRequest.Text = "Cancel && Reset";
            btnCanceRequest.UseVisualStyleBackColor = true;
            // 
            // txtReqAddress
            // 
            txtReqAddress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtReqAddress.Location = new Point(205, 939);
            txtReqAddress.Name = "txtReqAddress";
            txtReqAddress.Size = new Size(991, 31);
            txtReqAddress.TabIndex = 11;
            // 
            // ETIMSClient
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1423, 977);
            Controls.Add(txtReqAddress);
            Controls.Add(btnCanceRequest);
            Controls.Add(btnSubmitRequest);
            Controls.Add(splitContainer1);
            Controls.Add(tabControlEtims);
            KeyPreview = true;
            Name = "ETIMSClient";
            Text = "e-TIMS Client";
            tabControlEtims.ResumeLayout(false);
            tabItems.ResumeLayout(false);
            grpItems.ResumeLayout(false);
            lytTableItems.ResumeLayout(false);
            tabPurchases.ResumeLayout(false);
            grpPurchases.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tabStock.ResumeLayout(false);
            grpStock.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tabSales.ResumeLayout(false);
            grpSales.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tabSetup.ResumeLayout(false);
            grpSetup.ResumeLayout(false);
            lytTableSetup.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            grpRequest.ResumeLayout(false);
            grpResponse.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControlEtims;
        private TabPage tabItems;
        private TabPage tabPurchases;
        private SplitContainer splitContainer1;
        private GroupBox grpRequest;
        private GroupBox grpResponse;
        private GroupBox grpItems;
        private Button btnSelectItem;
        private Button btnSaveCompose;
        private Button btnSaveItem;
        private Button btnSelectImports;
        private TableLayoutPanel lytTableItems;
        private TabPage tabStock;
        private TabPage tabSales;
        private TabPage tabSetup;
        private GroupBox grpSetup;
        private TableLayoutPanel lytTableSetup;
        private Button btnViewQueue;
        private GroupBox grpSales;
        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox grpStock;
        private GroupBox grpPurchases;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel2;
        private Button btnClearPurch;
        private Button btnClearStock;
        private Button btnClearSales;
        private Button btnGetOEInvoice;
        private Button btnGetOECRNote;
        private ScintillaNET.Scintilla reqEditor;
        private ScintillaNET.Scintilla respEditor;
        private Button btnGetARInvoice;
        private Button btnGetARCRNote;
        private Button btnSetupTaxes;
        private Button btnPOInvoice;
        private Button btnGetNotices;
        private Button btnGetPurch;
        private Button btnSubmitRequest;
        private Button btnCanceRequest;
        private TextBox txtReqAddress;
    }
}