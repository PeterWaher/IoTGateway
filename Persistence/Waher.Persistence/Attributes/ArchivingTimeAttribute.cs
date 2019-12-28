using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute defines that objects of this type can be archived, and the time objects can be archived.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class ArchivingTimeAttribute : Attribute
	{
		private readonly int days;

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
		}

		/// <summary>
		/// Number of days to archive objects of this type. If equal to <see cref="int.MaxValue"/>, no limit is defined.
		/// </summary>
		public int Days
		{
			get { return this.days; }
		}
	}
}
