namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes
{
	/// <summary>
	/// Identifies a Field Type in a TEDS.
	/// </summary>
	public struct ClassTypePair
	{
		/// <summary>
		/// Identifies a Field Type in a TEDS.
		/// </summary>
		/// <param name="Class">TEDS class.</param>
		/// <param name="Type">Field Type in class.</param>
		public ClassTypePair(byte Class, byte Type)
		{
			this.Class = Class;
			this.Type = Type;
		}

		/// <summary>
		/// TEDS class.
		/// </summary>
		public byte Class;

		/// <summary>
		/// Field Type in class.
		/// </summary>
		public byte Type;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is ClassTypePair P &&
				this.Class == P.Class &&
				this.Type == P.Type;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.Class.GetHashCode();
			Result ^= Result << 5 ^ this.Type.GetHashCode();

			return Result;
		}
	}
}
