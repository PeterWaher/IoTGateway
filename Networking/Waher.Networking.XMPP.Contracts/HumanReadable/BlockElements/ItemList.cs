using System.Threading.Tasks;

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
			set
			{
				this.UnregisterParent(this.items);
				this.items = value;
				this.RegisterParent(this.items);
			}
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override async Task<HumanReadableElement> IsWellDefined()
		{
			if (this.items is null)
				return this;

			bool Found = false;

			foreach (Item E in this.items)
			{
				if (E is null)
					return this;

				HumanReadableElement E2 = await E.IsWellDefined();
				if (!(E2 is null))
					return E2;

				Found = true;
			}

			return Found ? null : this;
		}

	}
}
