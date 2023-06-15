using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Dynamic member operator
	/// </summary>
	public class DynamicMember : NullCheckBinaryOperator
	{
		/// <summary>
		/// Dynamic member operator
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public DynamicMember(ScriptNode Left, ScriptNode Right, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Left, Right, NullCheck, Start, Length, Expression)
		{
            this.isAsync = true;
        }

        /// <summary>
        /// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
        /// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
        /// </summary>
        public override bool IsAsynchronous => true;

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            return this.EvaluateAsync(Variables).Result;
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
            IElement Operand = this.left.Evaluate(Variables);
			if (this.nullCheck && Operand.AssociatedObjectValue is null)
				return Operand;

			IElement Name = this.right.Evaluate(Variables);

            return await EvaluateDynamicMember(Operand, Name, this.nullCheck, this);
		}

		/// <summary>
		/// Evaluates a dynamic member.
		/// </summary>
		/// <param name="Operand">Operand</param>
		/// <param name="Member">Member</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Node">Script node.</param>
		/// <returns>Resulting value.</returns>
		public static async Task<IElement> EvaluateDynamicMember(IElement Operand, IElement Member, bool NullCheck, ScriptNode Node)
        {
            if (Member.IsScalar)
            {
                if (!(Member.AssociatedObjectValue is string s))
                    throw new ScriptRuntimeException("Member names must be strings.", Node);

                return await NamedMember.EvaluateDynamic(Operand, s, NullCheck, Node);
            }
            else
            {
                if (Operand.IsScalar)
                {
                    LinkedList<IElement> Elements = new LinkedList<IElement>();

                    foreach (IElement E in Member.ChildElements)
                        Elements.AddLast(await EvaluateDynamicMember(Operand, E, NullCheck, Node));

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
                                Elements.AddLast(await EvaluateDynamicMember(eOperand.Current, eMember.Current, NullCheck, Node));
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
                                MemberResult.AddLast(await EvaluateDynamicMember (OperandChild, MemberChild, NullCheck, Node));

                            OperandResult.AddLast(Member.Encapsulate(MemberResult, Node));
                        }

                        return Operand.Encapsulate(OperandResult, Node);
                    }
                }
            }
        }

	}
}
