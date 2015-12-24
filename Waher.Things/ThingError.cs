using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Things
{
	/// <summary>
	/// Contains information about an error on a thing
	/// </summary>
	public class ThingError : ThingReference
	{
		private DateTime timestamp;
		private string errorMessage;

		/// <summary>
		/// Contains information about an error on a thing
		/// </summary>
		/// <param name="NodeId">ID of node.</param>
		/// <param name="SourceId">Optional ID of source containing node.</param>
		/// <param name="CacheType">Optional Type of cache in which the Node ID is unique.</param>
		public ThingError(string NodeId, string SourceId, string CacheType, DateTime Timestamp, string ErrorMessage)
			: base(NodeId, SourceId, CacheType)
		{
			this.timestamp = Timestamp;
			this.errorMessage = ErrorMessage;
		}

		/// <summary>
		/// Timestamp of error.
		/// </summary>
		public DateTime Timestamp { get { return this.timestamp; } }

		/// <summary>
		/// Error message.
		/// </summary>
		public string ErrorMessage { get { return this.errorMessage; } }
	}
}
