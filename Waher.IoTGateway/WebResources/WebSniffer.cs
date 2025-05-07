using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.Sniffers.Model;
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
		private readonly DateTime created = DateTime.UtcNow;
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
			this.expires = DateTime.UtcNow.Add(MaxLife);
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

		/// <inheritdoc/>
		public override Task DisposeAsync()
		{
			this.outgoing?.Dispose();
			this.outgoing = null;

			return base.DisposeAsync();
		}

		/// <summary>
		/// Sniffer ID
		/// </summary>
		public string SnifferId => this.snifferId;

		/// <summary>
		/// How the sniffer handles binary data.
		/// </summary>
		public override BinaryPresentationMethod BinaryPresentationMethod => this.binaryPresentationMethod;

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
				DateTime Now = DateTime.UtcNow;

				if ((Now - this.tabIdTimestamp).TotalSeconds > 2 || this.tabIds is null || this.tabIds.Length == 0)
				{
					this.tabIds = ClientEvents.GetTabIDsForLocation(this.resource, true, "SnifferId", this.snifferId);
					this.tabIdTimestamp = Now;
				}

				if (this.feedbackCheck && Message.StartsWith('{') && Message.EndsWith('}'))
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
			await this.Push(DateTime.UtcNow, "Sniffer closed.", "Information", false);
			this.comLayer.Remove(this);
			await this.DisposeAsync();
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
			if (Data is null)
				return "<" + Count.ToString() + " bytes>";

			switch (Data is null ? BinaryPresentationMethod.ByteCount : this.binaryPresentationMethod)
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
					return "<" + Count.ToString() + " bytes>";
			}
		}

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferError Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Text, "Error");
		}

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferException Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Text, "Exception");
		}

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferInformation Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Text, "Information");
		}

		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferRxBinary Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Data, Event.Offset, Event.Count, "Rx");
		}

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferRxText Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Text, "Rx");
		}

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferTxBinary Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Data, Event.Offset, Event.Count, "Tx");
		}

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferTxText Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Text, "Tx");
		}

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		/// <param name="Cancel">Cancellation token.</param>
		public override Task Process(SnifferWarning Event, CancellationToken Cancel)
		{
			return this.Process(Event.Timestamp, Event.Text, "Warning");
		}

	}
}
