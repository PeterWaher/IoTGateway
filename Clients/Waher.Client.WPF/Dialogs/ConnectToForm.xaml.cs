using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for ConnectToForm.xaml
	/// </summary>
	public partial class ConnectToForm : Window
	{
		private XmppClient client = null;
		private string passwordHash = string.Empty;
		private string passwordHashMethod = string.Empty;

		public ConnectToForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Password hash of a successfully authenticated client.
		/// </summary>
		public string PasswordHash { get { return this.passwordHash; } }

		/// <summary>
		/// Password hash method of a successfully authenticated client.
		/// </summary>
		public string PasswordHashMethod { get { return this.passwordHashMethod; } }

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			if (!int.TryParse(this.XmppPort.Text, out int Port) || Port <= 0 || Port > 65535)
			{
				MessageBox.Show(this, "Invalid port number. Valid port numbers are positive integers between 1 and 65535. The default port number is 5222.",
					"Error", MessageBoxButton.OK, MessageBoxImage.Error);
				this.XmppPort.Focus();
				return;
			}

			bool Create = this.CreateAccount.IsChecked.HasValue && this.CreateAccount.IsChecked.Value;
			if (Create && this.Password.Password != this.RetypePassword.Password)
			{
				MessageBox.Show(this, "The two passwords must match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Password.Focus();
				return;
			}

			this.CloseClient();
			this.ConnectButton.IsEnabled = false;
			this.XmppServer.IsEnabled = false;
			this.XmppPort.IsEnabled = false;
			this.AccountName.IsEnabled = false;
			this.Password.IsEnabled = false;
			this.RetypePassword.IsEnabled = false;
			this.TrustServerCertificate.IsEnabled = false;
			this.CreateAccount.IsEnabled = false;

			this.client = new XmppClient(this.XmppServer.Text, Port, this.AccountName.Text, this.Password.Password, "en",
				typeof(App).Assembly);

			if (this.AllowInsecureAuthentication.IsChecked.HasValue && this.AllowInsecureAuthentication.IsChecked.Value)
			{
				this.client.AllowPlain = true;
				this.client.AllowCramMD5 = true;
				this.client.AllowDigestMD5 = true;
			}

			if (Create)
			{
				this.client.AllowRegistration();
				this.client.OnRegistrationForm += Client_OnRegistrationForm;
			}

			if (this.TrustServerCertificate.IsChecked.HasValue && this.TrustServerCertificate.IsChecked.Value)
				this.client.TrustServer = true;

			this.client.OnStateChanged += new StateChangedEventHandler(Client_OnStateChanged);
			this.client.OnConnectionError += new XmppExceptionEventHandler(Client_OnConnectionError);
			this.client.Connect();
		}

		private void Client_OnRegistrationForm(object Sender, DataForm Form)
		{
			Field FormType = Form["FORM_TYPE"];
			if (FormType != null && FormType.ValueString == "urn:xmpp:captcha")
			{
				ParameterDialog Dialog = new ParameterDialog(Form);

				MainWindow.currentInstance.Dispatcher.Invoke(() => Dialog.ShowDialog());
			}
			else
				Form.Submit();
		}

		private void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.XmppStateChanged), NewState);
		}

		private void XmppStateChanged(object P)
		{
			XmppState NewState = (XmppState)P;

			switch (NewState)
			{
				case XmppState.Offline:
					this.ConnectionState.Content = "Offline.";
					break;

				case XmppState.Connecting:
					this.ConnectionState.Content = "Connecting.";
					break;

				case XmppState.StreamNegotiation:
					this.ConnectionState.Content = "Stream negotiation.";
					break;

				case XmppState.StreamOpened:
					this.ConnectionState.Content = "Stream opened.";
					break;

				case XmppState.StartingEncryption:
					this.ConnectionState.Content = "Starting encryption.";
					break;

				case XmppState.Authenticating:
					this.ConnectionState.Content = "Authenticating user.";
					break;

				case XmppState.Registering:
					this.ConnectionState.Content = "Registering account.";
					break;

				case XmppState.Binding:
					this.ConnectionState.Content = "Binding resource.";
					break;

				case XmppState.FetchingRoster:
					this.ConnectionState.Content = "Fetching roster.";
					break;

				case XmppState.SettingPresence:
					this.ConnectionState.Content = "Setting presence.";
					break;

				case XmppState.Connected:
					this.passwordHash = this.client.PasswordHash;
					this.passwordHashMethod = this.client.PasswordHashMethod;
					this.ConnectionState.Content = "Connected.";
					this.CloseClient();
					this.DialogResult = true;
					break;

				case XmppState.Error:
					this.ConnectionState.Content = "Error.";
					this.CloseClient();
					break;

			}
		}

		private void CloseClient()
		{
			if (this.client != null)
			{
				XmppClient Client = this.client;
				this.client = null;
				Client.Dispose();
			}

			this.ConnectButton.IsEnabled = true;
			this.XmppServer.IsEnabled = true;
			this.XmppPort.IsEnabled = true;
			this.AccountName.IsEnabled = true;
			this.Password.IsEnabled = true;
			this.TrustServerCertificate.IsEnabled = true;
			this.CreateAccount.IsEnabled = true;

			this.RetypePassword.IsEnabled = (this.CreateAccount.IsChecked.HasValue && this.CreateAccount.IsChecked.Value);
		}

		private void Client_OnConnectionError(object Sender, Exception Exception)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.ShowError), Exception);
		}

		private void ShowError(object P)
		{
			Exception ex = (Exception)P;
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public XmppClient Client
		{
			get { return this.client; }
		}

		private void CreateAccount_Click(object sender, RoutedEventArgs e)
		{
			this.RetypePassword.IsEnabled = this.CreateAccount.IsChecked.HasValue && this.CreateAccount.IsChecked.Value;
		}

	}
}
