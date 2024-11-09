using System.Windows;
using System.Windows.Controls;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs.PubSub
{
	/// <summary>
	/// Interaction logic for AddAffiliateForm.xaml
	/// </summary>
	public partial class AddAffiliateForm : Window
	{
		public AddAffiliateForm()
		{
			this.InitializeComponent();
		}

		private void ComboBox_SelectionChanged(object Sender, SelectionChangedEventArgs e)
		{
			this.FormChanged();
		}

		private void Jid_TextChanged(object Sender, TextChangedEventArgs e)
		{
			this.FormChanged();
		}

		private void FormChanged()
		{
			this.AddButton.IsEnabled = XmppClient.BareJidRegEx.IsMatch(this.Jid.Text) && this.Affiliation.SelectedIndex >= 0;
		}

		public void AddButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		public void CancelButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
