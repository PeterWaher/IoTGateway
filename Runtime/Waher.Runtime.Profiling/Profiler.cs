using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using Waher.Runtime.Profiling.Events;
using Waher.Runtime.Profiling.Export;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Options for presenting time in reports.
	/// </summary>
	public enum TimeUnit
	{
		/// <summary>
		/// A time unit is selected based on amount of time, for particular event.
		/// </summary>
		DynamicPerEvent,

		/// <summary>
		/// A time unit is selected based on amount of time, for particular thread.
		/// </summary>
		DynamicPerThread,

		/// <summary>
		/// A time unit is selected based on amount of time, for entire profiling.
		/// </summary>
		DynamicPerProfiling,

		/// <summary>
		/// Time is presented in microseconds
		/// </summary>
		MicroSeconds,

		/// <summary>
		/// Time is presented in milliseconds
		/// </summary>
		MilliSeconds,

		/// <summary>
		/// Time is presented in seconds
		/// </summary>
		Seconds,

		/// <summary>
		/// Time is presented in minutes
		/// </summary>
		Minutes,

		/// <summary>
		/// Time is presented in hours
		/// </summary>
		Hours,

		/// <summary>
		/// Time is presented in days
		/// </summary>
		Days
	}

	/// <summary>
	/// Class that keeps track of events and timing.
	/// </summary>
	public class Profiler
	{
		private readonly SortedDictionary<string, int> exceptionOrdinals = new SortedDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
		private readonly SortedDictionary<string, int> eventOrdinals = new SortedDictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
		private readonly List<ProfilerThread> threads = new List<ProfilerThread>();
		private readonly SortedDictionary<int, object> notes = new SortedDictionary<int, object>();
		private readonly Dictionary<string, ProfilerThread> threadsByName = new Dictionary<string, ProfilerThread>();
		private readonly ProfilerThread mainThread;
		private readonly Stopwatch watch;
		private DateTime started = DateTime.MinValue;
		private double timeScale = 1;
		private int threadOrder = 0;

		/// <summary>
		/// Class that keeps track of events and timing.
		/// </summary>
		public Profiler()
			: this("Main", ProfilerThreadType.Sequential)
		{
		}

		/// <summary>
		/// Class that keeps track of events and timing.
		/// </summary>
		/// <param name="Name">Name of main thread.</param>
		public Profiler(string Name)
			: this(Name, ProfilerThreadType.Sequential)
		{
		}

		/// <summary>
		/// Class that keeps track of events and timing.
		/// </summary>
		/// <param name="Type">Type of profiler thread.</param>
		public Profiler(ProfilerThreadType Type)
			: this("Main", Type)
		{
		}

		/// <summary>
		/// Class that keeps track of events and timing.
		/// </summary>
		/// <param name="Name">Name of main thread.</param>
		/// <param name="Type">Type of profiler thread.</param>
		public Profiler(string Name, ProfilerThreadType Type)
		{
			this.mainThread = this.CreateThread(Name, Type);
			this.watch = new Stopwatch();
		}

		/// <summary>
		/// Main thread.
		/// </summary>
		public ProfilerThread MainThread => this.mainThread;

		/// <summary>
		/// Creates a new profiler thread.
		/// </summary>
		/// <param name="Name">Name of profiler thread.</param>
		/// <param name="Type">Type of profiler thread.</param>
		/// <returns>Profiler thread reference.</returns>
		public ProfilerThread CreateThread(string Name, ProfilerThreadType Type)
		{
			lock (this.threads)
			{
				ProfilerThread Result = new ProfilerThread(Name, ++this.threadOrder, Type, this);
				this.threads.Add(Result);
				this.threadsByName[Name] = Result;
				return Result;
			}
		}

		/// <summary>
		/// Gets a profiler thread. If none is available, a new is created.
		/// </summary>
		/// <param name="Name">Name of profiler thread.</param>
		/// <param name="Type">Type of profiler thread.</param>
		/// <returns>Profiler thread reference.</returns>
		public ProfilerThread GetThread(string Name, ProfilerThreadType Type)
		{
			lock (this.threads)
			{
				if (this.threadsByName.TryGetValue(Name, out ProfilerThread Result))
					return Result;

				Result = new ProfilerThread(Name, ++this.threadOrder, Type, this);
				this.threads.Add(Result);
				this.threadsByName[Name] = Result;
				return Result;
			}
		}

		/// <summary>
		/// Creates a new profiler thread.
		/// </summary>
		/// <param name="Name">Name of profiler thread.</param>
		/// <param name="Type">Type of profiler thread.</param>
		/// <param name="Parent">Parent thread.</param>
		/// <returns>Profiler thread reference.</returns>
		internal ProfilerThread CreateThread(string Name, ProfilerThreadType Type, ProfilerThread Parent)
		{
			lock (this.threads)
			{
				ProfilerThread Result = new ProfilerThread(Name, ++this.threadOrder, Type, Parent);
				this.threads.Add(Result);
				this.threadsByName[Name] = Result;
				return Result;
			}
		}

		/// <summary>
		/// Gets a profiler thread. If none is available, a new is created.
		/// </summary>
		/// <param name="Name">Name of profiler thread.</param>
		/// <param name="Type">Type of profiler thread.</param>
		/// <param name="Parent">Parent thread.</param>
		/// <returns>Profiler thread reference.</returns>
		internal ProfilerThread GetThread(string Name, ProfilerThreadType Type, ProfilerThread Parent)
		{
			lock (this.threads)
			{
				if (this.threadsByName.TryGetValue(Name, out ProfilerThread Result))
					return Result;

				Result = new ProfilerThread(Name, ++this.threadOrder, Type, Parent);
				this.threads.Add(Result);
				this.threadsByName[Name] = Result;
				return Result;
			}
		}

		/// <summary>
		/// Starts measuring time.
		/// </summary>
		public void Start()
		{
			this.started = DateTime.Now;
			this.watch.Start();
			this.mainThread.Start();
		}

		/// <summary>
		/// Stops measuring time.
		/// </summary>
		public void Stop()
		{
			this.mainThread.Stop();
			this.watch.Stop();
		}

		/// <summary>
		/// Elapsed ticks since start.
		/// </summary>
		public long ElapsedTicks => this.watch.ElapsedTicks;

		/// <summary>
		/// When profiling was started.
		/// </summary>
		public DateTime Started => this.started;

		/// <summary>
		/// Converts a <see cref="System.DateTime"/> to number of ticks, since start
		/// of profiling.
		/// </summary>
		/// <param name="Timepoint">Timepoint.</param>
		/// <returns>Number of ticks, since start of profiling.</returns>
		public long GetTicks(DateTime Timepoint)
		{
			double Seconds = (Timepoint - this.started).TotalSeconds;
			return (long)(Seconds * Stopwatch.Frequency + 0.5);
		}

		/// <summary>
		/// Main Thread changes state.
		/// </summary>
		/// <param name="State">String representation of the new state.</param>
		public void NewState(string State)
		{
			this.mainThread.NewState(State);
		}

		/// <summary>
		/// A new sample value has been recored
		/// </summary>
		/// <param name="Sample">New sample value.</param>
		public void NewSample(double Sample)
		{
			this.mainThread.NewSample(Sample);
		}

		/// <summary>
		/// Main Thread goes idle.
		/// </summary>
		public void Idle()
		{
			this.mainThread.Idle();
		}

		/// <summary>
		/// Sets the (binary) state of the Main Thread to "high".
		/// </summary>
		public void High()
		{
			this.mainThread.High();
		}

		/// <summary>
		/// Sets the (binary) state Main Thread to "low".
		/// </summary>
		public void Low()
		{
			this.mainThread.Low();
		}

		/// <summary>
		/// Records an interval in the main thread.
		/// </summary>
		/// <param name="From">Starting timepoint.</param>
		/// <param name="To">Ending timepoint.</param>
		/// <param name="Label">Interval label.</param>
		public void Interval(DateTime From, DateTime To, string Label)
		{
			this.mainThread.Interval(this.GetTicks(From), this.GetTicks(To), Label);
		}

		/// <summary>
		/// Records an interval in the main thread.
		/// </summary>
		/// <param name="From">Starting timepoint, in ticks.</param>
		/// <param name="To">Ending timepoint, in ticks.</param>
		/// <param name="Label">Interval label.</param>
		public void Interval(long From, long To, string Label)
		{
			this.mainThread.Interval(From, To, Label);
		}

		/// <summary>
		/// Event occurred on main thread
		/// </summary>
		/// <param name="Name">Name of event.</param>
		public void Event(string Name)
		{
			this.mainThread.Event(Name);
		}

		/// <summary>
		/// Event occurred on main thread
		/// </summary>
		/// <param name="Name">Name of event.</param>
		/// <param name="Label">Optional label.</param>
		public void Event(string Name, string Label)
		{
			this.mainThread.Event(Name, Label);
		}

		/// <summary>
		/// Event occurred on main thread
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		public void Exception(System.Exception Exception)
		{
			this.mainThread.Exception(Exception);
		}

		/// <summary>
		/// Event occurred on main thread
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <param name="Label">Optional label.</param>
		public void Exception(System.Exception Exception, string Label)
		{
			this.mainThread.Exception(Exception, Label);
		}

		/// <summary>
		/// Number of seconds, corresponding to a measured number of high-frequency clock ticks.
		/// </summary>
		/// <param name="Ticks">Ticks</param>
		/// <returns>Number of seconds.</returns>
		public double ToSeconds(long Ticks)
		{
			return ((double)Ticks) / Stopwatch.Frequency;
		}

		/// <summary>
		/// Time (amount, unit), corresponding to a measured number of high-frequency clock ticks.
		/// </summary>
		/// <param name="Ticks">Ticks</param>
		/// <param name="Thread">Thread associated with event.</param>
		/// <param name="TimeUnit">Time unit to use.</param>
		/// <returns>Corresponding time.</returns>
		public KeyValuePair<double, string> ToTime(long Ticks, ProfilerThread Thread, TimeUnit TimeUnit)
		{
			double Amount = this.ToSeconds(Ticks);
			double Reference;

			switch (TimeUnit)
			{
				case TimeUnit.MicroSeconds:
					return new KeyValuePair<double, string>(Amount * 1e6, "μs");

				case TimeUnit.MilliSeconds:
					return new KeyValuePair<double, string>(Amount * 1e3, "ms");

				case TimeUnit.Seconds:
					return new KeyValuePair<double, string>(Amount, "s");

				case TimeUnit.Minutes:
					return new KeyValuePair<double, string>(Amount / 60, "min");

				case TimeUnit.Hours:
					return new KeyValuePair<double, string>(Amount / (60 * 60), "h");

				case TimeUnit.Days:
					return new KeyValuePair<double, string>(Amount / (24 * 60 * 60), "d");

				case TimeUnit.DynamicPerProfiling:
					Reference = this.ToSeconds(this.MainThread.StoppedAt ?? this.ElapsedTicks);
					break;

				case TimeUnit.DynamicPerThread:
					Reference = this.ToSeconds(Thread.StoppedAt ?? this.ElapsedTicks);
					break;

				case TimeUnit.DynamicPerEvent:
				default:
					Reference = Amount;
					break;
			}

			Reference *= this.timeScale;

			if (Reference < 1)
			{
				Amount *= 1e3;
				Reference *= 1e3;
				if (Reference < 1)
				{
					Amount *= 1e3;
					return new KeyValuePair<double, string>(Amount, "μs");
				}
				else
					return new KeyValuePair<double, string>(Amount, "ms");
			}
			else if (Reference > 100)
			{
				Amount /= 60;
				Reference /= 60;
				if (Reference > 100)
				{
					Amount /= 60;
					Reference /= 60;
					if (Reference > 100)
					{
						Amount /= 24;
						return new KeyValuePair<double, string>(Amount, "d");
					}
					else
						return new KeyValuePair<double, string>(Amount, "h");
				}
				else
					return new KeyValuePair<double, string>(Amount, "min");
			}
			else
				return new KeyValuePair<double, string>(Amount, "s");
		}

		/// <summary>
		/// String representation of time, corresponding to a measured number of high-frequency clock ticks.
		/// </summary>
		/// <param name="Ticks">Ticks</param>
		/// <param name="Thread">Thread associated with event.</param>
		/// <param name="TimeUnit">Time unit to use.</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		/// <returns>Corresponding time as a string.</returns>
		public string ToTimeStr(long Ticks, ProfilerThread Thread, TimeUnit TimeUnit, int NrDecimals)
		{
			KeyValuePair<double, string> Time = this.ToTime(Ticks, Thread, TimeUnit);
			return Time.Key.ToString("F" + NrDecimals.ToString()) + " " + Time.Value;
		}

		/// <summary>
		/// Exports events to XML.
		/// </summary>
		/// <param name="TimeUnit">Time unit to use.</param>
		/// <returns>XML</returns>
		public string ExportXml(TimeUnit TimeUnit)
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true
			};

			using (XmlWriter Output = XmlWriter.Create(sb, Settings))
			{
				this.ExportXml(Output, TimeUnit);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Exports events to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		/// <param name="TimeUnit">Time unit to use.</param>
		public void ExportXml(XmlWriter Output, TimeUnit TimeUnit)
		{
			Output.WriteStartElement("Profiler", "http://waher.se/schema/Profiler.xsd");
			Output.WriteAttributeString("ticksPerSecond", Stopwatch.Frequency.ToString());
			Output.WriteAttributeString("timePerTick", this.ToTimeStr(1, this.mainThread, TimeUnit.DynamicPerEvent, 7));

			ProfilerThread[] Threads;

			lock (this.threads)
			{
				Threads = this.threads.ToArray();
			}

			foreach (ProfilerThread Thread in Threads)
			{
				if (Thread.Parent is null)
					Thread.ExportXml(Output, TimeUnit);
			}

			Output.WriteEndElement();
		}

		/// <summary>
		/// Exports events to PlantUML.
		/// </summary>
		/// <param name="TimeUnit">Time unit to use.</param>
		/// <returns>PlantUML</returns>
		public string ExportPlantUml(TimeUnit TimeUnit)
		{
			StringBuilder sb = new StringBuilder();
			this.ExportPlantUml(sb, TimeUnit);
			return sb.ToString();
		}

		/// <summary>
		/// Exports events to PlantUML.
		/// </summary>
		/// <param name="Output">PlantUML output.</param>
		/// <param name="TimeUnit">Time unit to use.</param>
		public void ExportPlantUml(StringBuilder Output, TimeUnit TimeUnit)
		{
			this.ExportPlantUml(Output, TimeUnit, 1000);
		}

		/// <summary>
		/// Exports events to PlantUML.
		/// </summary>
		/// <param name="Output">PlantUML output.</param>
		/// <param name="TimeUnit">Time unit to use.</param>
		/// <param name="GoalWidth">Goal width of diagram, in pixels.</param>
		public void ExportPlantUml(StringBuilder Output, TimeUnit TimeUnit, int GoalWidth)
		{
			switch (TimeUnit)
			{
				case TimeUnit.DynamicPerEvent:
				case TimeUnit.DynamicPerThread:
					throw new InvalidOperationException("Diagram requires the same time base to be used through-out.");
			}

			Output.AppendLine("@startuml");

			ProfilerThread[] Threads;

			lock (this.threads)
			{
				Threads = this.threads.ToArray();
			}

			foreach (ProfilerThread Thread in Threads)
			{
				if (Thread.Parent is null)
					Thread.ExportPlantUmlDescription(Output, TimeUnit);
			}

			lock (this.eventOrdinals)
			{
				foreach (KeyValuePair<string, int> P in this.eventOrdinals)
				{
					Output.Append("concise \"");
					Output.Append(P.Key);
					Output.Append("\" as E");
					Output.AppendLine(P.Value.ToString());
				}
			}

			lock (this.exceptionOrdinals)
			{
				foreach (KeyValuePair<string, int> P in this.exceptionOrdinals)
				{
					Output.Append("concise \"");
					Output.Append(P.Key);
					Output.Append("\" as X");
					Output.AppendLine(P.Value.ToString());
				}
			}

			double TimeSpan;
			double StepSize;
			int NrSteps;

			do
			{
				KeyValuePair<double, string> TotalTime = this.ToTime(this.mainThread.StoppedAt ?? this.ElapsedTicks, this.mainThread, TimeUnit);

				TimeSpan = TotalTime.Key;
				StepSize = Math.Pow(10, Math.Round(Math.Log10(TimeSpan / 10)));
				NrSteps = (int)Math.Floor(TimeSpan / StepSize);

				if (NrSteps >= 50)
					StepSize *= 5;
				else if (NrSteps >= 25)
					StepSize *= 2.5;
				else if (NrSteps >= 20)
					StepSize *= 2;
				else if (NrSteps <= 2)
					StepSize /= 5;
				else if (NrSteps <= 4)
					StepSize /= 2.5;
				else if (NrSteps <= 5)
					StepSize /= 2;

				if (StepSize < 1)
					this.timeScale *= 1e-3;
			}
			while (StepSize < 1 && StepSize > 0);

			StepSize = Math.Floor(StepSize);
			NrSteps = (int)Math.Floor(TimeSpan / StepSize);
			int PixelsPerStep = NrSteps > 0 ? GoalWidth / NrSteps : 0;

			Output.Append("scale ");
			Output.Append(StepSize.ToString("F0"));
			Output.Append(" as ");
			Output.Append(PixelsPerStep);
			Output.AppendLine(" pixels");

			PlantUmlStates States = new PlantUmlStates(TimeUnit);

			foreach (ProfilerThread Thread in Threads)
			{
				if (Thread.Parent is null)
					Thread.ExportPlantUmlEvents(States);
			}

			foreach (KeyValuePair<long, StringBuilder> P in States.ByTime)
			{
				KeyValuePair<double, string> Time = this.ToTime(P.Key, null, TimeUnit);
				Output.Append('@');
				Output.AppendLine(Time.Key.ToString("F7").Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."));
				Output.Append(P.Value.ToString());
			}

			Output.Append(States.Summary.ToString());
			Output.AppendLine("@enduml");
		}

		/// <summary>
		/// Gets the ordinal for an event.
		/// </summary>
		/// <param name="Event">Event</param>
		/// <returns>Event Ordinal</returns>
		public int GetEventOrdinal(string Event)
		{
			return GetOrdinal(this.eventOrdinals, Event);
		}

		private static int GetOrdinal(SortedDictionary<string, int> Ordinals, string Key)
		{
			lock (Ordinals)
			{
				if (Ordinals.TryGetValue(Key, out int Ordinal))
					return Ordinal;

				Ordinal = Ordinals.Count;
				Ordinals[Key] = Ordinal;

				return Ordinal;
			}
		}

		/// <summary>
		/// Gets the ordinal for a type of exception.
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		/// <returns>Exception Type Ordinal</returns>
		public int GetExceptionOrdinal(System.Exception Exception)
		{
			return GetOrdinal(this.exceptionOrdinals, Exception.GetType().FullName);
		}

		/// <summary>
		/// Adds a note to the profile.
		/// </summary>
		/// <param name="Note">Note to add.</param>
		/// <returns>Note index. First note added receives index 1.</returns>
		public int AddNote(object Note)
		{
			int Result;

			lock (this.notes)
			{
				Result = this.notes.Count + 1;
				this.notes[Result] = Note;
			}

			return Result;
		}

		/// <summary>
		/// Number of notes added.
		/// </summary>
		public int NoteCount
		{
			get
			{
				lock (this.notes)
				{
					return this.notes.Count;
				}
			}
		}

		/// <summary>
		/// Tries to get a note from the profile.
		/// </summary>
		/// <param name="Index">1-based note index.</param>
		/// <param name="Note">Note, if found.</param>
		/// <returns>If a note was found with the corresponding index.</returns>
		public bool TryGetNote(int Index, out object Note)
		{
			lock (this.notes)
			{
				return this.notes.TryGetValue(Index, out Note);
			}
		}
	}
}
