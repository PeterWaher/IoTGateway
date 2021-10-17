using System;
using System.Text;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Abstract base class for contractual parameters
	/// </summary>
	public abstract class Parameter : LocalizableDescription
	{
		private string name;
		private string guide = string.Empty;
		private string exp = string.Empty;
		private Expression parsed = null;

		/// <summary>
		/// Parameter name
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Parameter guide text
		/// </summary>
		public string Guide
		{
			get => this.guide;
			set => this.guide = value;
		}

		/// <summary>
		/// Parameter validation script expression.
		/// </summary>
		public string Expression
		{
			get => this.exp;
			set
			{
				this.exp = value;
				this.parsed = null;
			}
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public abstract object ObjectValue
		{
			get;
		}

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public abstract void Serialize(StringBuilder Xml);

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>If parameter value is valid.</returns>
		public virtual bool IsParameterValid(Variables Variables)
		{
			if (!string.IsNullOrEmpty(this.exp))
			{
				try
				{
					if (this.parsed is null)
						this.parsed = new Expression(this.exp);

					object Result = this.parsed.Evaluate(Variables);

					if (!(Result is bool b) || !b)
						return false;
				}
				catch (Exception)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public abstract void Populate(Variables Variables);
	}
}
