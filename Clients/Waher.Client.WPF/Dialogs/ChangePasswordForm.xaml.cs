using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Client.WPF.Model;

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
			get { return this.passwordHash; }
			set { this.passwordHash = value; }
		}

		/// <summary>
		/// Password hash method of a successfully authenticated client.
		/// </summary>
		public string PasswordHashMethod
		{
			get { return this.passwordHashMethod; }
			set { this.passwordHashMethod = value; }
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
