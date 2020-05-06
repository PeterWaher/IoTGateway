using System;
using System.Collections.Generic;
using System.Text;

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

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is AtomicUnit U)
				return this.name.Equals(U.name);
			else
				return false;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.name.GetHashCode();
		}

		/// <summary>
		/// <see cref="Object.ToString"/>
		/// </summary>
		public override string ToString()
		{
			return this.name;
		}
	}
}
