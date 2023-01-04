using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
    /// <summary>
    /// Orders strings in descending length order
    /// </summary>
    public class DescendingLengthOrder : IComparer<TokenCount>
    {
        /// <summary>
        /// Orders strings in descending length order
        /// </summary>
        public DescendingLengthOrder()
        {
        }

        /// <summary>
        /// <see cref="IComparer<string>.Compare"/>
        /// </summary>
        public int Compare(TokenCount x, TokenCount y)
        {
            int i = y.Token.Length - x.Token.Length;
            if (i != 0)
                return i;
            else
                return string.Compare(x.Token, y.Token);
        }
    }
}
