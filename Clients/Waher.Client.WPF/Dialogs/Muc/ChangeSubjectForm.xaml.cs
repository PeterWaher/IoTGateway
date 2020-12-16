using System;
using System.Windows;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Dialogs.Muc
{
	/// <summary>
	/// Interaction logic for ChangeSubjectForm.xaml
	/// </summary>
	public partial class ChangeSubjectForm : Window
	{
		public ChangeSubjectForm()
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

		private void Subject_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			this.ApplyButton.IsEnabled = !string.IsNullOrEmpty(this.Subject.Text.Trim());
		}
	}
}
