namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Abstract base class for item lists
	/// </summary>
	public abstract class ItemList : BlockElement
	{
		private Item[] items;

		/// <summary>
		/// Items
		/// </summary>
		public Item[] Items
		{
			get => this.items;
			set => this.items = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				if (this.items is null)
					return false;

				foreach (Item E in this.items)
				{
					if (E is null || !E.IsWellDefined)
						return false;
				}

				return true;
			}
		}

	}
}
