using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Waher.Client.WPF.Dialogs.Xmpp
{
	/// <summary>
	/// Interaction logic for IqForm.xaml
	/// </summary>
	public partial class IqForm : Window
	{
		private bool xmlOk = true;
		private bool toOk = false;

		public IqForm()
		{
			InitializeComponent();
		}

		private void CustomXml_TextInput(object sender, TextCompositionEventArgs e)
		{
			try
			{
				string s = e.Text.Trim();

				if (!string.IsNullOrEmpty(s))
				{
					XmlDocument Doc = new XmlDocument();
					Doc.LoadXml(s);

					this.CustomXml.Background = null;
					this.xmlOk = true;
				}
			}
			catch (Exception)
			{
				this.CustomXml.Background = new SolidColorBrush(Colors.PeachPuff);
				this.xmlOk = false;
			}
			finally
			{
				this.SendButton.IsEnabled = this.xmlOk && this.toOk;
			}
		}

		private void To_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			string s = this.To.Text.Trim();

			this.toOk = !string.IsNullOrEmpty(s);
			this.To.Background = this.toOk ? null : new SolidColorBrush(Colors.PeachPuff);

			this.SendButton.IsEnabled = this.xmlOk && this.toOk;
		}

		private void SendButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.xmlOk && this.toOk)
				this.DialogResult = true;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
