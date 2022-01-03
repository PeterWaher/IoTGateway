using System;

namespace Waher.Script.Units
{
	/// <summary>
	/// Represents an atomic unit.
	/// </summary>
	public sealed class AtomicUnit
	{
		private readonly string name;

		/// <summary>
		/// Represents an atomic unit.
		/// </summary>
		/// <param name="Name">Unit name.</param>
		public AtomicUnit(string Name)
		{
			this.name = Name;
		}

		/// <summary>
		/// Unit name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is AtomicUnit U)
				return this.name.Equals(U.name);
			else
				return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.name.GetHashCode();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.name;
		}
	}
}
