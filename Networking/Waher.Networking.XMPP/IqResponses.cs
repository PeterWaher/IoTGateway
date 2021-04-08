using System;
using System.Collections.Generic;

namespace Waher.Networking.XMPP
{
	internal class IqResponse
	{
		public DateTime Expires;
		public string Xml;
		public string Key;
		public bool Ok;
	}

	/// <summary>
	/// Maintains a set of IQ responses, for a limited time.
	/// </summary>
	public class IqResponses : IDisposable
	{
		private readonly TimeSpan timeout;
		private readonly LinkedList<IqResponse> byTime = new LinkedList<IqResponse>();
		private readonly Dictionary<string, IqResponse> byKey = new Dictionary<string, IqResponse>();
		private readonly object synchObj = new object();

		/// <summary>
		/// Maintains a set of IQ responses, for a limited time.
		/// </summary>
		/// <param name="Timeout">Time to keep responses in memory</param>
		public IqResponses(TimeSpan Timeout)
		{
			this.timeout = Timeout;
		}

		/// <summary>
		/// Adds a response.
		/// </summary>
		/// <param name="To">To whom.</param>
		/// <param name="Id">ID of request.</param>
		/// <param name="Xml">XML of response.</param>
		/// <param name="Ok">If response is a result (true) or an error (false).</param>
		public void Add(string To, string Id, string Xml, bool Ok)
		{
			string Key = To + " " + Id;
			IqResponse Response = new IqResponse()
			{
				Key = Key,
				Ok = Ok,
				Expires = DateTime.Now.Add(this.timeout),
				Xml = Xml
			};

			lock (this.synchObj)
			{
				this.RemoveOldLocked();

				this.byTime.AddLast(Response);
				this.byKey[Key] = Response;
			}
		}

		/// <summary>
		/// Tries to get an existing response from memory.
		/// </summary>
		/// <param name="To">Recipient.</param>
		/// <param name="Id">ID of request.</param>
		/// <param name="Xml">XML of response, if found.</param>
		/// <param name="Ok">If response is a result (true) or an error (false), if found.</param>
		/// <returns>If a response was found or not.</returns>
		public bool TryGet(string To, string Id, out string Xml, out bool Ok)
		{
			string Key = To + " " + Id;

			lock (this.synchObj)
			{
				this.RemoveOldLocked();

				if (this.byKey.TryGetValue(Key,out IqResponse Response))
				{
					Xml = Response.Xml;
					Ok = Response.Ok;

					return true;
				}
			}

			Xml = null;
			Ok = false;

			return false;
		}

		/// <summary>
		/// Removes all responses from memory.
		/// </summary>
		public void Clear()
		{
			lock (this.synchObj)
			{
				this.byKey.Clear();
				this.byTime.Clear();
			}
		}

		private void RemoveOldLocked()
		{
			LinkedListNode<IqResponse> Loop = this.byTime.First;
			if (Loop is null)
				return;

			DateTime TP = DateTime.Now;
			LinkedListNode<IqResponse> Temp;

			while (!(Loop is null) && Loop.Value.Expires <= TP)
			{
				this.byKey.Remove(Loop.Value.Key);

				Temp = Loop.Next;
				this.byTime.Remove(Loop);
				Loop = Temp;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			lock (this.synchObj)
			{
				this.byKey.Clear();
				this.byTime.Clear();
			}
		}
	}
}
