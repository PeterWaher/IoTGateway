using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Waher.Networking.XMPP.Provisioning;

namespace Waher.Client.WPF.Dialogs
{
	/// <summary>
	/// Interaction logic for AddContactForm.xaml
	/// </summary>
	public partial class ClaimDeviceForm : Window
	{
		private MetaDataTag[] tags = null;

		public ClaimDeviceForm()
		{
			InitializeComponent();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void ClaimButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void ClaimUri_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				this.tags = ThingRegistryClient.DecodeIoTDiscoURI(this.ClaimUri.Text);

				this.TagsListView.Items.Clear();

				foreach (MetaDataTag Tag in this.tags)
					this.TagsListView.Items.Add(Tag);

				this.ClaimButton.IsEnabled = true;
			}
			catch (Exception)
			{
				this.ClaimButton.IsEnabled = false;
			}
		}

		public MetaDataTag[] Tags => this.tags;
		public bool MakePublic => this.Public.IsChecked.HasValue && this.Public.IsChecked.Value;
	}
}
