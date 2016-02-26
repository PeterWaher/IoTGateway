using System;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
    /// <summary>
    /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
    /// </summary>
    public class Create : Function
    {
        private ScriptNode type;
        private ScriptNode[] parameters;
        private int nrParameters;

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Parameters">Constructor parameters.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode[] Parameters, int Start, int Length)
            : base(Start, Length)
        {
            this.type = Type;
            this.parameters = Parameters;
            this.nrParameters = Parameters.Length;
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, int Start, int Length)
            : this(Type, new ScriptNode[0], Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1 }, Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2 }, Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3 }, Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Argument4">Constructor argument 4.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
            int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4 }, Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Argument4">Constructor argument 4.</param>
        /// <param name="Argument5">Constructor argument 5.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
            ScriptNode Argument5, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5 }, Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Argument4">Constructor argument 4.</param>
        /// <param name="Argument5">Constructor argument 5.</param>
        /// <param name="Argument6">Constructor argument 6.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
            ScriptNode Argument5, ScriptNode Argument6, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6 }, Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Argument4">Constructor argument 4.</param>
        /// <param name="Argument5">Constructor argument 5.</param>
        /// <param name="Argument6">Constructor argument 6.</param>
        /// <param name="Argument7">Constructor argument 7.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
            ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7 }, Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Argument4">Constructor argument 4.</param>
        /// <param name="Argument5">Constructor argument 5.</param>
        /// <param name="Argument6">Constructor argument 6.</param>
        /// <param name="Argument7">Constructor argument 7.</param>
        /// <param name="Argument8">Constructor argument 8.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
            ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, ScriptNode Argument8, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7, Argument8 },
                  Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Argument4">Constructor argument 4.</param>
        /// <param name="Argument5">Constructor argument 5.</param>
        /// <param name="Argument6">Constructor argument 6.</param>
        /// <param name="Argument7">Constructor argument 7.</param>
        /// <param name="Argument8">Constructor argument 8.</param>
        /// <param name="Argument9">Constructor argument 9.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
            ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, ScriptNode Argument8, ScriptNode Argument9,
            int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7, Argument8, Argument9 },
                  Start, Length)
        {
        }

        /// <summary>
        /// Creates an object of a specific class. The first argument must evaluate to the type that is to be created.
        /// </summary>
        /// <param name="Type">Type.</param>
        /// <param name="Argument1">Constructor argument 1.</param>
        /// <param name="Argument2">Constructor argument 2.</param>
        /// <param name="Argument3">Constructor argument 3.</param>
        /// <param name="Argument4">Constructor argument 4.</param>
        /// <param name="Argument5">Constructor argument 5.</param>
        /// <param name="Argument6">Constructor argument 6.</param>
        /// <param name="Argument7">Constructor argument 7.</param>
        /// <param name="Argument8">Constructor argument 8.</param>
        /// <param name="Argument9">Constructor argument 9.</param>
        /// <param name="Argument10">Constructor argument 10.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Create(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
            ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, ScriptNode Argument8, ScriptNode Argument9,
            ScriptNode Argument10, int Start, int Length)
            : this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7, Argument8, Argument9, Argument10 },
                  Start, Length)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "create"; }
        }

        /// <summary>
        /// Optional aliases. If there are no aliases for the function, null is returned.
        /// </summary>
        public override string[] Aliases
        {
            get { return new string[] { "new" }; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames
        {
            get { return new string[] { "Type" }; }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            IElement E = this.type.Evaluate(Variables);
            TypeValue TV = E as TypeValue;
            if (TV == null)
                throw new ScriptRuntimeException("First argument must evaluate to the type to be created.", this);

            IElement[] Arguments = null;
            ParameterInfo[] Parameters;
            object[] ParameterValues;
            object Value;
            int i, c;

            lock (this.synchObject)
            {
                if (this.lastType == TV.Value && this.lastGenericType != null)
                {
                    c = this.genericArguments.Length;
                    if (this.nrParameters < c)
                        throw new ScriptRuntimeException("Expected " + c.ToString() + " generic type arguments.", this);

                    for (i = 0; i < c; i++)
                    {
                        E = this.parameters[i].Evaluate(Variables);
                        TV = E as TypeValue;
                        if (TV == null)
                            throw new ScriptRuntimeException("Generic type arguments must evaluate to types to be sed.", this);

                        if (this.genericArguments[i] != TV.Value)
                        {
                            this.lastType = null;
                            this.lastGenericType = null;
                            this.genericArguments = null;
                            this.constructor = null;
                            break;
                        }
                    }
                }

                if (this.lastType != TV.Value)
                {
                    Type T = TV.Value;

                    this.constructor = null;

                    if (T.ContainsGenericParameters)
                    {
                        this.genericArguments = T.GetGenericArguments();

                        c = this.genericArguments.Length;
                        if (this.nrParameters < c)
                            throw new ScriptRuntimeException("Expected " + c.ToString() + " generic type arguments.", this);

                        for (i = 0; i < c; i++)
                        {
                            E = this.parameters[i].Evaluate(Variables);
                            TV = E as TypeValue;
                            if (TV == null)
                                throw new ScriptRuntimeException("Generic type arguments must evaluate to types to be sed.", this);

                            this.genericArguments[i] = TV.Value;
                        }

                        this.lastGenericType = T.MakeGenericType(this.genericArguments);
                    }
                    else
                    {
                        this.genericArguments = null;
                        this.lastGenericType = null;
                        c = 0;
                    }

                    this.lastType = TV.Value;
                    this.constructor = null;
                }
                else if (this.genericArguments != null)
                    c = this.genericArguments.Length;
                else
                    c = 0;

                Arguments = new IElement[this.nrParameters - c];
                for (i = c; i < this.nrParameters; i++)
                    Arguments[i - c] = this.parameters[i].Evaluate(Variables);

                if (this.constructor != null)
                {
                    if (this.constructorParametersTypes.Length != this.nrParameters - c)
                        this.constructor = null;
                    else
                    {
                        for (i = c; i < this.constructorParametersTypes.Length; i++)
                        {
                            if (!Arguments[i].TryConvertTo(this.constructorParametersTypes[i].ParameterType, out Value))
                                break;

                            this.constructorArguments[i] = Value;
                        }

                        if (i < this.constructorParametersTypes.Length)
                            this.constructor = null;
                    }
                }

                if (this.constructor == null)
                {
                    ConstructorInfo[] Constructors;

                    if (this.lastGenericType != null)
                        Constructors = this.lastGenericType.GetConstructors();
                    else
                        Constructors = this.lastType.GetConstructors();

                    ParameterValues = null;

                    foreach (ConstructorInfo CI in Constructors)
                    {
                        Parameters = CI.GetParameters();
                        if (Parameters.Length != this.nrParameters - c)
                            continue;

                        for (i = c; i < Parameters.Length; i++)
                        {
                            if (!Arguments[i].TryConvertTo(Parameters[i].ParameterType, out Value))
                                break;

                            if (ParameterValues == null)
                                ParameterValues = new object[Parameters.Length];

                            ParameterValues[i] = Value;
                        }

                        if (i < Parameters.Length)
                            continue;

                        this.constructor = CI;
                        this.constructorParametersTypes = Parameters;
                        this.constructorArguments = ParameterValues;
                        break;
                    }
                    
                    if (this.constructor == null)
                        throw new ScriptRuntimeException("Invalid number or type of parameters.", this);
                }
            }

            return Expression.Encapsulate(this.constructor.Invoke(this.constructorArguments));
        }

        private Type lastType = null;
        private Type lastGenericType = null;
        private Type[] genericArguments = null;
        private ConstructorInfo constructor = null;
        private ParameterInfo[] constructorParametersTypes = null;
        private object[] constructorArguments = null;
        private object synchObject = new object();
    }
}
