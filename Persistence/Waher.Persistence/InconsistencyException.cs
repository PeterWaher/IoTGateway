using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Waher.Events;

namespace Waher.Persistence
{
	/// <summary>
	/// Database inconsistency exception. Raised when an inconsistency in the database has been found.
	/// </summary>
	public class InconsistencyException : Exception, IEventObject
	{
		private readonly string collection;

		/// <summary>
		/// Database inconsistency exception. Raised when an inconsistency in the database has been found.
		/// </summary>
		public InconsistencyException(string Collection, string Message)
			: base(Message)
		{
			this.collection = Collection;
		}

		/// <summary>
		/// Object identifier related to the object. (Collection)
		/// </summary>
		public string Object => this.collection;
	}
}
