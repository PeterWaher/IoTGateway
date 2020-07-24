using System;
using System.Reflection;

namespace Waher.Persistence.Filters
{
	/// <summary>
	/// Abstract base class for all field filters.
	/// </summary>
	public abstract class FilterField : Filter, ICustomFilter
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

		/// <summary>
		/// Checks if an object passes the test or not.
		/// </summary>
		/// <param name="Object">Untyped object</param>
		/// <returns>If the object passes the test.</returns>
		public bool Passes(object Object)
		{
			if (Object is null)
				return false;

			try
			{
				Type T = Object.GetType();
				PropertyInfo PI = T.GetRuntimeProperty(this.fieldName);
				object Value;

				if (!(PI is null))
					Value = PI.GetValue(Object);
				else
				{
					FieldInfo FI = T.GetRuntimeField(this.fieldName);
					if (!(FI is null))
						Value = FI.GetValue(Object);
					else
						return false;
				}

				return this.Compare(Value);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Performs a comparison on the object with the field value <paramref name="Value"/>.
		/// </summary>
		/// <param name="Value">Field value for comparison.</param>
		/// <returns>Result of comparison.</returns>
		public abstract bool Compare(object Value);
	}
}
