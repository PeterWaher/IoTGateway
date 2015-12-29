using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string fileName = string.Empty;

		public MainWindow()
		{
			InitializeComponent();
		}

		private static readonly string registryKey = Registry.CurrentUser + @"\Software\Waher Data AB\Waher.Client.WPF";

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			object Value;

			try
			{
				Value = Registry.GetValue(registryKey, "WindowLeft", (int)this.Left);
				if (Value != null && Value is int)
					this.Left = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowTop", (int)this.Top);
				if (Value != null && Value is int)
					this.Top = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowWidth", (int)this.Width);
				if (Value != null && Value is int)
					this.Width = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowHeight", (int)this.Height);
				if (Value != null && Value is int)
					this.Height = (int)Value;

				Value = Registry.GetValue(registryKey, "ConnectionTreeWidth", (int)this.ConnectionTree.Width);
				if (Value != null && Value is int)
					this.ConnectionsGrid.ColumnDefinitions[0].Width = new GridLength((int)Value);

				Value = Registry.GetValue(registryKey, "WindowState", this.WindowState.ToString());
				if (Value != null && Value is string)
					this.WindowState = (WindowState)Enum.Parse(typeof(WindowState), (string)Value);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private bool CheckSaved()
		{
			if (Connections.Modified)
			{
				switch (MessageBox.Show(this, "You have unsaved changes. Do you want to save these changes before closing the application?",
					"Save unsaved changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
				{
					case MessageBoxResult.Yes:
						if (this.SaveNewFile())
							break;
						else
							return false;

					case MessageBoxResult.No:
						break;

					case MessageBoxResult.Cancel:
						return false;
				}
			}

			return true;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.CheckSaved())
			{
				e.Cancel = true;
				return;
			}

			Registry.SetValue(registryKey, "WindowLeft", (int)this.Left, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowTop", (int)this.Top, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowWidth", (int)this.Width, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowHeight", (int)this.Height, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "ConnectionTreeWidth", (int)this.ConnectionsGrid.ColumnDefinitions[0].Width.Value, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowState", this.WindowState.ToString(), RegistryValueKind.String);
			Registry.SetValue(registryKey, "FileName", this.fileName, RegistryValueKind.String);
		}

		private void ConnectToButton_Click(object sender, RoutedEventArgs e)
		{
			ConnectToForm Dialog = new ConnectToForm();
			Dialog.Owner = this;
			bool? Result = Dialog.ShowDialog();

			if (Result.HasValue && Result.Value)
			{
				string Host = Dialog.XmppServer.Text;
				int Port = int.Parse(Dialog.XmppPort.Text);
				string Account = Dialog.AccountName.Text;
				string Password = Dialog.Password.Password;
				bool TrustCertificate = Dialog.TrustServerCertificate.IsChecked.HasValue && Dialog.TrustServerCertificate.IsChecked.Value;

				XmppAccountNode Node = new XmppAccountNode(null, Host, Port, Account, Password, TrustCertificate);
				TreeViewItem Item = new TreeViewItem();
				Item.Tag = Node;
				Node.Tag = Item;
				Item.Header = Node.Header;

				Connections.Add(Node);
				this.ConnectionTree.Items.Add(Item);
			}
		}

		private bool SaveNewFile()
		{
			SaveFileDialog Dialog = new SaveFileDialog();
			Dialog.AddExtension = true;
			Dialog.CheckPathExists = true;
			Dialog.CreatePrompt = false;
			Dialog.DefaultExt = "xml";
			Dialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
			Dialog.Title = "Save connection file";

			bool? Result = Dialog.ShowDialog(this);

			if (Result.HasValue && Result.Value)
			{
				this.fileName = Dialog.FileName;
				this.SaveFile();
				return true;
			}
			else
				return false;
		}

		private void SaveFile()
		{ 
			// TODO
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(this.fileName))
				this.SaveNewFile();
			else
				this.SaveFile();
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			if (!this.CheckSaved())
				return;

			try
			{
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void NewButton_Click(object sender, RoutedEventArgs e)
		{
			if (!this.CheckSaved())
				return;

			// TODO
		}

	}
}
