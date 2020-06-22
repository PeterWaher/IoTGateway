using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Content.Markdown;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Client.WPF.Dialogs;

namespace Waher.Client.WPF.Model.Legal
{
	public class LegalService : XmppComponent
	{
		private ContractsClient contractsClient;

		private LegalService(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent, JID, Name, Node, Features)
		{
		}

		public static async Task<LegalService> Create(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
		{
			LegalService Result = new LegalService(Parent, JID, Name, Node, Features);

			Result.contractsClient = await ContractsClient.Create(Result.Account.Client, JID);
			Result.contractsClient.IdentityUpdated += Result.ContractsClient_IdentityUpdated;

			return Result;
		}

		public ContractsClient ContractsClient
		{
			get { return this.contractsClient; }
		}

		public override void Dispose()
		{
			this.contractsClient?.Dispose();
			this.contractsClient = null;

			base.Dispose();
		}

		public override ImageSource ImageResource => XmppAccountNode.database;

		public override string ToolTip
		{
			get
			{
				return "Legal Services";
			}
		}

		public override void AddContexMenuItems(ref string CurrentGroup, ContextMenu Menu)
		{
			base.AddContexMenuItems(ref CurrentGroup, Menu);

			MenuItem Item;

			this.GroupSeparator(ref CurrentGroup, "Database", Menu);

			Menu.Items.Add(Item = new MenuItem()
			{
				Header = "_Register Legal Identity...",
				IsEnabled = true,
				Icon = new Image()
				{
					Source = new BitmapImage(new Uri("../Graphics/Places-user-identity-icon_16.png", UriKind.Relative)),
					Width = 16,
					Height = 16
				}
			});

			Item.Click += this.RegisterLegalIdentity_Click;

			Menu.Items.Add(Item = new MenuItem()
			{
				Header = "M_y legal identities...",
				IsEnabled = true
			});

			Item.Click += this.MyLegalIdentities_Click;

			Menu.Items.Add(Item = new MenuItem()
			{
				Header = "_Obsolete legal identities...",
				IsEnabled = true
			});

			Item.Click += this.ObsoleteLegalIdentities_Click;

			Menu.Items.Add(Item = new MenuItem()
			{
				Header = "Report legal identities as _compromized...",
				IsEnabled = true
			});

			Item.Click += this.CompromizedLegalIdentities_Click;
		}

		private void RegisterLegalIdentity_Click(object sender, RoutedEventArgs e)
		{
			LegalIdentityForm Form = new LegalIdentityForm
			{
				Owner = MainWindow.currentInstance
			};
			bool? Result = Form.ShowDialog();
			string s;

			if (Result.HasValue && Result.Value)
			{
				List<Property> Properties = new List<Property>();

				if (!string.IsNullOrEmpty(s = Form.FirstName.Text))
					Properties.Add(new Property("FIRST", s));

				if (!string.IsNullOrEmpty(s = Form.MiddleNames.Text))
					Properties.Add(new Property("MIDDLE", s));

				if (!string.IsNullOrEmpty(s = Form.LastName.Text))
					Properties.Add(new Property("LAST", s));

				if (!string.IsNullOrEmpty(s = Form.PersonalNumber.Text))
					Properties.Add(new Property("PNR", s));

				if (!string.IsNullOrEmpty(s = Form.Address.Text))
					Properties.Add(new Property("ADDR", s));

				if (!string.IsNullOrEmpty(s = Form.Address2.Text))
					Properties.Add(new Property("ADDR2", s));

				if (!string.IsNullOrEmpty(s = Form.PostalCode.Text))
					Properties.Add(new Property("ZIP", s));

				if (!string.IsNullOrEmpty(s = Form.Area.Text))
					Properties.Add(new Property("AREA", s));

				if (!string.IsNullOrEmpty(s = Form.City.Text))
					Properties.Add(new Property("CITY", s));

				if (!string.IsNullOrEmpty(s = Form.Region.Text))
					Properties.Add(new Property("REGION", s));

				if (!string.IsNullOrEmpty(s = Form.Country.Text))
					Properties.Add(new Property("COUNTRY", s));

				this.contractsClient.Apply(Properties.ToArray(), (sender2, e2) =>
				{
					if (!e2.Ok)
						MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to register legal identity." : e2.ErrorText);

					return Task.CompletedTask;

				}, null);
			}
		}

		private Task ContractsClient_IdentityUpdated(object Sender, LegalIdentityEventArgs e)
		{
			StringBuilder Markdown = new StringBuilder();

			Markdown.AppendLine("Legal identity updated:");
			Markdown.AppendLine();
			Output(XmppClient.GetBareJID(e.To), Markdown, e.Identity.GetTags());

			MainWindow.UpdateGui(() =>
			{
				MainWindow.currentInstance.ChatMessage(XmppClient.GetBareJID(e.From), XmppClient.GetBareJID(e.To),
					Markdown.ToString(), true);
			});

			return Task.CompletedTask;
		}

		internal static void Output(string JID, StringBuilder Markdown, KeyValuePair<string, object>[] Tags)
		{
			Markdown.AppendLine("| Legal Identity ||");
			Markdown.AppendLine("|:------|:--------|");
			Markdown.Append("| JID   | ");
			Markdown.Append(MarkdownDocument.Encode(JID));
			Markdown.AppendLine(" |");

			foreach (KeyValuePair<string, object> P in Tags)
			{
				string s = P.Key;

				switch (s)
				{
					case "FIRST": s = "First Name"; break;
					case "MIDDLE": s = "Middle Name(s)"; break;
					case "LAST": s = "Last Name"; break;
					case "PNR": s = "Personal Number"; break;
					case "ADDR": s = "Address"; break;
					case "ADDR2": s = "Address, row 2"; break;
					case "ZIP": s = "Postal Code (ZIP)"; break;
					case "AREA": s = "Area"; break;
					case "CITY": s = "City"; break;
					case "REGION": s = "Region (State)"; break;
					case "COUNTRY": s = "Country"; break;
				}

				Markdown.Append("| ");
				Markdown.Append(MarkdownDocument.Encode(s).Replace("\r\n", "\n").Replace("\n", "<br/>").Replace("\r", "<br/>"));
				Markdown.Append(" | ");
				Markdown.Append(MarkdownDocument.Encode(P.Value.ToString()).Replace("\r\n", "\n").Replace("\n", "<br/>").Replace("\r", "<br/>"));
				Markdown.AppendLine(" |");
			}
		}

		private void MyLegalIdentities_Click(object sender, RoutedEventArgs e)
		{
			this.contractsClient.GetLegalIdentities((sender2, e2) =>
			{
				if (e2.Ok)
				{
					if (e2.Identities is null || e2.Identities.Length == 0)
						MainWindow.MessageBox("No legal identities are regitered.", "Identities", MessageBoxButton.OK, MessageBoxImage.Information);
					else
					{
						foreach (LegalIdentity Identity in e2.Identities)
						{
							StringBuilder Markdown = new StringBuilder();

							Output(XmppClient.GetBareJID(e2.To), Markdown, Identity.GetTags());

							MainWindow.UpdateGui(() =>
							{
								MainWindow.currentInstance.ChatMessage(XmppClient.GetBareJID(e2.From), XmppClient.GetBareJID(e2.To),
									Markdown.ToString(), true);
							});
						}
					}
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to get list of identities." : e2.ErrorText);

				return Task.CompletedTask;

			}, null);
		}

		private void ObsoleteLegalIdentities_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show(MainWindow.currentInstance, "Are you sure you want to obsolete registered legal identities?",
				"Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes) != MessageBoxResult.Yes)
			{
				return;
			}

			this.contractsClient.GetLegalIdentities((sender2, e2) =>
			{
				if (e2.Ok)
				{
					if (e2.Identities is null || e2.Identities.Length == 0)
						MainWindow.MessageBox("No legal identities are regitered.", "Identities", MessageBoxButton.OK, MessageBoxImage.Information);
					else
					{
						int Nr = 0;

						foreach (LegalIdentity Identity in e2.Identities)
						{
							if (Identity.State == IdentityState.Approved || Identity.State == IdentityState.Created)
							{
								this.contractsClient.ObsoleteLegalIdentity(Identity.Id, null, null);
								Nr++;
							}
						}

						if (Nr == 0)
							MainWindow.MessageBox("No legal identities found to obsolete.", "Identities", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to get list of identities." : e2.ErrorText);

				return Task.CompletedTask;

			}, null);
		}

		private void CompromizedLegalIdentities_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show(MainWindow.currentInstance, "Are you sure you want to report your registered legal identities as compromized?",
				"Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes) != MessageBoxResult.Yes)
			{
				return;
			}

			this.contractsClient.GetLegalIdentities((sender2, e2) =>
			{
				if (e2.Ok)
				{
					if (e2.Identities is null || e2.Identities.Length == 0)
						MainWindow.MessageBox("No legal identities are regitered.", "Identities", MessageBoxButton.OK, MessageBoxImage.Information);
					else
					{
						int Nr = 0;

						foreach (LegalIdentity Identity in e2.Identities)
						{
							if (Identity.State == IdentityState.Approved ||
								Identity.State == IdentityState.Created ||
								Identity.State == IdentityState.Obsoleted)
							{
								this.contractsClient.CompromisedLegalIdentity(Identity.Id, null, null);
								Nr++;
							}
						}

						if (Nr == 0)
							MainWindow.MessageBox("No legal identities found to report as compromized.", "Identities", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to get list of identities." : e2.ErrorText);

				return Task.CompletedTask;

			}, null);
		}

	}
}
