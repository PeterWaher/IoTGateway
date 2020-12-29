using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Sending sniffer events to the corresponding web page(s).
	/// </summary>
	public class WebSniffer : SnifferBase
	{
		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private readonly DateTime created = DateTime.Now;
		private readonly DateTime expires;
		private readonly ISniffable sniffable;
		private readonly string[] privileges;
		private readonly string userVariable;
		private readonly string resource;
		private readonly string snifferId;
		private string[] tabIds = null;
		private DateTime tabIdTimestamp = DateTime.MinValue;

		/// <summary>
		/// Sending sniffer events to the corresponding web page(s).
		/// </summary>
		/// <param name="SnifferId">Sniffer ID</param>
		/// <param name="PageResource">Resource of page displaying the sniffer.</param>
		/// <param name="MaxLife">Maximum life of sniffer.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		/// <param name="Sniffable">Object being sniffed</param>
		/// <param name="UserVariable">Event is only pushed to clients with a session contining a variable 
		/// named <paramref name="UserVariable"/> having a value derived from <see cref="IUser"/>.</param>
		/// <param name="Privileges">Event is only pushed to clients with a user variable having the following set of privileges.</param>
		public WebSniffer(string SnifferId, string PageResource, TimeSpan MaxLife, BinaryPresentationMethod BinaryPresentationMethod, ISniffable Sniffable,
			string UserVariable, params string[] Privileges)
			: base()
		{
			this.expires = DateTime.Now.Add(MaxLife);
			this.sniffable = Sniffable;
			this.snifferId = SnifferId;
			this.resource = PageResource;
			this.binaryPresentationMethod = BinaryPresentationMethod;
			this.tabIds = null;
			this.userVariable = UserVariable;
			this.privileges = Privileges;
		}

		/// <summary>
		/// Sniffer ID
		/// </summary>
		public string SnifferId => this.snifferId;

		private void Process(DateTime Timestamp, string Message, string Function)
		{
			if (Timestamp >= this.expires)
				Task.Run(() => this.Close());
			else
				Task.Run(() => this.Push(Timestamp, Message, Function, true));
		}

		private async Task Push(DateTime Timestamp, string Message, string Function, bool CloseIfNoTabs)
		{
			try
			{
				DateTime Now = DateTime.Now;

				if ((Now - this.tabIdTimestamp).TotalSeconds > 2 || this.tabIds is null || this.tabIds.Length == 0)
				{
					this.tabIds = ClientEvents.GetTabIDsForLocation(this.resource, true, "SnifferId", this.snifferId);
					this.tabIdTimestamp = Now;
				}

				int Tabs = await ClientEvents.PushEvent(this.tabIds, Function, JSON.Encode(new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("timestamp", XML.Encode(Timestamp)),
					new KeyValuePair<string, object>("message", Message)
				}, true), true, this.userVariable, this.privileges);

				if (CloseIfNoTabs && Tabs <= 0 && (Now - this.created).TotalSeconds >= 5)
					await this.Close();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task Close()
		{
			await this.Push(DateTime.Now, "Sniffer closed.", "Information", false);
			this.sniffable.Remove(this);
		}

		private void Process(DateTime Timestamp, byte[] Data, string Function)
		{
			if (Timestamp >= this.expires)
				Task.Run(() => this.Close());
			else
				Task.Run(() => this.Push(Timestamp, this.HexOutput(Data), Function, true));
		}

		private string HexOutput(byte[] Data)
		{
			switch (this.binaryPresentationMethod)
			{
				case BinaryPresentationMethod.Hexadecimal:
					StringBuilder sb = new StringBuilder();
					int i = 0;
					bool First = true;

					foreach (byte b in Data)
					{
						if (i > 0)
							sb.Append(' ');
						else if (First)
							First = false;
						else
							sb.AppendLine();

						sb.Append(b.ToString("X2"));

						i = (i + 1) & 31;
					}

					return sb.ToString();

				case BinaryPresentationMethod.Base64:
					return Convert.ToBase64String(Data);

				case BinaryPresentationMethod.ByteCount:
				default:
					return "<" + Data.Length.ToString() + " bytes>";
			}
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public override void Error(DateTime Timestamp, string Error)
		{
			this.Process(Timestamp, Error, "Error");
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public override void Exception(DateTime Timestamp, string Exception)
		{
			this.Process(Timestamp, Exception, "Exception");
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public override void Information(DateTime Timestamp, string Comment)
		{
			this.Process(Timestamp, Comment, "Information");
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override void ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			this.Process(Timestamp, Data, "Rx");
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override void ReceiveText(DateTime Timestamp, string Text)
		{
			this.Process(Timestamp, Text, "Rx");
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public override void TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			this.Process(Timestamp, Data, "Tx");
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override void TransmitText(DateTime Timestamp, string Text)
		{
			this.Process(Timestamp, Text, "Tx");
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public override void Warning(DateTime Timestamp, string Warning)
		{
			this.Process(Timestamp, Warning, "Warning");
		}
	}
}
