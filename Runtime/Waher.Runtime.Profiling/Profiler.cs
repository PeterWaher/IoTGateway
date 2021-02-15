using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of events and timing.
	/// </summary>
	public class Profiler
	{
		private readonly List<ProfilerThread> threads = new List<ProfilerThread>();
		private readonly ProfilerThread mainThread;
		private readonly Stopwatch watch;

		/// <summary>
		/// Class that keeps track of events and timing.
		/// </summary>
		public Profiler()
			: this("Main")
		{
		}

		/// <summary>
		/// Class that keeps track of events and timing.
		/// </summary>
		/// <param name="Name">Name of main thread.</param>
		public Profiler(string Name)
		{
			this.mainThread = this.CreateThread(Name);
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
		/// <returns>Profiler thread reference.</returns>
		public ProfilerThread CreateThread(string Name)
		{
			ProfilerThread Result = new ProfilerThread(Name, this);
			this.threads.Add(Result);
			return Result;
		}

		/// <summary>
		/// Creates a new profiler thread.
		/// </summary>
		/// <param name="Name">Name of profiler thread.</param>
		/// <param name="Parent">Parent thread.</param>
		/// <returns>Profiler thread reference.</returns>
		internal ProfilerThread CreateThread(string Name, ProfilerThread Parent)
		{
			ProfilerThread Result = new ProfilerThread(Name, Parent);
			this.threads.Add(Result);
			return Result;
		}

		/// <summary>
		/// Starts measuring time.
		/// </summary>
		public void Start()
		{
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
		/// Main Thread changes state.
		/// </summary>
		/// <param name="State">String representation of the new state.</param>
		public void NewState(string State)
		{
			this.mainThread.NewState(State);
		}

		/// <summary>
		/// Main Thread goes idle.
		/// </summary>
		public void Idle()
		{
			this.mainThread.Idle();
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
		/// <param name="Exception">Exception object.</param>
		public void Exception(Exception Exception)
		{
			this.mainThread.Exception(Exception);
		}

		/// <summary>
		/// Exports events to XML.
		/// </summary>
		/// <returns>XML</returns>
		public string ExportXml()
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true
			};

			using (XmlWriter Output = XmlWriter.Create(sb, Settings))
			{
				this.ExportXml(Output);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Exports events to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public void ExportXml(XmlWriter Output)
		{
			Output.WriteStartElement("Profiler", "http://waher.se/schema/Profiler.xsd");

			Output.WriteEndElement();
		}
	}
}
