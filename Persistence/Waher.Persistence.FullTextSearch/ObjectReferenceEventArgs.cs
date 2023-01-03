using System;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Delegate for object reference event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task ObjectReferenceEventHandler(object Sender, ObjectReferenceEventArgs e);

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
