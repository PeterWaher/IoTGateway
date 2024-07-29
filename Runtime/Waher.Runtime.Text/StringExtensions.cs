namespace Waher.Runtime.Text
{
    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Gets a row from a text document, represented as a string.
        /// </summary>
        /// <param name="Text">Text document.</param>
        /// <param name="LineNumber">Line number (1=first row).</param>
        /// <returns>Row, or empty string if line number outside of available rows.</returns>
        public static string GetRow(this string Text, int LineNumber)
        {
            if (LineNumber <= 0)
                return string.Empty;

            Text = Text.Replace("\r\n", "\n").Replace('\r', '\n');

            int i = Text.IndexOf('\n');
            int j = 0;

            while (LineNumber > 1 && i >= 0)
            {
                j = i + 1;
                i = Text.IndexOf('\n', j);
                LineNumber++;
            }

            if (i < j)
                return string.Empty;
            else
                return Text.Substring(j, i - j);
        }
    }
}