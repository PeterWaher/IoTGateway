using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using F = Waher.Persistence.Filters;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// This filter selects objects that have a named field matching a given regular expression.
	/// </summary>
	public class FilterFieldLikeRegEx : F.FilterFieldLikeRegEx, IApplicableFilter
	{
		private Regex regexCs = null;
		private Regex regexCi = null;

		/// <summary>
		/// This filter selects objects that have a named field matching a given regular expression.
		/// </summary>
		/// <param name="FieldName">Field Name.</param>
		/// <param name="RegularExpression">Regular expression.</param>
		public FilterFieldLikeRegEx(string FieldName, string RegularExpression)
			: base(FieldName, RegularExpression)
		{
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
		public async Task<bool> AppliesTo(object Object, IObjectSerializer Serializer, FilesProvider Provider)
		{
			object Value = await Serializer.TryGetFieldValue(this.FieldName, Object);

			if (Value is null)
			{
				Type T = Object.GetType();
				if (Serializer.ValueType == T)
					return false;

				if (T == this.prevType)
					Serializer = this.prevSerializer;
				else
				{
					Serializer = this.prevSerializer = await Provider.GetObjectSerializer(T);
					this.prevType = T;
				}

				Value = await Serializer.TryGetFieldValue(this.FieldName, Object);
				if (Value is null)
					return false;
			}

			Match M;

			if (Value is string s)
			{
				if (this.regexCs is null)
					this.regexCs = new Regex(this.RegularExpression, RegexOptions.Singleline);

				M = this.regexCs.Match(s);
			}
			else if (Value is CaseInsensitiveString cis)
			{
				if (this.regexCi is null)
					this.regexCi = new Regex(this.RegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase);

				M = this.regexCi.Match(s = cis.Value);
			}
			else
			{
				s = Value.ToString();

				if (!(this.regexCs is null))
					M = this.regexCs.Match(s);
				else if (!(this.regexCi is null))
					M = this.regexCi.Match(s);
				else
				{
					this.regexCs = new Regex(this.RegularExpression, RegexOptions.Singleline);
					M = this.regexCs.Match(s);
				}
			}

			return M.Success && M.Index == 0 && M.Length == s.Length;
		}

		private Type prevType = null;
		private IObjectSerializer prevSerializer = null;

		/// <summary>
		/// Parsed regular expression.
		/// </summary>
		public Regex Regex
		{
			get
			{
				if (!(this.regexCs is null))
					return this.regexCs;
				else if (!(this.regexCi is null))
					return this.regexCi;
				else
				{
					this.regexCs = new Regex(this.RegularExpression, RegexOptions.Singleline);
					return this.regexCs;
				}
			}
		}
	}
}
