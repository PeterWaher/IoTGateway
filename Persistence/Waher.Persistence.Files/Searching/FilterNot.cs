using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Serialization;
using F = Waher.Persistence.Filters;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// This filter selects objects that does not conform to the child-filter provided.
	/// </summary>
	public class FilterNot : F.FilterNot, IApplicableFilter
	{
		private readonly IApplicableFilter childFilter;

		/// <summary>
		/// This filter selects objects that does not conform to the child-filter provided.
		/// </summary>
		/// <param name="Filter">Child filter.</param>
		internal FilterNot(IApplicableFilter Filter)
			: base((F.Filter)Filter)
		{
			this.childFilter = Filter;
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
			return !this.childFilter.AppliesTo(Object, Serializer, Provider);
		}
	}
}
