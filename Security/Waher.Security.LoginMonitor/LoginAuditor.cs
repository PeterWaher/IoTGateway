using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace Waher.Security.LoginMonitor
{
	/// <summary>
	/// Class that monitors login events, and help applications determine malicious intent. 
	/// Register instance of class with <see cref="Log.Register(IEventSink)"/> to activate it.
	/// Call its <see cref="GetEarliestLoginOpportunity(string, string)"/> method to get information about 
	/// when a remote endpoint can login.
	/// 
	/// Stream of events are analyzed, to detect events with Event ID "LoginSuccessful" and "LoginFailure". Individual state objects 
	/// are maintained for each remote endpoint, allowing services to query the LoginAuditor class (using its 
	/// <see cref="GetEarliestLoginOpportunity(string, string)"/> method) about the current state of each remote endpoint trying to login.
	/// </summary>
	public class LoginAuditor : EventSink
	{
		private readonly Dictionary<string, RemoteEndpoint> states = new Dictionary<string, RemoteEndpoint>();
		private readonly LoginInterval[] intervals;
		private readonly int nrIntervals;

		/// <summary>
		/// Class that monitors login events, and help applications determine malicious intent. 
		/// Register instance of class with <see cref="Log.Register(IEventSink)"/> to activate it.
		/// Call its <see cref="GetEarliestLoginOpportunity(string, string)"/> method to get information about 
		/// when a remote endpoint can login.
		/// 
		/// Stream of events are analyzed, to detect events with Event ID "LoginSuccessful" and "LoginFailure". Individual state objects 
		/// are maintained for each remote endpoint, allowing services to query the LoginAuditor class (using its 
		/// <see cref="GetEarliestLoginOpportunity(string, string)"/> method) about the current state of each remote endpoint trying to login.
		/// </summary>
		/// <param name="ObjectID">Log Object ID</param>
		/// <param name="LoginIntervals">Number of login attempts possible during given time period. Numbers must be positive, and
		/// interval ascending. If continually failing past accepted intervals, remote endpoint will be registered as malicious.</param>
		public LoginAuditor(string ObjectID, params LoginInterval[] LoginIntervals)
			: base(ObjectID)
		{
			this.intervals = LoginIntervals;
			this.nrIntervals = this.intervals.Length;
		}

		/// <summary>
		/// Queues an event to be output.
		/// </summary>
		/// <param name="Event">Event to queue.</param>
		public override Task Queue(Event Event)
		{
			switch (Event.EventId)
			{
				case "LoginFailure":
					return this.ProcessLoginFailure(Event.Actor, this.FindProtocol(Event), Event.Timestamp);

				case "LoginSuccessful":
					return this.ProcessLoginSuccessful(Event.Actor, this.FindProtocol(Event));

				default:
					return Task.CompletedTask;
			}
		}

		private string FindProtocol(Event Event)
		{
			if (Event.Tags is null)
				return string.Empty;

			foreach (KeyValuePair<string, object> Tag in Event.Tags)
			{
				if (Tag.Key == "Protocol")
					return Tag.Value?.ToString();
			}

			return string.Empty;
		}

		private string RemovePort(string s)
		{
			int i = s.LastIndexOf(':');
			if (i < 0)
				return s;

			string s2 = s.Substring(0, i);

			if (int.TryParse(s.Substring(i + 1), out int _) && IPAddress.TryParse(s2, out IPAddress _))
				return s2;
			else
				return s;
		}

		private async Task<RemoteEndpoint> GetStateObject(string RemoteEndpoint, string Protocol)
		{
			RemoteEndpoint EP;

			RemoteEndpoint = this.RemovePort(RemoteEndpoint);

			lock (this.states)
			{
				if (this.states.TryGetValue(RemoteEndpoint, out EP))
					return EP;
			}

			bool Created = false;
			bool Updated = false;

			EP = await Database.FindFirstIgnoreRest<RemoteEndpoint>(new FilterFieldEqualTo("Endpoint", RemoteEndpoint), "Created");

			if (EP is null)
			{
				EP = new RemoteEndpoint()
				{
					Endpoint = RemoteEndpoint,
					LastProtocol = Protocol,
					Creted = DateTime.Now,
					Blocked = false,
					State = new int[this.nrIntervals],
					Timestamps = new DateTime[this.nrIntervals]
				};

				EP.Reset(false);
				Created = true;
			}
			else
			{
				if (EP.State is null || EP.State.Length != this.nrIntervals)
				{
					EP.State = new int[this.nrIntervals];
					EP.Timestamps = new DateTime[this.nrIntervals];

					EP.Reset(false);
					Updated = true;
				}
			}

			lock (this.states)
			{
				if (this.states.TryGetValue(RemoteEndpoint, out RemoteEndpoint EP2))
					return EP2;

				this.states[RemoteEndpoint] = EP;
			}

			if (Created)
				await Database.Insert(EP);
			else if (Updated)
				await Database.Update(EP);

			return EP;
		}

		/// <summary>
		/// Processes a successful login attempt.
		/// </summary>
		/// <param name="RemoteEndpoint">String-representation of remote endpoint.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		public async Task ProcessLoginSuccessful(string RemoteEndpoint, string Protocol)
		{
			RemoteEndpoint EP = await this.GetStateObject(RemoteEndpoint, Protocol);
			if (EP.LastProtocol == Protocol && !EP.LastFailed)
				return;

			EP.LastProtocol = Protocol;
			EP.Reset(false);

			await Database.Update(EP);
		}

		/// <summary>
		/// Processes a failed login attempt.
		/// </summary>
		/// <param name="RemoteEndpoint">String-representation of remote endpoint.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		public async Task ProcessLoginFailure(string RemoteEndpoint, string Protocol, DateTime Timestamp)
		{
			RemoteEndpoint EP = await this.GetStateObject(RemoteEndpoint, Protocol);
			int i;

			if (EP.LastFailed)
			{
				i = 0;

				while (i < this.nrIntervals)
				{
					if (EP.State[i] < this.intervals[i].NrAttempts)
					{
						EP.State[i]++;
						break;
					}
					else
					{
						EP.State[i] = 1;
						EP.Timestamps[i] = Timestamp;

						i++;
						if (i >= this.nrIntervals)
						{
							await this.Block(EP);
							return;
						}
					}
				}
			}
			else
			{
				for (i = 0; i < this.nrIntervals; i++)
				{
					EP.State[i] = 1;
					EP.Timestamps[i] = Timestamp;
				}
			}

			await Database.Update(EP);
		}

		/// <summary>
		/// Checks when a remote endpoint can login.
		/// </summary>
		/// <param name="RemoteEndpoint">String-representation of remote endpoint.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		/// <returns>When the remote endpoint is allowed to login:
		/// 
		/// null = Remote endpoint can login now.
		/// <see cref="DateTime.MaxValue"/> = Remote endpoint has been blocked and cannot login.
		/// Other <see cref="DateTime"/> values indicate when, at the earliest, the remote endpoint
		/// is allowed to login again.
		/// </returns>
		public async Task<DateTime?> GetEarliestLoginOpportunity(string RemoteEndpoint, string Protocol)
		{
			RemoteEndpoint EP = await this.GetStateObject(RemoteEndpoint, Protocol);

			if (EP.Blocked)
				return DateTime.MaxValue;

			if (!EP.LastFailed)
				return null;

			int i = 0;
			while (i < this.nrIntervals && EP.State[i] >= this.intervals[i].NrAttempts)
				i++;

			if (i == 0)
				return null;

			if (i >= this.nrIntervals)
			{
				await this.Block(EP);
				return DateTime.MaxValue;
			}

			return EP.Timestamps[i - 1].Add(this.intervals[i - 1].Interval);
		}

		private async Task Block(RemoteEndpoint EP)
		{
			EP.Blocked = true;
			Log.Warning("Remote endpoint blocked due to excessive number of failed login attempts.", 
				EP.Endpoint, this.ObjectID, "RemoteEndpointBlocked", EventLevel.Major);

			await Database.Update(EP);
		}

		/// <summary>
		/// Unblocks a remote endpoint and resets counters for it.
		/// </summary>
		/// <param name="RemoteEndpoint">String-representation of remote endpoint.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		public async Task UnblockAndReset(string RemoteEndpoint, string Protocol)
		{
			RemoteEndpoint EP = await this.GetStateObject(RemoteEndpoint, Protocol);

			EP.LastProtocol = Protocol;
			EP.Reset(true);

			await Database.Update(EP);
		}

	}
}
