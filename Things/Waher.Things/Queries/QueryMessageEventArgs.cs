using System;

namespace Waher.Things.Queries
{
	/// <summary>
	/// Query event type.
	/// </summary>
	public enum QueryEventType
	{
		/// <summary>
		/// Informational event.
		/// </summary>
		Information,

		/// <summary>
		/// Warning event.
		/// </summary>
		Warning,

		/// <summary>
		/// Error event.
		/// </summary>
		Error,

		/// <summary>
		/// Exception event.
		/// </summary>
		Exception
	}

	/// <summary>
	/// Event level.
	/// </summary>
	public enum QueryEventLevel
	{
		/// <summary>
		/// Minor event
		/// </summary>
		Minor,

		/// <summary>
		/// Medium event
		/// </summary>
		Medium,

		/// <summary>
		/// Major event
		/// </summary>
		Major
	}

	/// <summary>
	/// Defines a query message.
	/// </summary>
	public class QueryMessageEventArgs : QueryEventArgs
	{
		private readonly QueryEventType type;
		private readonly QueryEventLevel level;
		private readonly string body;

		/// <summary>
		/// Defines a query message.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Type">Event Type.</param>
		/// <param name="Level">Event Level.</param>
		/// <param name="Body">Event message body.</param>
		public QueryMessageEventArgs(Query Query, QueryEventType Type, QueryEventLevel Level, string Body)
			: base(Query)
		{
			this.type = Type;
			this.level = Level;
			this.body = Body;
		}

		/// <summary>
		/// Event Type.
		/// </summary>
		public QueryEventType Type
		{
			get { return this.type; }
		}

		/// <summary>
		/// Event Level.
		/// </summary>
		public QueryEventLevel Level
		{
			get { return this.level; }
		}

		/// <summary>
		/// Event message body.
		/// </summary>
		public string Body
		{
			get { return this.body; }
		}
	}
}
