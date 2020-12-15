using System;
using System.Windows;
using System.Windows.Controls;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for EnterRoomForm.xaml
	/// </summary>
	public partial class EnterRoomForm : Window
	{
		public EnterRoomForm(string Domain)
		{
			InitializeComponent();

			this.Domain.Text = Domain;
		}

		public EnterRoomForm(string RoomId, string Domain)
		{
			InitializeComponent();

			this.Title = "Enter room";
			this.RoomID.Text = RoomId;
			this.RoomID.IsReadOnly = true;
			this.Domain.Text = Domain;
			this.Domain.IsReadOnly = true;
			this.AddButton.Content = "Enter";
			this.AddButton.ToolTip = "Enter the Room.";
			this.CancelButton.ToolTip = "Closes the dialog without entering the room.";

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
				!string.IsNullOrEmpty(this.Domain.Text.Trim()) &&
				!string.IsNullOrEmpty(this.NickName.Text.Trim());
		}
	}
}
