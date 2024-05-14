using System;
using System.Collections.Generic;

namespace Waher.Networking.XMPP.Concentrator
{
    /// <summary>
    /// Result of a set properties operation.
    /// </summary>
    public class SetEditableFormResult
    {
        /// <summary>
        /// If any errors were encountered.
        /// </summary>
        public KeyValuePair<string, string>[] Errors;

        /// <summary>
        /// Actual property values set.
        /// </summary>
        public List<KeyValuePair<string, object>> Tags;

        /// <summary>
        /// Adds an error to the list of errors.
        /// </summary>
        /// <param name="Key">Key</param>
        /// <param name="Value">Value</param>
        public void AddError(string Key, string Value)
        {
            if (this.Errors is null)
                this.Errors = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(Key, Value) };
            else
            {
                int c = this.Errors.Length;
                Array.Resize(ref this.Errors, c + 1);
                this.Errors[c] = new KeyValuePair<string, string>(Key, Value);
            }
        }

        /// <summary>
        /// Adds a Tag to the result set.
        /// </summary>
        /// <param name="Key">Key</param>
        /// <param name="Value">Value</param>
        public void AddTag(string Key, object Value)
        {
            if (this.Tags is null)
                this.Tags = new List<KeyValuePair<string, object>>();

            this.Tags.Add(new KeyValuePair<string, object>(Key, Value));
        }
    }
}