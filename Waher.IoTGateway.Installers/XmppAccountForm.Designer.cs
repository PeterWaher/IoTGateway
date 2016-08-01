namespace Waher.IoTGateway.Installers
{
	partial class XmppAccountForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XmppAccountForm));
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.label2 = new System.Windows.Forms.Label();
			this.Broker = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Font = new System.Drawing.Font("Tahoma", 14F);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(714, 526);
			this.tabControl1.TabIndex = 1;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.linkLabel1);
			this.tabPage1.Controls.Add(this.label5);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.Broker);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Location = new System.Drawing.Point(4, 37);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(706, 485);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Broker";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 25F);
			this.label1.Location = new System.Drawing.Point(8, 21);
			this.label1.MinimumSize = new System.Drawing.Size(75, 75);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 75);
			this.label1.TabIndex = 0;
			this.label1.Text = "1";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label1.Paint += new System.Windows.Forms.PaintEventHandler(this.label_Paint);
			// 
			// tabPage2
			// 
			this.tabPage2.Location = new System.Drawing.Point(4, 37);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(706, 485);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Port";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// tabPage3
			// 
			this.tabPage3.Location = new System.Drawing.Point(4, 37);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(706, 485);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Account Name";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// tabPage4
			// 
			this.tabPage4.Location = new System.Drawing.Point(4, 37);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(706, 485);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Password";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// tabPage5
			// 
			this.tabPage5.Location = new System.Drawing.Point(4, 37);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(706, 485);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "Display Name";
			this.tabPage5.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(111, 43);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(261, 29);
			this.label2.TabIndex = 1;
			this.label2.Text = "Select service provider:";
			// 
			// Broker
			// 
			this.Broker.AutoCompleteCustomSource.AddRange(new string[] {
            "thingsociety.com",
            "kode.im"});
			this.Broker.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.Broker.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.Broker.FormattingEnabled = true;
			this.Broker.Location = new System.Drawing.Point(116, 75);
			this.Broker.Name = "Broker";
			this.Broker.Size = new System.Drawing.Size(549, 36);
			this.Broker.TabIndex = 2;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 12F);
			this.label3.Location = new System.Drawing.Point(116, 147);
			this.label3.MaximumSize = new System.Drawing.Size(550, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(546, 48);
			this.label3.TabIndex = 3;
			this.label3.Text = "The service provider will provide you with a backend communication infrastructure" +
    " and a communication identity.";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Tahoma", 12F);
			this.label4.Location = new System.Drawing.Point(116, 215);
			this.label4.MaximumSize = new System.Drawing.Size(550, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(549, 120);
			this.label4.TabIndex = 4;
			this.label4.Text = resources.GetString("label4.Text");
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Tahoma", 12F);
			this.label5.Location = new System.Drawing.Point(116, 349);
			this.label5.MaximumSize = new System.Drawing.Size(550, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(521, 48);
			this.label5.TabIndex = 5;
			this.label5.Text = "A non-exhaustive list of publicly available brokers can be found here:";
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(111, 415);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(339, 29);
			this.linkLabel1.TabIndex = 6;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "https://xmpp.net/directory.php";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
			// 
			// XmppAccountForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(711, 524);
			this.Controls.Add(this.tabControl1);
			this.Name = "XmppAccountForm";
			this.Text = "Create Account";
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox Broker;
	}
}