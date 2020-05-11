using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Events.Files
{
	/// <summary>
	/// Outputs sniffed data to an XML writer.
	/// </summary>
	public class XmlWriterEventSink : EventSink, IDisposable
	{
		/// <summary>
		/// XML writer object.
		/// </summary>
		protected XmlWriter output;

		private readonly object synchObject = new object();
		private bool disposed = false;

		/// <summary>
		/// Outputs sniffed data to an XML writer.
		/// </summary>
		/// <param name="ObjectID">Object ID</param>
		/// <param name="Output">Output</param>
		public XmlWriterEventSink(string ObjectID, XmlWriter Output)
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
		public override Task Queue(Event Event)
		{
			if (this.disposed)
				return Task.CompletedTask;

			lock (this.synchObject)
			{
				this.BeforeWrite();
				try
				{
					this.output.WriteStartElement(Event.Type.ToString());
					this.output.WriteAttributeString("timestamp", Encode(Event.Timestamp));
					this.output.WriteAttributeString("level", Event.Level.ToString());


					if (!string.IsNullOrEmpty(Event.EventId))
						this.output.WriteAttributeString("id", Event.EventId);

					if (!string.IsNullOrEmpty(Event.Object))
						this.output.WriteAttributeString("object", Event.Object);

					if (!string.IsNullOrEmpty(Event.Actor))
						this.output.WriteAttributeString("actor", Event.Actor);

					if (!string.IsNullOrEmpty(Event.Module))
						this.output.WriteAttributeString("module", Event.Module);

					if (!string.IsNullOrEmpty(Event.Facility))
						this.output.WriteAttributeString("facility", Event.Facility);

					this.output.WriteStartElement("Message");

					foreach (string Row in GetRows(Event.Message))
						this.output.WriteElementString("Row", Row);

					this.output.WriteEndElement();

					if (Event.Tags != null && Event.Tags.Length > 0)
					{
						foreach (KeyValuePair<string, object> Tag in Event.Tags)
						{
							this.output.WriteStartElement("Tag");
							this.output.WriteAttributeString("key", Tag.Key);

							if (Tag.Value != null)
								this.output.WriteAttributeString("value", Tag.Value.ToString());

							this.output.WriteEndElement();
						}
					}

					if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
					{
						this.output.WriteStartElement("StackTrace");

						foreach (string Row in GetRows(Event.StackTrace))
							this.output.WriteElementString("Row", Row);

						this.output.WriteEndElement();
					}

					this.output.WriteEndElement();
					this.output.Flush();
				}
				finally
				{
					this.AfterWrite();
				}
			}

			return Task.CompletedTask;
		}

		private static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

		internal static string Encode(DateTime DT)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(DT.Year.ToString("D4"));
			sb.Append('-');
			sb.Append(DT.Month.ToString("D2"));
			sb.Append('-');
			sb.Append(DT.Day.ToString("D2"));
			sb.Append('T');
			sb.Append(DT.Hour.ToString("D2"));
			sb.Append(':');
			sb.Append(DT.Minute.ToString("D2"));
			sb.Append(':');
			sb.Append(DT.Second.ToString("D2"));
			sb.Append('.');
			sb.Append(DT.Millisecond.ToString("D3"));

			if (DT.Kind == DateTimeKind.Utc)
				sb.Append("Z");

			return sb.ToString();
		}

	}
}
