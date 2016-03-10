using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Markdown;
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
		{
			this.client = Client;
			this.sensorServer = SensorServer;

			this.client.OnChatMessage += new MessageEventHandler(client_OnChatMessage);

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

		private void client_OnChatMessage(object Sender, MessageEventArgs e)
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
				Expression Exp;

				try
				{
					Exp = new Expression(s);
				}
				catch (Exception)
				{
					this.Que(e.From);
					return;
				}

				try
				{
					Variables Variables = this.GetVariables(e.From);
					IElement Result = Exp.Root.Evaluate(Variables);
					Variables["Ans"] = Result;

					this.SendChatMessage(e.From, Result.ToString(), false);
				}
				catch (Exception ex)
				{
					this.Error(e.From, ex.Message);
				}
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
			Variables Variables;

			if (!this.sessions.TryGetValue(Address, out Variables))
			{
				Variables = new Variables();
				this.sessions.Add(Address, Variables);
			}

			return Variables;
		}

		private void UpdateReadoutVariables(string Address, InternalReadoutFieldsEventArgs e)
		{
			Variables Variables = this.GetVariables(Address);
			Dictionary<string, SortedDictionary<DateTime, Field>> Fields;
			SortedDictionary<DateTime, Field> Times;
			Variable v;

			if (Variables.TryGetVariable(" Readout ", out v) &&
				(Fields = v.ValueObject as Dictionary<string, SortedDictionary<DateTime, Field>>) != null)
			{
				foreach (Field Field in e.Fields)
				{
					if (!Fields.TryGetValue(Field.Name, out Times))
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

							foreach (KeyValuePair<DateTime, Field> P2 in P.Value)
								Values.Add(new ObjectVector(new ObjectValue(P2.Key), new ObjectValue(this.FieldElement(P2.Value)),
									new ObjectValue(P2.Value.Type), new ObjectValue(P2.Value.QoS)));

							Variables[this.PascalCasing(P.Key)] = VectorDefinition.Encapsulate(Values.ToArray(), true, null);
						}
					}
				}
			}
		}

		private IElement FieldElement(Field Field)
		{
			QuantityField Q = Field as QuantityField;
			if (Q != null)
			{
				if (string.IsNullOrEmpty(Q.Unit))
					return new DoubleNumber(Q.Value);

				try
				{
					Expression Exp = new Expression("1 " + Q.Unit);
					PhysicalQuantity Q2 = Exp.Evaluate(null) as PhysicalQuantity;
					if (Q2 != null)
						return new PhysicalQuantity(Q.Value, Q2.Unit);
				}
				catch (Exception)
				{
					// Ignore
				}

				return new StringValue(Q.ValueString);
			}

			Int32Field I32 = Field as Int32Field;
			if (I32 != null)
				return new DoubleNumber(I32.Value);

			Int64Field I64 = Field as Int64Field;
			if (I64 != null)
				return new DoubleNumber(I64.Value);

			StringField S = Field as StringField;
			if (S != null)
				return new StringValue(S.Value);

			BooleanField B = Field as BooleanField;
			if (B != null)
				return new BooleanValue(B.Value);

			DateTimeField DT = Field as DateTimeField;
			if (DT != null)
				return new DateTimeValue(DT.Value);

			DateField D = Field as DateField;
			if (D != null)
				return new DateTimeValue(D.Value);

			DurationField DU = Field as DurationField;
			if (DU != null)
				return new ObjectValue(DU.Value);

			EnumField E = Field as EnumField;
			if (E != null)
				return new ObjectValue(E.Value);

			TimeField T = Field as TimeField;
			if (T != null)
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

			this.UpdateReadoutVariables(From, e);

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

			this.UpdateReadoutVariables(From, e);

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
			this.SendChatMessage(To,
				"|Command | Description\r\n" +
				"|---|---\r\n" +
				"|#|Displays the short version of the menu.\r\n" +
				"|##|Displays the extended version of the menu.\r\n" +
				"|?|Reads non-historical values of the currently selected object.\r\n" +
				"|??|Performs a full readout of the currently selected object.\r\n" +
				"|FIELD?|Reads the non-historical field \"FIELD\" of the currently selected object.\r\n" +
				"|FIELD??|Reads all values from the field \"FIELD\" of the currently selected object.\r\n" +
				"|=|Displays available variables in the session.\r\n" +
				"| |Anything else is assumed to be evaluated as a [mathematical expression](https://github.com/PeterWaher/IoTGateway/tree/master/Script/Waher.Script#script-syntax).\r\n" +
				"\r\n" +
				"When reading the device, results will be available as pascal cased variables in the current session. You can use these to perform calculations. If a single field value is available for a specific field name, the corresponding variable will contain only the field value. If several values are available for a given field name, the corresponding variable will contain a matrix with their corresponding contents. Use column indexing `Field[Col,]` to access individual columns.", true);
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
