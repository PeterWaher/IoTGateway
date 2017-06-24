using System;
using SkiaSharp;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Control;
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
		private Cache<string, Variables> sessions = new Cache<string, Variables>(1000, TimeSpan.MaxValue, new TimeSpan(0, 20, 0));
		private SensorServer sensorServer;
		private ControlServer controlServer;
		private XmppClient client;

		/// <summary>
		/// Class managing a chat interface for things.
		/// 
		/// The chat interface is defined in:
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.xml
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		public ChatServer(XmppClient Client, SensorServer SensorServer)
			: this(Client, SensorServer, null)
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
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		public ChatServer(XmppClient Client, ControlServer ControlServer)
			: this(Client, null, ControlServer)
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
		/// <param name="SensorServer">Sensor Server. Can be null, if not supporting a sensor interface.</param>
		/// <param name="ControlServer">Control Server. Can be null, if not supporting a control interface.</param>
		public ChatServer(XmppClient Client, SensorServer SensorServer, ControlServer ControlServer)
		{
			this.client = Client;
			this.sensorServer = SensorServer;
			this.controlServer = ControlServer;

			this.client.OnChatMessage += new MessageEventHandler(Client_OnChatMessage);

			this.client.RegisterFeature("urn:xmpp:iot:chat");
			this.client.SetPresence(Availability.Chat);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterFeature("urn:xmpp:iot:chat");
		}

		private void SendChatMessage(string To, string Message, bool Markdown)
		{
			if (Markdown)
			{
				MarkdownDocument MarkdownDocument = new MarkdownDocument(Message, new MarkdownSettings(null, false));
				string PlainText = MarkdownDocument.GeneratePlainText();

				this.client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, To,
					"<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">" + XML.Encode(Message) + "</content>",
					PlainText, string.Empty, string.Empty, string.Empty, string.Empty, null, null);
			}
			else
				this.client.SendChatMessage(To, Message);
		}

		private void Client_OnChatMessage(object Sender, MessageEventArgs e)
		{
			string s = e.Body;
			if (e.Content != null && e.Content.LocalName == "content" && e.Content.NamespaceURI == "urn:xmpp:content" &&
				XML.Attribute(e.Content, "type") == "text/markdown")
			{
				string s2 = e.Content.InnerText;

				if (!string.IsNullOrEmpty(s2))
					s = s2;
			}

			if (s == null || string.IsNullOrEmpty(s = s.Trim()))
				return;

			if (s == "??")
			{
				this.InitReadout(e.From);
				this.SendChatMessage(e.From, "Readout started...\r\n\r\n|Field|Localized|Value|Unit|Timestamp|Type|QoS|\r\n|---|---|--:|---|:-:|:-:|:-:|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
					this.AllFieldsRead, this.AllFieldsErrorsRead, e.From);
			}
			else if (s == "?")
			{
				this.InitReadout(e.From);
				this.SendChatMessage(e.From, "Readout started...\r\n\r\n|Field|Value|Unit|\r\n|---|--:|---|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.AllExceptHistorical, null, DateTime.MinValue, DateTime.MaxValue,
					this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, e.From);
			}
			else if (s.EndsWith("??"))
			{
				this.InitReadout(e.From);
				string Field = s.Substring(0, s.Length - 2).Trim();
				this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...\r\n\r\n|Field|Localized|Value|Unit|Timestamp|Type|QoS|\r\n|---|---|--:|---|:-:|:-:|:-:|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, new string[] { Field }, DateTime.MinValue, DateTime.MaxValue,
					this.AllFieldsRead, this.AllFieldsErrorsRead, e.From);
			}
			else if (s.EndsWith("?"))
			{
				this.InitReadout(e.From);
				string Field = s.Substring(0, s.Length - 1).Trim();
				this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...\r\n\r\n|Field|Value|Unit|\r\n|---|--:|---|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.AllExceptHistorical, new string[] { Field }, DateTime.MinValue, DateTime.MaxValue,
					this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, e.From);
			}
			else if (s == "#")
				this.ShowMenu(e.From, false);
			else if (s == "##")
				this.ShowMenu(e.From, true);
			else if (s == "=")
			{
				StringBuilder Markdown = new StringBuilder();
				Variables Variables = this.GetVariables(e.From);

				Markdown.AppendLine("|Variable|Value|");
				Markdown.AppendLine("|:-------|:---:|");

				foreach (Variable v in Variables)
				{
					Markdown.Append('|');
					Markdown.Append(v.Name);
					Markdown.Append('|');
					Markdown.Append(v.ValueElement.ToString().Replace("|", "&#124;"));
					Markdown.AppendLine("|");
				}

				this.SendChatMessage(e.From, Markdown.ToString(), true);
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

							this.SendChatMessage(e.From, "Control parameter set.", false);

							return;
						}
					}
					catch (Exception ex)
					{
						this.Error(e.From, ex.Message);
					}
				}

				this.Execute(s, e.From);
			}
		}

		private void Execute(string s, string From)
		{
			Expression Exp;

			try
			{
				Exp = new Expression(s);
			}
			catch (Exception)
			{
				this.Que(From);
				return;
			}

			Variables Variables = this.GetVariables(From);
			TextWriter Bak = Variables.ConsoleOut;
			StringBuilder sb = new StringBuilder();
			bool Markdown;

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
						using (SKSurface Surface = SKSurface.Create(Bmp.Width, Bmp.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
						{
							SKCanvas Canvas = Surface.Canvas;
							SKPaint Border = new SKPaint()
							{
								IsAntialias = true,
								Style = SKPaintStyle.Stroke,
								StrokeWidth = 2,
								Color = SKColors.Black
							};

							Canvas.DrawImage(Bmp, 0, 0);
							Canvas.DrawRect(new SKRect(0, 0, Bmp.Width, Bmp.Height), Border);
							Border.Dispose();

							SKImage Img2 = Surface.Snapshot();
							SKData Data2 = Img2.Encode(SKEncodedImageFormat.Png, 100);
							byte[] Bin = Data2.ToArray();

							s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
							s = "![" + Result.ToString() + "](data:image/png;base64," + s + ")";
							Markdown = true;

							Data2.Dispose();
							Img2.Dispose();
						}
					}
				}
				else if ((Img = Result.AssociatedObjectValue as SKImage) != null)
				{
					SKData Data2 = Img.Encode(SKEncodedImageFormat.Png, 100);
					byte[] Bin = Data2.ToArray();

					s = System.Convert.ToBase64String(Bin, 0, Bin.Length);
					s = "![" + Result.ToString() + "](data:image/png;base64," + s + ")";
					Markdown = true;
				}
				else
				{
					s = Result.ToString();
					Markdown = false;
				}

				this.SendChatMessage(From, s, Markdown);
			}
			catch (Exception ex)
			{
				this.Error(From, ex.Message);
			}
			finally
			{
				Variables.ConsoleOut.Flush();
				Variables.ConsoleOut = Bak;
				Variables.Release();
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
				Variables = new Variables();
				this.sessions.Add(Address, Variables);
			}

			return Variables;
		}

		private string UpdateReadoutVariables(string Address, InternalReadoutFieldsEventArgs e, string From)
		{
			Variables Variables = this.GetVariables(Address);
			Dictionary<string, SortedDictionary<DateTime, Field>> Fields;
			string Exp = null;

			if (Variables.TryGetVariable(" Readout ", out Variable v) &&
				(Fields = v.ValueObject as Dictionary<string, SortedDictionary<DateTime, Field>>) != null)
			{
				foreach (Field Field in e.Fields)
				{
					if (!Fields.TryGetValue(Field.Name, out SortedDictionary<DateTime, Field> Times))
					{
						Times = new SortedDictionary<DateTime, Field>();
						Fields[Field.Name] = Times;
					}

					Times[Field.Timestamp] = Field;
				}

				if (e.Done)
				{
					Variables.Remove(" Readout ");

					foreach (KeyValuePair<string, SortedDictionary<DateTime, Field>> P in Fields)
					{
						if (P.Value.Count == 1)
						{
							foreach (Field Field in P.Value.Values)
								Variables[this.PascalCasing(P.Key)] = this.FieldElement(Field);
						}
						else
						{
							List<ObjectVector> Values = new List<ObjectVector>();
							IElement E;
							string s;
							bool Numeric = true;

							foreach (KeyValuePair<DateTime, Field> P2 in P.Value)
							{
								E = this.FieldElement(P2.Value);
								Values.Add(new ObjectVector(new DateTimeValue(P2.Key),
									Expression.Encapsulate(E),
									new ObjectValue(P2.Value.Type), new ObjectValue(P2.Value.QoS)));

								if (!(E is DoubleNumber) && !(E is PhysicalQuantity))
									Numeric = false;
							}

							Variables[s = this.PascalCasing(P.Key)] = VectorDefinition.Encapsulate(Values.ToArray(), true, null);

							if (Fields.Count == 1 && Numeric)
								Exp = "plot2dline(" + s + "[0,], " + s + "[1,])";
						}
					}
				}
			}

			return Exp;
		}

		private IElement FieldElement(Field Field)
		{
			if (Field is QuantityField Q)
			{
				if (string.IsNullOrEmpty(Q.Unit))
					return new DoubleNumber(Q.Value);

				try
				{
					Expression Exp = new Expression("1 " + Q.Unit);
					if (Exp.Evaluate(null) is PhysicalQuantity Q2)
						return new PhysicalQuantity(Q.Value, Q2.Unit);
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
			string From = (string)e.State;
			QuantityField QF;

			string Exp = this.UpdateReadoutVariables(From, e, From);

			foreach (Field F in e.Fields)
			{
				QF = F as QuantityField;

				if (QF != null)
				{
					this.SendChatMessage(From, "|" + MarkdownDocument.Encode(F.Name) + "|" + CommonTypes.Encode(QF.Value, QF.NrDecimals) + "|" +
						MarkdownDocument.Encode(QF.Unit) + "|", true);
				}
				else
					this.SendChatMessage(From, "|" + MarkdownDocument.Encode(F.Name) + "|" + MarkdownDocument.Encode(F.ValueString) + "||", true);
			}

			if (!string.IsNullOrEmpty(Exp))
				this.Execute(Exp, From);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);

			// TODO: Localization
		}

		private void MomentaryFieldsErrorsRead(object Sender, InternalReadoutErrorsEventArgs e)
		{
			string From = (string)e.State;

			foreach (ThingError Error in e.Errors)
				this.SendChatMessage(From, "|" + MarkdownDocument.Encode(Error.ToString()) + "|||", true);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);
		}

		private void AllFieldsRead(object Sender, InternalReadoutFieldsEventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			string From = (string)e.State;
			string s;
			QuantityField QF;
			DateTime TP;

			string Exp = this.UpdateReadoutVariables(From, e, From);

			foreach (Field F in e.Fields)
			{
				TP = F.Timestamp;

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
				sb.Append('|');

				this.SendChatMessage(From, sb.ToString(), true);
				sb.Clear();
			}

			if (!string.IsNullOrEmpty(Exp))
				this.Execute(Exp, From);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);

			// TODO: Localization
		}

		private void AllFieldsErrorsRead(object Sender, InternalReadoutErrorsEventArgs e)
		{
			string From = (string)e.State;

			foreach (ThingError Error in e.Errors)
				this.SendChatMessage(From, "|" + MarkdownDocument.Encode(Error.ToString()) + "|||||||", true);

			if (e.Done)
				this.SendChatMessage(From, "Readout complete.", false);
		}

		private void Que(string To)
		{
			this.Error(To, "Sorry. Can't understand what you're trying to say. Type # to display the menu.");
		}

		private void Error(string To, string ErrorMessage)
		{
			this.SendChatMessage(To, "**" + MarkdownDocument.Encode(ErrorMessage) + "**", true);
		}

		private void ShowMenu(string To, bool Extended)
		{
			StringBuilder Output = new StringBuilder();

			Output.AppendLine("|Command | Description");
			Output.AppendLine("|---|---");
			Output.AppendLine("|#|Displays the short version of the menu.");
			Output.AppendLine("|##|Displays the extended version of the menu.");

			if (this.sensorServer != null)
			{
				Output.AppendLine("|?|Reads non-historical values of the currently selected object.");
				Output.AppendLine("|??|Performs a full readout of the currently selected object.");
				Output.AppendLine("|FIELD?|Reads the non-historical field \"FIELD\" of the currently selected object.");
				Output.AppendLine("|FIELD??|Reads all values from the field \"FIELD\" of the currently selected object.");
			}

			if (this.controlServer != null)
				Output.AppendLine("|PARAMETER:=VALUE|Sets the control parameter named \"PARAMETER\" to the value VALUE.");

			Output.AppendLine("|=|Displays available variables in the session.");
			Output.AppendLine("| |Anything else is assumed to be evaluated as a [mathematical expression](https://github.com/PeterWaher/IoTGateway/tree/master/Script/Waher.Script#script-syntax)");

			this.SendChatMessage(To, Output.ToString(), true);

			if (Extended)
			{
				Output.Clear();

				Output.AppendLine("When reading the device, results will be available as pascal cased variables in the current session. You can use ");
				Output.AppendLine("these to perform calculations. If a single field value is available for a specific field name, the corresponding ");
				Output.AppendLine("variable will contain only the field value. If several values are available for a given field name, the corresponding");
				Output.AppendLine("variable will contain a matrix with their corresponding contents. Use column indexing `Field[Col,]` to access ");
				Output.AppendLine("individual columns.");

				Output.AppendLine();
				Output.AppendLine("If reading all values from a single field, using the `FIELD??` syntax, and multiple numerical values are returned, ");
				Output.AppendLine("a graph will be returned, corresponding to the script `plot2dline(FIELD[0,],FIELD[1,])`. You can control the graph ");
				Output.AppendLine("size using the variables `GraphWidth` and `GraphHeight`.");

				this.SendChatMessage(To, Output.ToString(), true);
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
