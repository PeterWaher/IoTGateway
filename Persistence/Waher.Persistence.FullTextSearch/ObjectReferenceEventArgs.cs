using System;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Event arguments for object reference events.
	/// </summary>
	public class ObjectReferenceEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for object reference events.
		/// </summary>
		/// <param name="Reference">Object reference.</param>
		public ObjectReferenceEventArgs(ObjectReference Reference)
		{
			this.Reference = Reference;
		}

		/// <summary>
		/// Object reference.
		/// </summary>
		public ObjectReference Reference { get; }
	}
}
