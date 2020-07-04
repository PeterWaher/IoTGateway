using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using F = Waher.Persistence.Filters;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Custom filter used to filter objects using an external expression.
	/// </summary>
	public class FilterCustom : F.Filter, IApplicableFilter, F.ICustomFilter
	{
		private readonly F.ICustomFilter customFilter;

		/// <summary>
		/// Custom filter used to filter objects using an external expression.
		/// </summary>
		internal FilterCustom(F.ICustomFilter CustomFilter)
		{
			this.customFilter = CustomFilter;
		}

		/// <summary>
		/// Gets an array of constant fields. Can return null, if there are no constant fields.
		/// </summary>
		public string[] ConstantFields => null;

		/// <summary>
		/// Checks if the filter applies to the object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Serializer">Corresponding object serializer.</param>
		/// <param name="Provider">Files provider.</param>
		/// <returns>If the filter can be applied.</returns>
		public bool AppliesTo(object Object, IObjectSerializer Serializer, FilesProvider Provider)
		{
			return this.customFilter.Passes(Object);
		}

		/// <summary>
		/// Creates a copy of the filter.
		/// </summary>
		/// <returns>Copy of filter.</returns>
		public override Filter Copy()
		{
			return new FilterCustom(this.customFilter);
		}

		/// <summary>
		/// Calculates the logical inverse of the filter.
		/// </summary>
		/// <returns>Logical inverse of the filter.</returns>
		public override Filter Negate()
		{
			return new FilterNot(this);
		}

		/// <summary>
		/// Returns a normalized filter.
		/// </summary>
		/// <returns>Normalized filter.</returns>
		public override Filter Normalize()
		{
			return this.Copy();
		}

		/// <summary>
		/// Checks if an object passes the test or not.
		/// </summary>
		/// <param name="Object">Untyped object</param>
		/// <returns>If the object passes the test.</returns>
		public bool Passes(object Object)
		{
			return this.customFilter.Passes(Object);
		}
	}
}
