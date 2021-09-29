using System;
using System.Windows;

namespace Waher.Client.WPF.Dialogs.Xmpp
{
	/// <summary>
	/// Interaction logic for IqResultForm.xaml
	/// </summary>
	public partial class IqResultForm : Window
	{
		public IqResultForm()
		{
			InitializeComponent();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
