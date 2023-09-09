using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;

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
		/// <summary>
		/// http://waher.se/Schema/EventOutput.xsd
		/// </summary>
		public const string LogNamespace = "http://waher.se/Schema/EventOutput.xsd";

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
			StringBuilder sb = new StringBuilder();
			string ElementName = Event.Type.ToString();

			sb.Append('<');
			sb.Append(ElementName);
			sb.Append(" xmlns='");
			sb.Append(LogNamespace);
			sb.Append("' timestamp='");
			sb.Append(XML.Encode(Event.Timestamp));
			sb.Append("' level='");
			sb.Append(Event.Level.ToString());

			if (!string.IsNullOrEmpty(Event.EventId))
			{
				sb.Append("' id='");
				sb.Append(Event.EventId);
			}

			if (!string.IsNullOrEmpty(Event.Object))
			{
				sb.Append("' object='");
				sb.Append(Event.Object);
			}

			if (!string.IsNullOrEmpty(Event.Actor))
			{
				sb.Append("' actor='");
				sb.Append(Event.Actor);
			}

			if (!string.IsNullOrEmpty(Event.Module))
			{
				sb.Append("' module='");
				sb.Append(Event.Module);
			}

			if (!string.IsNullOrEmpty(Event.Facility))
			{
				sb.Append("' facility='");
				sb.Append(Event.Facility);
			}

			sb.Append("'><Message>");

			foreach (string Row in GetRows(Event.Message))
			{
				sb.Append("<Row>");
				sb.Append(XML.Encode(Row));
				sb.Append("</Row>");
			}

			sb.Append("</Message>");

			if (!(Event.Tags is null) && Event.Tags.Length > 0)
			{
				foreach (KeyValuePair<string, object> Tag in Event.Tags)
				{
					sb.Append("<Tag key='");
					sb.Append(XML.Encode(Tag.Key));

					if (!(Tag.Value is null))
					{
						sb.Append("' value='");
						sb.Append(XML.Encode(Tag.Value.ToString()));
					}

					sb.Append("'/>");
				}
			}

			if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
			{
				sb.Append("<StackTrace>");

				foreach (string Row in GetRows(Event.StackTrace))
				{
					sb.Append("<Row>");
					sb.Append(XML.Encode(Row));
					sb.Append("</Row>");
				}

				sb.Append("</StackTrace>");
			}

			sb.Append("</");
			sb.Append(ElementName);
			sb.Append('>');

			return this.Queue(sb.ToString(), false);
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
					this.Raise(this.BeforeConnect);
					await this.pipe.ConnectAsync(5000);
					this.Raise(this.AfterConnect);
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
				Log.Critical(ex);
			}
		}

		private void Raise(EventHandler Event)
		{
			if (!(Event is null))
			{
				try
				{
					Event(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
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
		public event EventHandler BeforeConnect;

		/// <summary>
		/// Raised after connecting to the pipe stream
		/// </summary>
		public event EventHandler AfterConnect;

		private static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}
	}
}
