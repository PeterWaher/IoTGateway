using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Persistence.Serialization;

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
		private readonly GenericObject leftGen;
		private readonly GenericObject rightGen;
		private readonly object left;
		private readonly object right;
		private readonly string leftName;
		private readonly string rightName;
		private readonly bool hasLeftName;
		private readonly bool hasRightName;
		private readonly bool isLeftGen;
		private readonly bool isRightGen;

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
			this.leftGen = Left as GenericObject;
			this.leftType = this.left?.GetType();
			this.leftName = LeftName;
			this.hasLeftName = !string.IsNullOrEmpty(this.leftName);
			this.isLeftGen = !(this.leftGen is null);
			this.right = Right;
			this.rightGen = Right as GenericObject;
			this.rightType = this.right?.GetType();
			this.rightName = RightName;
			this.hasRightName = !string.IsNullOrEmpty(this.rightName);
			this.isRightGen = !(this.rightGen is null);
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
				if (this.isLeftGen && this.leftGen.TryGetFieldValue(Index, out object Value))
					return Value;
				else if (this.isRightGen && this.rightGen.TryGetFieldValue(Index, out Value))
					return Value;
				else if (this.hasLeftName && string.Compare(Index, this.leftName, true) == 0)
					return this.left;
				else if (this.hasRightName && string.Compare(Index, this.rightName, true) == 0)
					return this.right;

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
				{
					if (Rec.Indexed)
						return Rec.PI.GetValue(Rec.Left ? this.left : this.right, new object[] { Index });
					else
						return Rec.PI.GetValue(Rec.Left ? this.left : this.right);
				}
				else if (!(Rec.FI is null))
					return Rec.FI.GetValue(Rec.Left ? this.left : this.right);
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
					return new Rec() { PI = PI, Left = true };

				FI = this.leftType.GetRuntimeField(Index);
				if (!(FI is null))
					return new Rec() { FI = FI, Left = true };

				PI = this.leftType.GetRuntimeProperty("Item");
				if (!(PI is null))
					return new Rec() { PI = PI, Left = true, Indexed = true };
			}

			if (!(this.rightType is null))
			{
				PI = this.rightType.GetRuntimeProperty(Index);
				if (!(PI is null))
					return new Rec() { PI = PI };

				FI = this.rightType.GetRuntimeField(Index);
				if (!(FI is null))
					return new Rec() { FI = FI };

				PI = this.rightType.GetRuntimeProperty("Item");
				if (!(PI is null))
					return new Rec() { PI = PI, Indexed = true };
			}

			return new Rec();
		}

		private class Rec
		{
			public PropertyInfo PI;
			public FieldInfo FI;
			public bool Left;
			public bool Indexed;
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is JoinedObject Obj &&
				!((this.left is null) ^ (Obj.left is null)) &&
				!((this.right is null) ^ (Obj.right is null)) &&
				(this.left?.Equals(Obj.left) ?? true) &&
				(this.right?.Equals(Obj.right) ?? true));
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.left?.GetHashCode() ?? 0;
			Result ^= Result << 5 ^ (this.right?.GetHashCode() ?? 0);
			return Result;

		}
	}
}
