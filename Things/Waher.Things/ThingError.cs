using System;
using System.Collections.Generic;
using System.Text;
using Waher.Events;

namespace Waher.Things
{
	/// <summary>
	/// Contains information about an error on a thing
	/// </summary>
	public class ThingError : ThingReference
	{
		private readonly DateTime timestamp;
		private readonly string errorMessage;

		/// <summary>
		/// Contains information about an error on a thing
		/// </summary>
		/// <param name="Thing">Thing reference.</param>
		/// <param name="ErrorMessage">Error message.</param>
		public ThingError(IThingReference Thing, string ErrorMessage)
			: this(Thing.NodeId, Thing.SourceId, Thing.Partition, DateTime.Now, ErrorMessage)
		{
		}

		/// <summary>
		/// Contains information about an error on a thing
		/// </summary>
		/// <param name="NodeId">ID of node.</param>
		/// <param name="SourceId">Optional ID of source containing node.</param>
		/// <param name="Partition">Optional partition in which the Node ID is unique.</param>
		/// <param name="ErrorMessage">Error message.</param>
		public ThingError(string NodeId, string SourceId, string Partition, string ErrorMessage)
			: this(NodeId, SourceId, Partition, DateTime.Now, ErrorMessage)
		{
		}

		/// <summary>
		/// Contains information about an error on a thing
		/// </summary>
		/// <param name="Thing">Thing reference.</param>
		/// <param name="Timestamp">Timestamp.</param>
		/// <param name="ErrorMessage">Error message.</param>
		public ThingError(IThingReference Thing, DateTime Timestamp, string ErrorMessage)
			: this(Thing.NodeId, Thing.SourceId, Thing.Partition, Timestamp, ErrorMessage)
		{
		}

		/// <summary>
		/// Contains information about an error on a thing
		/// </summary>
		/// <param name="NodeId">ID of node.</param>
		/// <param name="SourceId">Optional ID of source containing node.</param>
		/// <param name="Partition">Optional partition in which the Node ID is unique.</param>
		/// <param name="Timestamp">Timestamp.</param>
		/// <param name="ErrorMessage">Error message.</param>
		public ThingError(string NodeId, string SourceId, string Partition, DateTime Timestamp, string ErrorMessage)
			: base(NodeId, SourceId, Partition)
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

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder Output = new StringBuilder();

			Output.Append(this.timestamp.ToString("d"));
			Output.Append(", ");
			Output.Append(this.timestamp.ToString("T"));

			if (!string.IsNullOrEmpty(this.NodeId))
			{
				Output.Append(", ");
				Output.Append(this.NodeId);

				if (!string.IsNullOrEmpty(this.SourceId))
				{
					Output.Append(", ");
					Output.Append(this.SourceId);

					if (!string.IsNullOrEmpty(this.Partition))
					{
						Output.Append(", ");
						Output.Append(this.Partition);
					}
				}
			}

			Output.Append(": ");
			Output.Append(this.errorMessage);

			return Output.ToString();
		}
	}
}
