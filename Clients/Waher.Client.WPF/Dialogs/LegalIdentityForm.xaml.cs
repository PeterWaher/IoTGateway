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
	/// Interaction logic for LegalIdentityForm.xaml
	/// </summary>
	public partial class LegalIdentityForm : Window
	{
		public LegalIdentityForm()
		{
			InitializeComponent();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void RegisterButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
