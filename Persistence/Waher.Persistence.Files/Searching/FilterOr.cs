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
		/// <param name="ApplicableFilters">Applicable filters.</param>
		/// <param name="Filters">Child filters.</param>
		internal FilterOr(IApplicableFilter[] ApplicableFilters, F.Filter[] Filters)
			: base(Filters)
		{
			this.applicableFilters = ApplicableFilters;
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
			foreach (IApplicableFilter F in this.applicableFilters)
			{
				if (F.AppliesTo(Object, Serializer, Provider))
					return true;
			}

			return false;
		}
	}
}
