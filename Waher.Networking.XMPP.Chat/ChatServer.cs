using System;
using System.Collections.Generic;
using System.Text;
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

		private void client_OnChatMessage(XmppClient Sender, MessageEventArgs e)
		{
			string s = e.Body;
			if (s == null || string.IsNullOrEmpty(s = s.Trim()))
				return;

			if (s == "??")
			{
				this.client.SendChatMessage(e.From, "Readout started...");
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, null, DateTime.MinValue, DateTime.MaxValue,
					this.AllFieldsRead, this.ErrorsRead, e.From);
			}
			else if (s == "?")
			{
				this.client.SendChatMessage(e.From, "Readout started...");
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.Momentary, null, DateTime.MinValue, DateTime.MaxValue,
					this.MomentaryFieldsRead, this.ErrorsRead, e.From);
			}
			else if (s.EndsWith("??"))
			{
				string Field = s.Substring(0, s.Length - 2);
				this.client.SendChatMessage(e.From, "Readout of " + Field + " started...");
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.All, new string[] { Field }, DateTime.MinValue, DateTime.MaxValue,
					this.AllFieldsRead, this.ErrorsRead, e.From);
			}
			else if (s.EndsWith("?"))
			{
				string Field = s.Substring(0, s.Length - 1);
				this.client.SendChatMessage(e.From, "Readout of " + Field + " started...");
				this.sensorServer.DoInternalReadout(e.From, null, FieldType.Momentary, new string[] { Field }, DateTime.MinValue, DateTime.MaxValue,
					this.MomentaryFieldsRead, this.ErrorsRead, e.From);
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

			foreach (Field F in e.Fields)
				this.client.SendChatMessage(From, F.Name + "\t" + F.ValueString);

			if (e.Done)
				this.client.SendChatMessage(From, "Readout complete.");

			// TODO: Localization
			// TODO: IM-HTML with tables.
		}

		private void AllFieldsRead(object Sender, InternalReadoutFieldsEventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			string From = (string)e.State;
			DateTime TP;

			foreach (Field F in e.Fields)
			{
				TP = F.Timestamp;

				sb.Append(F.Name);
				sb.Append('\t');
				sb.Append(F.Name);
				sb.Append('\t');
				sb.Append(F.ValueString);
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
				sb.Append(F.QoS.ToString());

				this.client.SendChatMessage(From, sb.ToString());
				sb.Clear();
			}

			if (e.Done)
				this.client.SendChatMessage(From, "Readout complete.");

			// TODO: Localization
			// TODO: IM-HTML with tables.
		}

		private void ErrorsRead(object Sender, InternalReadoutErrorsEventArgs e)
		{
			string From = (string)e.State;

			foreach (ThingError Error in e.Errors)
				this.Error(From, Error.ToString());

			if (e.Done)
				this.client.SendChatMessage(From, "Readout complete.");
		}

		private void Que(string To)
		{
			this.Error(To, "Sorry. Can't understand what you're trying to say. Type # to display the menu.");
		}

		private void Error(string To, string ErrorMessage)
		{
			this.client.SendChatMessage(To, ErrorMessage);
			// TODO: IM-HTML with red color.
		}

		private void ShowMenu(string To, bool Extended)
		{
			this.client.SendChatMessage(To, "#\tDisplays the short version of the menu.");
			this.client.SendChatMessage(To, "##\tDisplays the extended version of the menu.");
			this.client.SendChatMessage(To, "?\tReads momentary values of the currently selected object.");
			this.client.SendChatMessage(To, "??\tPerforms a full readout of the currently selected object.");

			// TODO: IM-HTML with tables.
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
