using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Provisioning.SearchOperators;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Questions;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Model.Concentrator;

namespace Waher.Client.WPF.Model.Provisioning
{
	public class ThingRegistry : XmppComponent, IMenuAggregator
	{
		private readonly bool supportsProvisioning;
		private ThingRegistryClient registryClient;
		private ProvisioningClient provisioningClient;

		public ThingRegistry(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent, JID, Name, Node, Features)
		{
			this.supportsProvisioning = Features.ContainsKey(ProvisioningClient.NamespaceProvisioningOwner);
			this.registryClient = new ThingRegistryClient(this.Account.Client, JID);

			if (this.supportsProvisioning)
			{
				XmppAccountNode Account = this.Account;
				XmppClient Client = Account.Client;

				this.provisioningClient = new ProvisioningClient(Client, JID)
				{
					ManagePresenceSubscriptionRequests = false
				};

				this.provisioningClient.IsFriendQuestion += this.ProvisioningClient_IsFriendQuestion;
				this.provisioningClient.CanReadQuestion += this.ProvisioningClient_CanReadQuestion;
				this.provisioningClient.CanControlQuestion += this.ProvisioningClient_CanControlQuestion;

				foreach (MessageEventArgs Message in Account.GetUnhandledMessages("isFriend", ProvisioningClient.NamespaceProvisioningOwner))
				{
					try
					{
						this.ProvisioningClient_IsFriendQuestion(this, new IsFriendEventArgs(this.provisioningClient, Message));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}
			else
				this.provisioningClient = null;
		}

		public bool SupportsProvisioning
		{
			get { return this.supportsProvisioning; }
		}

		public ThingRegistryClient ThingRegistryClient
		{
			get { return this.registryClient; }
		}

		public ProvisioningClient ProvisioningClient
		{
			get { return this.provisioningClient; }
		}

		private async void ProvisioningClient_IsFriendQuestion(object Sender, IsFriendEventArgs e)
		{
			try
			{
				IsFriendQuestion Question = await Database.FindFirstDeleteRest<IsFriendQuestion>(new FilterAnd(
					new FilterFieldEqualTo("Key", e.Key), new FilterFieldEqualTo("JID", e.JID)));

				if (Question is null)
				{
					Question = new IsFriendQuestion()
					{
						Created = DateTime.Now,
						Key = e.Key,
						JID = e.JID,
						RemoteJID = e.RemoteJID,
						OwnerJID = XmppClient.GetBareJID(e.To),
						ProvisioningJID = this.provisioningClient.ProvisioningServerAddress
					};

					await Database.Insert(Question);

					MainWindow.UpdateGui(() =>
						MainWindow.currentInstance.NewQuestion(this.Account, this.provisioningClient, Question));
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void ProvisioningClient_CanReadQuestion(object Sender, CanReadEventArgs e)
		{
			try
			{
				CanReadQuestion Question = await Database.FindFirstDeleteRest<CanReadQuestion>(new FilterAnd(
					new FilterFieldEqualTo("Key", e.Key), new FilterFieldEqualTo("JID", e.JID)));

				if (Question is null)
				{
					Question = new CanReadQuestion()
					{
						Created = DateTime.Now,
						Key = e.Key,
						JID = e.JID,
						RemoteJID = e.RemoteJID,
						OwnerJID = XmppClient.GetBareJID(e.To),
						ProvisioningJID = this.provisioningClient.ProvisioningServerAddress,
						ServiceTokens = e.ServiceTokens,
						DeviceTokens = e.DeviceTokens,
						UserTokens = e.UserTokens,
						FieldNames = e.Fields,
						Categories = e.FieldTypes,
						NodeId = e.NodeId,
						SourceId = e.SourceId,
						Partition = e.Partition
					};

					await Database.Insert(Question);

					MainWindow.UpdateGui(() =>
						MainWindow.currentInstance.NewQuestion(this.Account, this.provisioningClient, Question));
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void ProvisioningClient_CanControlQuestion(object Sender, CanControlEventArgs e)
		{
			try
			{
				CanControlQuestion Question = await Database.FindFirstDeleteRest<CanControlQuestion>(new FilterAnd(
					new FilterFieldEqualTo("Key", e.Key), new FilterFieldEqualTo("JID", e.JID)));

				if (Question is null)
				{
					Question = new CanControlQuestion()
					{
						Created = DateTime.Now,
						Key = e.Key,
						JID = e.JID,
						RemoteJID = e.RemoteJID,
						OwnerJID = XmppClient.GetBareJID(e.To),
						ProvisioningJID = this.provisioningClient.ProvisioningServerAddress,
						ServiceTokens = e.ServiceTokens,
						DeviceTokens = e.DeviceTokens,
						UserTokens = e.UserTokens,
						ParameterNames = e.Parameters,
						NodeId = e.NodeId,
						SourceId = e.SourceId,
						Partition = e.Partition
					};

					await Database.Insert(Question);

					MainWindow.UpdateGui(() =>
						MainWindow.currentInstance.NewQuestion(this.Account, this.provisioningClient, Question));
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		public override void Dispose()
		{
			if (this.registryClient != null)
			{
				this.registryClient.Dispose();
				this.registryClient = null;
			}

			if (this.provisioningClient != null)
			{
				this.provisioningClient.Dispose();
				this.provisioningClient = null;
			}

			base.Dispose();
		}

		public override ImageSource ImageResource => XmppAccountNode.database;

		public override string ToolTip
		{
			get
			{
				if (this.supportsProvisioning)
					return "Thing Registry & Provisioning Server";
				else
					return "Thing Registry";
			}
		}

		public override bool CanSearch => true;

		public override void Search()
		{
			SearchForThingsDialog Dialog = new SearchForThingsDialog()
			{
				Owner = MainWindow.currentInstance
			};

			bool? Result = Dialog.ShowDialog();

			if (Result.HasValue && Result.Value)
			{
				Rule[] Rules = Dialog.GetRules();
				List<SearchOperator> Operators = new List<SearchOperator>();
				bool Numeric;

				foreach (Rule Rule in Rules)
				{
					Numeric = CommonTypes.TryParse(Rule.Value1, out double d);

					switch (Rule.Operator)
					{
						case Operator.Equality:
							if (Numeric)
								Operators.Add(new NumericTagEqualTo(Rule.Tag, d));
							else
								Operators.Add(new StringTagEqualTo(Rule.Tag, Rule.Value1));
							break;

						case Operator.NonEquality:
							if (Numeric)
								Operators.Add(new NumericTagNotEqualTo(Rule.Tag, d));
							else
								Operators.Add(new StringTagNotEqualTo(Rule.Tag, Rule.Value1));
							break;

						case Operator.GreaterThan:
							if (Numeric)
								Operators.Add(new NumericTagGreaterThan(Rule.Tag, d));
							else
								Operators.Add(new StringTagGreaterThan(Rule.Tag, Rule.Value1));
							break;

						case Operator.GreaterThanOrEqualTo:
							if (Numeric)
								Operators.Add(new NumericTagGreaterThanOrEqualTo(Rule.Tag, d));
							else
								Operators.Add(new StringTagGreaterThanOrEqualTo(Rule.Tag, Rule.Value1));
							break;

						case Operator.LesserThan:
							if (Numeric)
								Operators.Add(new NumericTagLesserThan(Rule.Tag, d));
							else
								Operators.Add(new StringTagLesserThan(Rule.Tag, Rule.Value1));
							break;

						case Operator.LesserThanOrEqualTo:
							if (Numeric)
								Operators.Add(new NumericTagLesserThanOrEqualTo(Rule.Tag, d));
							else
								Operators.Add(new StringTagLesserThanOrEqualTo(Rule.Tag, Rule.Value1));
							break;

						case Operator.InRange:
							Numeric &= CommonTypes.TryParse(Rule.Value2, out double d2);

							if (Numeric)
								Operators.Add(new NumericTagInRange(Rule.Tag, d, true, d2, true));
							else
								Operators.Add(new StringTagInRange(Rule.Tag, Rule.Value1, true, Rule.Value2, true));
							break;

						case Operator.NotInRange:
							Numeric &= CommonTypes.TryParse(Rule.Value2, out d2);

							if (Numeric)
								Operators.Add(new NumericTagNotInRange(Rule.Tag, d, true, d2, true));
							else
								Operators.Add(new StringTagNotInRange(Rule.Tag, Rule.Value1, true, Rule.Value2, true));
							break;

						case Operator.Wildcard:
							Operators.Add(new StringTagMask(Rule.Tag, Rule.Value1, "*"));
							break;

						case Operator.RegularExpression:
							Operators.Add(new StringTagRegEx(Rule.Tag, Rule.Value1));
							break;
					}
				}

				this.registryClient.Search(0, 100, Operators.ToArray(), (sender, e) =>
				{
					this.ShowResult(e);
				}, null);
			}
		}

		private void ShowResult(SearchResultEventArgs e)
		{
			if (e.Ok)
			{
				List<Field> Headers = new List<Field>()
				{
					new TextSingleField(null, "_JID", "JID", false, null, null, string.Empty, null, null, string.Empty, false, false, false)
				};
				List<Dictionary<string, string>> Records = new List<Dictionary<string, string>>();
				Dictionary<string, bool> TagNames = new Dictionary<string, bool>();
				bool HasNodeId = false;
				bool HasSourceId = false;
				bool HasPartition = false;

				foreach (SearchResultThing Thing in e.Things)
				{
					HasNodeId |= !string.IsNullOrEmpty(Thing.Node.NodeId);
					HasSourceId |= !string.IsNullOrEmpty(Thing.Node.SourceId);
					HasPartition |= !string.IsNullOrEmpty(Thing.Node.Partition);
				}

				if (HasNodeId)
				{
					Headers.Add(new TextSingleField(null, "_NodeId", "Node ID", false, null, null, string.Empty, null, null,
						string.Empty, false, false, false));
				}

				if (HasSourceId)
				{
					Headers.Add(new TextSingleField(null, "_SourceId", "Source ID", false, null, null, string.Empty, null, null,
						string.Empty, false, false, false));
				}

				if (HasPartition)
				{
					Headers.Add(new TextSingleField(null, "_Partition", "Partition", false, null, null, string.Empty, null, null,
						string.Empty, false, false, false));
				}

				foreach (SearchResultThing Thing in e.Things)
				{
					Dictionary<string, string> Record = new Dictionary<string, string>()
							{
								{ "_JID", Thing.Jid }
							};
					string Label;

					if (HasNodeId)
						Record["_NodeId"] = Thing.Node.NodeId;

					if (HasSourceId)
						Record["_SourceId"] = Thing.Node.SourceId;

					if (HasPartition)
						Record["_Partition"] = Thing.Node.Partition;

					foreach (MetaDataTag Tag in Thing.Tags)
					{
						Record[Tag.Name] = Tag.StringValue;

						if (!TagNames.ContainsKey(Tag.Name))
						{
							TagNames[Tag.Name] = true;

							switch (Tag.Name)
							{
								case "ALT": Label = "Altitude"; break;
								case "APT": Label = "Apartment"; break;
								case "AREA": Label = "Area"; break;
								case "BLD": Label = "Building"; break;
								case "CITY": Label = "City"; break;
								case "CLASS": Label = "Class"; break;
								case "COUNTRY": Label = "Country"; break;
								case "LAT": Label = "Latitude"; break;
								case "LONG": Label = "Longitude"; break;
								case "MAN": Label = "Manufacturer"; break;
								case "MLOC": Label = "Meter Location"; break;
								case "MNR": Label = "Meter Number"; break;
								case "MODEL": Label = "Model"; break;
								case "NAME": Label = "Name"; break;
								case "PURL": Label = "Product URL"; break;
								case "REGION": Label = "Region"; break;
								case "ROOM": Label = "Room"; break;
								case "SN": Label = "Serial Number"; break;
								case "STREET": Label = "Street"; break;
								case "STREETNR": Label = "Street Number"; break;
								case "V": Label = "Version"; break;
								default: Label = Tag.Name; break;
							}

							Headers.Add(new TextSingleField(null, Tag.Name, Label, false, null, null, string.Empty, null, null,
								string.Empty, false, false, false));
						}
					}

					Records.Add(Record);
				}

				// TODO: Pages, if more things available.

				MainWindow.UpdateGui(() =>
				{
					TabItem TabItem = MainWindow.NewTab("Search Result");
					MainWindow.currentInstance.Tabs.Items.Add(TabItem);

					SearchResultView View = new SearchResultView(Headers.ToArray(), Records.ToArray());
					TabItem.Content = View;

					MainWindow.currentInstance.Tabs.SelectedItem = TabItem;
				});
			}
			else
				MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to perform search." : e.ErrorText);
		}

		public override bool CanAddChildren => true;

		public override void Add()
		{
			ClaimDeviceForm Form = new ClaimDeviceForm();
			bool? Result = Form.ShowDialog();

			if (Result.HasValue && Result.Value)
			{
				this.registryClient.Mine(Form.MakePublic, Form.Tags, (sender, e) =>
				{
					if (e.Ok)
					{
						StringBuilder Msg = new StringBuilder();

						Msg.AppendLine("Device successfully claimed.");
						Msg.AppendLine();
						Msg.Append("JID: ");
						Msg.AppendLine(e.JID);

						if (!e.Node.IsEmpty)
						{
							if (!string.IsNullOrEmpty(e.Node.NodeId))
							{
								Msg.Append("Node ID: ");
								Msg.AppendLine(e.Node.NodeId);
							}

							if (!string.IsNullOrEmpty(e.Node.SourceId))
							{
								Msg.Append("Source ID: ");
								Msg.AppendLine(e.Node.SourceId);
							}

							if (!string.IsNullOrEmpty(e.Node.Partition))
							{
								Msg.Append("Partition: ");
								Msg.AppendLine(e.Node.Partition);
							}
						}

						MainWindow.SuccessBox(Msg.ToString());

						if (this.Account.Client.GetRosterItem(e.JID) is null)
							this.Account.Client.RequestPresenceSubscription(e.JID);
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to claim device." : e.ErrorText);

				}, null);
			}
		}

		public override void AddContexMenuItems(ref string CurrentGroup, ContextMenu Menu)
		{
			base.AddContexMenuItems(ref CurrentGroup, Menu);

			if (this.supportsProvisioning)
			{
				MenuItem Item;

				this.GroupSeparator(ref CurrentGroup, "Database", Menu);

				Menu.Items.Add(Item = new MenuItem()
				{
					Header = "M_y devices...",
					IsEnabled = true,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/Safe-icon_16.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});

				Item.Click += this.MyDevices_Click;

				Menu.Items.Add(Item = new MenuItem()
				{
					Header = "_Recycle device rule caches...",
					IsEnabled = true,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/Recycle-Bin-empty-icon_16.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});

				Item.Click += this.RecycleDeviceRuleCaches_Click;
			}
		}

		private void MyDevices_Click(object sender, RoutedEventArgs e)
		{
			this.provisioningClient.GetDevices(0, 100, (sender2, e2) =>
			{
				this.ShowResult(e2);
			}, null);
		}

		private void RecycleDeviceRuleCaches_Click(object sender, RoutedEventArgs e)
		{
			this.provisioningClient.ClearDeviceCaches((sender2, e2) =>
			{
				if (e2.Ok)
					MainWindow.SuccessBox("The rule caches in your connected devices have been cleared.");
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to clear rule caches in your connected devices." : e2.ErrorText);
			}, null);
		}

		public void AddContexMenuItems(TreeNode TreeNode, ref string CurrentGroup, ContextMenu Menu)
		{
			MenuItem Item;

			if (TreeNode is XmppContact Contact)
			{
				if (this.registryClient != null)
				{
					this.GroupSeparator(ref CurrentGroup, "Registry", Menu);

					Menu.Items.Add(Item = new MenuItem()
					{
						Header = "_Disown device...",
						IsEnabled = true,
						Tag = Contact,
						Icon = new Image()
						{
							Source = new BitmapImage(new Uri("../Graphics/Transfer-icon_16.png", UriKind.Relative)),
							Width = 16,
							Height = 16
						}
					});

					Item.Click += this.DisownDevice_Click;
				}

				if (this.provisioningClient != null)
				{
					this.GroupSeparator(ref CurrentGroup, "Registry", Menu);

					Menu.Items.Add(Item = new MenuItem()
					{
						Header = "_Clear rule cache...",
						IsEnabled = true,
						Tag = Contact,
						Icon = new Image()
						{
							Source = new BitmapImage(new Uri("../Graphics/Recycle-Bin-empty-icon_16.png", UriKind.Relative)),
							Width = 16,
							Height = 16
						}
					});

					Item.Click += this.ClearRuleCache_Click;

					Menu.Items.Add(Item = new MenuItem()
					{
						Header = "_Reconfigure device...",
						IsEnabled = true,
						Tag = Contact,
						Icon = new Image()
						{
							Source = new BitmapImage(new Uri("../Graphics/renew-icon_16.png", UriKind.Relative)),
							Width = 16,
							Height = 16
						}
					});

					Item.Click += this.ReconfigureDevice_Click;
				}
			}
			else if (TreeNode is Node Node)
			{
				if (this.registryClient != null)
				{
					this.GroupSeparator(ref CurrentGroup, "Registry", Menu);

					Menu.Items.Add(Item = new MenuItem()
					{
						Header = "_Disown device...",
						IsEnabled = true,
						Tag = Node,
						Icon = new Image()
						{
							Source = new BitmapImage(new Uri("../Graphics/Transfer-icon_16.png", UriKind.Relative)),
							Width = 16,
							Height = 16
						}
					});

					Item.Click += this.DisownDeviceNode_Click;
				}

				if (this.provisioningClient != null)
				{
					this.GroupSeparator(ref CurrentGroup, "Registry", Menu);

					Menu.Items.Add(Item = new MenuItem()
					{
						Header = "_Reconfigure device...",
						IsEnabled = true,
						Tag = Node,
						Icon = new Image()
						{
							Source = new BitmapImage(new Uri("../Graphics/renew-icon_16.png", UriKind.Relative)),
							Width = 16,
							Height = 16
						}
					});

					Item.Click += this.ReconfigureDeviceNode_Click;
				}
			}
		}

		private void ClearRuleCache_Click(object sender, RoutedEventArgs e)
		{
			MenuItem Item = (MenuItem)sender;
			XmppContact Contact = (XmppContact)Item.Tag;

			this.provisioningClient.ClearDeviceCache(Contact.BareJID, (sender2, e2) =>
			{
				if (e2.Ok)
					MainWindow.SuccessBox("The rule cache in " + Contact.BareJID + " has been cleared.");
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to clear the rule cache in " + Contact.BareJID + "." : e2.ErrorText);

			}, null);
		}

		private void ReconfigureDevice_Click(object sender, RoutedEventArgs e)
		{
			MenuItem Item = (MenuItem)sender;
			XmppContact Contact = (XmppContact)Item.Tag;

			this.provisioningClient.DeleteDeviceRules(Contact.BareJID, (sender2, e2) =>
			{
				if (e2.Ok)
					MainWindow.SuccessBox("The rules in " + Contact.BareJID + " have been deleted.");
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to delete the rules in " + Contact.BareJID + "." : e2.ErrorText);

			}, null);
		}

		private void ReconfigureDeviceNode_Click(object sender, RoutedEventArgs e)
		{
			MenuItem Item = (MenuItem)sender;
			Node Node = (Node)Item.Tag;

			this.provisioningClient.DeleteDeviceRules(Node.Concentrator.BareJID, Node.NodeId, Node.SourceId, Node.Partition, (sender2, e2) =>
			{
				if (e2.Ok)
					MainWindow.SuccessBox("The rules in " + Node.Header + " have been deleted.");
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to delete the rules in " + Node.Header + "." : e2.ErrorText);

			}, null);
		}

		private void DisownDevice_Click(object sender, RoutedEventArgs e)
		{
			MenuItem Item = (MenuItem)sender;
			XmppContact Contact = (XmppContact)Item.Tag;

			this.registryClient.Disown(Contact.BareJID, (sender2, e2) =>
			{
				if (e2.Ok)
					MainWindow.SuccessBox(Contact.BareJID + " has been disowned.");
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to disown " + Contact.BareJID + "." : e2.ErrorText);

			}, null);
		}

		private void DisownDeviceNode_Click(object sender, RoutedEventArgs e)
		{
			MenuItem Item = (MenuItem)sender;
			Node Node = (Node)Item.Tag;

			this.registryClient.Disown(Node.Concentrator.BareJID, Node.NodeId, Node.SourceId, Node.Partition, (sender2, e2) =>
			{
				if (e2.Ok)
					MainWindow.SuccessBox(Node.Header + " has been disowned.");
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e2.ErrorText) ? "Unable to disown " + Node.Header + "." : e2.ErrorText);
			}, null);
		}

	}
}
