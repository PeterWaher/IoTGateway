using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Represents a joined object.
	/// </summary>
	public class JoinedObject
	{
		private readonly Dictionary<string, Rec> properties = new Dictionary<string, Rec>();
		private readonly Type leftType;
		private readonly Type rightType;
		private readonly object left;
		private readonly object right;
		private readonly string leftName;
		private readonly string rightName;
		private readonly bool hasLeftName;
		private readonly bool hasRightName;

		/// <summary>
		/// Represents a joined object.
		/// </summary>
		/// <param name="Left">Left object</param>
		/// <param name="LeftName">Name of left source.</param>
		/// <param name="Right">Right object</param>
		/// <param name="RightName">Name of right source.</param>
		public JoinedObject(object Left, string LeftName, object Right, string RightName)
		{
			this.left = Left;
			this.leftType = this.left?.GetType();
			this.leftName = LeftName;
			this.hasLeftName = !string.IsNullOrEmpty(this.leftName);
			this.right = Right;
			this.rightType = this.right?.GetType();
			this.rightName = RightName;
			this.hasRightName = !string.IsNullOrEmpty(this.rightName);
		}

		/// <summary>
		/// Gets property of field values from the joined object.
		/// </summary>
		/// <param name="Index">Property or field name.</param>
		/// <returns>Property or field value, if exists; null otherwise.</returns>
		public object this[string Index]
		{
			get
			{
				Rec Rec;

				lock (this.properties)
				{
					if (!this.properties.TryGetValue(Index, out Rec))
					{
						Rec = this.GetRecLocked(Index);
						this.properties[Index] = Rec;
					}
				}

				if (!(Rec.PI is null))
					return Rec.PI.GetValue(Rec.Left ? this.left : this.right);
				else if (!(Rec.FI is null))
					return Rec.FI.GetValue(Rec.Left ? this.left : this.right);
				else if (this.hasLeftName && string.Compare(Index, this.leftName, true) == 0)
					return this.left;
				else if (this.hasRightName && string.Compare(Index, this.rightName, true) == 0)
					return this.right;
				else
					return null;
			}
		}

		private Rec GetRecLocked(string Index)
		{
			PropertyInfo PI;
			FieldInfo FI;

			if (!(this.leftType is null))
			{
				PI = this.leftType.GetRuntimeProperty(Index);
				if (!(PI is null))
					return new Rec() { PI = PI, FI = null, Left = true };

				FI = this.leftType.GetRuntimeField(Index);
				if (!(FI is null))
					return new Rec() { PI = null, FI = FI, Left = true };
			}

			if (!(this.rightType is null))
			{
				PI = this.rightType.GetRuntimeProperty(Index);
				if (!(PI is null))
					return new Rec() { PI = PI, FI = null, Left = false };

				FI = this.rightType.GetRuntimeField(Index);
				if (!(FI is null))
					return new Rec() { PI = null, FI = FI, Left = false };
			}

			return new Rec() { PI = null, FI = null, Left = false };
		}

		private class Rec
		{
			public PropertyInfo PI;
			public FieldInfo FI;
			public bool Left;
		}

	}
}
