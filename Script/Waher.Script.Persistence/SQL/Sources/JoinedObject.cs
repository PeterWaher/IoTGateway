using System;
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
		private readonly Dictionary<string, PropertyRecord> properties = new Dictionary<string, PropertyRecord>();
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

				PropertyRecord Rec;

				lock (this.properties)
				{
					if (!this.properties.TryGetValue(Index, out Rec))
					{
						Rec = this.GetRecLocked(Index);
						this.properties[Index] = Rec;
					}
				}

				if (!(Rec.Property is null))
				{
					if (Rec.NrIndexParameters == 1)
						return ScriptNode.UnnestPossibleTaskSync(Rec.Property.GetValue(Rec.Left ? this.left : this.right, Rec.GetIndexArguments(Index)));   // TODO: Async
					else
						return ScriptNode.UnnestPossibleTaskSync(Rec.Property.GetValue(Rec.Left ? this.left : this.right));   // TODO: Async
				}
				else if (!(Rec.Field is null))
					return ScriptNode.UnnestPossibleTaskSync(Rec.Field.GetValue(Rec.Left ? this.left : this.right));   // TODO: Async
				else
					return null;
			}
		}

		private PropertyRecord GetRecLocked(string Index)
		{
			PropertyInfo PI;
			FieldInfo FI;

			if (!(this.leftType is null))
			{
				PI = this.leftType.GetRuntimeProperty(Index);
				if (!(PI is null))
				{
					if (PI.CanRead && PI.GetMethod.IsPublic)
						return new PropertyRecord(PI, true);
					else
						return new PropertyRecord();
				}

				FI = this.leftType.GetRuntimeField(Index);
				if (!(FI is null))
				{
					if (FI.IsPublic)
						return new PropertyRecord(FI, true);
					else
						return new PropertyRecord();
				}

				if (VectorIndex.TryGetIndexProperty(this.leftType, true, false, out PI,
					out ParameterInfo[] IndexArguments))
				{
					return new PropertyRecord(PI, true, IndexArguments);
				}
			}

			if (!(this.rightType is null))
			{
				PI = this.rightType.GetRuntimeProperty(Index);
				if (!(PI is null))
				{
					if (PI.CanRead && PI.GetMethod.IsPublic)
						return new PropertyRecord(PI, false);
					else
						return new PropertyRecord();
				}

				FI = this.rightType.GetRuntimeField(Index);
				if (!(FI is null))
				{
					if (FI.IsPublic)
						return new PropertyRecord(FI, false);
					else
						return new PropertyRecord();
				}

				if (VectorIndex.TryGetIndexProperty(this.rightType, true, false, out PI,
					out ParameterInfo[] IndexArguments))
				{
					return new PropertyRecord(PI, false, IndexArguments);
				}
			}

			return new PropertyRecord();
		}

		private class PropertyRecord
		{
			public PropertyInfo Property;
			public ParameterInfo[] IndexArguments;
			public FieldInfo Field;
			public bool Left;
			public int NrIndexParameters;
			public bool IsStringIndex;

			public PropertyRecord()
			{
				this.Property = null;
				this.Field = null;
				this.Left = false;
				this.IndexArguments = null;
				this.IsStringIndex = false;
				this.NrIndexParameters = 0;
			}

			public PropertyRecord(PropertyInfo Property, bool Left)
			{
				this.Property = Property;
				this.Field = null;
				this.Left = Left;
				this.IndexArguments = null;
				this.IsStringIndex = false;
				this.NrIndexParameters = 0;
			}

			public PropertyRecord(FieldInfo Field, bool Left)
			{
				this.Property = null;
				this.Field = Field;
				this.Left = Left;
				this.IndexArguments = null;
				this.IsStringIndex = false;
				this.NrIndexParameters = 0;
			}

			public PropertyRecord(PropertyInfo Property, bool Left, ParameterInfo[] IndexArguments)
			{
				this.Property = Property;
				this.Field = null;
				this.Left = Left;
				this.IndexArguments = IndexArguments;

				if (IndexArguments is null)
				{
					this.NrIndexParameters = 0;
					this.IsStringIndex = false;
				}
				else
				{
					this.NrIndexParameters = this.IndexArguments.Length;
					this.IsStringIndex = this.NrIndexParameters == 1 &&
						this.IndexArguments[0].ParameterType == typeof(string);
				}
			}

			public PropertyRecord(FieldInfo Field, bool Left, ParameterInfo[] IndexArguments)
			{
				this.Property = null;
				this.Field = Field;
				this.Left = Left;
				this.IndexArguments = IndexArguments;

				if (IndexArguments is null)
				{
					this.NrIndexParameters = 0;
					this.IsStringIndex = false;
				}
				else
				{
					this.NrIndexParameters = this.IndexArguments.Length;
					this.IsStringIndex = this.NrIndexParameters == 1 &&
						this.IndexArguments[0].ParameterType == typeof(string);
				}
			}

			public object[] GetIndexArguments(string Name)
			{
				if (this.IsStringIndex)
					return new object[] { Name };

				object Converted = Expression.ConvertTo(Name, this.IndexArguments[0].ParameterType, null);

				return new object[] { Converted };
			}
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
