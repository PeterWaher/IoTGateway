using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Operators.Assignments
{
    /// <summary>
    /// Named member Assignment operator.
    /// </summary>
    public class NamedMemberAssignment : BinaryOperator
    {
        private string name;

        /// <summary>
        /// Named member Assignment operator.
        /// </summary>
        /// <param name="NamedMember">Named member</param>
        /// <param name="Operand">Operand.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public NamedMemberAssignment(NamedMember NamedMember, ScriptNode Operand, int Start, int Length, Expression Expression)
            : base(NamedMember.Operand, Operand, Start, Length, Expression)
        {
            this.name = NamedMember.Name;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement Left = this.left.Evaluate(Variables);
            IElement Right = this.right.Evaluate(Variables);
            object LeftValue = Left.AssociatedObjectValue;
            Type Type = LeftValue.GetType();

            lock (this.synchObject)
            {
                if (Type != this.type)
                {
                    this.type = Type;
                    this.property = Type.GetProperty(this.name);
                    if (this.property != null)
                    {
                        this.field = null;
                        this.nameIndex = null;
                    }
                    else
                    {
                        this.field = Type.GetField(this.name);
                        if (this.field != null)
                            this.nameIndex = null;
                        else
                        {
#if WINDOWS_UWP
							this.property = Type.GetProperty("Item", typeof(object), NamedMember.stringType);
#else
							this.property = Type.GetProperty("Item", NamedMember.stringType);
#endif
							if (this.property == null)
                                this.nameIndex = null;
                            else if (this.nameIndex == null)
                                this.nameIndex = new string[] { this.name };
                        }
                    }
                }

                if (this.property != null)
                {
                    Type = this.property.PropertyType;
                    if (!Type.IsAssignableFrom(Right.GetType()))
                        this.property.SetValue(LeftValue, Expression.ConvertTo(Right, Type, this), this.nameIndex);
                    else
                        this.property.SetValue(LeftValue, Right, this.nameIndex);
                }
                else if (this.field != null)
                {
                    Type = this.field.FieldType;
                    if (!Type.IsAssignableFrom(Right.GetType()))
                        this.field.SetValue(Left, Expression.ConvertTo(Right, Type, this));
                    else
                        this.field.SetValue(Left, Right);
                }
                else
                    throw new ScriptRuntimeException("Member not found.", this);
            }

            return Right;
        }

        private Type type = null;
        private PropertyInfo property = null;
        private FieldInfo field = null;
        private string[] nameIndex = null;
        private object synchObject = new object();



    }
}
