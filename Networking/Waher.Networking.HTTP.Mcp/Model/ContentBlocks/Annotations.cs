using System;
using System.Collections.Generic;
using Waher.Content.Xml;

namespace Waher.Networking.HTTP.Mcp.Model.ContentBlocks
{
	/// <summary>
	/// Abstract base class for annotated objects.
	/// </summary>
	public class Annotations
	{
		/// <summary>
		/// Abstract base class for annotated objects.
		/// </summary>
		public Annotations()
		{
		}

		/// <summary>
		/// Abstract base class for annotated objects.
		/// </summary>
		public Annotations(McpRole[]? Audience, double? Priority, DateTime? LastModified)
		{
			this.Audience = Audience;
			this.Priority = Priority;
			this.LastModified = LastModified;
		}

		/// <summary>
		///  Describes who the intended audience of this object or data is.
		///  
		///  It can include multiple entries to indicate content useful for multiple audiences.
		/// </summary>
		public McpRole[]? Audience { get; }

		/// <summary>
		/// Describes how important this data is for operating the server.
		/// 
		/// A value of 1 means "most important," and indicates that the data is
		/// effectively required, while 0 means "least important," and indicates that
		/// the data is entirely optional.
		/// </summary>
		public double? Priority { get; }

		/// <summary>
		/// The moment the resource was last modified, as an ISO 8601 formatted string.
		/// 
		/// Examples: last activity timestamp in an open file, timestamp when the resource
		/// was attached, etc.
		/// </summary>
		public DateTime? LastModified { get; }

		/// <summary>
		/// Annotates an object.
		/// </summary>
		/// <param name="Object">Object to annotate.</param>
		public virtual void Annotate(Dictionary<string, object?> Object)
		{
			if (!(this.Audience is null))
			{
				int i, c = this.Audience.Length;
				string[] Audience = new string[c];

				for (i = 0; i < c; i++)
					Audience[i] = this.Audience[i].ToString().ToLowerInvariant();

				Object["audience"] = Audience;
			}

			if (this.Priority.HasValue)
				Object["priority"] = this.Priority.Value;

			if (this.LastModified.HasValue)
				Object["lastModified"] = XML.Encode(this.LastModified.Value.ToUniversalTime());
		}
	}
}
