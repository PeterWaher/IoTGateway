using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Provisioning;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Things.ControlParameters;

namespace Waher.Client.WPF.Controls.Questions
{
	public class CanControlQuestion : NodeQuestion
	{
		private SortedDictionary<string, bool> parametersSorted = null;
		private ListBox parametersListBox = null;
		private ProvisioningClient client;
		private QuestionView questionView;
		private OperationRange range;
		private string parameter = null;
		private string[] parameterNames = null;
		private string[] availableParameterNames = null;
		private bool registered = false;

		public CanControlQuestion()
			: base()
		{
		}

		[DefaultValueNull]
		public string[] ParameterNames
		{
			get { return this.parameterNames; }
			set { this.parameterNames = value; }
		}

		[DefaultValueNull]
		public string[] AvailableParameterNames
		{
			get { return this.availableParameterNames; }
			set { this.availableParameterNames = value; }
		}

		public override string QuestionString => "Allowed to control?";

		public override void PopulateDetailsDialog(QuestionView QuestionView, ProvisioningClient ProvisioningClient)
		{
			StackPanel Details = QuestionView.Details;
			TextBlock TextBlock;
			ListBox ListBox;
			Button Button;

			this.client = ProvisioningClient;
			this.questionView = QuestionView;

			Details.Children.Add(new TextBlock()
			{
				FontSize = 18,
				FontWeight = FontWeights.Bold,
				Text = "Allowed to control?"
			});

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6)
			});

			TextBlock.Inlines.Add("Device: ");
			this.AddJidName(this.JID, ProvisioningClient, TextBlock);

			this.AddNodeInfo(Details);

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6)
			});

			TextBlock.Inlines.Add("Caller: ");
			this.AddJidName(this.RemoteJID, ProvisioningClient, TextBlock);


			Details.Children.Add(new Label()
			{
				Content = "Parameter restriction:",
				Margin = new Thickness(0, 6, 0, 0)
			});

			Details.Children.Add(ListBox = new ListBox()
			{
				MaxHeight = 150,
				SelectionMode = SelectionMode.Multiple,
				Margin = new Thickness(0, 0, 0, 6)
			});

			this.parametersListBox = ListBox;

			if (this.availableParameterNames == null)
			{
				if (this.parameterNames != null)
				{
					foreach (string ParameterName in this.parameterNames)
					{
						ListBox.Items.Add(new ListBoxItem()
						{
							Content = ParameterName,
							IsSelected = true,
							Tag = ParameterName
						});
					}
				}
			}
			else
			{
				foreach (string ParameterName in this.availableParameterNames)
				{
					ListBox.Items.Add(new ListBoxItem()
					{
						Content = ParameterName,
						IsSelected = (this.parameterNames == null || Array.IndexOf<string>(this.parameterNames, ParameterName) >= 0),
						Tag = ParameterName
					});
				}
			}

			StackPanel StackPanel = CanReadQuestion.AddAllClearButtons(Details, ListBox);

			if (this.availableParameterNames == null)
			{
				StackPanel.Children.Add(Button = new Button()
				{
					Margin = new Thickness(6, 6, 6, 6),
					Padding = new Thickness(20, 0, 20, 0),
					Content = "Get List",
					Tag = ListBox
				});

				Button.Click += GetListButton_Click;
			}

			Details.Children.Add(TextBlock = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 6, 0, 6),
				Text = "Is the caller allowed to control your device?"
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

			this.AddTokens(Details, this.client, this.YesTokenButton_Click, this.NoTokenButton_Click);
		}

		private void GetListButton_Click(object sender, RoutedEventArgs e)
		{
			XmppClient Client = this.client.Client;

			((Button)sender).IsEnabled = false;

			RosterItem Item = Client[this.JID];
			if (Item == null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
			{
				if (!this.registered)
				{
					this.registered = true;
					this.questionView.Owner.RegisterRosterEventHandler(this.JID, this.RosterItemUpdated);
				}

				Client.RequestPresenceSubscription(this.JID);
			}
			else if (!Item.HasLastPresence || !Item.LastPresence.IsOnline)
				Client.RequestPresenceSubscription(this.JID);
			else
				this.DoRequest(Item);
		}

		public override void Dispose()
		{
			if (this.registered)
			{
				this.registered = false;
				this.questionView.Owner.UnregisterRosterEventHandler(this.JID, this.RosterItemUpdated);
			}

			base.Dispose();
		}

		private void RosterItemUpdated(object Sender, RosterItem Item)
		{
			if ((Item.State == SubscriptionState.Both || Item.State == SubscriptionState.To) && Item.HasLastPresence && Item.LastPresence.IsOnline)
			{
				this.questionView.Owner.UnregisterRosterEventHandler(this.JID, this.RosterItemUpdated);
				this.DoRequest(Item);
			}
		}

		private void DoRequest(RosterItem Item)
		{
			this.parametersSorted = new SortedDictionary<string, bool>();

			if (this.IsNode)
			{
				this.questionView.Owner.ControlClient.GetForm(Item.LastPresenceFullJid, "en", this.ControlFormResponse, null,
					this.GetNodeReference());
			}
			else
				this.questionView.Owner.ControlClient.GetForm(Item.LastPresenceFullJid, "en", this.ControlFormResponse, null);
		}

		private async void ControlFormResponse(object Sender, DataFormEventArgs e)
		{
			try
			{
				if (e.Ok)
				{
					string[] Parameters;

					lock (this.parametersSorted)
					{
						foreach (Field F in e.Form.Fields)
						{
							if (!F.ReadOnly && !F.Exclude)
								this.parametersSorted[F.Var] = true;
						}

						Parameters = new string[this.parametersSorted.Count];
						this.parametersSorted.Keys.CopyTo(Parameters, 0);
					}

					this.availableParameterNames = Parameters;
					await Database.Update(this);

					MainWindow.currentInstance.Dispatcher.Invoke(() =>
					{
						SortedDictionary<string, bool> Selected = null;
						bool AllSelected = this.parameterNames == null;

						if (!AllSelected)
						{
							Selected = new SortedDictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);

							foreach (ListBoxItem Item in this.parametersListBox.Items)
							{
								if (Item.IsSelected)
									Selected[(string)Item.Tag] = true;
							}
						}

						this.parametersListBox.Items.Clear();

						foreach (string ParameterName in this.availableParameterNames)
						{
							this.parametersListBox.Items.Add(new ListBoxItem()
							{
								Content = ParameterName,
								IsSelected = AllSelected || Selected.ContainsKey(ParameterName),
								Tag = ParameterName
							});
						}
					});
				}
				else
				{
					MainWindow.currentInstance.Dispatcher.Invoke(() =>
					{
						MessageBox.Show(MainWindow.currentInstance, string.IsNullOrEmpty(e.ErrorText) ? "Unable to get control form." : e.ErrorText,
							"Error", MessageBoxButton.OK, MessageBoxImage.Error);
					});
				}
			}
			catch (Exception ex)
			{
				MainWindow.currentInstance.Dispatcher.Invoke(() =>
					MessageBox.Show(MainWindow.currentInstance, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
			}
		}

		private string[] GetParameters()
		{
			List<string> Result = new List<string>();
			bool All = true;

			foreach (ListBoxItem Item in this.parametersListBox.Items)
			{
				if (Item.IsSelected)
					Result.Add((string)Item.Tag);
				else
					All = false;
			}

			if (this.availableParameterNames == null || !All)
				return Result.ToArray();
			else
				return null;
		}

		private void NoAllButton_Click(object sender, RoutedEventArgs e)
		{
			this.range = OperationRange.All;
			this.client.CanControlResponseAll(this.JID, this.RemoteJID, this.Key, false, this.GetParameters(), null, null);
			this.Process();
		}

		private void YesAllButton_Click(object sender, RoutedEventArgs e)
		{
			this.range = OperationRange.All;
			this.client.CanControlResponseAll(this.JID, this.RemoteJID, this.Key, true, this.GetParameters(), null, null);
			this.Process();
		}

		private void NoDomainButton_Click(object sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Domain;
			this.client.CanControlResponseDomain(this.JID, this.RemoteJID, this.Key, false, this.GetParameters(), null, null);
			this.Process();
		}

		private void YesDomainButton_Click(object sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Domain;
			this.client.CanControlResponseDomain(this.JID, this.RemoteJID, this.Key, true, this.GetParameters(), null, null);
			this.Process();
		}

		private void NoButton_Click(object sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Caller;
			this.client.CanControlResponseCaller(this.JID, this.RemoteJID, this.Key, false, this.GetParameters(), null, null);
			this.Process();
		}

		private void YesButton_Click(object sender, RoutedEventArgs e)
		{
			this.range = OperationRange.Caller;
			this.client.CanControlResponseCaller(this.JID, this.RemoteJID, this.Key, true, this.GetParameters(), null, null);
			this.Process();
		}

		private void NoTokenButton_Click(object sender, RoutedEventArgs e)
		{
			this.TokenButtonClick(sender, e, false);
		}

		private void TokenButtonClick(object sender, RoutedEventArgs e, bool Result)
		{
			Button Button = (Button)sender;
			object[] P = (object[])Button.Tag;
			this.parameter = (string)P[0];
			this.range = (OperationRange)P[1];

			switch (this.range)
			{
				case OperationRange.ServiceToken:
					this.client.CanControlResponseService(this.JID, this.RemoteJID, this.Key, Result, this.GetParameters(), this.parameter, null, null);
					break;

				case OperationRange.DeviceToken:
					this.client.CanControlResponseDevice(this.JID, this.RemoteJID, this.Key, Result, this.GetParameters(), this.parameter, null, null);
					break;

				case OperationRange.UserToken:
					this.client.CanControlResponseUser(this.JID, this.RemoteJID, this.Key, Result, this.GetParameters(), this.parameter, null, null);
					break;
			}

			this.Process();
		}

		private void YesTokenButton_Click(object sender, RoutedEventArgs e)
		{
			this.TokenButtonClick(sender, e, true);
		}

		private Task Process()
		{
			return this.Processed(this.questionView);
		}

		public override bool IsResolvedBy(Question Question)
		{
			if (Question is CanControlQuestion CanControlQuestion)
			{
				if (this.JID != CanControlQuestion.JID)
					return false;

				switch (this.range)
				{
					case OperationRange.Caller:
						return (this.RemoteJID == CanControlQuestion.RemoteJID);

					case OperationRange.Domain:
						return (IsFriendQuestion.GetDomain(this.RemoteJID) == IsFriendQuestion.GetDomain(CanControlQuestion.RemoteJID));

					case OperationRange.All:
						return true;

					case OperationRange.ServiceToken:
						return CanReadQuestion.MatchesToken(this.parameter, CanControlQuestion.ServiceTokens);

					case OperationRange.DeviceToken:
						return CanReadQuestion.MatchesToken(this.parameter, CanControlQuestion.DeviceTokens);

					case OperationRange.UserToken:
						return CanReadQuestion.MatchesToken(this.parameter, CanControlQuestion.UserTokens);

					default:
						return false;
				}
			}
			else
				return false;
		}

	}
}
