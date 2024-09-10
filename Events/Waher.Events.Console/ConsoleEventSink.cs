using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Console;

namespace Waher.Events.Console
{
	/// <summary>
	/// Outputs events to the console standard output.
	/// </summary>
	public class ConsoleEventSink : EventSink
	{
		private const int TabWidth = 8;
		private readonly bool beep;
		private readonly bool includeStackTraces = false;
		private bool consoleWidthWorks = true;

		/// <summary>
		/// Outputs events to the console standard output.
		/// </summary>
		public ConsoleEventSink()
			: this(true, false)
		{
		}

		/// <summary>
		/// Outputs events to the console standard output.
		/// </summary>
		/// <param name="Beep">Beep if events of type <see cref="EventType.Critical"/>, <see cref="EventType.Alert"/>
		/// or <see cref="EventType.Emergency"/> are logged.</param>
		public ConsoleEventSink(bool Beep)
			: this(Beep, false)
		{
		}

		/// <summary>
		/// Outputs events to the console standard output.
		/// </summary>
		/// <param name="Beep">Beep if events of type <see cref="EventType.Critical"/>, <see cref="EventType.Alert"/>
		/// or <see cref="EventType.Emergency"/> are logged.</param>
		/// <param name="IncludeStackTraces">If Stack traces should be included in console output.</param>
		public ConsoleEventSink(bool Beep, bool IncludeStackTraces)
			: base("Console Event Sink")
		{
			this.beep = Beep;
			this.includeStackTraces = IncludeStackTraces;
		}

		/// <inheritdoc/>
		public override Task Queue(Event Event)
		{
			ConsoleColor? ForegroundColor;
			ConsoleColor? BackgroundColor;
			int Width = 80;
			StringBuilder sb = new StringBuilder();
			bool WriteLine = true;
			int i;

			if (this.consoleWidthWorks)
			{
				try
				{
					Width = System.Console.WindowWidth;
					WriteLine = Width > 0;

					if (!WriteLine)
						Width = 80;
				}
				catch (Exception)
				{
					this.consoleWidthWorks = false;
				}
			}

			switch (Event.Type)
			{
				case EventType.Debug:
					ForegroundColor = ConsoleColor.White;
					BackgroundColor = ConsoleColor.DarkBlue;
					break;

				case EventType.Informational:
					ForegroundColor = ConsoleColor.Green;
					BackgroundColor = ConsoleColor.Black;
					break;

				case EventType.Notice:
					ForegroundColor = ConsoleColor.White;
					BackgroundColor = ConsoleColor.Black;
					break;

				case EventType.Warning:
					ForegroundColor = ConsoleColor.Yellow;
					BackgroundColor = ConsoleColor.Black;
					break;

				case EventType.Error:
					ForegroundColor = ConsoleColor.Red;
					BackgroundColor = ConsoleColor.Black;
					break;

				case EventType.Critical:
					ForegroundColor = ConsoleColor.Yellow;
					BackgroundColor = ConsoleColor.Red;

					if (this.beep)
						ConsoleOut.Beep();
					break;

				case EventType.Alert:
					ForegroundColor = ConsoleColor.Yellow;
					BackgroundColor = ConsoleColor.DarkRed;

					if (this.beep)
						ConsoleOut.Beep();
					break;

				case EventType.Emergency:
					ForegroundColor = ConsoleColor.White;
					BackgroundColor = ConsoleColor.Magenta;

					if (this.beep)
						ConsoleOut.Beep();
					break;

				default:
					ForegroundColor = null;
					BackgroundColor = null;
					break;
			}

			if (!string.IsNullOrEmpty(Event.EventId))
			{
				sb.Append(Event.EventId);
				sb.Append(": ");
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
						i = sb.ToString().Length % TabWidth;
						sb.Append(new string(' ', TabWidth - i));
					}

					sb.Append(Part);
				}
			}
			else
				sb.Append(Event.Message);

			if (WriteLine)
				sb.AppendLine();
			else
			{
				i = sb.ToString().Length % Width;
				if (i > 0)
					sb.Append(new string(' ', Width - i));
			}

			if (this.includeStackTraces && !string.IsNullOrEmpty(Event.StackTrace))
			{
				sb.Append(Event.StackTrace);

				if (WriteLine)
					sb.AppendLine();
				else
				{
					i = sb.ToString().Length % Width;
					if (i > 0)
						sb.Append(new string(' ', Width - i));
				}
			}

			sb.Append("  ");

			this.AddTag(sb, Width, "Timestamp", Event.Timestamp.ToString(), true, WriteLine);
			this.AddTag(sb, Width, "Level", Event.Level.ToString(), false, WriteLine);

			if (!string.IsNullOrEmpty(Event.Object))
				this.AddTag(sb, Width, "Object", Event.Object, false, WriteLine);

			if (!string.IsNullOrEmpty(Event.Actor))
				this.AddTag(sb, Width, "Actor", Event.Actor, false, WriteLine);

			if (!string.IsNullOrEmpty(Event.Facility))
				this.AddTag(sb, Width, "Facility", Event.Facility, false, WriteLine);

			if (!string.IsNullOrEmpty(Event.Module))
				this.AddTag(sb, Width, "Module", Event.Module, false, WriteLine);

			if (!(Event.Tags is null) && Event.Tags.Length > 0)
			{
				foreach (KeyValuePair<string, object> P in Event.Tags)
				{
					if (P.Value is Array A)
					{
						StringBuilder sb2 = new StringBuilder();
						bool First = true;

						foreach (object Item in A)
						{
							if (First)
								First = false;
							else
								sb2.Append(", ");

							sb2.Append(Item.ToString());
						}

						this.AddTag(sb, Width, P.Key, sb2.ToString(), false, WriteLine);
					}
					else
						this.AddTag(sb, Width, P.Key, P.Value, false, WriteLine);
				}
			}

			if (WriteLine)
				sb.AppendLine();
			else
			{
				i = sb.ToString().Length % Width;
				if (i > 0)
					sb.Append(new string(' ', Width - i));
			}

			ConsoleOut.Write((Output) =>
			{
				Output.WriteLine(sb.ToString());
			}, ForegroundColor, BackgroundColor);

			return Task.CompletedTask;
		}

		private void AddTag(StringBuilder sb, int Width, string Key, object Value, bool First, bool WriteLine)
		{
			string ValueStr = Value is null ? string.Empty : Value.ToString();

			if (Width > 0)
			{
				int i = sb.ToString().Length % Width;

				if (i + Key.Length + 1 + ValueStr.Length + (First ? 0 : 2) > Width)
				{
					if (WriteLine)
					{
						sb.AppendLine();
						sb.Append("  ");
					}
					else
						sb.Append(new string(' ', Width - i + 3));

					First = true;
				}
			}

			if (!First)
				sb.Append(", ");
			else if (Width <= 0)
				sb.Append(' ');

			sb.Append(Key);
			sb.Append('=');
			sb.Append(Value);
		}

	}
}
