using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Abstract base class for all field filters.
	/// </summary>
	public abstract class FilterField : Filter
	{
		private readonly string fieldName;

		/// <summary>
		/// Abstract base class for all field filters.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		public FilterField(string FieldName)
			: base()
		{
			this.fieldName = FieldName;
		}

		/// <summary>
		/// FIeld Name.
		/// </summary>
		public string FieldName
		{
			get { return this.fieldName; }
		}
	}
}
