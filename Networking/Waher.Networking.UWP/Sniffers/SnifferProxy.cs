using System;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers
{
    /// <summary>
    /// A sniffer that redirects incoming events to another sniffable object.
    /// </summary>
    public class SnifferProxy : ISniffer
	{
		private readonly ICommunicationLayer sniffable;

		/// <summary>
		/// A sniffer that redirects incoming events to another sniffable object.
		/// </summary>
		/// <param name="Sniffable">Sniffable object to receive incoming sniffer events.</param>
		public SnifferProxy(ICommunicationLayer Sniffable)
		{
			if (Sniffable is null)
				throw new ArgumentNullException(nameof(Sniffable));

			this.sniffable = Sniffable;
		}

		/// <summary>
		/// Object receiving incoming sniffer events.
		/// </summary>
		public ICommunicationLayer Sniffable => this.sniffable;

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public Task ReceiveBinary(byte[] Data)
		{
			return this.sniffable.ReceiveBinary(Data);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public Task ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			return this.sniffable.ReceiveBinary(Timestamp, Data);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public Task TransmitBinary(byte[] Data)
		{
			return this.sniffable.TransmitBinary(Data);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			return this.sniffable.TransmitBinary(Timestamp, Data);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public Task ReceiveText(string Text)
		{
			return this.sniffable.ReceiveText(Text);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public Task ReceiveText(DateTime Timestamp, string Text)
		{
			return this.sniffable.ReceiveText(Timestamp, Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public Task TransmitText(string Text)
		{
			return this.sniffable.TransmitText(Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public Task TransmitText(DateTime Timestamp, string Text)
		{
			return this.sniffable.TransmitText(Timestamp, Text);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public Task Information(string Comment)
		{
			return this.sniffable.Information(Comment);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public Task Information(DateTime Timestamp, string Comment)
		{
			return this.sniffable.Information(Timestamp, Comment);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public Task Warning(string Warning)
		{
			return this.sniffable.Warning(Warning);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public Task Warning(DateTime Timestamp, string Warning)
		{
			return this.sniffable.Warning(Timestamp, Warning);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public Task Error(string Error)
		{
			return this.sniffable.Error(Error);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public Task Error(DateTime Timestamp, string Error)
		{
			return this.sniffable.Error(Timestamp, Error);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public Task Exception(string Exception)
		{
			return this.sniffable.Exception(Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public Task Exception(DateTime Timestamp, string Exception)
		{
			return this.sniffable.Exception(Timestamp, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public Task Exception(Exception Exception)
		{
			return this.sniffable.Exception(Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public Task Exception(DateTime Timestamp, Exception Exception)
		{
			return this.sniffable.Exception(Timestamp, Exception);
		}
	}
}
