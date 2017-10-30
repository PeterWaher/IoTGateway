using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using F = Waher.Persistence.Filters;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// This filter selects objects that conform to all child-filters provided.
	/// </summary>
	public class FilterAnd : F.FilterAnd, IApplicableFilter
	{
		private IApplicableFilter[] applicableFilters;

		/// <summary>
		/// This filter selects objects that conform to all child-filters provided.
		/// </summary>
		/// <param name="ApplicableFilters">Applicable filters.</param>
		/// <param name="Filters">Child filters.</param>
		internal FilterAnd(IApplicableFilter[] ApplicableFilters, F.Filter[] Filters)
			: base(Filters)
		{
			this.applicableFilters = ApplicableFilters;
		}

		/// <summary>
		/// Checks if the filter applies to the object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <param name="Serializer">Corresponding object serializer.</param>
		/// <param name="Provider">Files provider.</param>
		/// <returns>If the filter can be applied.</returns>
		public bool AppliesTo(object Object, IObjectSerializer Serializer, FilesProvider Provider)
		{
			foreach (IApplicableFilter F in this.applicableFilters)
			{
				if (!F.AppliesTo(Object, Serializer, Provider))
					return false;
			}

			return true;
		}
	}
}
