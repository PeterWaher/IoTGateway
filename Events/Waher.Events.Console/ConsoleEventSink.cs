using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Events.Console
{
	/// <summary>
	/// Outputs events to the console standard output.
	/// </summary>
	public class ConsoleEventSink : EventSink
	{
		private const int TabWidth = 8;
		private bool beep;

		/// <summary>
		/// Outputs events to the console standard output.
		/// </summary>
		public ConsoleEventSink()
			: this(true)
		{
		}

		/// <summary>
		/// Outputs events to the console standard output.
		/// </summary>
		/// <param name="Beep">Beep if events of type <see cref="EventType.Critical"/>, <see cref="EventType.Alert"/>
		/// or <see cref="EventType.Emergency"/> are logged.</param>
		public ConsoleEventSink(bool Beep)
			: base("Console Event Sink")
		{
			this.beep = Beep;
		}

		/// <summary>
		/// <see cref="IEventSink.Queue"/>
		/// </summary>
		public override void Queue(Event Event)
		{
			StringBuilder sb = new StringBuilder();
			ConsoleColor FgBak = System.Console.ForegroundColor;
			ConsoleColor BgBak = System.Console.BackgroundColor;
			int Width;
			StringBuilder Output = new StringBuilder();
			bool WriteLine = false;
			int i;

			try
			{
				Width = System.Console.WindowWidth;

				if (System.Console.CursorLeft > 1)
					System.Console.Out.WriteLine();
			}
			catch (Exception)
			{
				Width = 80;
				WriteLine = true;
			}

			try
			{
				switch (Event.Type)
				{
					case EventType.Debug:
						System.Console.ForegroundColor = ConsoleColor.White;
						System.Console.BackgroundColor = ConsoleColor.DarkBlue;
						break;

					case EventType.Informational:
						System.Console.ForegroundColor = ConsoleColor.Green;
						System.Console.BackgroundColor = ConsoleColor.Black;
						break;

					case EventType.Notice:
						System.Console.ForegroundColor = ConsoleColor.White;
						System.Console.BackgroundColor = ConsoleColor.Black;
						break;

					case EventType.Warning:
						System.Console.ForegroundColor = ConsoleColor.Yellow;
						System.Console.BackgroundColor = ConsoleColor.Black;
						break;

					case EventType.Error:
						System.Console.ForegroundColor = ConsoleColor.Red;
						System.Console.BackgroundColor = ConsoleColor.Black;
						break;

					case EventType.Critical:
						System.Console.ForegroundColor = ConsoleColor.Yellow;
						System.Console.BackgroundColor = ConsoleColor.Red;

						if (this.beep)
							System.Console.Beep();
						break;

					case EventType.Alert:
						System.Console.ForegroundColor = ConsoleColor.Yellow;
						System.Console.BackgroundColor = ConsoleColor.DarkRed;

						if (this.beep)
							System.Console.Beep();
						break;

					case EventType.Emergency:
						System.Console.ForegroundColor = ConsoleColor.White;
						System.Console.BackgroundColor = ConsoleColor.Magenta;

						if (this.beep)
							System.Console.Beep();
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

				if (WriteLine)
					Output.AppendLine();
				else
				{
					i = Output.ToString().Length % Width;
					if (i > 0)
						Output.Append(new string(' ', Width - i));
				}

				Output.Append("  ");

				this.AddTag(Output, Width, "Timestamp", Event.Timestamp.ToString(), true, WriteLine);
				this.AddTag(Output, Width, "Level", Event.Level.ToString(), false, WriteLine);

				if (!string.IsNullOrEmpty(Event.Object))
					this.AddTag(Output, Width, "Object", Event.Object, false, WriteLine);

				if (!string.IsNullOrEmpty(Event.Actor))
					this.AddTag(Output, Width, "Actor", Event.Actor, false, WriteLine);

				if (!string.IsNullOrEmpty(Event.Facility))
					this.AddTag(Output, Width, "Facility", Event.Facility, false, WriteLine);

				if (!string.IsNullOrEmpty(Event.Module))
					this.AddTag(Output, Width, "Module", Event.Module, false, WriteLine);

				foreach (KeyValuePair<string, object> P in Event.Tags)
					this.AddTag(Output, Width, P.Key, P.Value, false, WriteLine);

				if (WriteLine)
					Output.AppendLine();
				else
				{
					i = Output.ToString().Length % Width;
					if (i > 0)
						Output.Append(new string(' ', Width - i));
				}

				System.Console.Out.WriteLine(Output.ToString());
			}
			finally
			{
				System.Console.ForegroundColor = FgBak;
				System.Console.BackgroundColor = BgBak;
			}
		}

		private void AddTag(StringBuilder Output, int Width, string Key, object Value, bool First, bool WriteLine)
		{
			string ValueStr = Value == null ? string.Empty : Value.ToString();
			int i = Output.ToString().Length % Width;

			if (i + Key.Length + 1 + ValueStr.Length + (First ? 0 : 2) > Width)
			{
				if (WriteLine)
				{
					Output.AppendLine();
					Output.Append("  ");
				}
				else
					Output.Append(new string(' ', Width - i + 3));

				First = true;
			}

			if (!First)
				Output.Append(", ");

			Output.Append(Key);
			Output.Append('=');
			Output.Append(Value);
		}

	}
}
