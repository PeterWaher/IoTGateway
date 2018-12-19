using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Dynamic member operator
	/// </summary>
	public class DynamicMember : BinaryOperator 
	{
		/// <summary>
		/// Dynamic member operator
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DynamicMember(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
            IElement Operand = this.left.Evaluate(Variables);
            IElement Name = this.right.Evaluate(Variables);

            return EvaluateDynamicMember(Operand, Name, this);
		}

		/// <summary>
		/// Evaluates a dynamic member.
		/// </summary>
		/// <param name="Operand">Operand</param>
		/// <param name="Member">Member</param>
		/// <param name="Node">Script node.</param>
		/// <returns>Resulting value.</returns>
        public static IElement EvaluateDynamicMember(IElement Operand, IElement Member, ScriptNode Node)
        {
            if (Member.IsScalar)
            {
                StringValue s = Member as StringValue;
                if (s is null)
                    throw new ScriptRuntimeException("Member names must be strings.", Node);

                return NamedMember.EvaluateDynamic(Operand, s.Value, Node);
            }
            else
            {
                if (Operand.IsScalar)
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();

                    foreach (IElement E in Member.ChildElements)
                        Elements.AddLast(EvaluateDynamicMember(Operand, E, Node));

                    return Member.Encapsulate(Elements, Node);
                }
                else
                {
                    ICollection <IElement> OperandElements = Operand.ChildElements;
                    ICollection<IElement> MemberElements = Member.ChildElements;

                    if (OperandElements.Count == MemberElements.Count)
                    {
                        LinkedList<IElement> Elements = new LinkedList<IElement>();
                        IEnumerator<IElement> eOperand = OperandElements.GetEnumerator();
                        IEnumerator<IElement> eMember = MemberElements.GetEnumerator();

                        try
                        {
                            while (eOperand.MoveNext() && eMember.MoveNext())
                                Elements.AddLast(EvaluateDynamicMember(eOperand.Current, eMember.Current, Node));
                        }
                        finally
                        {
                            eOperand.Dispose();
                            eMember.Dispose();
                        }

                        return Operand.Encapsulate(Elements, Node);
                    }
                    else
                    {
                        LinkedList<IElement> OperandResult = new LinkedList<IElement>();

                        foreach (IElement OperandChild in OperandElements)
                        {
                            LinkedList<IElement> MemberResult = new LinkedList<IElement>();

                            foreach (IElement MemberChild in MemberElements)
                                MemberResult.AddLast(EvaluateDynamicMember(OperandChild, MemberChild, Node));

                            OperandResult.AddLast(Member.Encapsulate(MemberResult, Node));
                        }

                        return Operand.Encapsulate(OperandResult, Node);
                    }
                }
            }
        }

	}
}
