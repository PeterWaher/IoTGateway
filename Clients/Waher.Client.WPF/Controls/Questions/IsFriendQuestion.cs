using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence.Attributes;

namespace Waher.Client.WPF.Controls.Questions
{
	public class IsFriendQuestion : Question
	{
		private ProvisioningClient client;
		private QuestionView questionView;
		private bool response;
		private RuleRange range;

		public IsFriendQuestion()
		{
		}

		public override string QuestionString => "Allowed to connect?";

		public override void PopulateDetailsDialog(QuestionView QuestionView, ProvisioningClient ProvisioningClient)
		{
			StackPanel Details = QuestionView.Details;
			TextBlock TextBlock;
			Button Button;

			this.client = ProvisioningClient;
			this.questionView = QuestionView;

			Details.Children.Add(new TextBlock()
			{
				FontSize = 18,
				FontWeight = FontWeights.Bold,
				Text = "Allowed to connect?"
			});

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6)
			});

			TextBlock.Inlines.Add("Device: ");
			this.AddJidName(this.JID, ProvisioningClient, TextBlock);

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6)
			});

			TextBlock.Inlines.Add("Caller: ");
			this.AddJidName(this.RemoteJID, ProvisioningClient, TextBlock);

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6),
				Text = "Is the caller allowed to connect to your device?"
			});

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "Yes"
			});

			Button.Click += YesButton_Click;

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "No"
			});

			Button.Click += NoButton_Click;

			string s = this.RemoteJID;
			int i = s.IndexOf('@');
			if (i >= 0)
			{
				s = s.Substring(i + 1);

				Details.Children.Add(Button = new Button()
				{
					Margin = new Thickness(0, 6, 0, 6),
					Content = "Yes, to anyone from " + s
				});

				Button.Click += YesDomainButton_Click;

				Details.Children.Add(Button = new Button()
				{
					Margin = new Thickness(0, 6, 0, 6),
					Content = "No, to no one from " + s
				});

				Button.Click += NoDomainButton_Click;
			}

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "Yes, to anyone"
			});

			Button.Click += YesAllButton_Click;

			Details.Children.Add(Button = new Button()
			{
				Margin = new Thickness(0, 6, 0, 6),
				Content = "No, to no one"
			});

			Button.Click += NoAllButton_Click;
		}

		internal static string GetDomain(string s)
		{
			int i = s.IndexOf('@');
			if (i < 0)
				return s;
			else
				return s.Substring(i + 1);
		}

		private void Process(bool Response, RuleRange Range)
		{
			this.response = Response;
			this.range = Range;
			this.client.IsFriendResponse(this.JID, this.RemoteJID, this.Key, Response, Range, this.RuleCallback, null);
		}

		private async void RuleCallback(object Sender, IqResultEventArgs e)
		{
			try
			{
				if (e.Ok)
					await this.Processed(this.questionView);
				else
				{
					MainWindow.currentInstance.Dispatcher.Invoke(() => MessageBox.Show(MainWindow.currentInstance,
						string.IsNullOrEmpty(e.ErrorText) ? "Unable to set rule." : e.ErrorText, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
				}
			}
			catch (Exception ex)
			{
				MainWindow.currentInstance.Dispatcher.Invoke(() => MessageBox.Show(MainWindow.currentInstance,
					ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
			}
		}

		private void NoAllButton_Click(object sender, RoutedEventArgs e)
		{
			this.Process(false, RuleRange.All);
		}

		private void YesAllButton_Click(object sender, RoutedEventArgs e)
		{
			this.Process(true, RuleRange.All);
		}

		private void NoDomainButton_Click(object sender, RoutedEventArgs e)
		{
			this.Process(false, RuleRange.Domain);
		}

		private void YesDomainButton_Click(object sender, RoutedEventArgs e)
		{
			this.Process(true, RuleRange.Domain);
		}

		private void NoButton_Click(object sender, RoutedEventArgs e)
		{
			this.Process(false, RuleRange.Caller);
		}

		private void YesButton_Click(object sender, RoutedEventArgs e)
		{
			this.Process(true, RuleRange.Caller);
		}

		public override bool IsResolvedBy(Question Question)
		{
			if (Question is IsFriendQuestion IsFriendQuestion)
			{
				if (this.JID != IsFriendQuestion.JID)
					return false;

				switch (this.range)
				{
					case RuleRange.Caller:
						return (this.RemoteJID == IsFriendQuestion.RemoteJID);

					case RuleRange.Domain:
						return (GetDomain(this.RemoteJID) == GetDomain(IsFriendQuestion.RemoteJID));

					case RuleRange.All:
						return true;

					default:
						return false;
				}
			}
			else
				return false;
		}

	}
}
