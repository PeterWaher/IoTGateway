using System;
using System.Windows;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for BanOccupantForm.xaml
	/// </summary>
	public partial class BanOccupantForm : Window
	{
		public BanOccupantForm(string NickName)
		{
			InitializeComponent();

			this.NickName.Text = NickName;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void YesButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
