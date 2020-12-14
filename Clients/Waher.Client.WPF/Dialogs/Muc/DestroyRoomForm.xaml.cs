using System;
using System.Windows;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for DestroyRoomForm.xaml
	/// </summary>
	public partial class DestroyRoomForm : Window
	{
		public DestroyRoomForm(string RoomName)
		{
			InitializeComponent();

			this.RoomName.Text = RoomName;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void YesButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void AlternativeRoomJid_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			this.YesButton.IsEnabled = string.IsNullOrEmpty(this.AlternativeRoomJid.Text) ||
				XmppClient.BareJidRegEx.IsMatch(this.AlternativeRoomJid.Text);
		}
	}
}
