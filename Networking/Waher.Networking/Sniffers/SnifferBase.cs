using System;
using System.Collections;
using System.Collections.Generic;
using Waher.Events;
using Waher.Networking.Sniffers.Model;

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
		public virtual void ReceiveBinary(byte[] Data)
		{
			this.ReceiveBinary(DateTime.Now, Data);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public abstract void ReceiveBinary(DateTime Timestamp, byte[] Data);

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public virtual void TransmitBinary(byte[] Data)
		{
			this.TransmitBinary(DateTime.Now, Data);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public abstract void TransmitBinary(DateTime Timestamp, byte[] Data);

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public virtual void ReceiveText(string Text)
		{
			this.ReceiveText(DateTime.Now, Text);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public abstract void ReceiveText(DateTime Timestamp, string Text);

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public virtual void TransmitText(string Text)
		{
			this.TransmitText(DateTime.Now, Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public abstract void TransmitText(DateTime Timestamp, string Text);

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public virtual void Information(string Comment)
		{
			this.Information(DateTime.Now, Comment);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public abstract void Information(DateTime Timestamp, string Comment);

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public virtual void Warning(string Warning)
		{
			this.Warning(DateTime.Now, Warning);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public abstract void Warning(DateTime Timestamp, string Warning);

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public virtual void Error(string Error)
		{
			this.Error(DateTime.Now, Error);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public abstract void Error(DateTime Timestamp, string Error);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public virtual void Exception(string Exception)
		{
			this.Exception(DateTime.Now, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public abstract void Exception(DateTime Timestamp, string Exception);

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public virtual void Exception(Exception Exception)
		{
			this.Exception(DateTime.Now, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public virtual void Exception(DateTime Timestamp, Exception Exception)
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

				this.Exception(Timestamp, Exception.Message + "\r\n\r\n" + Log.CleanStackTrace(Exception.StackTrace));

				if (Inner is null)
					Exception = null;
				else
				{
					Exception = Inner.First.Value;
					Inner.RemoveFirst();
				}
			}
		}

	}
}
