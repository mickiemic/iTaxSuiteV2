namespace iTaxSuite.WinForms.Clients
{
    partial class VSCUStock
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            tabControl1 = new TabControl();
            tabPurchases = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            dataGridPurchHead = new DataGridView();
            dataGridPurchDet = new DataGridView();
            btnSavePurchase = new Button();
            tabImports = new TabPage();
            tableLayoutPanel2 = new TableLayoutPanel();
            btnSaveImport = new Button();
            dataGridImpHead = new DataGridView();
            dataGridImpDet = new DataGridView();
            sqlCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            tabControl1.SuspendLayout();
            tabPurchases.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridPurchHead).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridPurchDet).BeginInit();
            tabImports.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridImpHead).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridImpDet).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(tabPurchases);
            tabControl1.Controls.Add(tabImports);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1436, 967);
            tabControl1.TabIndex = 0;
            // 
            // tabPurchases
            // 
            tabPurchases.Controls.Add(tableLayoutPanel1);
            tabPurchases.Location = new Point(4, 34);
            tabPurchases.Name = "tabPurchases";
            tabPurchases.Padding = new Padding(3);
            tabPurchases.Size = new Size(1428, 929);
            tabPurchases.TabIndex = 0;
            tabPurchases.Text = "Purchases";
            tabPurchases.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(dataGridPurchHead, 0, 0);
            tableLayoutPanel1.Controls.Add(dataGridPurchDet, 0, 2);
            tableLayoutPanel1.Controls.Add(btnSavePurchase, 0, 1);
            tableLayoutPanel1.Location = new Point(6, 5);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 86.54709F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 13.4529152F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 383F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            tableLayoutPanel1.Size = new Size(1418, 913);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // dataGridPurchHead
            // 
            dataGridPurchHead.AllowUserToAddRows = false;
            dataGridPurchHead.AllowUserToDeleteRows = false;
            dataGridPurchHead.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridPurchHead.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridPurchHead.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridPurchHead.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridPurchHead.Location = new Point(3, 3);
            dataGridPurchHead.MultiSelect = false;
            dataGridPurchHead.Name = "dataGridPurchHead";
            dataGridPurchHead.RowHeadersWidth = 62;
            dataGridPurchHead.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridPurchHead.Size = new Size(1412, 424);
            dataGridPurchHead.TabIndex = 0;
            // 
            // dataGridPurchDet
            // 
            dataGridPurchDet.AllowUserToAddRows = false;
            dataGridPurchDet.AllowUserToDeleteRows = false;
            dataGridPurchDet.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridPurchDet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridPurchDet.Location = new Point(3, 499);
            dataGridPurchDet.Name = "dataGridPurchDet";
            dataGridPurchDet.RowHeadersWidth = 62;
            dataGridPurchDet.Size = new Size(1412, 377);
            dataGridPurchDet.TabIndex = 2;
            // 
            // btnSavePurchase
            // 
            btnSavePurchase.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnSavePurchase.Enabled = false;
            btnSavePurchase.Location = new Point(1240, 432);
            btnSavePurchase.Margin = new Padding(2);
            btnSavePurchase.Name = "btnSavePurchase";
            btnSavePurchase.Size = new Size(176, 62);
            btnSavePurchase.TabIndex = 3;
            btnSavePurchase.Text = "Save Purchase";
            btnSavePurchase.UseVisualStyleBackColor = true;
            btnSavePurchase.Click += btnSavePurchase_Click;
            // 
            // tabImports
            // 
            tabImports.Controls.Add(tableLayoutPanel2);
            tabImports.Location = new Point(4, 34);
            tabImports.Name = "tabImports";
            tabImports.Padding = new Padding(3);
            tabImports.Size = new Size(1428, 929);
            tabImports.TabIndex = 1;
            tabImports.Text = "Imports";
            tabImports.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(btnSaveImport, 0, 1);
            tableLayoutPanel2.Controls.Add(dataGridImpHead, 0, 0);
            tableLayoutPanel2.Controls.Add(dataGridImpDet, 0, 2);
            tableLayoutPanel2.Location = new Point(6, 6);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 89.02953F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 10.9704638F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 422F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(1416, 917);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // btnSaveImport
            // 
            btnSaveImport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnSaveImport.Location = new Point(1238, 424);
            btnSaveImport.Margin = new Padding(2);
            btnSaveImport.Name = "btnSaveImport";
            btnSaveImport.Size = new Size(176, 48);
            btnSaveImport.TabIndex = 4;
            btnSaveImport.Text = "Save Import";
            btnSaveImport.UseVisualStyleBackColor = true;
            // 
            // dataGridImpHead
            // 
            dataGridImpHead.AllowUserToAddRows = false;
            dataGridImpHead.AllowUserToDeleteRows = false;
            dataGridImpHead.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = SystemColors.Control;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dataGridImpHead.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dataGridImpHead.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dataGridImpHead.DefaultCellStyle = dataGridViewCellStyle4;
            dataGridImpHead.Location = new Point(3, 3);
            dataGridImpHead.MultiSelect = false;
            dataGridImpHead.Name = "dataGridImpHead";
            dataGridImpHead.RowHeadersWidth = 62;
            dataGridImpHead.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridImpHead.Size = new Size(1410, 416);
            dataGridImpHead.TabIndex = 1;
            // 
            // dataGridImpDet
            // 
            dataGridImpDet.AllowUserToAddRows = false;
            dataGridImpDet.AllowUserToDeleteRows = false;
            dataGridImpDet.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridImpDet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridImpDet.Location = new Point(3, 477);
            dataGridImpDet.Name = "dataGridImpDet";
            dataGridImpDet.RowHeadersWidth = 62;
            dataGridImpDet.Size = new Size(1410, 416);
            dataGridImpDet.TabIndex = 3;
            // 
            // sqlCommand1
            // 
            sqlCommand1.CommandTimeout = 30;
            sqlCommand1.EnableOptimizedParameterBinding = false;
            // 
            // VSCUStock
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1451, 991);
            Controls.Add(tabControl1);
            Name = "VSCUStock";
            Text = "VSCU Stock Control";
            tabControl1.ResumeLayout(false);
            tabPurchases.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridPurchHead).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridPurchDet).EndInit();
            tabImports.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridImpHead).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridImpDet).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPurchases;
        private TabPage tabImports;
        private DataGridView dataGridPurchHead;
        private Microsoft.Data.SqlClient.SqlCommand sqlCommand1;
        private TableLayoutPanel tableLayoutPanel1;
        private DataGridView dataGridPurchDet;
        private Button btnSavePurchase;
        private TableLayoutPanel tableLayoutPanel2;
        private DataGridView dataGridImpHead;
        private DataGridView dataGridImpDet;
        private Button btnSaveImport;
    }
}