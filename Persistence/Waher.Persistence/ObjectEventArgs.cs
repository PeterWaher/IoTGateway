using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Persistence
{
	/// <summary>
	/// Event handler for object events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ObjectEventHandler(object Sender, ObjectEventArgs e);

	/// <summary>
	/// Event arguments for database object events.
	/// </summary>
	public class ObjectEventArgs : EventArgs
	{
		private readonly object obj;

		/// <summary>
		/// Event arguments for database object events.
		/// </summary>
		/// <param name="Object">Object</param>
		public ObjectEventArgs(object Object)
		{
			this.obj = Object;
		}

		/// <summary>
		/// Object
		/// </summary>
		public object Object => this.obj;
	}
}
