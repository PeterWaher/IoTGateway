using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers.Model;
using Waher.Runtime.Queue;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Abstract base class for sniffers. Implements default method overloads.
	/// </summary>
	public abstract class SnifferBase : ISniffer, ISniffEventProcessor, IDisposableAsync
	{
		private AsyncProcessor<SnifferEvent> processor = new AsyncProcessor<SnifferEvent>(1);
		private readonly bool onlyBinaryCount;

		/// <summary>
		/// Abstract base class for sniffers. Implements default method overloads.
		/// </summary>
		public SnifferBase()
		{
			this.onlyBinaryCount = this.BinaryPresentationMethod == BinaryPresentationMethod.ByteCount;
		}

		/// <summary>
		/// How the sniffer handles binary data.
		/// </summary>
		public abstract BinaryPresentationMethod BinaryPresentationMethod { get; }

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		[Obsolete("Use DisposeAsync() instead.")]
		public void Dispose()
		{
			this.DisposeAsync().Wait();
		}

		/// <summary>
		/// <see cref="IDisposableAsync.DisposeAsync"/>
		/// </summary>
		public virtual async Task DisposeAsync()
		{
			AsyncProcessor<SnifferEvent> Processor = this.processor;
			this.processor = null;

			if (!(Processor is null))
			{
				Processor.CloseForTermination();
				await Processor.WaitUntilIdle();
				await Processor.DisposeAsync();
			}
		}

		/// <summary>
		/// Number of items in queue.
		/// </summary>
		public int QueueSize => this.processor?.QueueSize ?? 0;

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(int Count)
		{
			this.ReceiveBinary(DateTime.UtcNow, Count);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, int Count)
		{
			this.processor?.Queue(new SnifferRxBinary(Timestamp, Count, this));
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data)
		{
			this.ReceiveBinary(DateTime.UtcNow, ConstantBuffer, Data);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			this.ReceiveBinary(Timestamp, ConstantBuffer, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			this.ReceiveBinary(DateTime.UtcNow, ConstantBuffer, Data, Offset, Count);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where received data begins.</param>
		/// <param name="Count">Number of bytes received.</param>
		public void ReceiveBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			if (this.onlyBinaryCount)
				this.processor?.Queue(new SnifferRxBinary(Timestamp, Count, this));
			else
			{
				if (!ConstantBuffer)
				{
					if (Count > 0x100000)   // Avoid cloning buffers > 1 MB
					{
						this.processor?.Queue(new SnifferRxBinary(Timestamp, Count, this));
						return;
					}

					Data = CloneSection(Data, Offset, Count);
					Offset = 0;
				}

				this.processor?.Queue(new SnifferRxBinary(Timestamp, Data, Offset, Count, this));
			}
		}

		/// <summary>
		/// Clones a section of a byte array.
		/// </summary>
		/// <param name="Data">Byte array</param>
		/// <param name="Offset">Offset from where to start the cloning.</param>
		/// <param name="Count">Number of bytes to clone.</param>
		/// <returns>Cloned section.</returns>
		public static byte[] CloneSection(byte[] Data, int Offset, int Count)
		{
			if (Data is null)
				return null;
			else
			{
				byte[] Data2 = new byte[Count];
				Array.Copy(Data, Offset, Data2, 0, Count);
				return Data2;
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(int Count)
		{
			this.TransmitBinary(DateTime.UtcNow, Count);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, int Count)
		{
			this.processor?.Queue(new SnifferTxBinary(Timestamp, Count, this));
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data)
		{
			this.TransmitBinary(DateTime.UtcNow, ConstantBuffer, Data);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data)
		{
			this.TransmitBinary(Timestamp, ConstantBuffer, Data, 0, Data.Length);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			this.TransmitBinary(DateTime.UtcNow, ConstantBuffer, Data, Offset, Count);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="ConstantBuffer">If the contents of the buffer remains constant (true),
		/// or if the contents in the buffer may change after the call (false).</param>
		/// <param name="Data">Binary Data.</param>
		/// <param name="Offset">Offset into buffer where transmitted data begins.</param>
		/// <param name="Count">Number of bytes transmitted.</param>
		public void TransmitBinary(DateTime Timestamp, bool ConstantBuffer, byte[] Data, int Offset, int Count)
		{
			if (this.onlyBinaryCount)
				this.processor?.Queue(new SnifferTxBinary(Timestamp, Count, this));
			else
			{
				if (!ConstantBuffer)
				{
					if (Count > 0x100000)   // Avoid cloning buffers > 1 MB
					{
						this.processor?.Queue(new SnifferTxBinary(Timestamp, Count, this));
						return;
					}

					Data = CloneSection(Data, Offset, Count);
					Offset = 0;
				}

				this.processor?.Queue(new SnifferTxBinary(Timestamp, Data, Offset, Count, this));
			}
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			this.ReceiveText(DateTime.UtcNow, Text);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void ReceiveText(DateTime Timestamp, string Text)
		{
			this.processor?.Queue(new SnifferRxText(Timestamp, Text, this));
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			this.TransmitText(DateTime.UtcNow, Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public void TransmitText(DateTime Timestamp, string Text)
		{
			this.processor?.Queue(new SnifferTxText(Timestamp, Text, this));
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			this.Information(DateTime.UtcNow, Comment);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public void Information(DateTime Timestamp, string Comment)
		{
			this.processor?.Queue(new SnifferInformation(Timestamp, Comment, this));
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			this.Warning(DateTime.UtcNow, Warning);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public void Warning(DateTime Timestamp, string Warning)
		{
			this.processor?.Queue(new SnifferWarning(Timestamp, Warning, this));
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			this.Error(DateTime.UtcNow, Error);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public void Error(DateTime Timestamp, string Error)
		{
			this.processor?.Queue(new SnifferError(Timestamp, Error, this));
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(string Exception)
		{
			this.Exception(DateTime.UtcNow, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, string Exception)
		{
			this.processor?.Queue(new SnifferException(Timestamp, Exception, this));
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception)
		{
			this.Exception(DateTime.UtcNow, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public void Exception(DateTime Timestamp, Exception Exception)
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

				if (Inner?.First is null)
					Exception = null;
				else
				{
					Exception = Inner.First.Value;
					Inner.RemoveFirst();
				}
			}
		}

		/// <summary>
		/// Waits until pending sniffer events have been processed.
		/// </summary>
		public async Task FlushAsync()
		{
			if (!(this.processor is null))
				await this.processor.WaitUntilIdle();
		}

		#region ISniffEventProcessor

		/// <summary>
		/// Processes a binary reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferRxBinary Event);

		/// <summary>
		/// Processes a binary transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferTxBinary Event);

		/// <summary>
		/// Processes a text reception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferRxText Event);

		/// <summary>
		/// Processes a text transmission event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferTxText Event);

		/// <summary>
		/// Processes an information event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferInformation Event);

		/// <summary>
		/// Processes a warning event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferWarning Event);

		/// <summary>
		/// Processes an error event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferError Event);

		/// <summary>
		/// Processes an exception event.
		/// </summary>
		/// <param name="Event">Sniffer event.</param>
		public abstract Task Process(SnifferException Event);

		#endregion
	}
}
