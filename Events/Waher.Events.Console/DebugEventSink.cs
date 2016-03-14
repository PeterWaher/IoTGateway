using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events.Debug
{
	/// <summary>
	/// Outputs events to the console standard output.
	/// </summary>
	public class DebugEventSink : EventSink
	{
		private const int TabWidth = 8;

		/// <summary>
		/// Outputs events to the console standard output.
		/// </summary>
		public DebugEventSink()
			: base("Debug Event Sink")
		{
		}

		/// <summary>
		/// <see cref="IEventSink.Queue"/>
		/// </summary>
		public override void Queue(Event Event)
		{
			StringBuilder Output = new StringBuilder();
			int i;

			switch (Event.Type)
			{
				case EventType.Debug:
					Output.Append("DEBUG: ");
					break;

				case EventType.Informational:
					Output.Append("INFO: ");
					break;

				case EventType.Notice:
					Output.Append("NOTICE: ");
					break;

				case EventType.Warning:
					Output.Append("WARNING: ");
					break;

				case EventType.Error:
					Output.Append("ERROR: ");
					break;

				case EventType.Critical:
					Output.Append("CRITICAL: ");
					break;

				case EventType.Alert:
					Output.Append("ALERT: ");
					break;

				case EventType.Emergency:
					Output.Append("EMERGENCY: ");
					break;
			}

			if (!string.IsNullOrEmpty(Event.EventId))
			{
				Output.Append(Event.EventId);
				Output.Append(": ");
			}

			if (Event.Message.IndexOf('\t') >= 0)
			{
				string[] Parts = Event.Message.Split('\t');
				bool First = true;

				foreach (string Part in Parts)
				{
					if (First)
						First = false;
					else
					{
						i = Output.ToString().Length % TabWidth;
						Output.Append(new string(' ', TabWidth - i));
					}

					Output.Append(Part);
				}
			}
			else
				Output.Append(Event.Message);

			Output.AppendLine();
			Output.Append("  ");

			this.AddTag(Output, "Timestamp", Event.Timestamp.ToString(), true);
			this.AddTag(Output, "Level", Event.Level.ToString(), false);

			if (!string.IsNullOrEmpty(Event.Object))
				this.AddTag(Output, "Object", Event.Object, false);

			if (!string.IsNullOrEmpty(Event.Actor))
				this.AddTag(Output, "Actor", Event.Actor, false);

			if (!string.IsNullOrEmpty(Event.Facility))
				this.AddTag(Output, "Facility", Event.Facility, false);

			if (!string.IsNullOrEmpty(Event.Module))
				this.AddTag(Output, "Module", Event.Module, false);

			foreach (KeyValuePair<string, object> P in Event.Tags)
				this.AddTag(Output, P.Key, P.Value, false);

			Output.AppendLine();

			System.Diagnostics.Debug.WriteLine(Output.ToString());
		}

		private void AddTag(StringBuilder Output, string Key, object Value, bool First)
		{
			if (!First)
				Output.Append(", ");

			Output.Append(Key);
			Output.Append('=');
			Output.Append(Value);
		}

	}
}
