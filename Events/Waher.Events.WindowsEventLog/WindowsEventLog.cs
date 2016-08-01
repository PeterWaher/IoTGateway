using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace Waher.Events.WindowsEventLog
{
	/// <summary>
	/// Defines an event sink that logs incoming events to a Windows Event Log.
	/// </summary>
	public class WindowsEventLog : EventSink
	{
		private EventLog eventLog;

		/// <summary>
		/// Defines an event sink that logs incoming events to a Windows Event Log.
		/// 
		/// NOTE: Application needs to run with privileges to access the registry to create event logs and sources. If no such
		/// privileges are given, events will be logged to the standard Application event log.
		/// </summary>
		/// <param name="LogName">The name of the log on the specified computer</param>
		/// <param name="Source">The source of event log entries.</param>
		/// <param name="MachineName">The computer on which the log exists.</param>
		/// <param name="MaximumKilobytes">The maximum event log size in kilobytes. The default is 512, indicating a maximum
		/// file size of 512 kilobytes.</param>
		public WindowsEventLog(string LogName, string Source, int MaximumKilobytes)
			: base("Windows Event Log")
		{
			try
			{
				if (!EventLog.SourceExists(Source))
					EventLog.CreateEventSource(Source, LogName);

				this.eventLog = new EventLog(LogName);
				this.eventLog.Source = Source;
				this.eventLog.MaximumKilobytes = MaximumKilobytes;
				this.eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 7);
			}
			catch (SecurityException)
			{
				this.eventLog = new EventLog("Application");
				this.eventLog.Source = "Application";
			}
		}

		/// <summary>
		/// Defines an event sink that logs incoming events to a Windows Event Log.
		/// 
		/// NOTE: Application needs to run with privileges to access the registry to create event logs and sources. If no such
		/// privileges are given, events will be logged to the standard Application event log.
		/// </summary>
		/// <param name="LogName">The name of the log on the specified computer</param>
		/// <param name="Source">The source of event log entries.</param>
		/// <param name="MachineName">The computer on which the log exists.</param>
		/// <param name="MaximumKilobytes">The maximum event log size in kilobytes. The default is 512, indicating a maximum
		/// file size of 512 kilobytes.</param>
		public WindowsEventLog(string LogName, string Source, string MachineName, int MaximumKilobytes)
			: base("Windows Event Log")
		{
			try
			{
				if (!EventLog.SourceExists(Source, MachineName))
				{
					EventSourceCreationData Data = new EventSourceCreationData(Source, LogName);
					Data.MachineName = MachineName;
					EventLog.CreateEventSource(Data);
				}

				this.eventLog = new EventLog(LogName, MachineName, Source);
				this.eventLog.MaximumKilobytes = MaximumKilobytes;
				this.eventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 7);
			}
			catch (SecurityException)
			{
				this.eventLog = new EventLog("Application", MachineName);
				this.eventLog.Source = "Application";
			}
		}

		/// <summary>
		/// <see cref="EventSink.Queue(Event)"/>
		/// </summary>
		public override void Queue(Event Event)
		{
			EventLogEntryType Type;

			switch (Event.Type)
			{
				case EventType.Alert:
				case EventType.Critical:
				case EventType.Emergency:
				case EventType.Error:
					Type = EventLogEntryType.Error;
					break;

				case EventType.Notice:
				case EventType.Warning:
					Type = EventLogEntryType.Warning;
					break;

				case EventType.Debug:
				case EventType.Informational:
				default:
					Type = EventLogEntryType.Information;
					break;
			}

			// TODO: Identify successful and failed logins.

			StringBuilder Message = new StringBuilder(Event.Message);
			Message.AppendLine();
			Message.AppendLine();

			Message.Append("Timestamp: ");
			Message.AppendLine(Event.Timestamp.ToString());
			Message.Append("Type: ");
			Message.AppendLine(Event.Type.ToString());
			Message.Append("Level: ");
			Message.AppendLine(Event.Level.ToString());

			if (!string.IsNullOrEmpty(Event.EventId))
			{
				Message.Append("Event ID: ");
				Message.AppendLine(Event.EventId);
			}

			if (!string.IsNullOrEmpty(Event.Object))
			{
				Message.Append("Object: ");
				Message.AppendLine(Event.Object);
			}

			if (!string.IsNullOrEmpty(Event.Object))
			{
				Message.Append("Object: ");
				Message.AppendLine(Event.Object);
			}

			if (!string.IsNullOrEmpty(Event.Actor))
			{
				Message.Append("Actor: ");
				Message.AppendLine(Event.Actor);
			}

			if (!string.IsNullOrEmpty(Event.Module))
			{
				Message.Append("Module: ");
				Message.AppendLine(Event.Module);
			}

			if (!string.IsNullOrEmpty(Event.Facility))
			{
				Message.Append("Facility: ");
				Message.AppendLine(Event.Facility);
			}

			if (Event.Tags != null)
			{
				foreach (KeyValuePair<string, object> Tag in Event.Tags)
				{
					Message.AppendLine(Tag.Key);
					Message.Append(": ");

					if (Tag.Value != null)
						Message.AppendLine(Tag.Value.ToString());
				}
			}

			if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
			{
				Message.AppendLine();
				Message.AppendLine("Stack Trace:");
				Message.AppendLine(Event.StackTrace);
			}

			this.eventLog.WriteEntry(Message.ToString(), Type);
		}
	}
}
