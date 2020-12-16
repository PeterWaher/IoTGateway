using System;
using System.Windows;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for SendRoomInvitationForm.xaml
	/// </summary>
	public partial class SendRoomInvitationForm : Window
	{
		public SendRoomInvitationForm()
		{
			InitializeComponent();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void SendButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void BareJid_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			this.SendButton.IsEnabled = !string.IsNullOrEmpty(this.BareJid.Text) &&
				XmppClient.BareJidRegEx.IsMatch(this.BareJid.Text);
		}
	}
}
