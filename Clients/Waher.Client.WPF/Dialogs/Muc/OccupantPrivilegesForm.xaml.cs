using System;
using System.Windows;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for OccupantPrivilegesForm.xaml
	/// </summary>
	public partial class OccupantPrivilegesForm : Window
	{
		public OccupantPrivilegesForm()
		{
			InitializeComponent();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

	}
}
