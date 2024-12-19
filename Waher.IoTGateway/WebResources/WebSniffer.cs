using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Runtime.Cache;
using Waher.Security;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Sending sniffer events to the corresponding web page(s).
	/// </summary>
	public class WebSniffer : SnifferBase, IDisposable
	{
		private readonly BinaryPresentationMethod binaryPresentationMethod;
		private readonly DateTime created = DateTime.Now;
		private readonly DateTime expires;
		private readonly ICommunicationLayer comLayer;
		private readonly string[] privileges;
		private readonly string userVariable;
		private readonly string resource;
		private readonly string snifferId;
		private readonly bool feedbackCheck;
		private string[] tabIds = null;
		private DateTime tabIdTimestamp = DateTime.MinValue;
		private Cache<string, bool> outgoing;

		/// <summary>
		/// Sending sniffer events to the corresponding web page(s).
		/// </summary>
		/// <param name="SnifferId">Sniffer ID</param>
		/// <param name="PageResource">Resource of page displaying the sniffer.</param>
		/// <param name="MaxLife">Maximum life of sniffer.</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		/// <param name="ComLayer">Object being sniffed</param>
		/// <param name="UserVariable">Event is only pushed to clients with a session contining a variable 
		/// named <paramref name="UserVariable"/> having a value derived from <see cref="IUser"/>.</param>
		/// <param name="Privileges">Event is only pushed to clients with a user variable having the following set of privileges.</param>
		public WebSniffer(string SnifferId, string PageResource, TimeSpan MaxLife, BinaryPresentationMethod BinaryPresentationMethod, ICommunicationLayer ComLayer,
			string UserVariable, params string[] Privileges)
			: base()
		{
			this.expires = DateTime.Now.Add(MaxLife);
			this.comLayer = ComLayer;
			this.snifferId = SnifferId;
			this.resource = PageResource;
			this.binaryPresentationMethod = BinaryPresentationMethod;
			this.tabIds = null;
			this.userVariable = UserVariable;
			this.privileges = Privileges;
			this.feedbackCheck = ComLayer is HttpServer;

			if (this.feedbackCheck)
				this.outgoing = new Cache<string, bool>(int.MaxValue, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), true);
			else
				this.outgoing = null;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public virtual void Dispose()
		{
			this.outgoing?.Dispose();
			this.outgoing = null;
		}

		/// <summary>
		/// Sniffer ID
		/// </summary>
		public string SnifferId => this.snifferId;

		private Task Process(DateTime Timestamp, string Message, string Function)
		{
			if (Timestamp >= this.expires)
				return this.Close();
			else
				return this.Push(Timestamp, Message, Function, true);
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

				if (this.feedbackCheck && Message.StartsWith("{") && Message.EndsWith("}"))
				{
					try
					{
						object Parsed = JSON.Parse(Message);
						if (Parsed is IDictionary<string, object> Obj &&
							Obj.TryGetValue("data", out object Temp) &&
							Temp is IDictionary<string, object> Obj2 &&
							Obj2.TryGetValue("timestamp", out object Timestamp2) &&
							Obj2.TryGetValue("message", out object Message2) &&
							(this.outgoing?.ContainsKey(this.ToJson(Timestamp2, Message2)) ?? true))
						{
							return;
						}
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				string Data = this.ToJson(XML.Encode(Timestamp), Message);

				this.outgoing?.Add(Data, true);

				int Tabs = await ClientEvents.PushEvent(this.tabIds, Function, Data, true, this.userVariable, this.privileges);

				if (CloseIfNoTabs && Tabs <= 0 && (Now - this.created).TotalSeconds >= 5)
					await this.Close();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private string ToJson(object Timestamp, object Message)
		{
			return JSON.Encode(new KeyValuePair<string, object>[]
			{
				new KeyValuePair<string, object>("timestamp", Timestamp),
				new KeyValuePair<string, object>("message", Message)
			}, false);
		}

		private async Task Close()
		{
			await this.Push(DateTime.Now, "Sniffer closed.", "Information", false);
			this.comLayer.Remove(this);
			this.Dispose();
		}

		private Task Process(DateTime Timestamp, byte[] Data, int Offset, int Count, string Function)
		{
			if (Timestamp >= this.expires)
				return this.Close();
			else
				return this.Push(Timestamp, this.HexOutput(Data, Offset, Count), Function, true);
		}

		private string HexOutput(byte[] Data, int Offset, int Count)
		{
			switch (this.binaryPresentationMethod)
			{
				case BinaryPresentationMethod.Hexadecimal:
					StringBuilder sb = new StringBuilder();
					int i = 0;
					bool First = true;
					byte b;

					while (Count-- > 0)
					{
						b = Data[Offset++];

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
		public override Task Error(DateTime Timestamp, string Error)
		{
			return this.Process(Timestamp, Error, "Error");
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public override Task Exception(DateTime Timestamp, string Exception)
		{
			return this.Process(Timestamp, Exception, "Exception");
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public override Task Information(DateTime Timestamp, string Comment)
		{
			return this.Process(Timestamp, Comment, "Information");
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public override Task ReceiveBinary(DateTime Timestamp, byte[] Data, int Offset, int Count)
		{
			return this.Process(Timestamp, Data, Offset, Count, "Rx");
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task ReceiveText(DateTime Timestamp, string Text)
		{
			return this.Process(Timestamp, Text, "Rx");
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public override Task TransmitBinary(DateTime Timestamp, byte[] Data, int Offset, int Count)
		{
			return this.Process(Timestamp, Data, Offset, Count, "Tx");
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public override Task TransmitText(DateTime Timestamp, string Text)
		{
			return this.Process(Timestamp, Text, "Tx");
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public override Task Warning(DateTime Timestamp, string Warning)
		{
			return this.Process(Timestamp, Warning, "Warning");
		}

	}
}
