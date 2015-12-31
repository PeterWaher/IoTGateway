using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		private const string WindowTitle = "Simple XMPP IoT Client";

		private Connections connections;
		private string fileName = string.Empty;

		public MainWindow()
		{
			InitializeComponent();

			this.connections = new Connections(this);
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

				Value = Registry.GetValue(registryKey, "FileName", string.Empty);
				if (Value != null && Value is string)
				{
					this.FileName = (string)Value;
					if (!string.IsNullOrEmpty(this.fileName))
						this.Load(this.fileName);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private bool CheckSaved()
		{
			if (this.connections.Modified)
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

			this.connections.New();
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
				string PasswordHash = Dialog.PasswordHash;
				string PasswordHashMethod = Dialog.PasswordHashMethod;
				bool TrustCertificate = Dialog.TrustServerCertificate.IsChecked.HasValue && Dialog.TrustServerCertificate.IsChecked.Value;

				XmppAccountNode Node = new XmppAccountNode(this.connections, null, Host, Port, Account, PasswordHash, PasswordHashMethod, TrustCertificate);
				this.connections.Add(Node);
				this.Add(Node);
			}
		}

		private void Add(TreeNode Node)
		{
			this.ConnectionTree.Items.Add(Node);
			this.NodeAdded(null, Node);
		}

		public void NodeAdded(TreeNode Parent, TreeNode ChildNode)
		{
			ChildNode.Updated += new EventHandler(Node_Updated);
		}

		private void Node_Updated(object sender, EventArgs e)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.RefreshTree), sender);
		}

		private void RefreshTree(object P)
		{
			TreeNode Node = (TreeNode)P;
			this.ConnectionTree.Items.Refresh();
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
				this.FileName = Dialog.FileName;
				this.SaveFile();
				return true;
			}
			else
				return false;
		}

		private void SaveFile()
		{
			if (string.IsNullOrEmpty(this.fileName))
				this.SaveNewFile();
			else
				this.connections.Save(this.fileName);
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			this.SaveFile();
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			if (!this.CheckSaved())
				return;

			try
			{
				OpenFileDialog Dialog = new OpenFileDialog();
				Dialog.AddExtension = true;
				Dialog.CheckFileExists = true;
				Dialog.CheckPathExists = true;
				Dialog.DefaultExt = "xml";
				Dialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
				Dialog.Multiselect = false;
				Dialog.ShowReadOnly = true;
				Dialog.Title = "Open connection file";

				bool? Result = Dialog.ShowDialog(this);

				if (Result.HasValue && Result.Value)
					this.Load(Dialog.FileName);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Load(string FileName)
		{
			this.connections.Load(FileName);
			this.FileName = FileName;

			foreach (TreeNode Node in this.connections.RootNodes)
				this.Add(Node);
		}

		private void NewButton_Click(object sender, RoutedEventArgs e)
		{
			if (!this.CheckSaved())
				return;

			ConnectionTree.Items.Clear();
			this.connections.New();
			this.FileName = string.Empty;
		}

		public string FileName
		{
			get { return this.fileName; }
			set
			{
				this.fileName = value;
				if (string.IsNullOrEmpty(this.fileName))
					this.Title = WindowTitle;
				else
					this.Title = this.fileName + " - " + WindowTitle;
			}
		}

		private void ConnectionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeNode Node = this.ConnectionTree.SelectedItem as TreeNode;

			if (Node == null)
			{
				this.AddButton.IsEnabled = false;
				this.DeleteButton.IsEnabled = false;
				this.RefreshButton.IsEnabled = false;
			}
			else
			{
				this.AddButton.IsEnabled = Node.CanAddChildren;
				this.DeleteButton.IsEnabled = true;
				this.RefreshButton.IsEnabled = Node.CanRecycle;
			}
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			TreeNode Node = this.ConnectionTree.SelectedItem as TreeNode;
			if (Node == null || !Node.CanAddChildren)
				return;

			Node.Add();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			TreeNode Node = this.ConnectionTree.SelectedItem as TreeNode;
			if (Node == null || !Node.CanRecycle)
				return;

			Node.Recycle();
		}

		private void DeleteButton_Click(object sender, RoutedEventArgs e)
		{
			TreeNode Node = this.ConnectionTree.SelectedItem as TreeNode;
			if (Node == null)
				return;

			if (Node.Parent == null)
				this.connections.Delete(Node);
			else
				Node.Parent.Delete(Node);

			this.ConnectionTree.Items.Refresh();
		}

	}
}
