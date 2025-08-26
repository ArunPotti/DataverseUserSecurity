namespace DataverseUserSecurity
{
    partial class MyPluginControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MyPluginControl));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbLoadData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbLblSearch = new System.Windows.Forms.ToolStripLabel();
            this.tsbTxtSearch = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbExport = new System.Windows.Forms.ToolStripButton();
            this.dgUsersData = new System.Windows.Forms.DataGridView();
            this.lblRetrievedUsers = new System.Windows.Forms.Label();
            this.toolStripMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgUsersData)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tssSeparator1,
            this.tsbLoadData,
            this.toolStripSeparator1,
            this.tsbLblSearch,
            this.tsbTxtSearch,
            this.toolStripSeparator2,
            this.tsbExport});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1991, 34);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.Image = ((System.Drawing.Image)(resources.GetObject("tsbClose.Image")));
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(83, 29);
            this.tsbClose.Text = "Close";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 34);
            // 
            // tsbLoadData
            // 
            this.tsbLoadData.Image = ((System.Drawing.Image)(resources.GetObject("tsbLoadData.Image")));
            this.tsbLoadData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadData.Name = "tsbLoadData";
            this.tsbLoadData.Size = new System.Drawing.Size(121, 29);
            this.tsbLoadData.Text = "Load Data";
            this.tsbLoadData.Click += new System.EventHandler(this.tsbLoadData_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 34);
            // 
            // tsbLblSearch
            // 
            this.tsbLblSearch.Name = "tsbLblSearch";
            this.tsbLblSearch.Size = new System.Drawing.Size(64, 29);
            this.tsbLblSearch.Text = "Search";
            // 
            // tsbTxtSearch
            // 
            this.tsbTxtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsbTxtSearch.Name = "tsbTxtSearch";
            this.tsbTxtSearch.Size = new System.Drawing.Size(400, 34);
            this.tsbTxtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tsbTxtSearch_KeyUp);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 34);
            // 
            // tsbExport
            // 
            this.tsbExport.Image = ((System.Drawing.Image)(resources.GetObject("tsbExport.Image")));
            this.tsbExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExport.Name = "tsbExport";
            this.tsbExport.Size = new System.Drawing.Size(91, 29);
            this.tsbExport.Text = "Export";
            this.tsbExport.Click += new System.EventHandler(this.tsbExport_Click);
            // 
            // dgUsersData
            // 
            this.dgUsersData.AllowUserToOrderColumns = true;
            this.dgUsersData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgUsersData.Location = new System.Drawing.Point(0, 97);
            this.dgUsersData.Name = "dgUsersData";
            this.dgUsersData.ReadOnly = true;
            this.dgUsersData.RowHeadersWidth = 62;
            this.dgUsersData.RowTemplate.Height = 28;
            this.dgUsersData.Size = new System.Drawing.Size(1988, 745);
            this.dgUsersData.TabIndex = 5;
            // 
            // lblRetrievedUsers
            // 
            this.lblRetrievedUsers.AutoSize = true;
            this.lblRetrievedUsers.Location = new System.Drawing.Point(3, 59);
            this.lblRetrievedUsers.Name = "lblRetrievedUsers";
            this.lblRetrievedUsers.Size = new System.Drawing.Size(249, 20);
            this.lblRetrievedUsers.TabIndex = 9;
            this.lblRetrievedUsers.Text = "Total number of users retrieved : 0";
            // 
            // MyPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.lblRetrievedUsers);
            this.Controls.Add(this.dgUsersData);
            this.Controls.Add(this.toolStripMenu);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MyPluginControl";
            this.Size = new System.Drawing.Size(1991, 845);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgUsersData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ToolStripButton tsbLoadData;
        private System.Windows.Forms.ToolStripButton tsbExport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.DataGridView dgUsersData;
        private System.Windows.Forms.Label lblRetrievedUsers;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel tsbLblSearch;
        private System.Windows.Forms.ToolStripTextBox tsbTxtSearch;
    }
}
