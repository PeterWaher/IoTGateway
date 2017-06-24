using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Events.Files
{
	/// <summary>
	/// Outputs sniffed data to a text writer.
	/// </summary>
	public class TextWriterEventSink : EventSink, IDisposable
	{
		/// <summary>
		/// Text writer object.
		/// </summary>
		protected TextWriter output;

		private bool disposed = false;

		/// <summary>
		/// Outputs sniffed data to a text writer.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="Output">Output</param>
		public TextWriterEventSink(string ObjectID, TextWriter Output)
			: base(ObjectID)
		{
			this.output = Output;
		}

		/// <summary>
		/// Method is called before writing something to the text file.
		/// </summary>
		protected virtual void BeforeWrite()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method is called after writing something to the text file.
		/// </summary>
		protected virtual void AfterWrite()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			this.disposed = true;

			base.Dispose();
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override void Queue(Event Event)
		{
			if (this.disposed)
				return;

			this.BeforeWrite();
			try
			{
				this.output.Write(Event.Timestamp.ToShortDateString());
				this.output.Write(", ");
				this.output.Write(Event.Timestamp.ToLongTimeString());
				this.output.Write('\t');
				this.output.Write(Event.Type.ToString());
				this.output.Write('\t');
				this.output.Write(Event.Level.ToString());

				if (!string.IsNullOrEmpty(Event.EventId))
				{
					this.output.Write('\t');
					this.output.Write(Event.EventId);
				}

				if (!string.IsNullOrEmpty(Event.Object))
				{
					this.output.Write('\t');
					this.output.Write(Event.Object);
				}

				if (!string.IsNullOrEmpty(Event.Actor))
				{
					this.output.Write('\t');
					this.output.Write(Event.Actor);
				}

				this.output.WriteLine("\r\n");

				if (!string.IsNullOrEmpty(Event.Module))
				{
					this.output.Write('\t');
					this.output.Write(Event.Module);
				}

				if (!string.IsNullOrEmpty(Event.Facility))
				{
					this.output.Write('\t');
					this.output.Write(Event.Facility);
				}

				if (Event.Tags != null && Event.Tags.Length > 0)
				{
					this.output.WriteLine("\r\n");

					foreach (KeyValuePair<string, object> Tag in Event.Tags)
					{
						this.output.Write('\t');
						this.output.Write(Tag.Key);
						this.output.Write('=');

						if (Tag.Value != null)
							this.output.Write(Tag.Value.ToString());
					}
				}

				this.output.WriteLine("\r\n");
				this.output.WriteLine(Event.Message);

				if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
				{
					this.output.WriteLine("\r\n");
					this.output.WriteLine(Event.StackTrace);
				}
			}
			finally
			{
				this.AfterWrite();
			}
		}
	}
}
