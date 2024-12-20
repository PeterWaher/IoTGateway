using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Abstract base class for sniffers. Implements default method overloads.
	/// </summary>
	public abstract class SnifferBase : ISniffer
	{
		/// <summary>
		/// Abstract base class for sniffers. Implements default method overloads.
		/// </summary>
		public SnifferBase()
		{
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public virtual Task ReceiveBinary(byte[] Data)
		{
			return this.ReceiveBinary(DateTime.Now, Data);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public virtual Task ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			return this.ReceiveBinary(Timestamp, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public virtual Task ReceiveBinary(byte[] Data, int Offset, int Count)
		{
			return this.ReceiveBinary(DateTime.Now, Data, Offset, Count);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public abstract Task ReceiveBinary(DateTime Timestamp, byte[] Data, int Offset, int Count);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public virtual Task TransmitBinary(byte[] Data)
		{
			return this.TransmitBinary(DateTime.Now, Data);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public virtual Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			return this.TransmitBinary(Timestamp, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public virtual Task TransmitBinary(byte[] Data, int Offset, int Count)
		{
			return this.TransmitBinary(DateTime.Now, Data, Offset, Count);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public abstract Task TransmitBinary(DateTime Timestamp, byte[] Data, int Offset, int Count);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public virtual Task ReceiveText(string Text)
		{
			return this.ReceiveText(DateTime.Now, Text);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public abstract Task ReceiveText(DateTime Timestamp, string Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public virtual Task TransmitText(string Text)
		{
			return this.TransmitText(DateTime.Now, Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public abstract Task TransmitText(DateTime Timestamp, string Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public virtual Task Information(string Comment)
		{
			return this.Information(DateTime.Now, Comment);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public abstract Task Information(DateTime Timestamp, string Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public virtual Task Warning(string Warning)
		{
			return this.Warning(DateTime.Now, Warning);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public abstract Task Warning(DateTime Timestamp, string Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public virtual Task Error(string Error)
		{
			return this.Error(DateTime.Now, Error);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public abstract Task Error(DateTime Timestamp, string Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public virtual Task Exception(string Exception)
		{
			return this.Exception(DateTime.Now, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public abstract Task Exception(DateTime Timestamp, string Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public virtual Task Exception(Exception Exception)
		{
			return this.Exception(DateTime.Now, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public virtual async Task Exception(DateTime Timestamp, Exception Exception)
		{
			LinkedList<Exception> Inner = null;

			while (!(Exception is null))
			{
				if (Exception is AggregateException AggregateException)
				{
					if (Inner is null)
						Inner = new LinkedList<Exception>();

					foreach (Exception ex in AggregateException.InnerExceptions)
						Inner.AddLast(ex);
				}
				else if (!(Exception.InnerException is null))
				{
					if (Inner is null)
						Inner = new LinkedList<Exception>();

					Inner.AddLast(Exception.InnerException);
				}

				await this.Exception(Timestamp, Exception.Message + "\r\n\r\n" + Log.CleanStackTrace(Exception.StackTrace));

				if (Inner is null)
					Exception = null;
				else
				{
					Exception = Inner.First.Value;
					Inner.RemoveFirst();
					if (Inner.First is null)
						Inner = null;
				}
			}
		}

	}
}
