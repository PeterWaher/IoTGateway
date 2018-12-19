using System;
using SkiaSharp;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Cache;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Vectors;
using Waher.Security;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Things.ControlParameters;

namespace Waher.Networking.XMPP.Chat
{
	/// <summary>
	/// Class managing a chat interface for things.
	/// 
	/// The chat interface is defined in:
	/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
	/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
	/// </summary>
	public class ChatServer : XmppExtension
	{
		private readonly Cache<string, Variables> sessions;
		private readonly SensorServer sensorServer;
		private readonly ControlServer controlServer;
		private readonly ConcentratorServer concentratorServer;
		private readonly ProvisioningClient provisioningClient;
		private readonly BobClient bobClient;
		private HttpFileUploadClient httpUpload = null;

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, SensorServer SensorServer)
			: this(Client, BobClient, SensorServer, null, null)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, ControlServer ControlServer)
			: this(Client, BobClient, null, ControlServer, null)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, SensorServer SensorServer, ControlServer ControlServer)
			: this(Client, BobClient, SensorServer, ControlServer, null)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="ConcentratorServer">Concentrator Server.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, ConcentratorServer ConcentratorServer)
			: this(Client, BobClient, ConcentratorServer, null)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		/// <param name="ProvisioningClient">Provisioning client.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, SensorServer SensorServer, ProvisioningClient ProvisioningClient)
			: this(Client, BobClient, SensorServer, null, ProvisioningClient)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		/// <param name="ProvisioningClient">Provisioning client.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, ControlServer ControlServer, ProvisioningClient ProvisioningClient)
			: this(Client, BobClient, null, ControlServer, ProvisioningClient)
		{
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		/// <param name="ProvisioningClient">Provisioning client.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, SensorServer SensorServer, ControlServer ControlServer,
			ProvisioningClient ProvisioningClient)
			: base(Client)
		{
			this.bobClient = BobClient;
			this.sensorServer = SensorServer;
			this.controlServer = ControlServer;
			this.provisioningClient = ProvisioningClient;
			this.concentratorServer = null;

			this.client.OnChatMessage += new MessageEventHandler(Client_OnChatMessage);

			this.client.RegisterFeature("urn:xmpp:iot:chat");
			this.client.SetPresence(Availability.Chat);

			this.sessions = new Cache<string, Variables>(1000, TimeSpan.MaxValue, new TimeSpan(0, 20, 0));
			this.sessions.Removed += Sessions_Removed;
		}

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="BobClient">Bits-of-Binary client.</param>
		/// <param name="ConcentratorServer">Concentrator Server.</param>
		/// <param name="ProvisioningClient">Provisioning client.</param>
		public ChatServer(XmppClient Client, BobClient BobClient, ConcentratorServer ConcentratorServer, ProvisioningClient ProvisioningClient)
			: base(Client)
		{
			this.bobClient = BobClient;
			this.sensorServer = ConcentratorServer.SensorServer;
			this.controlServer = ConcentratorServer.ControlServer;
			this.provisioningClient = ProvisioningClient;
			this.concentratorServer = ConcentratorServer;

			this.client.OnChatMessage += new MessageEventHandler(Client_OnChatMessage);

			this.client.RegisterFeature("urn:xmpp:iot:chat");
			this.client.SetPresence(Availability.Chat);

			this.sessions = new Cache<string, Variables>(1000, TimeSpan.MaxValue, new TimeSpan(0, 20, 0));
			this.sessions.Removed += Sessions_Removed;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.client.UnregisterFeature("urn:xmpp:iot:chat");
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[0];

		private void SendMarkdownChatMessage(string To, string Message)
		{
			MarkdownDocument MarkdownDocument = new MarkdownDocument(Message, new MarkdownSettings(null, false));
			string PlainTextBody = MarkdownDocument.GeneratePlainText();

			this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
				"<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">" + XML.Encode(Message) + "</content>",
				PlainTextBody, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
		}

		private void SendHtmlChatMessage(string To, string Message, string PlainTextBody)
		{
			if (string.IsNullOrEmpty(PlainTextBody))
			{
				MarkdownDocument MarkdownDocument = new MarkdownDocument(Message, new MarkdownSettings(null, false));
				PlainTextBody = MarkdownDocument.GeneratePlainText();
			}

			this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
				"<html xmlns='http://jabber.org/protocol/xhtml-im'><body xmlns='http://www.w3.org/1999/xhtml'>" + Message + "</body></html>",
				PlainTextBody, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
		}

		private void SendPlainTextChatMessage(string To, string Message)
		{
			this.client.SendChatMessage(To, Message);
		}

		private async void Client_OnChatMessage(object Sender, MessageEventArgs e)
		{
			try
			{
				Variables Variables = this.GetVariables(e.From);
				RemoteXmppSupport Support = null;
				SortedDictionary<int, KeyValuePair<string, object>> Menu = null;
				IDataSource SelectedSource = null;
				INode SelectedNode = null;
				int i;

				if (this.httpUpload is null)
				{
					this.httpUpload = new HttpFileUploadClient(this.client);
					this.httpUpload.Discover(null);
				}

				if (!Variables.TryGetVariable(" Menu ", out Variable v) || (Menu = v.ValueObject as SortedDictionary<int, KeyValuePair<string, object>>) is null)
					Menu = null;

				if (this.concentratorServer != null)
				{
					if (!Variables.TryGetVariable(" Source ", out v) || (SelectedSource = v.ValueObject as IDataSource) is null)
					{
						if (this.concentratorServer.RootDataSources.Length == 1)
						{
							SelectedSource = this.concentratorServer.RootDataSources[0];
							Variables[" Source "] = SelectedSource;
						}
						else
						{
							foreach (IDataSource Source in this.concentratorServer.DataSources)
							{
								if (Source.SourceID == "MeteringTopology")
								{
									SelectedSource = Source;
									Variables[" Source "] = SelectedSource;
									break;
								}
							}
						}

						SelectedNode = null;
						Variables[" Node "] = SelectedNode;
					}

					if (!Variables.TryGetVariable(" Node ", out v) || (SelectedNode = v.ValueObject as INode) is null)
						SelectedNode = null;
				}

				if (!Variables.TryGetVariable(" Support ", out v) || (Support = v.ValueObject as RemoteXmppSupport) is null)
				{
					Support = new RemoteXmppSupport();

					if (e.Content != null && e.Content.LocalName == "content" && e.Content.NamespaceURI == "urn:xmpp:content" &&
						XML.Attribute(e.Content, "type") == "text/markdown")
					{
						Support.Markdown = true;
					}

					Variables[" Support "] = Support;

					this.client.SendServiceDiscoveryRequest(e.From, (sender, e2) =>
					{
						Variables Variables2 = (Variables)e2.State;
						RemoteXmppSupport Support2 = new RemoteXmppSupport()
						{
							Html = e2.Features.ContainsKey("http://jabber.org/protocol/xhtml-im"),
							ByteStreams = e2.Features.ContainsKey("http://jabber.org/protocol/bytestreams"),
							FileTransfer = e2.Features.ContainsKey("http://jabber.org/protocol/si/profile/file-transfer"),
							SessionInitiation = e2.Features.ContainsKey("http://jabber.org/protocol/si"),
							InBandBytestreams = e2.Features.ContainsKey("http://jabber.org/protocol/ibb"),
							BitsOfBinary = e2.Features.ContainsKey("urn:xmpp:bob"),
							Markdown = Support.Markdown
						};

						Variables2[" Support "] = Support2;

						List<string> Features = new List<string>()
						{
							"Plain text"
						};

						if (Support2.Markdown)
							Features.Add("Markdown");

						if (Support2.Html)
							Features.Add("HTML");

						if (Support2.ByteStreams)
							Features.Add("Bytestreams");

						if (Support2.FileTransfer)
							Features.Add("File transfer");

						if (Support2.SessionInitiation)
							Features.Add("Session Initiation");

						if (Support2.InBandBytestreams)
							Features.Add("In-band byte streams");

						if (Support2.BitsOfBinary)
							Features.Add("Bits of binary");

						i = 0;
						int c = Features.Count;
						StringBuilder Msg = new StringBuilder("I've detected you support ");

						foreach (string Feature in Features)
						{
							Msg.Append(Feature);

							i++;
							if (i == c)
								Msg.Append('.');
							else if (i == c - 1)
								Msg.Append(" and ");
							else if (i < c - 1)
								Msg.Append(", ");
						}

						this.SendPlainTextChatMessage(e.From, Msg.ToString());

						if (Variables.TryGetVariable(" User ", out v) &&
							v.ValueObject is User User &&
							Types.TryGetQualifiedNames(typeof(Expression).Namespace + ".Persistence", out string[] P))
						{
							this.provisioningClient.CanRead(e.FromBareJID, FieldType.All, null, null, null, null, null, (sender3, e3) =>
							{
								if (e3.CanRead && e3.FieldTypes == FieldType.All)
								{
									User.SetPrivilege(typeof(Expression).Namespace + ".Persistence.SQL.Select", true);
									this.SendPlainTextChatMessage(e.From, "SQL SELECT interface enabled.");
								}
							}, null);
						}

					}, Variables);
				}

				string s = e.Body;
				if (e.Content != null && e.Content.LocalName == "content" && e.Content.NamespaceURI == "urn:xmpp:content" &&
					XML.Attribute(e.Content, "type") == "text/markdown")
				{
					string s2 = e.Content.InnerText;

					if (!string.IsNullOrEmpty(s2))
						s = s2;

					Support.Markdown |= true;
				}

				if (s is null || string.IsNullOrEmpty(s = s.Trim()))
					return;

				switch (s.ToLower())
				{
					case "hi":
					case "hello":
					case "hej":
					case "hallo":
					case "hola":
						this.SendPlainTextChatMessage(e.From, "Hello. Type # to display the menu.");
						break;

					case "#":
						this.ShowHelp(e.From, false, Support);
						break;

					case "##":
						this.ShowHelp(e.From, true, Support);
						break;

					case "?":
					case "??":
						IThingReference ThingRef;
						bool Full = (s == "??");

						if (this.concentratorServer != null)
						{
							if (SelectedNode is null)
							{
								this.Error(e.From, "No node selected.", Support);
								break;
							}

							ThingRef = SelectedNode;
						}
						else
							ThingRef = ThingReference.Empty;

						IThingReference[] NodeReferences = new IThingReference[] { ThingRef };
						FieldType FieldTypes = Full ? FieldType.All : FieldType.AllExceptHistorical;
						InternalReadoutFieldsEventHandler FieldsHandler = Full ? (InternalReadoutFieldsEventHandler)this.AllFieldsRead : this.MomentaryFieldsRead;
						InternalReadoutErrorsEventHandler ErrorsHandler = Full ? (InternalReadoutErrorsEventHandler)this.AllFieldsErrorsRead : this.MomentaryFieldsErrorsRead;

						this.InitReadout(e.From);
						this.SendPlainTextChatMessage(e.From, "Readout started...");

						if (this.provisioningClient != null)
						{
							this.provisioningClient.CanRead(XmppClient.GetBareJID(e.From),
								FieldTypes, NodeReferences, null, new string[0], new string[0], new string[0],
								(sender2, e2) =>
								{
									if (e2.Ok && e2.CanRead)
									{
										this.sensorServer.DoInternalReadout(e.From, e2.Nodes, e2.FieldTypes, e2.FieldsNames,
											DateTime.MinValue, DateTime.MaxValue, FieldsHandler, ErrorsHandler,
											new object[] { e.From, true, null, Support });
									}
									else
										this.Error(e.From, "Access denied.", Support);

								}, null);
						}
						else
						{
							this.sensorServer.DoInternalReadout(e.From, NodeReferences, FieldTypes, null, DateTime.MinValue, DateTime.MaxValue,
								FieldsHandler, ErrorsHandler, new object[] { e.From, true, null, Support });
						}
						break;

					case "=":
						if (Support.Markdown)
						{
							StringBuilder Markdown = new StringBuilder();

							Markdown.AppendLine("|Variable|Value|");
							Markdown.AppendLine("|:-------|:---:|");

							foreach (Variable v2 in Variables)
							{
								if (v2.Name.StartsWith(" "))
									continue;

								string s2 = v2.ValueElement.ToString();
								if (s2.Length > 100)
									s2 = s2.Substring(0, 100) + "...";

								s2 = MarkdownDocument.Encode(s2).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");

								Markdown.Append('|');
								Markdown.Append(MarkdownDocument.Encode(v2.Name));
								Markdown.Append('|');
								Markdown.Append(s2);
								Markdown.AppendLine("|");

								if (Markdown.Length > 3000)
								{
									this.SendMarkdownChatMessage(e.From, Markdown.ToString());
									Markdown.Clear();
								}
							}

							if (Markdown.Length > 0)
								this.SendMarkdownChatMessage(e.From, Markdown.ToString());
						}
						else if (Support.Html)
						{
							StringBuilder Html = new StringBuilder();

							Html.AppendLine("<table><tr><th>Variable</th><th>Value</th></tr>");

							foreach (Variable v2 in Variables)
							{
								if (v2.Name.StartsWith(" "))
									continue;

								string s2 = v2.ValueElement.ToString();
								if (s2.Length > 100)
									s2 = s2.Substring(0, 100) + "...";

								s2 = XML.HtmlValueEncode(s2).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");

								Html.Append("<tr><td>");
								Html.Append(v2.Name);
								Html.Append("</td><td>");
								Html.Append(s2);
								Html.AppendLine("</td></tr>");
							}

							Html.Append("</table>");

							this.SendHtmlChatMessage(e.From, Html.ToString(), null);
						}
						else
						{
							StringBuilder Text = new StringBuilder();

							Text.AppendLine("Variable\tValue");

							foreach (Variable v2 in Variables)
							{
								if (v2.Name.StartsWith(" "))
									continue;

								string s2 = v2.ValueElement.ToString();
								if (s2.Length > 100)
									s2 = s2.Substring(0, 100) + "...";

								Text.Append(v2.Name);
								Text.Append('\t');
								Text.AppendLine(s2);

								if (Text.Length > 3000)
								{
									this.SendPlainTextChatMessage(e.From, Text.ToString());
									Text.Clear();
								}
							}

							if (Text.Length > 0)
								this.SendPlainTextChatMessage(e.From, Text.ToString());
						}
						break;

					case "/":
						if (this.concentratorServer is null)
							this.SendPlainTextChatMessage(e.From, "Device is not a concentrator.");
						else if (SelectedSource is null)
							this.SendPlainTextChatMessage(e.From, "No source selected.");
						else
						{
							IEnumerable<INode> Nodes = SelectedSource.RootNodes;
							bool First = true;

							SelectedNode = null;
							if (Nodes != null)
							{
								foreach (INode Node in Nodes)
								{
									if (First)
									{
										SelectedNode = Node;
										First = false;
									}
									else
									{
										SelectedNode = null;
										break;
									}
								}
							}

							await this.SelectNode(e, SelectedSource, SelectedNode, Variables, Menu, Support);
						}
						break;

					case "//":
						if (this.concentratorServer is null)
							this.SendPlainTextChatMessage(e.From, "Device is not a concentrator.");
						else
						{
							IEnumerable<IDataSource> Sources = this.concentratorServer.RootDataSources;
							bool First = true;

							SelectedSource = null;
							foreach (IDataSource Source in Sources)
							{
								if (First)
								{
									SelectedSource = Source;
									First = false;
								}
								else
								{
									SelectedSource = null;
									break;
								}
							}

							this.SelectSource(e, SelectedSource, Variables, Menu, Support);
						}
						break;

					case ":=":
						if (SelectedNode is null)
							this.Error(e.From, "No node selected.", Support);
						else if (SelectedNode is IActuator Actuator)
						{
							ControlParameter[] Parameters = Actuator.GetControlParameters();
							this.ControlParametersMenu(Parameters, SelectedNode, SelectedSource, Menu, Variables, Support, e);
						}
						else
							this.Error(e.From, "Selected node is not an actuator", Support);
						break;

					default:
						if (int.TryParse(s, out i) && i >= 0 && Menu != null && Menu.TryGetValue(i, out KeyValuePair<string, object> Item))
						{
							if (string.IsNullOrEmpty(Item.Key))
							{
								if (i == 0 && Item.Value is SortedDictionary<int, KeyValuePair<string, object>> PreviousMenu)
								{
									Menu = PreviousMenu;
									if (Menu is null)
										this.Error(e.From, "There's no previous menu.", Support);
									else
										this.SendMenu(e.From, Menu, Variables, Support);
								}
							}
							else
							{
								s = Item.Key;

								if (Menu.TryGetValue(-2, out KeyValuePair<string, object> Obj) && Obj.Value is INode Node &&
									Menu.TryGetValue(-1, out Obj) && Obj.Value is IDataSource Source)
								{
									if (Node.HasChildren)
									{
										foreach (INode ChildNode in await Node.ChildNodes)
										{
											if (ChildNode.NodeId == s)
											{
												await this.SelectNode(e, Source, ChildNode, Variables, Menu, Support);
												break;
											}
										}
									}
								}
								else if (Menu.TryGetValue(-1, out Obj))
								{
									if (Obj.Value is null)
									{
										foreach (IDataSource RootSource in this.concentratorServer.DataSources)
										{
											if (RootSource.SourceID == s)
											{
												this.SelectSource(e, RootSource, Variables, Menu, Support);
												break;
											}
										}
									}
									else if ((Source = Obj.Value as IDataSource) != null)
									{
										if (Source.HasChildren)
										{
											foreach (IDataSource ChildSource in Source.ChildSources)
											{
												if (ChildSource.SourceID == s)
												{
													this.SelectSource(e, ChildSource, Variables, Menu, Support);
													s = null;
													break;
												}
											}
										}

										if (!string.IsNullOrEmpty(s))
										{
											if (Source.RootNodes != null)
											{
												foreach (INode RootNode in Source.RootNodes)
												{
													if (RootNode.NodeId == s)
													{
														await this.SelectNode(e, Source, RootNode, Variables, Menu, Support);
														break;
													}
												}
											}
										}
									}
								}
							}
						}
						else if (s.EndsWith("?"))
						{
							Full = s.EndsWith("??");
							this.InitReadout(e.From);
							string Field = s.Substring(0, s.Length - (Full ? 2 : 1)).Trim();

							FieldTypes = Full ? FieldType.All : FieldType.AllExceptHistorical;
							FieldsHandler = Full ? (InternalReadoutFieldsEventHandler)this.AllFieldsRead : this.MomentaryFieldsRead;
							ErrorsHandler = Full ? (InternalReadoutErrorsEventHandler)this.AllFieldsErrorsRead : this.MomentaryFieldsErrorsRead;

							if (this.concentratorServer != null)
							{
								if (SelectedSource is null)
								{
									this.Error(e.From, "No source selected.", Support);
									break;
								}

								INode Node;

								i = Field.IndexOf('.');
								if (i >= 0)
								{
									Node = this.CheckMenuValue(Menu, Field.Substring(0, i)) as INode;
									Field = this.CheckMenuKey(Menu, Field.Substring(i + 1));

									if (Node is null)
									{
										this.Error(e.From, "Node not found.", Support);
										break;
									}
								}
								else if ((Node = this.CheckMenuValue(Menu, Field) as INode) != null)
									Field = string.Empty;
								else
								{
									Node = SelectedNode;

									if (Node is null || !Node.IsReadable)
									{
										if (SelectedSource != null && await SelectedSource.GetNodeAsync(new ThingReference(Field, SelectedSource.SourceID, string.Empty)) is INode Node2)
										{
											Node = Node2;
											Field = string.Empty;
										}
										else
										{
											this.Error(e.From, "No node selected.", Support);
											break;
										}
									}
								}

								ThingRef = Node;

								this.SendPlainTextChatMessage(e.From, "Readout of " + Field + " on " + Node.NodeId + " started...");

								NodeReferences = new IThingReference[] { ThingRef };

								if (string.IsNullOrEmpty(Field))
								{
									if (this.provisioningClient != null)
									{
										this.provisioningClient.CanRead(XmppClient.GetBareJID(e.From),
											FieldTypes, NodeReferences, null, new string[0], new string[0], new string[0],
											(sender2, e2) =>
											{
												if (e2.Ok && e2.CanRead)
												{
													this.sensorServer.DoInternalReadout(e.From, e2.Nodes, e2.FieldTypes, e2.FieldsNames,
														DateTime.MinValue, DateTime.MaxValue, FieldsHandler, ErrorsHandler,
														new object[] { e.From, true, Field, Support });
												}
												else
													this.Error(e.From, "Access denied.", Support);

											}, null);
									}
									else
									{
										this.sensorServer.DoInternalReadout(e.From, NodeReferences,
											FieldTypes, null, DateTime.MinValue, DateTime.MaxValue, FieldsHandler, ErrorsHandler,
											new object[] { e.From, true, Field, Support });
									}
								}
								else
								{
									if (this.provisioningClient != null)
									{
										this.provisioningClient.CanRead(XmppClient.GetBareJID(e.From),
											FieldTypes, NodeReferences, new string[] { Field }, new string[0], new string[0], new string[0],
											(sender2, e2) =>
											{
												if (e2.Ok && e2.CanRead)
												{
													this.sensorServer.DoInternalReadout(e.From, e2.Nodes, e2.FieldTypes, e2.FieldsNames,
														DateTime.MinValue, DateTime.MaxValue, FieldsHandler, ErrorsHandler,
														new object[] { e.From, true, Field, Support });
												}
												else
													this.Error(e.From, "Access denied.", Support);

											}, null);
									}
									else
									{
										this.sensorServer.DoInternalReadout(e.From, NodeReferences,
											FieldTypes, new string[] { Field }, DateTime.MinValue, DateTime.MaxValue, FieldsHandler, ErrorsHandler,
											new object[] { e.From, true, Field, Support });
									}
								}
							}
							else
							{
								this.SendPlainTextChatMessage(e.From, "Readout of " + Field + " started...");

								if (this.provisioningClient != null)
								{
									this.provisioningClient.CanRead(XmppClient.GetBareJID(e.From),
										FieldTypes, null, null, new string[0], new string[0], new string[0],
										(sender2, e2) =>
										{
											if (e2.Ok && e2.CanRead)
											{
												this.sensorServer.DoInternalReadout(e.From, e2.Nodes, e2.FieldTypes, e2.FieldsNames,
													DateTime.MinValue, DateTime.MaxValue, FieldsHandler, ErrorsHandler,
													new object[] { e.From, true, Field, Support });
											}
											else
												this.Error(e.From, "Access denied.", Support);

										}, null);
								}
								else
								{
									this.sensorServer.DoInternalReadout(e.From, null, FieldTypes, null, DateTime.MinValue, DateTime.MaxValue,
										FieldsHandler, ErrorsHandler, new object[] { e.From, true, Field, Support });
								}
							}
						}
						else if (SelectedSource != null && await SelectedSource.GetNodeAsync(new ThingReference(s, SelectedSource.SourceID, string.Empty)) is INode Node)
							await this.SelectNode(e, SelectedSource, Node, Variables, Menu, Support);
						else
						{
							if (this.controlServer != null && (i = s.IndexOf(":=")) > 0)
							{
								string ParameterName = this.CheckMenuKey(Menu, s.Substring(0, i).Trim());
								string ValueStr = s.Substring(i + 2).Trim();

								if (this.concentratorServer != null)
								{
									if (SelectedSource is null)
									{
										this.Error(e.From, "No source selected.", Support);
										break;
									}

									i = ParameterName.IndexOf('.');
									if (i >= 0)
									{
										s = ParameterName.Substring(0, i);
										ParameterName = this.CheckMenuKey(Menu, ParameterName.Substring(i + 1));

										Node = this.CheckMenuValue(Menu, s) as INode;
										if (Node is null && SelectedSource != null && await SelectedSource.GetNodeAsync(new ThingReference(s, SelectedSource.SourceID, string.Empty)) is INode Node3)
											Node = Node3;
									}
									else
									{
										if (string.IsNullOrEmpty(ValueStr))
										{
											Node = this.CheckMenuValue(Menu, ParameterName) as INode;

											if (Node != null)
												ParameterName = string.Empty;
											else
												Node = SelectedNode;
										}
										else
											Node = SelectedNode;
									}

									if (Node is null && Menu != null && Menu.TryGetValue(-2, out KeyValuePair<string, object> P) && P.Value is INode Node2)
										Node = Node2;

									ThingRef = Node;
								}
								else
								{
									i = ParameterName.IndexOf('.');
									if (i < 0)
										ThingRef = ThingReference.Empty;
									else
									{
										ThingRef = this.CheckMenuValue(Menu, ParameterName.Substring(0, i)) as IThingReference;
										ParameterName = this.CheckMenuKey(Menu, ParameterName.Substring(i + 1).TrimStart());
									}
								}

								if (ThingRef != null)
								{
									try
									{
										ControlParameter[] Parameters;

										if (string.IsNullOrEmpty(ValueStr))
										{
											if (this.concentratorServer != null && SelectedSource != null && (Node = await SelectedSource.GetNodeAsync(ThingRef)) != null)
											{
												if (Node is IActuator Actuator)
													Parameters = Actuator.GetControlParameters();
												else
													throw new Exception("Node is not an actuator.");
											}
											else if (SelectedNode != null)
											{
												if (SelectedNode is IActuator Actuator)
												{
													Node = SelectedNode;
													Parameters = Actuator.GetControlParameters();
												}
												else
													throw new Exception("Selected node is not an actuator.");
											}
											else if (this.concentratorServer != null)
												throw new Exception("No node selected.");
											else if (this.controlServer != null)
											{
												Node = null;
												Parameters = await this.controlServer.GetControlParameters(ThingRef);
											}
											else
												throw new Exception("Device not an actuator.");

											List<ControlParameter> Parameters2 = new List<ControlParameter>();

											foreach (ControlParameter P in Parameters)
											{
												if (P.Name.StartsWith(ParameterName))
													Parameters2.Add(P);
											}

											if (Parameters2.Count == 0)
												throw new Exception("No control parameter found starting with that name.");

											this.ControlParametersMenu(Parameters2.ToArray(), SelectedNode, SelectedSource, Menu, Variables, Support, e);
											break;
										}
										else
										{
											if (this.concentratorServer != null)
												Parameters = await this.controlServer.GetControlParameters(ThingRef);
											else
												Parameters = await this.controlServer.GetControlParameters(ThingRef);

											ControlParameter P0 = null;

											foreach (ControlParameter P in Parameters)
											{
												if (string.Compare(P.Name, ParameterName, true) == 0)
												{
													P0 = P;
													break;
												}
											}

											if (P0 is null)
												this.Execute(s, e.From, Support);
											else if (this.provisioningClient != null)
											{
												this.provisioningClient.CanControl(e.FromBareJID, new IThingReference[] { ThingRef },
													new string[] { ParameterName }, new string[0], new string[0], new string[0], (sender2, e2) =>
													{
														if (e2.Ok && e2.CanControl)
														{
															if (P0.SetStringValue(ThingRef, ValueStr))
																this.SendPlainTextChatMessage(e.From, "Control parameter set.");
															else
																this.Error(e.From, "Unable to set control parameter value.", Support);
														}
														else
															this.Error(e.From, "Access denied.", Support);
													}, null);
											}
											else
											{
												if (!P0.SetStringValue(ThingRef, ValueStr))
													throw new Exception("Unable to set control parameter value.");

												this.SendPlainTextChatMessage(e.From, "Control parameter set.");
											}
											return;
										}
									}
									catch (Exception ex)
									{
										this.Error(e.From, ex.Message, Support);
										break;
									}
								}
							}

							this.Execute(s, e.From, Support);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private void ControlParametersMenu(ControlParameter[] Parameters, INode SelectedNode, IDataSource SelectedSource,
			SortedDictionary<int, KeyValuePair<string, object>> PrevMenu, Variables Variables, RemoteXmppSupport Support,
			MessageEventArgs e)
		{
			SortedDictionary<int, KeyValuePair<string, object>> Menu = new SortedDictionary<int, KeyValuePair<string, object>>()
			{
				{ -3, new KeyValuePair<string, object>(null, Parameters) },
				{ -2, new KeyValuePair<string, object>(null, SelectedNode) },
				{ -1, new KeyValuePair<string, object>(null, SelectedSource) },
				{ 0, new KeyValuePair<string, object>(null, PrevMenu) }
			};

			int i = 0;

			if (this.provisioningClient != null)
			{
				int j, c = Parameters.Length;
				string[] ParameterNames = new string[c];

				for (j = 0; j < c; j++)
					ParameterNames[j] = Parameters[j].Name;

				this.provisioningClient.CanControl(e.FromBareJID, null, ParameterNames,
					new string[0], new string[0], new string[0], (sender2, e2) =>
					{
						if (e2.Ok && e2.CanControl)
						{
							List<ControlParameter> Parameters2 = new List<ControlParameter>();

							foreach (ControlParameter P in Parameters)
							{
								if (e2.ParameterNames is null || Array.IndexOf<string>(e2.ParameterNames, P.Name) >= 0)
								{
									Parameters2.Add(P);
									Menu[++i] = new KeyValuePair<string, object>(P.Name, P.Name + " (" + P.GetStringValue(SelectedNode) + ")");
								}
							}

							Menu[-3] = new KeyValuePair<string, object>(null, Parameters2.ToArray());

							this.SendMenu(e.From, Menu, Variables, Support);
						}
						else
							this.Error(e.From, "Access denied.", Support);
					}, null);
			}
			else
			{
				foreach (ControlParameter P in Parameters)
					Menu[++i] = new KeyValuePair<string, object>(P.Name, P.Name + " (" + P.GetStringValue(SelectedNode) + ")");

				this.SendMenu(e.From, Menu, Variables, Support);
			}
		}

		private string CheckMenuKey(SortedDictionary<int, KeyValuePair<string, object>> Menu, string s)
		{
			if (Menu != null &&
				int.TryParse(s, out int i) &&
				i > 0 &&
				Menu.TryGetValue(i, out KeyValuePair<string, object> Obj) &&
				!string.IsNullOrEmpty(Obj.Key))
			{
				return Obj.Key;
			}
			else
				return s;
		}

		private object CheckMenuValue(SortedDictionary<int, KeyValuePair<string, object>> Menu, string s)
		{
			if (Menu != null &&
				int.TryParse(s, out int i) &&
				i > 0 &&
				Menu.TryGetValue(i, out KeyValuePair<string, object> Obj) &&
				!string.IsNullOrEmpty(Obj.Key))
			{
				return Obj.Value;
			}
			else
				return null;
		}

		private async Task SelectNode(MessageEventArgs e, IDataSource SelectedSource, INode SelectedNode, Variables Variables,
			SortedDictionary<int, KeyValuePair<string, object>> Menu, RemoteXmppSupport Support)
		{
			Variables[" Node "] = SelectedNode;

			Menu = new SortedDictionary<int, KeyValuePair<string, object>>()
			{
				{ -2, new KeyValuePair<string, object>(null, SelectedNode) },
				{ -1, new KeyValuePair<string, object>(null, SelectedSource) },
				{ 0, new KeyValuePair<string, object>(null, Menu) }
			};
			int i = 0;

			if (SelectedNode is null)
			{
				Menu[-1] = new KeyValuePair<string, object>(null, SelectedSource);

				this.SendPlainTextChatMessage(e.From, "Root nodes of data source " + SelectedSource.SourceID + ":");

				if (SelectedSource.RootNodes != null)
				{
					foreach (INode Node in SelectedSource.RootNodes)
						Menu[++i] = new KeyValuePair<string, object>(Node.NodeId, Node);
				}
			}
			else
			{
				this.SendPlainTextChatMessage(e.From, "Selecting node " + SelectedNode.NodeId + " of " + SelectedSource.SourceID + ".");

				if (SelectedNode.HasChildren)
				{
					this.SendPlainTextChatMessage(e.From, "Child nodes:");

					foreach (INode Node in await SelectedNode.ChildNodes)
						Menu[++i] = new KeyValuePair<string, object>(Node.NodeId, Node);
				}
			}

			if (i > 0)
				this.SendMenu(e.From, Menu, Variables, Support);
		}

		private void SelectSource(MessageEventArgs e, IDataSource SelectedSource, Variables Variables, SortedDictionary<int, KeyValuePair<string, object>> Menu,
			RemoteXmppSupport Support)
		{
			Variables[" Source "] = SelectedSource;
			Menu = new SortedDictionary<int, KeyValuePair<string, object>>()
			{
				{ -1, new KeyValuePair<string, object>(null, SelectedSource) },
				{ 0, new KeyValuePair<string, object>(null, Menu) }
			};
			int i = 0;

			if (SelectedSource is null)
			{
				this.SendPlainTextChatMessage(e.From, "Root data sources:");

				foreach (IDataSource Source in this.concentratorServer.RootDataSources)
					Menu[++i] = new KeyValuePair<string, object>(Source.SourceID, Source);
			}
			else
			{
				this.SendPlainTextChatMessage(e.From, "Selecting data source " + SelectedSource.SourceID + ":");

				if (SelectedSource.HasChildren)
				{
					foreach (IDataSource Source in SelectedSource.ChildSources)
						Menu[++i] = new KeyValuePair<string, object>(Source.SourceID, Source);
				}

				if (SelectedSource.RootNodes != null)
				{
					foreach (INode Node in SelectedSource.RootNodes)
						Menu[++i] = new KeyValuePair<string, object>(Node.NodeId, Node);
				}
			}

			if (i > 0)
				this.SendMenu(e.From, Menu, Variables, Support);
		}

		private void SendMenu(string To, SortedDictionary<int, KeyValuePair<string, object>> Menu, Variables Variables, RemoteXmppSupport Support)
		{
			StringBuilder sb = new StringBuilder();
			bool HasBack = false;

			Variables[" Menu "] = Menu;

			if (Support.Markdown)
			{
				foreach (KeyValuePair<int, KeyValuePair<string, object>> P in Menu)
				{
					if (P.Key < 0)
						continue;
					else if (P.Key == 0)
						HasBack = P.Value.Value != null;
					else
					{
						sb.Append(P.Key);
						sb.Append(". ");

						if (P.Value.Value != null)
							sb.AppendLine(MarkdownDocument.Encode(P.Value.Value.ToString()));
					}
				}

				if (HasBack)
					sb.AppendLine("0. Back");

				this.SendMarkdownChatMessage(To, sb.ToString());
			}
			else if (Support.Html)
			{
				StringBuilder Html = new StringBuilder();

				Html.AppendLine("<ol>");

				foreach (KeyValuePair<int, KeyValuePair<string, object>> P in Menu)
				{
					if (P.Key < 0)
						continue;
					else if (P.Key == 0)
						HasBack = P.Value.Value != null;
					else
					{
						Html.Append("<li value=\">");
						Html.Append(P.Key);
						Html.Append("\">");

						if (P.Value.Value != null)
							Html.Append(XML.HtmlValueEncode(P.Value.Value.ToString()));

						Html.AppendLine("</li>");

						sb.Append(P.Key);
						sb.Append(". ");

						if (P.Value.Value != null)
							sb.AppendLine(P.Value.Value.ToString());
					}
				}

				if (HasBack)
				{
					sb.AppendLine("0. Back");

					Html.AppendLine("<li value=\"0\">Back</li>");
					Html.AppendLine("</ol>");
				}

				this.SendHtmlChatMessage(To, Html.ToString(), sb.ToString());
			}
			else
			{
				foreach (KeyValuePair<int, KeyValuePair<string, object>> P in Menu)
				{
					if (P.Key < 0)
						continue;
					else if (P.Key == 0)
						HasBack = P.Value.Value != null;
					else
					{
						sb.Append(P.Key);
						sb.Append(". ");

						if (P.Value.Value != null)
							sb.AppendLine(P.Value.Value.ToString());
					}
				}

				if (HasBack)
					sb.AppendLine("0. Back");

				this.SendPlainTextChatMessage(To, sb.ToString());
			}
		}

		private class RemoteXmppSupport
		{
			public bool Markdown = false;
			public bool Html = false;
			public bool ByteStreams = false;
			public bool FileTransfer = false;
			public bool SessionInitiation = false;
			public bool InBandBytestreams = false;
			public bool BitsOfBinary = false;
		}

		private void Execute(string s, string From, RemoteXmppSupport Support)
		{
			Expression Exp;

			try
			{
				Exp = new Expression(s);
			}
			catch (Exception)
			{
				this.Que(From, Support);
				return;
			}

			Variables Variables = this.GetVariables(From);
			TextWriter Bak = Variables.ConsoleOut;
			StringBuilder sb = new StringBuilder();

			Variables.Lock();
			Variables.ConsoleOut = new StringWriter(sb);
			try
			{
				if (!Variables.TryGetVariable(" User ", out Variable v) ||
					!(v.ValueObject is IUser User) ||
					!Exp.ForAll(this.IsAuthorized, User, false))
				{
					throw new Exception("Unauthorized to execute expression.");
				}

				IElement Result = Exp.Root.Evaluate(Variables);
				Variables["Ans"] = Result;

				SKImage Img;

				if (Result is Graph G)
				{
					using (SKImage Bmp = G.CreateBitmap(G.GetSettings(Variables, 600, 300)))
					{
						ImageResult(From, Bmp, Support, Variables, true);
						return;
					}
				}
				else if ((Img = Result.AssociatedObjectValue as SKImage) != null)
				{
					ImageResult(From, Img, Support, Variables, true);
					return;
				}
				else if (Result.AssociatedObjectValue is ObjectMatrix M && M.ColumnNames != null)
				{
					StringBuilder Markdown = new StringBuilder();

					foreach (string s2 in M.ColumnNames)
					{
						Markdown.Append("| ");
						Markdown.Append(MarkdownDocument.Encode(s2));
					}

					Markdown.AppendLine(" |");

					foreach (string s2 in M.ColumnNames)
						Markdown.Append("|---");

					Markdown.AppendLine("|");

					int x, y;

					for (y = 0; y < M.Rows; y++)
					{
						for (x = 0; x < M.Columns; x++)
						{
							Markdown.Append("| ");

							object Item = M.GetElement(x, y).AssociatedObjectValue;
							if (Item != null)
							{
								if (!(Item is string s2))
									s2 = Expression.ToString(Item);

								s2 = s2.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");
								Markdown.Append(MarkdownDocument.Encode(s2));
							}
						}

						Markdown.AppendLine(" |");
					}

					this.SendMarkdownChatMessage(From, Markdown.ToString());
				}
				else
				{
					s = Result.ToString();
					this.SendPlainTextChatMessage(From, s);
				}
			}
			catch (Exception ex)
			{
				this.Error(From, ex.Message, Support);
				this.Que(From, Support);
			}
			finally
			{
				Variables.ConsoleOut.Flush();
				Variables.ConsoleOut = Bak;
				Variables.Release();
			}
		}

		private bool IsAuthorized(ref ScriptNode Node, object State)
		{
			if (State is IUser User)
				return User.HasPrivilege(Node.GetType().FullName);
			else
				return false;
		}

		private void ImageResult(string To, SKImage Bmp, RemoteXmppSupport Support, Variables Variables, bool AllowHttpUpload)
		{
			SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
			byte[] Bin = Data.ToArray();
			Data.Dispose();

			this.ImageResult(To, Bin, Support, Variables, AllowHttpUpload);
		}

		private void ImageResult(string To, byte[] Bin, RemoteXmppSupport Support, Variables Variables, bool AllowHttpUpload)
		{
			string s;

			if (Support.Markdown)
			{
				s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
				s = "![Image result](data:image/png;base64," + s + ")";
				this.SendMarkdownChatMessage(To, s);
			}
			else if (AllowHttpUpload && this.httpUpload != null && this.httpUpload.HasSupport)
			{
				string FileName = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".png";
				string ContentType = "image/png";

				this.httpUpload.RequestUploadSlot(FileName, ContentType, Bin.Length, async (sender, e) =>
				{
					try
					{
						if (e.Ok)
						{
							using (HttpClient HttpClient = new HttpClient())
							{
								HttpClient.Timeout = TimeSpan.FromMilliseconds(30000);
								HttpClient.DefaultRequestHeaders.ExpectContinue = false;

								HttpContent Body = new ByteArrayContent(Bin);

								if (e.PutHeaders != null)
								{
									foreach (KeyValuePair<string, string> P in e.PutHeaders)
										Body.Headers.Add(P.Key, P.Value);

									Body.Headers.Add("Content-Type", ContentType);
								}

								HttpResponseMessage Response = await HttpClient.PutAsync(e.PutUrl, Body);
								if (!Response.IsSuccessStatusCode)
									ImageResult(To, Bin, Support, Variables, false);
								else if (Support.Html)
								{
									s = "<img alt=\"Image result\" src=\"" + e.GetUrl + "\"/>";
									this.SendHtmlChatMessage(To, s, e.GetUrl);
								}
								else
									this.SendPlainTextChatMessage(To, e.GetUrl);
							}
						}
						else
							ImageResult(To, Bin, Support, Variables, false);
					}
					catch (Exception)
					{
						ImageResult(To, Bin, Support, Variables, false);
					}

				}, null);

				return;
			}
			else if (Support.Html && this.bobClient != null && Support.BitsOfBinary)
			{
				s = this.bobClient.StoreData(Bin, "image/png");

				Dictionary<string, bool> ContentIDs;

				if (!Variables.TryGetVariable(" ContentIDs ", out Variable v) ||
					(ContentIDs = v.ValueObject as Dictionary<string, bool>) is null)
				{
					ContentIDs = new Dictionary<string, bool>();
					Variables[" ContentIDs "] = ContentIDs;
				}

				lock (ContentIDs)
				{
					ContentIDs[s] = true;
				}

				s = "<img alt=\"Image result\" src=\"cid:" + s + "\"/>";
				this.SendHtmlChatMessage(To, s, "Embedded bits of binary");
			}
			else if (Support.Html)
			{
				s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
				s = "<img alt=\"Image result\" src=\"data:image/png;base64," + s + "\"/>";
				this.SendHtmlChatMessage(To, s, "Embedded image.");
			}
			else
			{
				s = "Image result";
				this.SendPlainTextChatMessage(To, s);
			}
		}

		private void InitReadout(string Address)
		{
			Variables Variables = this.GetVariables(Address);
			Dictionary<string, SortedDictionary<DateTime, Field>> Fields = new Dictionary<string, SortedDictionary<DateTime, Field>>();
			Variables[" Readout "] = Fields;
		}

		private Variables GetVariables(string Address)
		{
			if (!this.sessions.TryGetValue(Address, out Variables Variables))
			{
				User User = new User(Address);

				User.SetPrivilege(typeof(Expression).Namespace, true);
				User.SetPrivilege(typeof(Expression).Namespace + ".Persistence", false);

				Variables = new Variables(new Variable(" User ", User));
				this.sessions.Add(Address, Variables);
			}

			return Variables;
		}

		private void Sessions_Removed(object Sender, CacheItemEventArgs<string, Variables> e)
		{
			if (this.bobClient != null &&
				e.Value.TryGetVariable(" ContentIDs ", out Variable v) &&
				v.ValueObject is Dictionary<string, bool> ContentIDs)
			{
				foreach (string ContentID in ContentIDs.Keys)
					this.bobClient.DeleteData(ContentID);
			}
		}

		private KeyValuePair<string, string>[] UpdateReadoutVariables(string Address, InternalReadoutFieldsEventArgs e, string From, string Field)
		{
			Variables Variables = this.GetVariables(Address);
			Dictionary<string, SortedDictionary<DateTime, Field>> Fields;
			List<KeyValuePair<string, string>> Exp = null;

			if (Variables.TryGetVariable(" Readout ", out Variable v) &&
				(Fields = v.ValueObject as Dictionary<string, SortedDictionary<DateTime, Field>>) != null)
			{
				foreach (Field F in e.Fields)
				{
					if (!string.IsNullOrEmpty(Field) && !F.Name.StartsWith(Field))
						continue;

					if (!Fields.TryGetValue(F.Name, out SortedDictionary<DateTime, Field> Times))
					{
						Times = new SortedDictionary<DateTime, Field>();
						Fields[F.Name] = Times;
					}

					Times[F.Timestamp] = F;
				}

				if (e.Done)
				{
					Variables.Remove(" Readout ");

					SortedDictionary<string, SeriesTypes> Series = null;
					string s;

					foreach (KeyValuePair<string, SortedDictionary<DateTime, Field>> P in Fields)
					{
						if (P.Value.Count == 1)
						{
							foreach (Field F in P.Value.Values)
								Variables[this.PascalCasing(P.Key)] = this.FieldElement(F);
						}
						else
						{
							List<ObjectVector> Values = new List<ObjectVector>();
							IElement E;
							bool Numeric = true;
							SeriesTypes Types;
							SeriesTypes Type;

							foreach (KeyValuePair<DateTime, Field> P2 in P.Value)
							{
								E = this.FieldElement(P2.Value);
								Values.Add(new ObjectVector(new DateTimeValue(P2.Key),
									Expression.Encapsulate(E),
									new ObjectValue(P2.Value.Type), new ObjectValue(P2.Value.QoS)));

								if (!(E is DoubleNumber) && !(E is PhysicalQuantity))
									Numeric = false;
							}

							if (Numeric)
							{
								string Suffix;

								s = P.Key;

								if (EndsWith(ref s, "Average") || EndsWith(ref s, "Avg"))
								{
									Type = SeriesTypes.Average;
									Suffix = "Average";
								}
								else if (EndsWith(ref s, "Minimum") || EndsWith(ref s, "Min"))
								{
									Type = SeriesTypes.Minimum;
									Suffix = "Minimum";
								}
								else if (EndsWith(ref s, "Maximum") || EndsWith(ref s, "Max"))
								{
									Type = SeriesTypes.Maximum;
									Suffix = "Maximum";
								}
								else
								{
									Type = SeriesTypes.Normal;
									Suffix = string.Empty;
								}

								Variables[this.PascalCasing(s) + Suffix] = VectorDefinition.Encapsulate(Values.ToArray(), true, null);

								if (Series is null)
								{
									Series = new SortedDictionary<string, SeriesTypes>();
									Types = Type;
								}
								else if (Series.TryGetValue(s, out Types))
									Types |= Type;
								else
									Types = Type;

								Series[s] = Types;
							}
						}
					}

					if (Series != null)
					{
						StringBuilder Expression = new StringBuilder();
						string VariableName;

						foreach (KeyValuePair<string, SeriesTypes> P in Series)
						{
							VariableName = this.PascalCasing(P.Key);

							if ((P.Value & SeriesTypes.Minimum) != 0)
							{
								Expression.Append("MinTP:=");
								Expression.Append(VariableName);
								Expression.Append("Minimum[0,];");
								Expression.Append("Min:=");
								Expression.Append(VariableName);
								Expression.Append("Minimum[1,];");
							}

							if ((P.Value & SeriesTypes.Maximum) != 0)
							{
								Expression.Append("MaxTP:=");
								Expression.Append(VariableName);
								Expression.Append("Maximum[0,];");
								Expression.Append("Max:=");
								Expression.Append(VariableName);
								Expression.Append("Maximum[1,];");
							}

							if ((P.Value & SeriesTypes.Average) != 0)
							{
								Expression.Append("AvgTP:=");
								Expression.Append(VariableName);
								Expression.Append("Average[0,];");
								Expression.Append("Avg:=");
								Expression.Append(VariableName);
								Expression.Append("Average[1,];");
							}

							if ((P.Value & SeriesTypes.Normal) != 0)
							{
								Expression.Append("TP:=");
								Expression.Append(VariableName);
								Expression.Append("[0,];");
								Expression.Append("V:=");
								Expression.Append(VariableName);
								Expression.Append("[1,];");
							}

							bool First = true;

							if ((P.Value & SeriesTypes.MinMax) == SeriesTypes.MinMax)
							{
								First = false;
								Expression.Append("MinMaxTP:=join(MinTP,reverse(MaxTP));");
								Expression.Append("MinMax:=join(Min,reverse(Max));");
								Expression.Append("polygon2d(MinMaxTP, MinMax, rgba(0, 0, 255, 32))");
							}
							else if ((P.Value & SeriesTypes.Minimum) != 0)
							{
								First = false;
								Expression.Append("plot2dline(MinTP, Min, \"Blue\")");
							}
							else if ((P.Value & SeriesTypes.Maximum) != 0)
							{
								First = false;
								Expression.Append("plot2dline(MaxTP, Max, \"Orange\")");
							}

							if ((P.Value & SeriesTypes.Average) != 0)
							{
								if (First)
									First = false;
								else
									Expression.Append('+');

								Expression.Append("plot2dline(AvgTP, Avg, \"Green\")");
							}

							if ((P.Value & SeriesTypes.Normal) != 0)
							{
								if (First)
									First = false;
								else
									Expression.Append('+');

								Expression.Append("plot2dline(TP, V, \"Red\")");
							}

							if (Exp is null)
								Exp = new List<KeyValuePair<string, string>>();

							s = Expression.ToString();
							Exp.Add(new KeyValuePair<string, string>(P.Key, s));
							Expression.Clear();
						}
					}
				}
			}

			return Exp?.ToArray();
		}

		[Flags]
		private enum SeriesTypes
		{
			Normal = 1,
			Minimum = 2,
			Maximum = 4,
			MinMax = 6,
			Average = 8
		}

		private static bool EndsWith(ref string s, string Suffix)
		{
			if (!s.EndsWith(Suffix, StringComparison.CurrentCultureIgnoreCase))
				return false;

			s = s.Substring(0, s.Length - Suffix.Length).TrimEnd();

			if (s.EndsWith(","))
				s = s.Substring(0, s.Length - 1).TrimEnd();

			return true;
		}

		private IElement FieldElement(Field Field)
		{
			if (Field is QuantityField Q)
			{
				if (string.IsNullOrEmpty(Q.Unit))
					return new DoubleNumber(Q.Value);

				if (Q.Unit == "%")
					return new PhysicalQuantity(Q.Value, new Script.Units.Unit("%"));

				try
				{
					Expression Exp = new Expression(Expression.ToString(Q.Value) + " " + Q.Unit);
					object Result = Exp.Evaluate(null);

					if (Result is PhysicalQuantity Q2)
						return Q2;
					else if (Result is double d)
						return new DoubleNumber(d);
				}
				catch (Exception)
				{
					// Ignore
				}

				return new StringValue(Q.ValueString);
			}

			if (Field is Int32Field I32)
				return new DoubleNumber(I32.Value);

			if (Field is Int64Field I64)
				return new DoubleNumber(I64.Value);

			if (Field is StringField S)
				return new StringValue(S.Value);

			if (Field is BooleanField B)
				return new BooleanValue(B.Value);

			if (Field is DateTimeField DT)
				return new DateTimeValue(DT.Value);

			if (Field is DateField D)
				return new DateTimeValue(D.Value);

			if (Field is DurationField DU)
				return new ObjectValue(DU.Value);

			if (Field is EnumField E)
				return new ObjectValue(E.Value);

			if (Field is TimeField T)
				return new ObjectValue(T.Value);

			return new StringValue(Field.ValueString);
		}

		private string PascalCasing(string Name)
		{
			StringBuilder Result = new StringBuilder();
			bool First = true;

			foreach (char ch in Name)
			{
				if (char.IsLetter(ch))
				{
					if (First)
					{
						First = false;
						Result.Append(char.ToUpper(ch));
					}
					else
						Result.Append(char.ToLower(ch));
				}
				else
					First = true;
			}

			return Result.ToString();
		}

		private void MomentaryFieldsRead(object Sender, InternalReadoutFieldsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			RemoteXmppSupport Support = (RemoteXmppSupport)P[3];
			StringBuilder sb = new StringBuilder();
			QuantityField QF;

			KeyValuePair<string, string>[] Exp = this.UpdateReadoutVariables(From, e, From, Field);

			foreach (Field F in e.Fields)
			{
				if (!string.IsNullOrEmpty(Field) && !F.Name.StartsWith(Field))
					continue;

				this.CheckMomentaryValuesHeader(P, sb, Support);

				QF = F as QuantityField;

				if (QF != null)
				{
					if (Support.Markdown)
					{
						sb.Append('|');
						sb.Append(MarkdownDocument.Encode(F.Name));
						sb.Append('|');
						sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
						sb.Append('|');
						sb.Append(MarkdownDocument.Encode(QF.Unit));
						sb.AppendLine("|");
					}
					else if (Support.Html)
					{
						sb.Append("<tr><td>");
						sb.Append(XML.Encode(F.Name));
						sb.Append("</td><td>");
						sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
						sb.Append("</td><td>");
						sb.Append(XML.Encode(QF.Unit));
						sb.AppendLine("</td></tr>");
					}
					else
					{
						sb.Append(F.Name);
						sb.Append('\t');
						sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
						sb.Append('\t');
						sb.AppendLine(QF.Unit);
					}
				}
				else
				{
					if (Support.Markdown)
					{
						sb.Append('|');
						sb.Append(MarkdownDocument.Encode(F.Name));
						sb.Append('|');
						sb.Append(MarkdownDocument.Encode(F.ValueString));
						sb.AppendLine("||");
					}
					else if (Support.Html)
					{
						sb.Append("<tr><td>");
						sb.Append(XML.Encode(F.Name));
						sb.Append("</td><td colspan=\"2\">");
						sb.Append(XML.Encode(F.ValueString));
						sb.AppendLine("</td></tr>");
					}
					else
					{
						sb.Append(F.Name);
						sb.Append('\t');
						sb.AppendLine(F.ValueString);
					}
				}


				if ((Support.Markdown || !Support.Html) && sb.Length > 3000)
				{
					this.SendMarkdownChatMessage(From, sb.ToString());
					sb.Clear();
				}
			}

			this.Send(From, sb, Support);
			this.SendExpressionResults(Exp, From, Support);

			if (e.Done)
				this.SendPlainTextChatMessage(From, "Readout complete.");

			// TODO: Localization
		}

		private void Send(string To, StringBuilder sb, RemoteXmppSupport Support)
		{
			if (sb.Length > 0)
			{
				if (Support.Markdown)
					this.SendMarkdownChatMessage(To, sb.ToString());
				else if (Support.Html)
				{
					sb.Append("</table>");
					this.SendHtmlChatMessage(To, sb.ToString(), null);
				}
				else
					this.SendPlainTextChatMessage(To, sb.ToString());
			}
		}

		private void SendExpressionResults(KeyValuePair<string, string>[] Exp, string From, RemoteXmppSupport Support)
		{
			if (Exp != null)
			{
				foreach (KeyValuePair<string, string> Expression in Exp)
				{
					if (Support.Markdown)
						this.SendMarkdownChatMessage(From, "## " + MarkdownDocument.Encode(Expression.Key));
					else if (Support.Html)
						this.SendHtmlChatMessage(From, "<h2>" + XML.Encode(Expression.Key) + "</h2>", Expression.Key);
					else
						this.SendPlainTextChatMessage(From, Expression.Key + ":");

					this.Execute(Expression.Value, From, Support);
				}
			}
		}

		private void CheckMomentaryValuesHeader(object[] P, StringBuilder sb, RemoteXmppSupport Support)
		{
			if ((bool)P[1])
			{
				P[1] = false;

				if (Support.Markdown)
				{
					sb.AppendLine("|Field|Value|Unit|");
					sb.AppendLine("|---|--:|---|");
				}
				else if (Support.Html)
					sb.AppendLine("<table><tr><th>Field</th><th>Value</th><th>Unit</th></tr>");
				else
					sb.AppendLine("Field\tValue\tUnit");
			}
		}

		private void MomentaryFieldsErrorsRead(object Sender, InternalReadoutErrorsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			RemoteXmppSupport Support = (RemoteXmppSupport)P[3];
			StringBuilder sb = new StringBuilder();

			foreach (ThingError Error in e.Errors)
			{
				this.CheckMomentaryValuesHeader(P, sb, Support);

				if (Support.Markdown)
				{
					sb.Append("|");
					sb.Append(MarkdownDocument.Encode(Error.ToString()));
					sb.AppendLine("|||");

					if (sb.Length > 3000)
					{
						this.SendMarkdownChatMessage(From, sb.ToString());
						sb.Clear();
					}
				}
				else if (Support.Html)
				{
					sb.Append("<tr><td colspan=\"3\">");
					sb.Append(XML.HtmlValueEncode(Error.ToString()));
					sb.AppendLine("</td></tr>");
				}
				else
				{
					sb.AppendLine(Error.ToString());

					if (sb.Length > 3000)
					{
						this.SendPlainTextChatMessage(From, sb.ToString());
						sb.Clear();
					}
				}
			}

			this.Send(From, sb, Support);

			if (e.Done)
				this.SendPlainTextChatMessage(From, "Readout complete.");
		}

		private void AllFieldsRead(object Sender, InternalReadoutFieldsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			RemoteXmppSupport Support = (RemoteXmppSupport)P[3];
			StringBuilder sb = new StringBuilder();
			QuantityField QF;
			DateTime TP;
			string s;

			KeyValuePair<string, string>[] Exp = this.UpdateReadoutVariables(From, e, From, Field);

			foreach (Field F in e.Fields)
			{
				if ((F.Type & FieldType.Historical) > 0)
					continue;

				if (!string.IsNullOrEmpty(Field) && !F.Name.StartsWith(Field))
					continue;

				this.CheckAllFieldsHeader(P, sb, Support);

				TP = F.Timestamp;

				if (Support.Markdown)
				{
					sb.Append('|');
					sb.Append(s = MarkdownDocument.Encode(F.Name));
					sb.Append('|');
					sb.Append(s);
					sb.Append('|');

					QF = F as QuantityField;

					if (QF != null)
					{
						sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
						sb.Append('|');
						sb.Append(MarkdownDocument.Encode(QF.Unit));
					}
					else
					{
						sb.Append(MarkdownDocument.Encode(F.ValueString));
						sb.Append('|');
					}

					sb.Append('|');
					sb.Append(TP.Year.ToString("D4"));
					sb.Append('-');
					sb.Append(TP.Month.ToString("D2"));
					sb.Append('-');
					sb.Append(TP.Day.ToString("D2"));
					sb.Append(' ');
					sb.Append(TP.Hour.ToString("D2"));
					sb.Append(':');
					sb.Append(TP.Minute.ToString("D2"));
					sb.Append(':');
					sb.Append(TP.Second.ToString("D2"));
					sb.Append('|');
					sb.Append(F.Type.ToString());
					sb.Append('|');
					sb.Append(F.QoS.ToString());
					sb.AppendLine("|");

					if (sb.Length > 3000)
					{
						this.SendMarkdownChatMessage(From, sb.ToString());
						sb.Clear();
					}
				}
				else if (Support.Html)
				{
					sb.Append("<tr><td>");
					sb.Append(s = XML.HtmlValueEncode(F.Name));
					sb.Append("</td><td>");
					sb.Append(s);

					QF = F as QuantityField;

					if (QF != null)
					{
						sb.Append("</td><td>");
						sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
						sb.Append("</td><td>");
						sb.Append(XML.HtmlValueEncode(QF.Unit));
					}
					else
					{
						sb.Append("</td><td colspan=\"2\">");
						sb.Append(XML.HtmlValueEncode(F.ValueString));
					}

					sb.Append("</td><td>");
					sb.Append(TP.Year.ToString("D4"));
					sb.Append('-');
					sb.Append(TP.Month.ToString("D2"));
					sb.Append('-');
					sb.Append(TP.Day.ToString("D2"));
					sb.Append(' ');
					sb.Append(TP.Hour.ToString("D2"));
					sb.Append(':');
					sb.Append(TP.Minute.ToString("D2"));
					sb.Append(':');
					sb.Append(TP.Second.ToString("D2"));
					sb.Append("</td><td>");
					sb.Append(F.Type.ToString());
					sb.Append("</td><td>");
					sb.Append(F.QoS.ToString());
					sb.AppendLine("</td></tr>");
				}
				else
				{
					sb.Append(s = F.Name);
					sb.Append('\t');
					sb.Append(s);
					sb.Append('\t');

					QF = F as QuantityField;

					if (QF != null)
					{
						sb.Append(CommonTypes.Encode(QF.Value, QF.NrDecimals));
						sb.Append('\t');
						sb.Append(MarkdownDocument.Encode(QF.Unit));
					}
					else
					{
						sb.Append(MarkdownDocument.Encode(F.ValueString));
						sb.Append("\t\t");
					}

					sb.Append('\t');
					sb.Append(TP.Year.ToString("D4"));
					sb.Append('-');
					sb.Append(TP.Month.ToString("D2"));
					sb.Append('-');
					sb.Append(TP.Day.ToString("D2"));
					sb.Append(' ');
					sb.Append(TP.Hour.ToString("D2"));
					sb.Append(':');
					sb.Append(TP.Minute.ToString("D2"));
					sb.Append(':');
					sb.Append(TP.Second.ToString("D2"));
					sb.Append('\t');
					sb.Append(F.Type.ToString());
					sb.Append('\t');
					sb.AppendLine(F.QoS.ToString());

					if (sb.Length > 3000)
					{
						this.SendPlainTextChatMessage(From, sb.ToString());
						sb.Clear();
					}
				}
			}

			this.Send(From, sb, Support);
			this.SendExpressionResults(Exp, From, Support);

			if (e.Done)
				this.SendPlainTextChatMessage(From, "Readout complete.");

			// TODO: Localization
		}

		private void CheckAllFieldsHeader(object[] P, StringBuilder sb, RemoteXmppSupport Support)
		{
			if ((bool)P[1])
			{
				P[1] = false;

				if (Support.Markdown)
				{
					sb.AppendLine("|Field|Localized|Value|Unit|Timestamp|Type|QoS|");
					sb.AppendLine("|---|---|--:|---|:-:|:-:|:-:|");
				}
				else if (Support.Html)
					sb.AppendLine("<table><tr><th>Field</th><th>Localized</th><th>Value</th><th>Unit</th><th>Timestamp</th><th>Type</th><th>QoS</th></tr>");
				else
					sb.AppendLine("Field\tLocalized\tValue\tUnit\tTimestamp\tType\tQoS");
			}
		}

		private void AllFieldsErrorsRead(object Sender, InternalReadoutErrorsEventArgs e)
		{
			object[] P = (object[])e.State;
			string From = (string)P[0];
			string Field = (string)P[2];
			RemoteXmppSupport Support = (RemoteXmppSupport)P[3];
			StringBuilder sb = new StringBuilder();

			foreach (ThingError Error in e.Errors)
			{
				this.CheckAllFieldsHeader(P, sb, Support);

				if (Support.Markdown)
				{
					sb.Append('|');
					sb.Append(MarkdownDocument.Encode(Error.ToString()));
					sb.AppendLine("|||||||");

					if (sb.Length > 3000)
					{
						this.SendMarkdownChatMessage(From, sb.ToString());
						sb.Clear();
					}
				}
				else if (Support.Html)
				{
					sb.Append("<tr><td colspan=\"7\">");
					sb.Append(XML.HtmlValueEncode(Error.ToString()));
					sb.AppendLine("</td></tr>");
				}
				else
				{
					sb.AppendLine(Error.ToString());

					if (sb.Length > 3000)
					{
						this.SendPlainTextChatMessage(From, sb.ToString());
						sb.Clear();
					}
				}
			}

			this.Send(From, sb, Support);

			if (e.Done)
				this.SendPlainTextChatMessage(From, "Readout complete.");
		}

		private void Que(string To, RemoteXmppSupport Support)
		{
			this.Error(To, "Sorry. Can't understand what you're trying to say. Type # to display the menu.", Support);
		}

		private void Error(string To, string ErrorMessage, RemoteXmppSupport Support)
		{
			if (Support.Markdown)
				this.SendMarkdownChatMessage(To, "**" + MarkdownDocument.Encode(ErrorMessage) + "**");
			else if (Support.Html)
				this.SendHtmlChatMessage(To, "<b>" + XML.HtmlValueEncode(ErrorMessage) + "</b>", ErrorMessage);
			else
				this.SendPlainTextChatMessage(To, ErrorMessage);
		}

		private void ShowHelp(string To, bool Extended, RemoteXmppSupport Support)
		{
			StringBuilder Output = new StringBuilder();

			if (Support.Markdown)
			{
				Output.AppendLine("|Command | Description|");
				Output.AppendLine("|---|---|");
				Output.AppendLine("|#|Displays the short version of the menu.|");
				Output.AppendLine("|##|Displays the extended version of the menu.|");

				if (this.concentratorServer != null)
				{
					Output.AppendLine("|/|Selects the root node in the current data source.|");
					Output.AppendLine("|//|Selects the root data source.|");
					Output.AppendLine("|NODE|Selects the node \"NODE\".|");
					Output.AppendLine("|NR|The number NR corresponds to the item in the menu last shown. The menu number can be used as a replacement for the corresponding string in all constructs.|");
				}

				if (this.sensorServer != null)
				{
					if (this.concentratorServer != null)
					{
						Output.AppendLine("|?|Reads non-historical values of the currently selected object.|");
						Output.AppendLine("|??|Performs a full readout of the currently selected object.|");
						Output.AppendLine("|FIELD?|Reads the non-historical fields that begin with \"FIELD\", of the currently selected object.|");
						Output.AppendLine("|FIELD??|Reads all values from fields that begin with \"FIELD\", of the currently selected object.|");
						Output.AppendLine("|NODE?|Reads the non-historical fields for the node \"NODE\".|");
						Output.AppendLine("|NODE??|Reads all values from fields for the node \"NODE\".|");
						Output.AppendLine("|NODE.FIELD?|Reads the non-historical fields that begin with \"FIELD\", of the node \"NODE\".|");
						Output.AppendLine("|NODE.FIELD??|Reads all values from fields that begin with \"FIELD\", of the node \"NODE\".|");
					}
					else
					{
						Output.AppendLine("|?|Reads non-historical values.|");
						Output.AppendLine("|??|Performs a full readout.|");
						Output.AppendLine("|FIELD?|Reads the non-historical fields that begin with \"FIELD\".|");
						Output.AppendLine("|FIELD??|Reads all values from fields that begin with \"FIELD\".|");
					}
				}

				if (this.controlServer != null)
				{
					Output.AppendLine("|:=|Lists available control parameters of the currently selected object.|");

					if (this.concentratorServer != null)
					{
						Output.AppendLine("|NODE:=|Lists available control parameters of the node \"NODE\".|");
						Output.AppendLine("|PARAMETER:=|Displays the current value of the control parameter \"PARAMETER\" of the currently selected object.|");
						Output.AppendLine("|PARAMETER:=VALUE|Sets the control parameter named \"PARAMETER\" of the currently selected object to the value VALUE.|");
						Output.AppendLine("|NODE.PARAMETER:=VALUE|Sets the control parameter named \"PARAMETER\" of the node \"NODE\" to the value VALUE.|");
					}
					else
						Output.AppendLine("|PARAMETER:=VALUE|Sets the control parameter named \"PARAMETER\" to the value VALUE.|");
				}

				Output.AppendLine("|=|Displays available variables in the session.|");
				Output.AppendLine("| |Anything else is assumed to be evaluated as a [mathematical expression](http://waher.se/Script.md)|");

				this.SendMarkdownChatMessage(To, Output.ToString());
			}
			else if (Support.Html)
			{
				Output.AppendLine("<table><tr><th>Command</th><th>Description</th></tr>");
				Output.AppendLine("<tr><td>#</td><td>Displays the short version of the menu.</td></tr>");
				Output.AppendLine("<tr><td>##</td><td>Displays the extended version of the menu.</td></tr>");

				if (this.concentratorServer != null)
				{
					Output.AppendLine("<tr><td>/</td><td>Selects the root node in the current data source.</td></tr>");
					Output.AppendLine("<tr><td>//</td><td>Selects the root data source.</td></tr>");
					Output.AppendLine("<tr><td>NODE</td><td>Selects the node \"NODE\"</td></tr>");
					Output.AppendLine("<tr><td>NR</td><td>The number NR corresponds to the item in the menu last shown.</td></tr>");
				}

				if (this.sensorServer != null)
				{
					if (this.concentratorServer != null)
					{
						Output.AppendLine("<tr><td>?</td><td>Reads non-historical values of the currently selected object.</td></tr>");
						Output.AppendLine("<tr><td>??</td><td>Performs a full readout of the currently selected object.</td></tr>");
						Output.AppendLine("<tr><td>FIELD?</td><td>Reads the non-historical fields that begin with \"FIELD\", of the currently selected object.</td></tr>");
						Output.AppendLine("<tr><td>FIELD??</td><td>Reads all values from fields that begin with \"FIELD\", of the currently selected object.</td></tr>");
						Output.AppendLine("<tr><td>NODE?</td><td>Reads the non-historical fields for the node \"NODE\".</td></tr>");
						Output.AppendLine("<tr><td>NODE??</td><td>Reads all values from fields for the node \"NODE\".</td></tr>");
						Output.AppendLine("<tr><td>FIELD?</td><td>Reads the non-historical fields that begin with \"FIELD\", for the node \"NODE\".</td></tr>");
						Output.AppendLine("<tr><td>FIELD??</td><td>Reads all values from fields that begin with \"FIELD\", for the node \"NODE\".</td></tr>");
					}
					else
					{
						Output.AppendLine("<tr><td>?</td><td>Reads non-historical values.</td></tr>");
						Output.AppendLine("<tr><td>??</td><td>Performs a full readout.</td></tr>");
						Output.AppendLine("<tr><td>FIELD?</td><td>Reads the non-historical fields that begin with \"FIELD\".</td></tr>");
						Output.AppendLine("<tr><td>FIELD??</td><td>Reads all values from fields that begin with \"FIELD\".</td></tr>");
					}
				}

				if (this.controlServer != null)
				{
					Output.AppendLine("<tr><td>:=</td><td>Lists available control parameters of the currently selected object.</td></tr>");

					if (this.concentratorServer != null)
					{
						Output.AppendLine("<tr><td>NODE:=</td><td>Lists available control parameters of the node \"NODE\".</td></tr>");
						Output.AppendLine("<tr><td>PARAMETER:=</td><td>Displays the current value of the control parameter \"PARAMETER\" of the currently selected object.</td></tr>");
						Output.AppendLine("<tr><td>PARAMETER:=VALUE</td><td>Sets the control parameter named \"PARAMETER\" of the currently selected object to the value VALUE.</td></tr>");
						Output.AppendLine("<tr><td>NODE.PARAMETER:=</td><td>Displays the current value of the control parameter \"PARAMETER\" of the node \"NODE\".</td></tr>");
						Output.AppendLine("<tr><td>NODE.PARAMETER:=VALUE</td><td>Sets the control parameter named \"PARAMETER\" of the node \"NODE\" to the value VALUE.</td></tr>");
					}
					else
						Output.AppendLine("<tr><td>PARAMETER:=VALUE</td><td>Sets the control parameter named \"PARAMETER\" to the value VALUE.</td></tr>");
				}

				Output.AppendLine("<tr><td>=</td><td>Displays available variables in the session.</td></tr>");
				Output.AppendLine("<tr><td> </td><td>Anything else is assumed to be evaluated as a [mathematical expression](http://waher.se/Script.md)</td></tr></table>");

				this.SendHtmlChatMessage(To, Output.ToString(), null);
			}
			else
			{
				Output.AppendLine("#\tDisplays the short version of the menu.");
				Output.AppendLine("##\tDisplays the extended version of the menu.");

				if (this.concentratorServer != null)
				{
					Output.AppendLine("/\tSelects the root node in the current data source.");
					Output.AppendLine("//\tSelects the root data source.");
					Output.AppendLine("NODE\tSelects the node \"NODE\".");
					Output.AppendLine("NR\tThe number NR corresponds to the item in the menu last shown.");
				}

				if (this.sensorServer != null)
				{
					if (this.concentratorServer != null)
					{
						Output.AppendLine("?\tReads non-historical values of the currently selected object.");
						Output.AppendLine("??\tPerforms a full readout of the currently selected object.");
						Output.AppendLine("FIELD?\tReads the non-historical fields that begin with \"FIELD\", of the currently selected object.");
						Output.AppendLine("FIELD??\tReads all values from fields that begin with \"FIELD\", of the currently selected object.");
						Output.AppendLine("NODE?\tReads the non-historical fields for the node \"NODE\".");
						Output.AppendLine("NODE??\tReads all values from fields for the node \"NODE\".");
						Output.AppendLine("NODE.FIELD?\tReads the non-historical fields that begin with \"FIELD\", of the node \"NODE\".");
						Output.AppendLine("NODE.FIELD??\tReads all values from fields that begin with \"FIELD\", of the node \"NODE\".");
					}
					else
					{
						Output.AppendLine("?\tReads non-historical values.");
						Output.AppendLine("??\tPerforms a full readout.");
						Output.AppendLine("FIELD?\tReads the non-historical fields that begin with \"FIELD\".");
						Output.AppendLine("FIELD??\tReads all values from fields that begin with \"FIELD\".");
					}
				}

				if (this.controlServer != null)
				{
					Output.AppendLine(":=\tLists available control parameters of the currently selected object.");

					if (this.concentratorServer != null)
					{
						Output.AppendLine("NODE:=\tLists available control parameters of the node \"NODE\".");
						Output.AppendLine("PARAMETER:=\tDisplays the current value of the control parameter \"PARAMETER\" of the currently selected object.");
						Output.AppendLine("PARAMETER:=VALUE\tSets the control parameter named \"PARAMETER\" of the currently selected object to the value VALUE.");
						Output.AppendLine("NODE.PARAMETER:=VALUE\tSets the control parameter named \"PARAMETER\" of the node \"NODE\" to the value VALUE.");
					}
					else
						Output.AppendLine("PARAMETER:=VALUE\tSets the control parameter named \"PARAMETER\" to the value VALUE.");
				}

				Output.AppendLine("=\tDisplays available variables in the session.");
				Output.AppendLine(" \tAnything else is assumed to be evaluated as a [mathematical expression](http://waher.se/Script.md)");

				this.SendPlainTextChatMessage(To, Output.ToString());
			}

			if (Extended)
			{
				Output.Clear();

				Output.Append("When reading the device, results will be available as pascal cased variables in the current session. You can use ");
				Output.Append("these to perform calculations. If a single field value is available for a specific field name, the corresponding ");
				Output.Append("variable will contain only the field value. If several values are available for a given field name, the corresponding");
				Output.Append("variable will contain a matrix with their corresponding contents. Use column indexing ");

				if (Support.Markdown)
					Output.Append("`Field[Col,]`");
				else if (Support.Html)
					Output.Append("<code>Field[Col,]</code>");
				else
					Output.Append("Field[Col,]");

				Output.AppendLine(" to access individual columns.");

				Output.AppendLine();
				Output.Append("Historical values with multiple numerical values will be shown in graph formats. ");
				Output.Append("You can control the graph size using the variables ");

				if (Support.Markdown)
				{
					Output.AppendLine("`GraphWidth` and `GraphHeight`.");
					this.SendMarkdownChatMessage(To, Output.ToString());
				}
				else if (Support.Html)
				{
					Output.AppendLine("<code>GraphWidth</code> and <code>GraphHeight</code>.");
					this.SendHtmlChatMessage(To, Output.ToString(), null);
				}
				else
				{
					Output.AppendLine("GraphWidth and GraphHeight.");
					this.SendPlainTextChatMessage(To, Output.ToString());
				}
			}
		}

		// TODO: Configuration
		// TODO: Node Commands.
		// TODO: Browsing data sources.
		// TODO: User authentication
		// TODO: Localization
	}
}
