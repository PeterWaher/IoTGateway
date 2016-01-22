using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
