using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for multivariate funcions.
    /// </summary>
    public abstract class FunctionMultiVariate : Function
    {
		/// <summary>
		/// One scalar parameter.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes1Scalar = new ArgumentType[] { ArgumentType.Scalar };

		/// <summary>
		/// Two scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes2Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Three scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes3Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Four scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes4Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		
		/// <summary>
		/// Five scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes5Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		
		/// <summary>
		/// Six scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes6Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		
		/// <summary>
		/// Seven scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes7Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		
		/// <summary>
		/// Eight scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes8Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };
		
		/// <summary>
		/// Nine scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes9Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };


		private ScriptNode[] arguments;
        private ArgumentType[] argumentTypes;
        private bool allNormal;
        private int nrArguments;

        /// <summary>
        /// Base class for funcions of one variable.
        /// </summary>
        /// <param name="Arguments">Arguments.</param>
        /// <param name="ArgumentTypes">Argument Types.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionMultiVariate(ScriptNode[] Arguments, ArgumentType[] ArgumentTypes, int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
			if (Arguments.Length != ArgumentTypes.Length)
				throw new ArgumentException("Size of ArgumentTypes must match the size of Arguments.", "ArgumentTypes");

            this.arguments = Arguments;
            this.argumentTypes = ArgumentTypes;
            this.nrArguments = this.arguments.Length;

            this.allNormal = true;
            foreach (ArgumentType Type in this.argumentTypes)
            {
                if (Type != ArgumentType.Normal)
                {
                    this.allNormal = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Function arguments.
        /// </summary>
        public ScriptNode[] Arguments
        {
            get { return this.arguments; }
        }

        /// <summary>
        /// Function argument types.
        /// </summary>
        public ArgumentType[] ArgumentTypes
        {
            get { return this.argumentTypes; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement[] Arg = new IElement[this.nrArguments];
			ScriptNode Node;
            int i;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (Node == null)
					Arg[i] = ObjectValue.Null;
				else
					Arg[i] = Node.Evaluate(Variables);
			}

            if (this.allNormal)
                return this.Evaluate(Arg, Variables);
            else
                return this.EvaluateCanonicalExtension(Arg, Variables);
        }

        private IElement EvaluateCanonicalExtension(IElement[] Arguments, Variables Variables)
        {
            ICollection<IElement> ChildElements;
            IEnumerator<IElement>[] e = new IEnumerator<IElement>[this.nrArguments];
            IElement Argument;
            Encapsulation Encapsulation = null;
            IMatrix M;
            ISet S;
            IVectorSpaceElement V;
            int Dimension = -1;
            int i, j;

            for (i = 0; i < this.nrArguments; i++)
            {
                Argument = Arguments[i];

                switch (this.argumentTypes[i])
                {
                    case ArgumentType.Normal:
                        e[i] = null;
                        break;

                    case ArgumentType.Scalar:
                        if (Argument.IsScalar)
                            e[i] = null;
                        else
                        {
                            ChildElements = Argument.ChildElements;

                            if (Dimension < 0)
                                Dimension = ChildElements.Count;
                            else if (ChildElements.Count != Dimension)
                                throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

                            e[i] = ChildElements.GetEnumerator();
                            if (Encapsulation == null)
                                Encapsulation = Argument.Encapsulate;
                        }
                        break;

                    case ArgumentType.Vector:
                        if (Argument is IVector)
                            e[i] = null;
                        else if ((M = Argument as IMatrix) != null)
                        {
                            if (Dimension < 0)
                                Dimension = M.Rows;
                            else if (M.Rows != Dimension)
                                throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

                            LinkedList<IElement> Vectors = new LinkedList<IElement>();

                            for (j = 0; j < Dimension; j++)
                                Vectors.AddLast(M.GetRow(j));

                            e[i] = Vectors.GetEnumerator();
                            if (Encapsulation == null)
                                Encapsulation = Operators.LambdaDefinition.EncapsulateToVector;
                        }
                        else if ((S = Argument as ISet) != null)
                        {
                            int? Size = S.Size;
                            if (!Size.HasValue)
                                throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

                            if (Dimension < 0)
                                Dimension = Size.Value;
                            else if (Size.Value != Dimension)
                                throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

                            e[i] = S.ChildElements.GetEnumerator();
                            if (Encapsulation == null)
                                Encapsulation = Argument.Encapsulate;
                        }
                        else
                        {
                            Arguments[i] = Operators.Vectors.VectorDefinition.Encapsulate(new IElement[] { Argument }, false, this);
                            e[i] = null;
                        }
                        break;

                    case ArgumentType.Set:
                        if (Argument is ISet)
                            e[i] = null;
                        else if ((V = Argument as IVectorSpaceElement) != null)
                        {
                            Arguments[i] = Operators.Sets.SetDefinition.Encapsulate(V.ChildElements, this);
                            e[i] = null;
                        }
                        else if ((M = Argument as IMatrix) != null)
                        {
                            if (Dimension < 0)
                                Dimension = M.Rows;
                            else if (M.Rows != Dimension)
                                throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

                            LinkedList<IElement> Vectors = new LinkedList<IElement>();

                            for (j = 0; j < Dimension; j++)
                                Vectors.AddLast(M.GetRow(j));

                            Arguments[i] = Argument = Operators.Sets.SetDefinition.Encapsulate(Vectors, this);
                            ChildElements = Argument.ChildElements;

                            e[i] = ChildElements.GetEnumerator();
                            if (Encapsulation == null)
                                Encapsulation = Operators.LambdaDefinition.EncapsulateToVector;
                        }
                        else
                        {
                            Arguments[i] = Operators.Sets.SetDefinition.Encapsulate(new IElement[] { Argument }, this);
                            e[i] = null;
                        }
                        break;

                    case ArgumentType.Matrix:
                        if (Argument is IMatrix)
                            e[i] = null;
                        else if ((V = Argument as IVectorSpaceElement) != null)
                        {
                            Arguments[i] = Operators.Matrices.MatrixDefinition.Encapsulate(V.ChildElements, 1, V.Dimension, this);
                            e[i] = null;
                        }
                        else if ((S = Argument as ISet) != null)
                        {
                            int? Size = S.Size;
                            if (!Size.HasValue)
                                throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

                            if (Dimension < 0)
                                Dimension = Size.Value;
                            else if (Size.Value != Dimension)
                                throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

                            e[i] = S.ChildElements.GetEnumerator();
                            if (Encapsulation == null)
                                Encapsulation = Argument.Encapsulate;
                        }
                        else
                        {
                            Arguments[i] = Operators.Matrices.MatrixDefinition.Encapsulate(new IElement[] { Argument }, 1, 1, this);
                            e[i] = null;
                        }
                        break;

                    default:
                        throw new ScriptRuntimeException("Unhandled argument type.", this);
                }
            }

            if (Encapsulation != null)
            {
                LinkedList<IElement> Result = new LinkedList<IElement>();
                IElement[] Arguments2 = new IElement[this.nrArguments];

                for (j = 0; j < Dimension; j++)
                {
                    for (i = 0; i < this.nrArguments; i++)
                    {
                        if (e[i] == null || !e[i].MoveNext())
                            Arguments2[i] = Arguments[i];
                        else
                            Arguments2[i] = e[i].Current;
                    }

                    Result.AddLast(this.EvaluateCanonicalExtension(Arguments2, Variables));
                }

                return Encapsulation(Result, this);
            }
            else
                return this.Evaluate(Arguments, Variables);
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Arguments">Function arguments.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public abstract IElement Evaluate(IElement[] Arguments, Variables Variables);

    }
}
