using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Waher.Events.WindowsEventLog
{
	/// <summary>
	/// Defines an event sink that logs incoming events to a Windows Event Log.
	/// </summary>
	public class WindowsEventLog : EventSink
	{
		private IntPtr eventLog = IntPtr.Zero;

		/// <summary>
		/// Defines an event sink that logs incoming events to a Windows Event Log.
		/// 
		/// NOTE: Application needs to run with privileges to access the registry to create event logs and sources. If no such
		/// privileges are given, events will be logged to the standard Application event log.
		/// </summary>
		/// <param name="Source">The source of event log entries.</param>
		public WindowsEventLog(string Source)
			: this(Source, null)
		{
		}

		/// <summary>
		/// Defines an event sink that logs incoming events to a Windows Event Log.
		/// 
		/// NOTE: Application needs to run with privileges to access the registry to create event logs and sources. If no such
		/// privileges are given, events will be logged to the standard Application event log.
		/// </summary>
		/// <param name="Source">The source of event log entries.</param>
		/// <param name="MachineName">The computer on which the log exists.</param>
		public WindowsEventLog(string Source, string MachineName)
			: base("Windows Event Log")
		{
			this.eventLog = Win32.RegisterEventSourceW(MachineName, Source);
			if (this.eventLog == IntPtr.Zero)
			{
				this.eventLog = Win32.RegisterEventSourceW(MachineName, "Application");
				if (this.eventLog == IntPtr.Zero)
					throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (this.eventLog != IntPtr.Zero)
			{
				Win32.DeregisterEventSource(this.eventLog);
				eventLog = IntPtr.Zero;
			}
		}

		/// <summary>
		/// <see cref="EventSink.Queue(Event)"/>
		/// </summary>
		public override void Queue(Event Event)
		{
			WindowsEventType Type;
			uint EventId = 0;   // https://msdn.microsoft.com/en-us/library/aa363651(v=vs.85).aspx

			switch (Event.Type)
			{
				case EventType.Alert:
				case EventType.Critical:
				case EventType.Emergency:
				case EventType.Error:
					Type = WindowsEventType.EVENTLOG_ERROR_TYPE;
					EventId = 0b11100000000000000000000000000000;
					break;

				case EventType.Notice:
				case EventType.Warning:
					Type = WindowsEventType.EVENTLOG_WARNING_TYPE;
					EventId = 0b10100000000000000000000000000000;
					break;

				case EventType.Debug:
				case EventType.Informational:
				default:
					Type = WindowsEventType.EVENTLOG_INFORMATION_TYPE;
					EventId = 0b01100000000000000000000000000000;
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

			string s = Message.ToString();
			List<string> Strings = new List<string>();
			int i = 0;
			int c = s.Length;

			while (i < c)
			{
				if (c - i > 30000)
				{
					Strings.Add(s.Substring(i, 30000));
					i += 30000;
				}
				else
				{
					Strings.Add(s.Substring(i));
					i = c;
				}
			}

			if (!Win32.ReportEventW(this.eventLog, Type, 0, EventId, IntPtr.Zero, 1, 0, Strings.ToArray(), IntPtr.Zero))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
