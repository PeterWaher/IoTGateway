using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute defines that objects of this type can be archived, and the time objects can be archived.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class ArchivingTimeAttribute : Attribute
	{
		private readonly int days;
		private readonly string propertyName;

		/// <summary>
		/// This attribute defines objects of this type can be archived, and that there is no time limit for how long
		/// objects of this type can be archived.
		/// </summary>
		public ArchivingTimeAttribute()
			: this(int.MaxValue)
		{
		}

		/// <summary>
		/// This attribute defines that objects of this type can be archived, and that there is a time limit for how long
		/// objects of this type can be archived.
		/// </summary>
		/// <param name="Days">Number of days to archive objects of this type. (Default=<see cref="int.MaxValue"/>, meaning no limit defined.)</param>
		public ArchivingTimeAttribute(int Days)
		{
			this.days = Days;
			this.propertyName = null;
		}

		/// <summary>
		/// This attribute defines that objects of this type can be archived, and that there is a time limit for how long
		/// objects of this type can be archived.
		/// </summary>
		/// <param name="PropertyName">A property on the object with this name defines the number of days to archive the object. (Default=<see cref="int.MaxValue"/>, meaning no limit defined.)</param>
		public ArchivingTimeAttribute(string PropertyName)
		{
			this.days = int.MaxValue;
			this.propertyName = PropertyName;
		}

		/// <summary>
		/// Number of days to archive objects of this type. If equal to <see cref="int.MaxValue"/>, no limit is defined.
		/// </summary>
		public int Days
		{
			get { return this.days; }
		}

		/// <summary>
		/// A property on the object with this name defines the number of days to archive the object. (Default=<see cref="int.MaxValue"/>, meaning no limit defined.)
		/// </summary>
		public string PropertyName
		{
			get { return this.propertyName; }
		}
	}
}
