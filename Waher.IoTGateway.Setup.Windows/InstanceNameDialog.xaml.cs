using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Waher.IoTGateway.Setup.Windows
{
	/// <summary>
	/// Interaction logic for InstanceNameDialog.xaml
	/// </summary>
	public partial class InstanceNameDialog : Window
	{
		/// <summary>
		/// Interaction logic for InstanceNameDialog.xaml
		/// </summary>
		/// <param name="Port">Port number.</param>
		public InstanceNameDialog(int Port)
		{
			this.InitializeComponent();

			this.Instance = string.Empty;
			this.Port = Port;
			this.PortNumber.Text = Port.ToString();
		}

		public string Instance { get; set; }
		public int Port { get; set; }

		private async void CheckInput(object Sender, TextChangedEventArgs e)
		{
			try
			{
				await this.CheckInput();
			}
			catch (Exception ex)
			{
				MainWindow.ShowError(ex);
			}
		}

		private async Task<bool> CheckInput()
		{
			string s = this.InstanceName.Text;

			if (string.IsNullOrEmpty(s))
			{
				this.ShowMessage("Instance name cannot be empty.");
				return false;
			}

			if (s != s.Trim())
			{
				this.ShowMessage("Instance name cannot contain white-space.");
				return false;
			}

			foreach (char ch in s)
			{
				if ((ch >= 'a' && ch <= 'z') ||
					(ch >= 'A' && ch <= 'Z') ||
					(ch >= '0' && ch <= '9'))
				{
					continue;
				}

				this.ShowMessage("Invalid character in instance name: " + ch);
				return false;
			}

			string Programs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), App.AppName + s);

			if (Directory.Exists(Programs))
			{
				this.ShowMessage("Instance name conflicts with an existing installation.");
				return false;
			}

			if (!int.TryParse(this.PortNumber.Text, out int Port) ||
				Port <= 0 || Port > ushort.MaxValue)
			{
				this.ShowMessage("The port number must be an integer between 1 and 65535. 80 is the default. Other common numbers include 8080, 8081, 8082, etc.");
				return false;
			}

			bool? b = await MainWindow.CheckPortNumber(Port);
			if (!b.HasValue)
			{
				this.ShowMessage("Unable to open Port number. Is a network available?");
				return false;
			}
			else if (!b.Value)
			{
				this.ShowMessage("Port number occupied by another service. Try another.");
				return false;
			}

			this.ShowMessage(string.Empty);
			this.Instance = s;
			this.Port = Port;

			return true;
		}

		private void ShowMessage(string s)
		{
			this.Message.Text = s;
			this.OkButton.IsEnabled = string.IsNullOrEmpty(s);
		}

		private async void Window_Initialized(object Sender, System.EventArgs e)
		{
			try
			{
				await this.CheckInput();
			}
			catch (Exception ex)
			{
				MainWindow.ShowError(ex);
			}
		}

		private void OkButton_Click(object Sender, RoutedEventArgs e)
		{
			if (MessageBox.Show(this, "Please press OK to confirm you want to install a new instance named \"" + this.Instance + "\" of " + 
				App.AppNameDisplayable + " on your computer, using port number " + this.Port.ToString() + ".",
				"Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
			{
				this.DialogResult = true;
			}
			else
				e.Handled = true;
		}
	}
}
