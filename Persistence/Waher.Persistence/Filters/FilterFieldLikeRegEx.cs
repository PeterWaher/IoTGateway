using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// This filter selects objects that have a named field matching a given regular expression.
	/// </summary>
	public class FilterFieldLikeRegEx : FilterField
	{
		private string regularExpression;

		/// <summary>
		/// This filter selects objects that have a named field matching a given regular expression.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="RegularExpression">Regular expression.</param>
		public FilterFieldLikeRegEx(string FieldName, string RegularExpression)
			: base(FieldName)
		{
			this.regularExpression = RegularExpression;
		}

		/// <summary>
		/// Regular expression.
		/// </summary>
		public string RegularExpression
		{
			get
			{
				return this.regularExpression;
			}
		}
	}
}
