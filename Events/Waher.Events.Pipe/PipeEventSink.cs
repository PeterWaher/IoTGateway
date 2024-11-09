using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Events.Files;
using Waher.Networking;

namespace Waher.Events.Pipe
{
	/// <summary>
	/// Delegate for methods that create object instances of <see cref="NamedPipeClientStream"/>.
	/// </summary>
	/// <param name="Name">Pipe name.</param>
	/// <returns>Object instance.</returns>
	public delegate NamedPipeClientStream NamedPipeClientStreamFactory(string Name);

	/// <summary>
	/// Writes logged events to an operating system pipe, for inter-process communication.
	/// </summary>
	public class PipeEventSink : EventSink
	{
		private readonly LinkedList<byte[]> pipeQueue = new LinkedList<byte[]>();
		private readonly NamedPipeClientStreamFactory pipeStreamFactory;
		private readonly string pipeName;
		private NamedPipeClientStream pipe;
		private bool writing = false;

		/// <summary>
		/// Writes logged events to an operating system pipe, for inter-process communication.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="PipeName">Name of pipe.</param>
		public PipeEventSink(string ObjectId, string PipeName)
			: this(ObjectId, PipeName, DefaultFactory)
		{
		}

		/// <summary>
		/// Writes logged events to an operating system pipe, for inter-process communication.
		/// </summary>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="PipeName">Name of pipe.</param>
		/// <param name="StreamFactory">Method used to create a pipe stream object.</param>
		public PipeEventSink(string ObjectId, string PipeName, NamedPipeClientStreamFactory StreamFactory)
			: base(ObjectId)
		{
			this.pipeStreamFactory = StreamFactory;
			this.pipe = null;
			this.pipeName = PipeName;
		}

		private static NamedPipeClientStream DefaultFactory(string Name)
		{
			return new NamedPipeClientStream(".", Name, PipeDirection.Out, PipeOptions.Asynchronous,
				TokenImpersonationLevel.Anonymous, HandleInheritability.None);
		}

		/// <summary>
		/// Pipe object.
		/// </summary>
		public NamedPipeClientStream Pipe => this.pipe;

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.pipe?.Dispose();
			this.pipe = null;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			return this.Queue(Event.ToXML(), false);
		}

		/// <summary>
		/// Queues XML-encoded information to be output.
		/// </summary>
		/// <param name="Xml">XML to queue to the pipe.</param>
		public Task Queue(string Xml)
		{
			return this.Queue(Xml, true);
		}

		/// <summary>
		/// Queues XML-encoded information to be output.
		/// </summary>
		/// <param name="Xml">XML to queue to the pipe.</param>
		private async Task Queue(string Xml, bool ValidateXml)
		{
			if (ValidateXml && !XML.IsValidXml(Xml))
				throw new ArgumentException("Invalid XML.", nameof(Xml));

			try
			{
				byte[] Bin = Encoding.UTF8.GetBytes(Xml);

				lock (this.pipeQueue)
				{
					if (this.writing)
					{
						this.pipeQueue.AddLast(Bin);
						return;
					}
					else
						this.writing = true;
				}

				if (!(this.pipe is null) && !this.pipe.IsConnected)
				{
					this.pipe.Dispose();
					this.pipe = null;
				}

				if (this.pipe is null)
				{
					this.pipe = this.pipeStreamFactory(this.pipeName);
					await this.BeforeConnect.Raise(this, EventArgs.Empty);
					await this.pipe.ConnectAsync(5000);
					await this.AfterConnect.Raise(this, EventArgs.Empty);
				}

				while (!(Bin is null))
				{
					await this.pipe.WriteAsync(Bin, 0, Bin.Length);

					lock (this.pipeQueue)
					{
						if (this.pipeQueue.First is null)
						{
							this.writing = false;
							Bin = null;
						}
						else
						{
							Bin = this.pipeQueue.First.Value;
							this.pipeQueue.RemoveFirst();
						}
					}
				}
			}
			catch (TimeoutException)
			{
				this.EmptyPipeQueue();
			}
			catch (IOException)
			{
				this.EmptyPipeQueue();
			}
			catch (Exception ex)
			{
				this.EmptyPipeQueue();
				Log.Exception(ex);
			}
		}

		private void EmptyPipeQueue()
		{
			lock (this.pipeQueue)
			{
				this.pipeQueue.Clear();
				this.writing = false;
			}
		}

		/// <summary>
		/// Raised before connecting to the pipe stream.
		/// </summary>
		public event EventHandlerAsync BeforeConnect;
		
		/// <summary>
		/// Raised after connecting to the pipe stream
		/// </summary>
		public event EventHandlerAsync AfterConnect;
	}
}
