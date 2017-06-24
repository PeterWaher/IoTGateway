using System;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script.Objects
{
    /// <summary>
    /// Namespace.
    /// </summary>
    public sealed class Namespace : Element
    {
        private static readonly Namespaces associatedSet = new Namespaces();

        private string value;

        /// <summary>
        /// Namespace value.
        /// </summary>
        /// <param name="Value">Namespace value.</param>
        public Namespace(string Value)
        {
            this.value = Value;
        }

        /// <summary>
        /// Namespace.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return this.value;
        }

        /// <summary>
        /// Associated Set.
        /// </summary>
        public override ISet AssociatedSet
        {
            get { return associatedSet; }
        }

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue
        {
            get { return this; }
        }

        /// <summary>
        /// <see cref="Object.Equals(object)"/>
        /// </summary>
        public override bool Equals(object obj)
        {
            Namespace E = obj as Namespace;
            if (E == null)
                return false;
            else
                return this.value == E.value;
        }

        /// <summary>
        /// <see cref="Object.GetHashCode()"/>
        /// </summary>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// Access to types and subnamespaces in the current namespace.
        /// </summary>
        /// <param name="Name">Name of local element.</param>
        /// <returns>Local element reference.</returns>
        public IElement this[string Name]
        {
            get
            {
                string FullName = this.value + "." + Name;
                Type T;

                T = Types.GetType(FullName);
                if (T != null)
                    return new TypeValue(T);

                if (Types.IsSubNamespace(this.value, Name))
                    return new Namespace(FullName);

                throw new ScriptException("No namespace or type named '" + Name + "'.");
            }
        }

    }
}
