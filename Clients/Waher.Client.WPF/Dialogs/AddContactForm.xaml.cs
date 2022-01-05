using System;
using System.Windows;
using System.Windows.Controls;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for AddContactForm.xaml
	/// </summary>
	public partial class AddContactForm : Window
	{
		public AddContactForm()
		{
			InitializeComponent();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void ContactJID_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.AddButton.IsEnabled = XmppClient.BareJidRegEx.IsMatch(this.ContactJID.Text);
		}

	}
}
