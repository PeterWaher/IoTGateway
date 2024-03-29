﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

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
		private string objectId = null;

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
						return ScriptNode.UnnestPossibleTaskSync(Rec.PI.GetValue(Rec.Left ? this.left : this.right, new object[] { Index }));   // TODO: Async
					else
						return ScriptNode.UnnestPossibleTaskSync(Rec.PI.GetValue(Rec.Left ? this.left : this.right));   // TODO: Async
				}
				else if (!(Rec.FI is null))
					return ScriptNode.UnnestPossibleTaskSync(Rec.FI.GetValue(Rec.Left ? this.left : this.right));   // TODO: Async
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
				{
					if (PI.CanRead && PI.GetMethod.IsPublic)
						return new Rec() { PI = PI, Left = true };
					else
						return new Rec();
				}

				FI = this.leftType.GetRuntimeField(Index);
				if (!(FI is null))
				{
					if (FI.IsPublic)
						return new Rec() { FI = FI, Left = true };
					else
						return new Rec();
				}

				if (VectorIndex.TryGetIndexProperty(this.leftType, true, false, out PI, out _))
					return new Rec() { PI = PI, Left = true, Indexed = true };
			}

			if (!(this.rightType is null))
			{
				PI = this.rightType.GetRuntimeProperty(Index);
				if (!(PI is null))
				{
					if (PI.CanRead && PI.GetMethod.IsPublic)
						return new Rec() { PI = PI };
					else
						return new Rec();
				}

				FI = this.rightType.GetRuntimeField(Index);
				if (!(FI is null))
				{
					if (FI.IsPublic)
						return new Rec() { FI = FI };
					else
						return new Rec();
				}

				if (VectorIndex.TryGetIndexProperty(this.rightType, true, false, out PI, out _))
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
		/// Joined Object ID
		/// </summary>
		public string ObjectId
		{
			get
			{
				if (this.objectId is null)
				{
					StringBuilder sb = new StringBuilder();
					object Id;

					if (!(this.left is null))
					{
						Id = Database.TryGetObjectId(this.left).Result;
						if (!(Id is null))
							sb.Append(Id.ToString());
					}

					sb.Append(':');

					if (!(this.right is null))
					{
						Id = Database.TryGetObjectId(this.right).Result;
						if (!(Id is null))
							sb.Append(Id.ToString());
					}

					this.objectId = sb.ToString();
				}

				return this.objectId;
			}
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return (obj is JoinedObject Obj &&
				this.ObjectId == Obj.ObjectId);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.ObjectId.GetHashCode();

		}
	}
}
