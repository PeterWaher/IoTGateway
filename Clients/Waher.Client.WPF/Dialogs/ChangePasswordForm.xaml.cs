using System;
using System.Windows;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for ChangePasswordForm.xaml
	/// </summary>
	public partial class ChangePasswordForm : Window
	{
		private string passwordHash = string.Empty;
		private string passwordHashMethod = string.Empty;

		public ChangePasswordForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Password hash of a successfully authenticated client.
		/// </summary>
		public string PasswordHash
		{
			get => this.passwordHash;
			set => this.passwordHash = value;
		}

		/// <summary>
		/// Password hash method of a successfully authenticated client.
		/// </summary>
		public string PasswordHashMethod
		{
			get => this.passwordHashMethod;
			set => this.passwordHashMethod = value;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void ChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.Password.Password != this.RetypePassword.Password)
			{
				MessageBox.Show(this, "The two passwords must match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Password.Focus();
			}
			else
				this.DialogResult = true;
		}
	}
}
