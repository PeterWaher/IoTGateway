using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using F = Waher.Persistence.Filters;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// This filter selects objects that have a named field matching a given regular expression.
	/// </summary>
	public class FilterFieldLikeRegEx : F.FilterFieldLikeRegEx, IApplicableFilter
	{
		private readonly Regex regex;

		/// <summary>
		/// This filter selects objects that have a named field matching a given regular expression.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="RegularExpression">Regular expression.</param>
		public FilterFieldLikeRegEx(string FieldName, string RegularExpression)
			: base(FieldName, RegularExpression)
		{
			this.regex = new Regex(RegularExpression, RegexOptions.Singleline);
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
			if (!Serializer.TryGetFieldValue(this.FieldName, Object, out object Value))
			{
				Type T = Object.GetType();
				if (Serializer.ValueType == T)
					return false;

				if (T == this.prevType)
					Serializer = this.prevSerializer;
				else
				{
					Serializer = this.prevSerializer = Provider.GetObjectSerializer(T);
					this.prevType = T;
				}

				if (!Serializer.TryGetFieldValue(this.FieldName, Object, out Value))
					return false;
			}

			string s = Value.ToString();
			Match M = this.regex.Match(s);

			return M.Success && M.Index == 0 && M.Length == s.Length;
		}

		private Type prevType = null;
		private IObjectSerializer prevSerializer = null;

		/// <summary>
		/// Parsed regular expression.
		/// </summary>
		public Regex Regex
		{
			get { return this.regex; }
		}
	}
}
