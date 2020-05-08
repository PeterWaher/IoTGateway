using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.LoginMonitor
{
	/// <summary>
	/// Event arguments for endpoint annotation events.
	/// </summary>
	public class AnnotateEndpointEventArgs : EventArgs
	{
		private readonly List<KeyValuePair<string, object>> tags = new List<KeyValuePair<string, object>>();
		private readonly string remoteEndpoint;

		/// <summary>
		/// Event arguments for endpoint annotation events.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		public AnnotateEndpointEventArgs(string RemoteEndpoint)
		{
			this.remoteEndpoint = RemoteEndpoint;
		}

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public string RemoteEndpoint => this.remoteEndpoint;

		/// <summary>
		/// Gets annotations, in the form of an array of tags.
		/// </summary>
		/// <returns>Array of tags.</returns>
		public KeyValuePair<string,object>[] GetTags()
		{
			return this.tags.ToArray();
		}

		/// <summary>
		/// Adds a tag to the list of tags.
		/// </summary>
		/// <param name="Key">Tag key name.</param>
		/// <param name="Value">Tag value.</param>
		public void AddTag(string Key, object Value)
		{
			this.tags.Add(new KeyValuePair<string, object>(Key, Value));
		}
	}
}
