using System;
using SkiaSharp;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP.BitsOfBinary;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.HttpFileUpload;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Cache;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Graphs;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators.Vectors;
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
	public class ChatServer : IDisposable
	{
		private Cache<string, Variables> sessions;
		private SensorServer sensorServer;
		private ControlServer controlServer;
		private XmppClient client;
		private BobClient bobClient;
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
			: this(Client, BobClient, SensorServer, null)
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
			: this(Client, BobClient, null, ControlServer)
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
		{
			this.client = Client;
			this.bobClient = BobClient;
			this.sensorServer = SensorServer;
			this.controlServer = ControlServer;

			this.client.OnChatMessage += new MessageEventHandler(Client_OnChatMessage);

			this.client.RegisterFeature("urn:xmpp:iot:chat");
			this.client.SetPresence(Availability.Chat);

			this.sessions = new Cache<string, Variables>(1000, TimeSpan.MaxValue, new TimeSpan(0, 20, 0));
			this.sessions.Removed += Sessions_Removed;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterFeature("urn:xmpp:iot:chat");
		}

		private enum ContentType
		{
			Markdown,
			Html,
			PlainText
		}

		private void SendChatMessage(string To, string Message, ContentType Type)
		{
			switch (Type)
			{
				case ContentType.Markdown:
					MarkdownDocument MarkdownDocument = new MarkdownDocument(Message, new MarkdownSettings(null, false));
					string PlainText = MarkdownDocument.GeneratePlainText();

					this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
						"<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">" + XML.Encode(Message) + "</content>",
						PlainText, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
					break;

				case ContentType.Html:
					MarkdownDocument = new MarkdownDocument(Message, new MarkdownSettings(null, false));
					PlainText = MarkdownDocument.GeneratePlainText();

					this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
						"<html xmlns='http://jabber.org/protocol/xhtml-im'><body xmlns='http://www.w3.org/1999/xhtml'>" + Message + "</body></html>",
						PlainText, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
					break;

				case ContentType.PlainText:
				default:
					this.client.SendChatMessage(To, Message);
					break;
			}
		}

		private void Client_OnChatMessage(object Sender, MessageEventArgs e)
		{
			Variables Variables = this.GetVariables(e.From);
			RemoteXmppSupport Support = null;

			if (this.httpUpload == null)
			{
				this.httpUpload = new HttpFileUploadClient(this.client);
				this.httpUpload.Discover(null);
			}

			if (!Variables.TryGetVariable(" Support ", out Variable v) || (Support = v.ValueObject as RemoteXmppSupport) == null)
			{
				Variables[" Support "] = null;

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
						BitsOfBinary = e2.Features.ContainsKey("urn:xmpp:bob")
					};

					Variables2[" Support "] = Support2;
				}, Variables);
			}

			if (Support == null)
				Support = new RemoteXmppSupport();

			string s = e.Body;
			if (e.Content != null && e.Content.LocalName == "content" && e.Content.NamespaceURI == "urn:xmpp:content" &&
				XML.Attribute(e.Content, "type") == "text/markdown")
			{
				string s2 = e.Content.InnerText;

				if (!string.IsNullOrEmpty(s2))
					s = s2;

				Support.Markdown |= true;
			}

			if (s == null || string.IsNullOrEmpty(s = s.Trim()))
				return;

			switch (s.ToLower())
			{
				case "hi":
				case "hello":
				case "hej":
				case "hallo":
				case "hola":
					this.SendChatMessage(e.From, "Hello. Type # to display the menu.", ContentType.PlainText);
					break;

				case "#":
					this.ShowMenu(e.From, false, Support);
					break;

				case "##":
					this.ShowMenu(e.From, true, Support);
					break;

				case "?":
					this.InitReadout(e.From);
					this.SendChatMessage(e.From, "Readout started...", ContentType.PlainText);
					this.sensorServer.DoInternalReadout(e.From, null, FieldType.AllExceptHistorical, null, DateTime.MinValue, DateTime.MaxValue,
						this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, new object[] { e.From, true, null, Support });
					break;

				case "??":
					this.SendChatMessage(e.From, "Readout started...", ContentType.PlainText);
					this.InitReadout(e.From);
					this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
						this.AllFieldsRead, this.AllFieldsErrorsRead, new object[] { e.From, true, null, Support });
					break;

				case "=":
					if (Support.Markdown)
					{
						StringBuilder Markdown = new StringBuilder();

						Markdown.AppendLine("|Variable|Value|");
						Markdown.AppendLine("|:-------|:---:|");

						foreach (Variable v2 in Variables)
						{
							string s2 = v2.ValueElement.ToString();
							if (s2.Length > 100)
								s2 = s2.Substring(0, 100) + "...";

							s2 = s2.Replace("|", "&#124;").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");

							Markdown.Append('|');
							Markdown.Append(v.Name);
							Markdown.Append('|');
							Markdown.Append(s2);
							Markdown.AppendLine("|");

							if (Markdown.Length > 3000)
							{
								this.SendChatMessage(e.From, Markdown.ToString(), ContentType.Markdown);
								Markdown.Clear();
							}
						}

						if (Markdown.Length > 0)
							this.SendChatMessage(e.From, Markdown.ToString(), ContentType.Markdown);
					}
					else if (Support.Html)
					{
						StringBuilder Html = new StringBuilder();

						Html.AppendLine("<table><tr><th>Variable</th><th>Value</th></tr>");

						foreach (Variable v2 in Variables)
						{
							string s2 = v2.ValueElement.ToString();
							if (s2.Length > 100)
								s2 = s2.Substring(0, 100) + "...";

							s2 = XML.HtmlValueEncode(s2).Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");

							Html.Append("<tr><td>");
							Html.Append(v.Name);
							Html.Append("</td><td>");
							Html.Append(s2);
							Html.AppendLine("</td></tr>");
						}

						Html.Append("</table>");

						this.SendChatMessage(e.From, Html.ToString(), ContentType.Html);
					}
					else
					{
						StringBuilder Text = new StringBuilder();

						Text.AppendLine("Variable\tValue");

						foreach (Variable v2 in Variables)
						{
							string s2 = v2.ValueElement.ToString();
							if (s2.Length > 100)
								s2 = s2.Substring(0, 100) + "...";

							Text.Append(v.Name);
							Text.Append('\t');
							Text.AppendLine(s2);

							if (Text.Length > 3000)
							{
								this.SendChatMessage(e.From, Text.ToString(), ContentType.PlainText);
								Text.Clear();
							}
						}

						if (Text.Length > 0)
							this.SendChatMessage(e.From, Text.ToString(), ContentType.PlainText);
					}
					break;

				default:
					if (s.EndsWith("??"))
					{
						this.InitReadout(e.From);
						string Field = s.Substring(0, s.Length - 2).Trim();
						this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...", ContentType.PlainText);
						this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
							this.AllFieldsRead, this.AllFieldsErrorsRead, new object[] { e.From, true, Field, Support });
					}
					else if (s.EndsWith("?"))
					{
						this.InitReadout(e.From);
						string Field = s.Substring(0, s.Length - 1).Trim();
						this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...", ContentType.PlainText);
						this.sensorServer.DoInternalReadout(e.From, null, FieldType.AllExceptHistorical, null, DateTime.MinValue, DateTime.MaxValue,
							this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, new object[] { e.From, true, Field, Support });
					}
					else
					{
						int i;

						if (this.controlServer != null && (i = s.IndexOf(":=")) > 0)
						{
							string ParameterName = s.Substring(0, i).Trim();
							string ValueStr = s.Substring(i + 2).Trim();
							ThingReference Ref;

							i = ParameterName.IndexOf('.');
							if (i < 0)
								Ref = null;
							else
							{
								Ref = new ThingReference(ParameterName.Substring(0, i), string.Empty, string.Empty);
								ParameterName = ParameterName.Substring(i + 1).TrimStart();
							}

							try
							{
								ControlParameter[] Parameters = this.controlServer.GetControlParameters(Ref);
								foreach (ControlParameter P in Parameters)
								{
									if (string.Compare(P.Name, ParameterName, true) != 0)
										continue;

									if (!P.SetStringValue(Ref, ValueStr))
										throw new Exception("Unable to set control parameter value.");

									this.SendChatMessage(e.From, "Control parameter set.", ContentType.PlainText);

									return;
								}
							}
							catch (Exception ex)
							{
								this.Error(e.From, ex.Message, Support);
							}
						}

						this.Execute(s, e.From, Support);
					}
					break;
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
			ContentType Type;

			Variables.Lock();
			Variables.ConsoleOut = new StringWriter(sb);
			try
			{
				IElement Result = Exp.Root.Evaluate(Variables);
				Variables["Ans"] = Result;

				Graph G = Result as Graph;
				SKImage Img;

				if (G != null)
				{
					GraphSettings Settings = new GraphSettings();
					object Obj;
					double d;

					Settings.Width = 600;
					Settings.Height = 300;

					if (Variables.TryGetVariable("GraphWidth", out Variable v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
						Settings.Width = (int)Math.Round(d);
					else
					{
						d = (double)Settings.Width;
						if (!Variables.ContainsVariable("GraphWidth"))
							Variables["GraphWidth"] = d;
					}

					Settings.MarginLeft = (int)Math.Round(15 * d / 640);
					Settings.MarginRight = Settings.MarginLeft;

					if (Variables.TryGetVariable("GraphHeight", out v) && (Obj = v.ValueObject) is double && (d = (double)Obj) >= 1)
						Settings.Height = (int)Math.Round(d);
					else
					{
						d = Settings.Height;
						if (!Variables.ContainsVariable("GraphHeight"))
							Variables["GraphHeight"] = d;
					}

					Settings.MarginTop = (int)Math.Round(15 * d / 480);
					Settings.MarginBottom = Settings.MarginTop;
					Settings.LabelFontSize = 12 * d / 480;

					using (SKImage Bmp = G.CreateBitmap(Settings))
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
				else
				{
					s = Result.ToString();
					Type = ContentType.PlainText;
				}

				this.SendChatMessage(From, s, Type);
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

		private void ImageResult(string To, SKImage Bmp, RemoteXmppSupport Support, Variables Variables, bool AllowHttpUpload)
		{
			SKData Data = Bmp.Encode(SKEncodedImageFormat.Png, 100);
			byte[] Bin = Data.ToArray();
			ContentType Type;
			string s;

			Data.Dispose();

			if (Support.Markdown)
			{
				s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
				s = "![Image result](data:image/png;base64," + s + ")";
				Type = ContentType.Markdown;
			}
			else if (Support.Html)
			{
				if (AllowHttpUpload && this.httpUpload != null && this.httpUpload.HasSupport)
				{
					string FileName = Guid.NewGuid().ToString().Replace("-", string.Empty) + ".png";
					string ContentType = "image/png";

					this.httpUpload.RequestUploadSlot(FileName, ContentType, Bin, async (sender, e) =>
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
										ImageResult(To, Bmp, Support, Variables, false);
									else
									{
										s = "<img alt=\"Image result\" src=\"" + e.GetUrl + "\"/>";
										this.SendChatMessage(To, s, ChatServer.ContentType.Html);
									}
								}
							}
							else
								ImageResult(To, Bmp, Support, Variables, false);
						}
						catch (Exception)
						{
							ImageResult(To, Bmp, Support, Variables, false);
						}

					}, null);

					return;
				}
				else if (this.bobClient != null && Support.BitsOfBinary)
				{
					s = this.bobClient.StoreData(Bin, "image/png");

					Dictionary<string, bool> ContentIDs;

					if (!Variables.TryGetVariable(" ContentIDs ", out Variable v) ||
						(ContentIDs = v.ValueObject as Dictionary<string, bool>) == null)
					{
						ContentIDs = new Dictionary<string, bool>();
						Variables[" ContentIDs "] = ContentIDs;
					}

					lock (ContentIDs)
					{
						ContentIDs[s] = true;
					}

					s = "<img alt=\"Image result\" src=\"cid:" + s + "\"/>";
				}
				else
				{
					s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
					s = "<img alt=\"Image result\" src=\"data:image/png;base64," + s + "\"/>";
				}

				Type = ContentType.Html;
			}
			else
			{
				s = "Image result";
				Type = ContentType.PlainText;
			}

			if (s != null)
				this.SendChatMessage(To, s, Type);
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
				Variables = new Variables();
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

								if (Series == null)
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

							if (Exp == null)
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
					this.SendChatMessage(From, sb.ToString(), ContentType.Markdown);
					sb.Clear();
				}
			}

			this.Send(From, sb, Support);
			this.SendExpressionResults(Exp, From, Support);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", ContentType.PlainText);

			// TODO: Localization
		}

		private void Send(string To, StringBuilder sb, RemoteXmppSupport Support)
		{
			if (sb.Length > 0)
			{
				if (Support.Markdown)
					this.SendChatMessage(To, sb.ToString(), ContentType.Markdown);
				else if (Support.Html)
				{
					sb.Append("</table>");
					this.SendChatMessage(To, sb.ToString(), ContentType.Html);
				}
				else
					this.SendChatMessage(To, sb.ToString(), ContentType.PlainText);
			}
		}

		private void SendExpressionResults(KeyValuePair<string, string>[] Exp, string From, RemoteXmppSupport Support)
		{
			if (Exp != null)
			{
				foreach (KeyValuePair<string, string> Expression in Exp)
				{
					if (Support.Markdown)
						this.SendChatMessage(From, "## " + MarkdownDocument.Encode(Expression.Key), ContentType.Markdown);
					else if (Support.Html)
						this.SendChatMessage(From, "<h2>" + XML.Encode(Expression.Key) + "</h2>", ContentType.Html);
					else
						this.SendChatMessage(From, Expression.Key + ":", ContentType.Html);

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
						this.SendChatMessage(From, sb.ToString(), ContentType.Markdown);
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
						this.SendChatMessage(From, sb.ToString(), ContentType.PlainText);
						sb.Clear();
					}
				}
			}

			this.Send(From, sb, Support);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", ContentType.PlainText);
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
						this.SendChatMessage(From, sb.ToString(), ContentType.Markdown);
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
						this.SendChatMessage(From, sb.ToString(), ContentType.PlainText);
						sb.Clear();
					}
				}
			}

			this.Send(From, sb, Support);
			this.SendExpressionResults(Exp, From, Support);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", ContentType.PlainText);

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
						this.SendChatMessage(From, sb.ToString(), ContentType.Markdown);
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
						this.SendChatMessage(From, sb.ToString(), ContentType.PlainText);
						sb.Clear();
					}
				}
			}

			this.Send(From, sb, Support);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", ContentType.PlainText);
		}

		private void Que(string To, RemoteXmppSupport Support)
		{
			this.Error(To, "Sorry. Can't understand what you're trying to say. Type # to display the menu.", Support);
		}

		private void Error(string To, string ErrorMessage, RemoteXmppSupport Support)
		{
			if (Support.Markdown)
				this.SendChatMessage(To, "**" + MarkdownDocument.Encode(ErrorMessage) + "**", ContentType.Markdown);
			else if (Support.Html)
				this.SendChatMessage(To, "<b>" + XML.HtmlValueEncode(ErrorMessage) + "</b>", ContentType.Html);
			else
				this.SendChatMessage(To, ErrorMessage, ContentType.PlainText);
		}

		private void ShowMenu(string To, bool Extended, RemoteXmppSupport Support)
		{
			StringBuilder Output = new StringBuilder();

			if (Support.Markdown)
			{
				Output.AppendLine("|Command | Description");
				Output.AppendLine("|---|---");
				Output.AppendLine("|#|Displays the short version of the menu.");
				Output.AppendLine("|##|Displays the extended version of the menu.");

				if (this.sensorServer != null)
				{
					Output.AppendLine("|?|Reads non-historical values of the currently selected object.");
					Output.AppendLine("|??|Performs a full readout of the currently selected object.");
					Output.AppendLine("|FIELD?|Reads the non-historical fields that begin with \"FIELD\", of the currently selected object.");
					Output.AppendLine("|FIELD??|Reads all values from fields that begin with \"FIELD\", of the currently selected object.");
				}

				if (this.controlServer != null)
					Output.AppendLine("|PARAMETER:=VALUE|Sets the control parameter named \"PARAMETER\" to the value VALUE.");

				Output.AppendLine("|=|Displays available variables in the session.");
				Output.AppendLine("| |Anything else is assumed to be evaluated as a [mathematical expression](http://waher.se/Script.md)");

				this.SendChatMessage(To, Output.ToString(), ContentType.Markdown);
			}
			else if (Support.Html)
			{
				Output.AppendLine("<table><tr><th>Command</th><th>Description</th></tr>");
				Output.AppendLine("<tr><td>#</td><td>Displays the short version of the menu.</td></tr>");
				Output.AppendLine("<tr><td>##</td><td>Displays the extended version of the menu.</td></tr>");

				if (this.sensorServer != null)
				{
					Output.AppendLine("<tr><td>?</td><td>Reads non-historical values of the currently selected object.</td></tr>");
					Output.AppendLine("<tr><td>??</td><td>Performs a full readout of the currently selected object.</td></tr>");
					Output.AppendLine("<tr><td>FIELD?</td><td>Reads the non-historical fields that begin with \"FIELD\", of the currently selected object.</td></tr>");
					Output.AppendLine("<tr><td>FIELD??</td><td>Reads all values from fields that begin with \"FIELD\", of the currently selected object.</td></tr>");
				}

				if (this.controlServer != null)
					Output.AppendLine("<tr><td>PARAMETER:=VALUE|Sets the control parameter named \"PARAMETER\" to the value VALUE.</td></tr>");

				Output.AppendLine("<tr><td>=</td><td>Displays available variables in the session.</td></tr>");
				Output.AppendLine("<tr><td> </td><td>Anything else is assumed to be evaluated as a [mathematical expression](http://waher.se/Script.md)</td></tr></table>");

				this.SendChatMessage(To, Output.ToString(), ContentType.Html);
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
					this.SendChatMessage(To, Output.ToString(), ContentType.Markdown);
				}
				else if (Support.Html)
				{
					Output.AppendLine("<code>GraphWidth</code> and <code>GraphHeight</code>.");
					this.SendChatMessage(To, Output.ToString(), ContentType.Html);
				}
				else
				{
					Output.AppendLine("GraphWidth and GraphHeight.");
					this.SendChatMessage(To, Output.ToString(), ContentType.PlainText);
				}
			}
		}

		// TODO: Support for concentrator.
		// TODO: Control
		// TODO: Configuration
		// TODO: Provisioning
		// TODO: Node Commands.
		// TODO: Browsing data sources.
		// TODO: User authentication
		// TODO: Localization
	}
}
