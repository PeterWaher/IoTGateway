using System;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Selectable item event arguments.
	/// </summary>
	/// <param name="Item">Selectable item reference.</param>
	public class SelectableItemEventArgs(SelectableItem Item) 
		: EventArgs
	{
		/// <summary>
		/// Selectable item reference.
		/// </summary>
		public SelectableItem Item { get; } = Item;
	}
}
