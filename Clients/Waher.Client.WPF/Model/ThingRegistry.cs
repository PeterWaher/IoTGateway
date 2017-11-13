using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Waher.Content;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Provisioning.SearchOperators;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Dialogs;

namespace Waher.Client.WPF.Model
{
	public class ThingRegistry : XmppComponent
	{
		private bool supportsProvisioning;
		private ThingRegistryClient registryClient;

		public ThingRegistry(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent, JID, Name, Node, Features)
		{
			this.supportsProvisioning = Features.ContainsKey(ProvisioningClient.NamespaceProvisioning);
			this.registryClient = new ThingRegistryClient(this.Account.Client, JID);
		}

		public override void Dispose()
		{
			if (this.registryClient != null)
			{
				this.registryClient.Dispose();
				this.registryClient = null;
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
							Operators.Add(new StringTagLike(Rule.Tag, Rule.Value1, "*"));
							break;
					}
				}

				this.registryClient.Search(0, 100, Operators.ToArray(), (sender, e) =>
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

						MainWindow.currentInstance.Dispatcher.Invoke(() =>
						{
							TabItem TabItem = new TabItem();
							MainWindow.currentInstance.Tabs.Items.Add(TabItem);

							SearchResultView View = new SearchResultView(Headers.ToArray(), Records.ToArray());

							TabItem.Header = "Search Result";
							TabItem.Content = View;

							MainWindow.currentInstance.Tabs.SelectedItem = TabItem;
						});
					}
					else
					{
						MainWindow.currentInstance.Dispatcher.Invoke(() => MessageBox.Show(MainWindow.currentInstance,
							string.IsNullOrEmpty(e.ErrorText) ? "Unable to perform search." : e.ErrorText, "Error",
							MessageBoxButton.OK, MessageBoxImage.Error));
					}
				}, null);
			}
		}
	}
}
