using System;
using System.Collections.Generic;

namespace Waher.Security.WAF
{
	/// <summary>
	/// Type of redirection
	/// </summary>
	internal enum RedirectionType
	{
		/// <summary>
		/// Temporary redirection
		/// </summary>
		TemporaryRedirection,

		/// <summary>
		/// Permanent redirection
		/// </summary>
		PermanentRedirection,

		/// <summary>
		/// See other location
		/// </summary>
		SeeOther
	}

	/// <summary>
	/// Information about a redirection.
	/// </summary>
	internal class RedirectionInfo
	{
		/// <summary>
		/// Information about a redirection.
		/// </summary>
		/// <param name="Type">Type of redirection.</param>
		/// <param name="Location">Location to redirect to.</param>
		/// <param name="Index">Index of element in the dictionary of redirections.</param>
		public RedirectionInfo(RedirectionType Type, string Location, int Index)
		{
			this.Type = Type;
			this.Location = Location;
			this.Index = Index;
		}

		/// <summary>
		/// Type of redirection
		/// </summary>
		public RedirectionType Type { get; }

		/// <summary>
		/// Location to redirect to
		/// </summary>
		public string Location { get; }

		/// <summary>
		/// Index of element in the dictionary of redirections.
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// When redirection was created.
		/// </summary>
		internal DateTime Created { get; } = DateTime.UtcNow;

		/// <summary>
		/// Index of element in the dictionary of pending redirections.
		/// </summary>
		internal LinkedListNode<RedirectionInfo> Node { get; set; }
	}
}
