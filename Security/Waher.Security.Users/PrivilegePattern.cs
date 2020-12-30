using System;
using System.Text.RegularExpressions;
using Waher.Events;
using Waher.Persistence.Attributes;

namespace Waher.Security.Users
{
	/// <summary>
	/// Contains a reference to a privilege
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public sealed class PrivilegePattern
	{
		private string expression = string.Empty;
		private bool include = true;
		private Regex regex = null;

		/// <summary>
		/// Contains a reference to a privilege
		/// </summary>
		public PrivilegePattern()
		{
		}

		/// <summary>
		/// Contains a reference to a privilege
		/// </summary>
		/// <param name="Pattern">Regular expression.</param>
		/// <param name="Include">If privileges matching the pattern should be included (true) or excluded (false).</param>
		public PrivilegePattern(string Pattern, bool Include)
		{
			this.expression = Pattern;
			this.regex = new Regex(Pattern, RegexOptions.Singleline);
			this.include = Include;
		}

		/// <summary>
		/// Privilege ID regular expression to match against.
		/// </summary>
		public string Expression
		{
			get => this.expression;
			set
			{
				this.expression = value;

				try
				{
					this.regex = new Regex(this.expression, RegexOptions.Singleline);
				}
				catch (Exception ex)
				{
					Log.Error("Invalid regular expression:\r\n" + this.expression + "\r\n\r\nError reported.\r\n" + ex.Message);
					this.regex = null;
				}
			}
		}

		/// <summary>
		/// If privileges matching the pattern are included (true) or excluded (false).
		/// </summary>
		[DefaultValue(true)]
		public bool Include
		{
			get => this.include;
			set => this.include = value;
		}

		/// <summary>
		/// If the privilege is included.
		/// </summary>
		/// <param name="Privilege">Full Privilege Id</param>
		/// <returns>true=yes, false=no, null=not applicable</returns>
		public bool? IsIncluded(string Privilege)
		{
			if (this.regex is null)
				return null;

			Match M = this.regex.Match(Privilege);

			if (M.Success && M.Index == 0 && M.Length == Privilege.Length)
				return this.include;
			else
				return null;
		}
	}
}
