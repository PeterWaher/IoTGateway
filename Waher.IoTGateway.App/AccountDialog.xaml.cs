using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Waher.IoTGateway.App
{
	public sealed partial class AccountDialog : ContentDialog
	{
		private string host = string.Empty;
		private int port = 5222;
		private string userName = string.Empty;
		private string password = string.Empty;
		private string key = string.Empty;
		private string secret = string.Empty;
		private bool trustServerCertificate = false;
		private bool allowInsecureAuthentication = false;
		private bool storePassword = false;
		private bool createAccount = false;

		public AccountDialog(string Host, int Port, string UserName, bool TrustServer, bool AllowInsecure, bool StorePassword, bool CreateAccount,
			string Key, string Secret)
		{
			this.host = Host;
			this.port = Port;
			this.userName = UserName;
			this.trustServerCertificate = TrustServer;
			this.allowInsecureAuthentication = AllowInsecure;
			this.storePassword = StorePassword;
			this.createAccount = CreateAccount;
			this.key = Key;
			this.secret = Secret;

			this.InitializeComponent();

			this.HostInput.Text = Host;
			this.PortInput.Text = Port.ToString();
			this.UserNameInput.Text = UserName;
			this.TrustServerCertificate.IsChecked = TrustServer;
			this.AllowInsecureAuthentication.IsChecked = AllowInsecure;
			this.StorePassword.IsChecked = StorePassword;
			this.CreateAccount.IsChecked = CreateAccount;
			this.AccountCreationKey.Text = Key;
			this.AccountCreationSecret.Text = Secret;
		}

		public string Host
		{
			get { return this.host; }
			set { this.host = value; }
		}

		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		public string UserName
		{
			get { return this.userName; }
			set { this.userName = value; }
		}

		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		public bool TrustServer
		{
			get { return this.trustServerCertificate; }
			set { this.trustServerCertificate = value; }
		}

		public bool AllowInsecure
		{
			get { return this.allowInsecureAuthentication; }
			set { this.allowInsecureAuthentication = value; }
		}

		public bool AllowStorePassword
		{
			get { return this.storePassword; }
			set { this.storePassword = value; }
		}

		public bool AllowRegistration
		{
			get { return this.createAccount; }
			set { this.createAccount = value; }
		}

		public string Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

		public string Secret
		{
			get { return this.secret; }
			set { this.secret = value; }
		}

		private void CreateAccount_Click(object sender, RoutedEventArgs e)
		{
			bool b = this.CreateAccount.IsChecked.HasValue && this.CreateAccount.IsChecked.Value;

			this.RetypePassword.IsEnabled = b;
			this.AccountCreationKey.IsEnabled = b;
			this.AccountCreationSecret.IsEnabled = b;
		}

		private void ContentDialog_ConnectButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (ushort.TryParse(this.PortInput.Text, out ushort Port))
				this.port = Port;
			else
				args.Cancel = true;

			this.host = this.HostInput.Text;
			this.userName = this.UserNameInput.Text;
			this.password = this.PasswordInput.Password;
		}

		private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
		}
	}
}
