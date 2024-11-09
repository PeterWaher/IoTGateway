using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Client.WPF.Model;

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
			this.InitializeComponent();
			this.ConnectionMethod_SelectionChanged(this, null);
		}

		/// <summary>
		/// Password hash of a successfully authenticated client.
		/// </summary>
		public string PasswordHash
		{
			get => this.passwordHash;
			set => this.passwordHash = value;
		}

		/// <summary>
		/// Password hash method of a successfully authenticated client.
		/// </summary>
		public string PasswordHashMethod
		{
			get => this.passwordHashMethod;
			set => this.passwordHashMethod = value;
		}

		private void CancelButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private async void ConnectButton_Click(object Sender, RoutedEventArgs e)
		{
			try
			{
				XmppCredentials Credentials = new XmppCredentials()
				{
					Host = this.XmppServer.Text,
					Account = this.AccountName.Text
				};

				switch ((TransportMethod)this.ConnectionMethod.SelectedIndex)
				{
					case TransportMethod.TraditionalSocket:
						if (!int.TryParse(this.XmppPort.Text, out int Port) || Port <= 0 || Port > 65535)
						{
							MessageBox.Show(this, "Invalid port number. Valid port numbers are positive integers between 1 and 65535. The default port number is " +
								XmppCredentials.DefaultPort.ToString() + ".", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							this.XmppPort.Focus();
							return;
						}

						Credentials.Port = Port;
						break;

					case TransportMethod.WS:
						Uri Uri;
						try
						{
							Uri = new Uri(this.UrlEndpoint.Text);
						}
						catch (Exception)
						{
							MessageBox.Show(this, "Invalid URI.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							this.UrlEndpoint.Focus();
							return;
						}

						string Scheme = Uri.Scheme.ToLower();

						if (Scheme != "ws" && Scheme != "wss")
						{
							MessageBox.Show(this, "Resource must be an WS or WSS URI.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							this.UrlEndpoint.Focus();
							return;
						}

						if (!Uri.IsAbsoluteUri)
						{
							MessageBox.Show(this, "URI must be an absolute URI.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							this.UrlEndpoint.Focus();
							return;
						}

						Credentials.UriEndpoint = this.UrlEndpoint.Text;
						break;

					case TransportMethod.BOSH:
						try
						{
							Uri = new Uri(this.UrlEndpoint.Text);
						}
						catch (Exception)
						{
							MessageBox.Show(this, "Invalid URI.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							this.UrlEndpoint.Focus();
							return;
						}

						Scheme = Uri.Scheme.ToLower();

						if (Scheme != "http" && Scheme != "https")
						{
							MessageBox.Show(this, "Resource must be an HTTP or HTTPS URI.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							this.UrlEndpoint.Focus();
							return;
						}

						if (!Uri.IsAbsoluteUri)
						{
							MessageBox.Show(this, "URI must be an absolute URI.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							this.UrlEndpoint.Focus();
							return;
						}

						Credentials.UriEndpoint = this.UrlEndpoint.Text;
						break;
				}

				bool Create = this.CreateAccount.IsChecked.HasValue && this.CreateAccount.IsChecked.Value;
				if (Create && this.Password.Password != this.RetypePassword.Password)
				{
					MessageBox.Show(this, "The two passwords must match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					this.Password.Focus();
					return;
				}

				await this.CloseClient();
				this.ConnectButton.IsEnabled = false;
				this.XmppServer.IsEnabled = false;
				this.ConnectionMethod.IsEnabled = false;
				this.XmppPort.IsEnabled = false;
				this.UrlEndpoint.IsEnabled = false;
				this.AccountName.IsEnabled = false;
				this.Password.IsEnabled = false;
				this.RetypePassword.IsEnabled = false;
				this.TrustServerCertificate.IsEnabled = false;
				this.CreateAccount.IsEnabled = false;

				if (this.Password.Password == this.passwordHash && !string.IsNullOrEmpty(this.passwordHash))
				{
					Credentials.Password = this.passwordHash;
					Credentials.PasswordType = this.passwordHashMethod;
				}
				else
					Credentials.Password = this.Password.Password;

				if (this.AllowInsecureAuthentication.IsChecked.HasValue && this.AllowInsecureAuthentication.IsChecked.Value)
				{
					Credentials.AllowPlain = true;
					Credentials.AllowCramMD5 = true;
					Credentials.AllowDigestMD5 = true;
				}

				if (this.TrustServerCertificate.IsChecked.HasValue && this.TrustServerCertificate.IsChecked.Value)
					Credentials.TrustServer = true;

				this.client = new XmppClient(Credentials, "en", typeof(App).Assembly)
				{
					AllowQuickLogin = true
				};

				if (Create)
				{
					this.client.AllowRegistration(this.ApiKey.Text, this.Secret.Password);
					this.client.OnRegistrationForm += this.Client_OnRegistrationForm;
				}

				this.client.OnStateChanged += this.Client_OnStateChanged;
				this.client.OnConnectionError += this.Client_OnConnectionError;
				await this.client.Connect();
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
			}
		}

		private async Task Client_OnRegistrationForm(object _, DataForm Form)
		{
			Field FormType = Form["FORM_TYPE"];
			if (!(FormType is null) && FormType.ValueString == "urn:xmpp:captcha")
			{
				MainWindow.UpdateGui(async () =>
				{
					ParameterDialog Dialog = await ParameterDialog.CreateAsync(Form);
					Dialog.ShowDialog();
				});
			}
			else
				await Form.Submit();
		}

		private Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			MainWindow.UpdateGui(this.XmppStateChanged, NewState);
			return Task.CompletedTask;
		}

		private async Task XmppStateChanged(object P)
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
					if (this.StorePassword.IsChecked.HasValue && this.StorePassword.IsChecked.Value)
					{
						this.passwordHash = this.Password.Password;
						this.passwordHashMethod = string.Empty;
					}
					else
					{
						this.passwordHash = this.client.PasswordHash;
						this.passwordHashMethod = this.client.PasswordHashMethod;
					}

					this.ConnectionState.Content = "Connected.";
					await this.CloseClient();
					this.DialogResult = true;
					break;

				case XmppState.Error:
					this.ConnectionState.Content = "Error.";
					await this.CloseClient();
					break;

			}
		}

		private async Task CloseClient()
		{
			if (!(this.client is null))
			{
				XmppClient Client = this.client;
				this.client = null;
				await Client.DisposeAsync();
			}

			this.ConnectButton.IsEnabled = true;
			this.XmppServer.IsEnabled = true;
			this.ConnectionMethod.IsEnabled = true;
			this.XmppPort.IsEnabled = true;
			this.UrlEndpoint.IsEnabled = true;
			this.AccountName.IsEnabled = true;
			this.Password.IsEnabled = true;
			this.TrustServerCertificate.IsEnabled = true;
			this.CreateAccount.IsEnabled = true;

			this.RetypePassword.IsEnabled = (this.CreateAccount.IsChecked.HasValue && this.CreateAccount.IsChecked.Value);
		}

		private Task Client_OnConnectionError(object _, Exception Exception)
		{
			MainWindow.UpdateGui(this.ShowError, Exception);
			return Task.CompletedTask;
		}

		private Task ShowError(object P)
		{
			Exception ex = (Exception)P;
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			return Task.CompletedTask;
		}

		public XmppClient Client => this.client;

		private void CreateAccount_Click(object Sender, RoutedEventArgs e)
		{
			bool Create = this.CreateAccount.IsChecked.HasValue && this.CreateAccount.IsChecked.Value;
			this.CreateParameters.Visibility = Create ? Visibility.Visible : Visibility.Collapsed;
			this.RetypePassword.IsEnabled = Create;
		}

		private void ConnectionMethod_SelectionChanged(object Sender, SelectionChangedEventArgs e)
		{
			if (this.PortLabel is null)
				return;

			switch ((TransportMethod)this.ConnectionMethod.SelectedIndex)
			{
				case TransportMethod.TraditionalSocket:
					this.PortLabel.Visibility = Visibility.Visible;
					this.XmppPort.Visibility = Visibility.Visible;
					this.UrlEndpointLabel.Visibility = Visibility.Collapsed;
					this.UrlEndpoint.Visibility = Visibility.Collapsed;
					break;

				case TransportMethod.BOSH:
				case TransportMethod.WS:
					this.PortLabel.Visibility = Visibility.Collapsed;
					this.XmppPort.Visibility = Visibility.Collapsed;
					this.UrlEndpointLabel.Visibility = Visibility.Visible;
					this.UrlEndpoint.Visibility = Visibility.Visible;
					break;
			}
		}
	}
}
