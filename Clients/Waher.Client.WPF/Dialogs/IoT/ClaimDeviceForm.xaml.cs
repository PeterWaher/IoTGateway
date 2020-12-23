using System;
using System.Windows;
using System.Windows.Controls;
using Waher.Networking.XMPP.Provisioning;

namespace Waher.Client.WPF.Dialogs.IoT
{
	/// <summary>
	/// Interaction logic for ClaimDeviceForm.xaml
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
				this.tags = ThingRegistryClient.DecodeIoTDiscoClaimURI(this.ClaimUri.Text);

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
