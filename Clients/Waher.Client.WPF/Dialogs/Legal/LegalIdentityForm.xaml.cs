using System;
using System.Windows;

namespace Waher.Client.WPF.Dialogs.Legal
{
	/// <summary>
	/// Interaction logic for LegalIdentityForm.xaml
	/// </summary>
	public partial class LegalIdentityForm : Window
	{
		public LegalIdentityForm()
		{
			InitializeComponent();
		}

		private void CancelButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void RegisterButton_Click(object Sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
