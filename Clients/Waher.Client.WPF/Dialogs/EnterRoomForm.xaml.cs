using System;
using System.Windows;
using System.Windows.Controls;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for EnterRoomForm.xaml
	/// </summary>
	public partial class EnterRoomForm : Window
	{
		public EnterRoomForm()
		{
			InitializeComponent();
		}

		public EnterRoomForm(string RoomId)
		{
			InitializeComponent();

			this.Title = "Enter room";
			this.RoomID.Text = RoomId;
			this.RoomID.IsReadOnly = true;
			this.AddButton.Content = "Enter";
			this.AddButton.ToolTip = "Enter the Room.";
			this.CancelButton.ToolTip = "Closes the dialog without entering the room.";

			if (string.IsNullOrEmpty(RoomId))
				this.RoomID.Focus();
			else
				this.NickName.Focus();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void FormChanged(object sender, TextChangedEventArgs e)
		{
			this.AddButton.IsEnabled = 
				!string.IsNullOrEmpty(this.RoomID.Text.Trim()) &&
				!string.IsNullOrEmpty(this.NickName.Text.Trim());
		}
	}
}
