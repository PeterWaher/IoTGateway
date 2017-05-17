using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using F = Waher.Persistence.Filters;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// This filter selects objects that conform to any of the child-filters provided.
	/// </summary>
	public class FilterOr : F.FilterOr, IApplicableFilter
	{
		private IApplicableFilter[] applicableFilters;

		/// <summary>
		/// This filter selects objects that conform to any of the child-filters provided.
		/// </summary>
		/// <param name="Filters">Child filters.</param>
		internal FilterOr(IApplicableFilter[] ApplicableFilters, F.Filter[] Filters)
			: base(Filters)
		{
			this.applicableFilters = ApplicableFilters;
		}

		/// <summary>
		/// Checks if the filter applies to the object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Serializer">Corresponding object serializer.</param>
		/// <returns>If the filter can be applied.</returns>
		public bool AppliesTo(object Object, IObjectSerializer Serializer)
		{
			foreach (IApplicableFilter F in this.applicableFilters)
			{
				if (F.AppliesTo(Object, Serializer))
					return true;
			}

			return false;
		}
	}
}
