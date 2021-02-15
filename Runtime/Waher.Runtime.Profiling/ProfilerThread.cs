using System;
using System.Collections.Generic;
using Waher.Runtime.Profiling.Events;

namespace Waher.Runtime.Profiling
{
	/// <summary>
	/// Class that keeps track of events and timing for one thread.
	/// </summary>
	public class ProfilerThread
	{
		private readonly List<ProfilerThread> subThreads = new List<ProfilerThread>();
		private readonly List<ProfilerEvent> events = new List<ProfilerEvent>();
		private readonly string name;
		private readonly Profiler profiler;
		private readonly ProfilerThread parent;

		/// <summary>
		/// Class that keeps track of events and timing for one thread.
		/// </summary>
		/// <param name="Name">Name of thread.</param>
		/// <param name="Profiler">Profiler reference.</param>
		public ProfilerThread(string Name, Profiler Profiler)
		{
			this.name = Name;
			this.profiler = Profiler;
			this.parent = null;
		}

		/// <summary>
		/// Class that keeps track of events and timing for one thread.
		/// </summary>
		/// <param name="Name">Name of thread.</param>
		/// <param name="Parent">Parent thread.</param>
		public ProfilerThread(string Name, ProfilerThread Parent)
		{
			this.name = Name;
			this.parent = Parent;
			this.profiler = this.parent.Profiler;
		}

		/// <summary>
		/// Name of thread.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Profiler reference.
		/// </summary>
		public Profiler Profiler => this.profiler;

		/// <summary>
		/// Parent profiler thread, if any.
		/// </summary>
		public ProfilerThread Parent => this.parent;

		/// <summary>
		/// Creates a new profiler thread.
		/// </summary>
		/// <param name="Name">Name of profiler thread.</param>
		/// <returns>Profiler thread reference.</returns>
		public ProfilerThread CreateSubThread(string Name)
		{
			ProfilerThread Result = this.profiler.CreateThread(Name, this);
			this.subThreads.Add(Result);
			return Result;
		}

		/// <summary>
		/// Thread changes state.
		/// </summary>
		/// <param name="State">String representation of the new state.</param>
		public void NewState(string State)
		{
			this.events.Add(new NewState(this.profiler.ElapsedTicks, State));
		}

		/// <summary>
		/// Thread goes idle.
		/// </summary>
		public void Idle()
		{
			this.events.Add(new Idle(this.profiler.ElapsedTicks));
		}

		/// <summary>
		/// Event occurred
		/// </summary>
		/// <param name="Name">Name of event.</param>
		public void Event(string Name)
		{
			this.events.Add(new Event(this.profiler.ElapsedTicks, Name));
		}

		/// <summary>
		/// Exception occurred
		/// </summary>
		/// <param name="Exception">Exception object.</param>
		public void Exception(System.Exception Exception)
		{
			this.events.Add(new Events.Exception(this.profiler.ElapsedTicks, Exception));
		}

		/// <summary>
		/// Processing starts.
		/// </summary>
		public void Start()
		{
			this.events.Add(new Start(this.profiler.ElapsedTicks));
		}

		/// <summary>
		/// Processing starts.
		/// </summary>
		public void Stop()
		{
			this.events.Add(new Stop(this.profiler.ElapsedTicks));
		}
	}
}
