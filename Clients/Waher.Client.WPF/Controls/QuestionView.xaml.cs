using System;
using System.Windows;
using System.Windows.Controls;
using Waher.Client.WPF.Model;
using Waher.Client.WPF.Controls.Questions;
using Waher.Networking.XMPP.Provisioning;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for QuestionView.xaml
	/// </summary>
	public partial class QuestionView : UserControl, ITabView
	{
		private readonly ProvisioningClient provisioningClient;
		private readonly XmppAccountNode owner;

		public QuestionView(XmppAccountNode Owner, ProvisioningClient ProvisioningClient)
		{
			this.owner = Owner;
			this.provisioningClient = ProvisioningClient;

			InitializeComponent();
		}

		public void Dispose()
		{
			foreach (Question Q in this.QuestionListView.Items)
				Q.Dispose();
		}

		public XmppAccountNode Owner=> this.owner;

		public string ProvisioningJid
		{
			get { return this.provisioningClient.ProvisioningServerAddress; }
		}

		public void NewQuestion(Question Question)
		{
			this.QuestionListView.Items.Add(Question);
		}

		public void NewButton_Click(object Sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void OpenButton_Click(object Sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void SaveAsButton_Click(object Sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void SaveButton_Click(object Sender, RoutedEventArgs e)
		{
			// TODO
		}

		private void QuestionListView_SelectionChanged(object Sender, SelectionChangedEventArgs e)
		{
			this.Details.Children.Clear();

			if (this.QuestionListView.SelectedItem is Question Question)
				Question.PopulateDetailsDialog(this, this.provisioningClient);
		}
	}
}
