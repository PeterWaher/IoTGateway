using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Networking.XMPP.Sensor;
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
		private XmppClient client;
		private SensorServer sensorServer;

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
			if (s == null || string.IsNullOrEmpty(s = s.Trim()))
				return;

			if (s == "??")
			{
				this.SendChatMessage(e.From, "Readout started...\r\n\r\n|Field|Localized|Value|Unit|Timestamp|Type|QoS|\r\n|---|---|--:|---|:-:|:-:|:-:|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
					this.AllFieldsRead, this.AllFieldsErrorsRead, e.From);
			}
			else if (s == "?")
			{
				this.SendChatMessage(e.From, "Readout started...\r\n\r\n|Field|Value|Unit|\r\n|---|--:|---|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.Momentary, null, DateTime.MinValue, DateTime.MaxValue,
					this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, e.From);
			}
			else if (s.EndsWith("??"))
			{
				string Field = s.Substring(0, s.Length - 2);
				this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...\r\n\r\n|Field|Localized|Value|Unit|Timestamp|Type|QoS|\r\n|---|---|--:|---|:-:|:-:|:-:|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, new string[] { Field }, DateTime.MinValue, DateTime.MaxValue,
					this.AllFieldsRead, this.AllFieldsErrorsRead, e.From);
			}
			else if (s.EndsWith("?"))
			{
				string Field = s.Substring(0, s.Length - 1);
				this.SendChatMessage(e.From, "Readout of " + MarkdownDocument.Encode(Field) + " started...\r\n\r\n|Field|Value|Unit|\r\n|---|--:|---|", true);
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.Momentary, new string[] { Field }, DateTime.MinValue, DateTime.MaxValue,
					this.MomentaryFieldsRead, this.MomentaryFieldsErrorsRead, e.From);
			}
			else if (s == "#")
				this.ShowMenu(e.From, false);
			else if (s == "##")
				this.ShowMenu(e.From, true);
			else
				this.Que(e.From);
		}

		private void MomentaryFieldsRead(object Sender, InternalReadoutFieldsEventArgs e)
		{
			string From = (string)e.State;
			QuantityField QF;

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
				"Command | Description\r\n" +
				"---|---\r\n" +
				"#|Displays the short version of the menu.\r\n" +
				"##|Displays the extended version of the menu.\r\n" +
				"?|Reads momentary values of the currently selected object.\r\n" +
				"??|Performs a full readout of the currently selected object.\r\n" +
				"FIELD?|Reads the momentary field \"FIELD\" of the currently selected object.\r\n" +
				"FIELD??|Reads all values from the field \"FIELD\" of the currently selected object.", true);
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
