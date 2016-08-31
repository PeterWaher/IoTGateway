using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Objects.VectorSpaces
{
	/// <summary>
	/// Object-valued vector.
	/// </summary>
	public sealed class ObjectVector : VectorSpaceElement
	{
		private IElement[] values = null;
		private ICollection<IElement> elements;
		private int dimension;

		/// <summary>
		/// Object-valued vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		public ObjectVector(ICollection<IElement> Elements)
		{
			this.elements = Elements;
			this.dimension = Elements.Count;
		}

		/// <summary>
		/// Object-valued vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		public ObjectVector(ICollection<object> Elements)
		{
			LinkedList<IElement> Elements2 = new LinkedList<IElement>();

			foreach (object Obj in Elements)
				Elements2.AddLast(Expression.Encapsulate(Obj));

			this.elements = Elements2;
			this.dimension = Elements2.Count;
		}

		/// <summary>
		/// Object-valued vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		public ObjectVector(Array Elements)
		{
			LinkedList<IElement> Elements2 = new LinkedList<IElement>();

			foreach (object Obj in Elements)
				Elements2.AddLast(Expression.Encapsulate(Obj));

			this.elements = Elements2;
			this.dimension = Elements2.Count;
		}

		/// <summary>
		/// Object-valued vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		public ObjectVector(params IElement[] Elements)
			: this((ICollection<IElement>)Elements)
		{
		}

		/// <summary>
		/// Object-valued vector.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		public ObjectVector(params object[] Elements)
			: this((ICollection<object>)Elements)
		{
		}

		/// <summary>
		/// Vector element values.
		/// </summary>
		public IElement[] Values
		{
			get
			{
				if (this.values == null)
				{
					IElement[] v = new IElement[this.dimension];
					this.elements.CopyTo(v, 0);
					this.values = v;
				}

				return this.values;
			}
		}

		/// <summary>
		/// Vector elements.
		/// </summary>
		public ICollection<IElement> Elements
		{
			get
			{
				return this.elements;
			}
		}

		/// <summary>
		/// Dimension of vector.
		/// </summary>
		public override int Dimension
		{
			get { return this.dimension; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = null;

			foreach (IElement Element in this.elements)
			{
				if (sb == null)
					sb = new StringBuilder("[");
				else
					sb.Append(", ");

				sb.Append(Element.ToString());
			}

			if (sb == null)
				return "[]";
			else
			{
				sb.Append(']');
				return sb.ToString();
			}
		}

		/// <summary>
		/// Associated Right-VectorSpace.
		/// </summary>
		public override IVectorSpace AssociatedVectorSpace
		{
			get
			{
				if (this.associatedVectorSpace == null)
					this.associatedVectorSpace = new ObjectVectors(this);

				return this.associatedVectorSpace;
			}
		}

		private ObjectVectors associatedVectorSpace = null;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue
		{
			get
			{
				if (this.associatedObjectValue != null)
					return this.associatedObjectValue;

				object[] V = new object[this.dimension];
				int i = 0;

				foreach (IElement E in this.elements)
				{
					if (E == null)
						V[i++] = null;
					else
						V[i++] = E.AssociatedObjectValue;
				}

				this.associatedObjectValue = V;
				return this.associatedObjectValue;
			}
		}

		private object[] associatedObjectValue = null;

		/// <summary>
		/// Tries to multiply a scalar to the current element.
		/// </summary>
		/// <param name="Scalar">Scalar to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IVectorSpaceElement MultiplyScalar(IFieldElement Scalar)
		{
			LinkedList<IElement> Elements = new LinkedList<IElement>();

			foreach (IElement Element in this.elements)
				Elements.AddLast(Operators.Arithmetics.Multiply.EvaluateMultiplication(Scalar, Element, null));

			return new ObjectVector(Elements);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			LinkedList<IElement> Elements = new LinkedList<IElement>();

			if (Element.IsScalar)
				return null;

			ICollection<IElement> ChildElements = Element.ChildElements;
			IEnumerator<IElement> e1 = this.elements.GetEnumerator();
			IEnumerator<IElement> e2 = ChildElements.GetEnumerator();

			while (e1.MoveNext() && e2.MoveNext())
				Elements.AddLast(Operators.Arithmetics.Add.EvaluateAddition(e1.Current, e2.Current, null));

			return new ObjectVector(Elements);
		}

		/// <summary>
		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			LinkedList<IElement> Elements = new LinkedList<IElement>();

			foreach (IElement Element in this.elements)
				Elements.AddLast(Operators.Arithmetics.Negate.EvaluateNegation(Element));

			return new ObjectVector(Elements);
		}

		/// <summary>
		/// Compares the element to another.
		/// </summary>
		/// <param name="obj">Other element to compare against.</param>
		/// <returns>If elements are equal.</returns>
		public override bool Equals(object obj)
		{
			ObjectVector ObjectVector = obj as ObjectVector;
			if (ObjectVector == null)
				return false;

			if (ObjectVector.dimension != this.dimension)
				return false;

			IEnumerator<IElement> e1 = this.elements.GetEnumerator();
			IEnumerator<IElement> e2 = ObjectVector.elements.GetEnumerator();

			while (e1.MoveNext() && e2.MoveNext())
			{
				if (!e1.Current.Equals(e2.Current))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates a hash code of the element.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			int Result = 0;

			foreach (IElement Element in this.elements)
				Result ^= Element.GetHashCode();

			return Result;
		}

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public override bool IsScalar
		{
			get { return false; }
		}

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public override ICollection<IElement> ChildElements
		{
			get { return this.Elements; }
		}

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public override IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			return VectorDefinition.Encapsulate(Elements, true, Node);
		}

		/// <summary>
		/// Returns the zero element of the group.
		/// </summary>
		public override IAbelianGroupElement Zero
		{
			get
			{
				return this.AssociatedVectorSpace.Zero;
			}
		}

		/// <summary>
		/// Gets an element of the vector.
		/// </summary>
		/// <param name="Index">Zero-based index into the vector.</param>
		/// <returns>Vector element.</returns>
		public override IElement GetElement(int Index)
		{
			if (Index < 0 || Index >= this.elements.Count)
				throw new ScriptException("Index out of bounds.");

			IElement[] V = this.Values;
			return V[Index];
		}

		/// <summary>
		/// Sets an element in the vector.
		/// </summary>
		/// <param name="Index">Index.</param>
		/// <param name="Value">Element to set.</param>
		public override void SetElement(int Index, IElement Value)
		{
			if (Index < 0 || Index >= this.dimension)
				throw new ScriptException("Index out of bounds.");

			IElement[] Values = this.Values;
			this.elements = null;

			Values[Index] = Value;
		}

		/// <summary>
		/// Converts the value to a .NET type.
		/// </summary>
		/// <param name="DesiredType">Desired .NET type.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public override bool TryConvertTo(Type DesiredType, out object Value)
		{
			if (DesiredType == typeof(object[]))
			{
				Value = this.Values;
				return true;
			}
			else if (DesiredType.IsAssignableFrom(typeof(ObjectVector)))
			{
				Value = this;
				return true;
			}
			else if (DesiredType.IsArray)
			{
				Type ElementType = DesiredType.GetElementType();
				Array Result = Array.CreateInstance(ElementType, this.dimension);
				IElement[] Values = this.Values;
				int i;

				for (i = 0; i < this.dimension; i++)
				{
					if (!Values[i].TryConvertTo(ElementType, out Value))
					{
						Value = null;
						return false;
					}

					Result.SetValue(Value, i);
				}

				Value = Result;
				return true;
			}
			else
			{
				Value = null;
				return false;
			}
		}

	}
}
