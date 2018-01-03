using System.Linq;

namespace OpenCAD
{
    namespace Utils
    {
        public static class StringUtils
        {
            static public char[] NUMERAL_CHARSET { get; } = GetCharRange('0', '9');
            static public char[] LOWER_ALPHABETICAL_CHARSET = GetCharRange('a', 'z');
            static public char[] UPPER_ALPHABETICAL_CHARSET = GetCharRange('A', 'Z');
            static public char[] ALPHABETICAL_CHARSET = LOWER_ALPHABETICAL_CHARSET.Concat(UPPER_ALPHABETICAL_CHARSET).ToArray();
            static public char[] WORD_CHARSET { get; } = NUMERAL_CHARSET.Concat(ALPHABETICAL_CHARSET).Concat(new char[] { '_' }).ToArray();

            static public char[] GetCharRange(char start, char end)
            {
                return Enumerable.Range(start, end - start + 1).Select(i => (char)i).ToArray();
            }
        }
    }
}