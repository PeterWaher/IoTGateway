using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
		
		private readonly object synchObject = new object();
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
		/// <see cref="IDisposableAsync.DisposeAsync"/>
		/// </summary>
		public override Task DisposeAsync()
		{
			this.disposed = true;

			this.DisposeOutput();
			
			return base.DisposeAsync();
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			if (this.disposed)
				return Task.CompletedTask;

			lock (this.synchObject)
			{
				this.BeforeWrite();
				try
				{
					if (!(this.output is null))
					{
						this.output.Write(Event.Timestamp.ToString("d"));
						this.output.Write(", ");
						this.output.Write(Event.Timestamp.ToString("T"));
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

						if (!(Event.Tags is null) && Event.Tags.Length > 0)
						{
							this.output.WriteLine("\r\n");

							foreach (KeyValuePair<string, object> Tag in Event.Tags)
							{
								this.output.Write('\t');
								this.output.Write(Tag.Key);
								this.output.Write('=');

								if (!(Tag.Value is null))
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

						this.output.Flush();
					}
				}
				catch (Exception)
				{
					try
					{
						this.DisposeOutput();
					}
					catch (Exception)
					{
						// Ignore
					}
				}
				finally
				{
					this.AfterWrite();
				}
			}

			return Task.CompletedTask;
		}
			
		/// <summary>
		/// If output can be disposed.
		/// </summary>
		public virtual bool CanDisposeOutput => true;

		/// <summary>
		/// Disposes of the current output.
		/// </summary>
		public virtual void DisposeOutput()
		{
			if (this.CanDisposeOutput)
			{
				this.output?.Flush();
				this.output?.Dispose();
			}

			this.output = null;
		}
	}
}
