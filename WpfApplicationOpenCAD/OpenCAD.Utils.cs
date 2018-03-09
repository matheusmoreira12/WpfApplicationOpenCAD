using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCAD
{
    namespace Utils
    {
        public static class StringUtils
        {
            public static readonly char[] NUMERAL_CHARSET = GetCharRange('0', '9');
            public static readonly char[] DECIMAL_SIGN_CHARACTERS = new[] { '+', '-' };
            public static readonly char[] DECIMAL_CHARSET = NUMERAL_CHARSET.Concat(new[] { DECIMAL_EXPONENT_CHARACTER,
                DECIMAL_SEPARATOR }).Concat(DECIMAL_SIGN_CHARACTERS).ToArray();
            public static readonly char[] LOWER_LETTERS_CHARSET = GetCharRange('a', 'z');
            public static readonly char[] UPPER_LETTERS_CHARSET = GetCharRange('A', 'Z');
            public static readonly char[] LETTERS_CHARSET = LOWER_LETTERS_CHARSET.Concat(UPPER_LETTERS_CHARSET).ToArray();
            public static readonly char[] WORD_CHARSET = NUMERAL_CHARSET.Concat(LETTERS_CHARSET).Concat(new char[] { '_' }).ToArray();

            public const char DECIMAL_SEPARATOR = '.';
            public const char DECIMAL_EXPONENT_CHARACTER = 'e';

            public static char[] GetCharRange(int start, int end)
            {
                return Unicode.UnicodeCodePoint.GetRange(start, end).Select(p => (char)p).ToArray();
            }

            public static bool CharIsNumeral(char c)
            {
                return NUMERAL_CHARSET.Contains(c);
            }

            public static bool CharIsDecimal(char c)
            {
                return DECIMAL_CHARSET.Contains(c);
            }

            public static bool CharIsLetter(char c)
            {
                return LETTERS_CHARSET.Contains(c);
            }

            public static bool CharIsUpperLetter(char c)
            {
                return UPPER_LETTERS_CHARSET.Contains(c);
            }

            public static bool CharIsLowerLetter(char c)
            {
                return LOWER_LETTERS_CHARSET.Contains(c);
            }

            public static bool CharIsWord(char c)
            {
                return WORD_CHARSET.Contains(c);
            }

            public static bool ReadIdentifier(StringScanner scanner, out string identifier)
            {
                using (var token = scanner.SaveIndex())
                {

                    if (CharIsLetter(scanner.CurrentChar))
                    {
                        scanner.Increment();

                        while (CharIsWord(scanner.CurrentChar))
                            scanner.Increment();

                        identifier = scanner.GetString(token);
                        return true;
                    }

                    scanner.RestoreIndex(token);
                    identifier = null;
                    return false;
                }
            }

            private static bool skipNumeralChars(StringScanner scanner)
            {
                var result = false;

                while (CharIsNumeral(scanner.CurrentChar))
                {
                    scanner.Increment();
                    result = true;
                }

                return result;
            }

            private static bool skipSignChars(StringScanner scanner)
            {
                var result = false;

                while (DECIMAL_SIGN_CHARACTERS.Contains(scanner.CurrentChar))
                {
                    scanner.Increment();
                    result = true;
                }

                return result;
            }

            public static bool ReadDecimalString(StringScanner scanner, out string decimalStr,
                out bool isFloatingPoint, out bool hasExponent)
            {
                using (var t = scanner.SaveIndex())
                {
                    isFloatingPoint = false;
                    hasExponent = false;

                    if (CharIsDecimal(scanner.CurrentChar))
                    {
                        scanner.Increment();

                        skipNumeralChars(scanner);

                        if (scanner.CurrentChar == DECIMAL_SEPARATOR)
                        {
                            scanner.Increment();

                            isFloatingPoint = true;

                            skipNumeralChars(scanner);
                        }

                        if (scanner.CurrentChar == DECIMAL_EXPONENT_CHARACTER)
                        {
                            scanner.Increment();

                            if (skipSignChars(scanner))
                                isFloatingPoint = true;

                            if (skipNumeralChars(scanner))
                                hasExponent = true;
                            else
                                throw new InvalidOperationException($"Unexpected token \"{scanner.CurrentChar}\". " +
                                    "Expected an exponent quantifier instead.");
                        }

                        decimalStr = scanner.GetString(t);
                        return true;
                    }

                    decimalStr = null;
                    scanner.RestoreIndex(t);
                    return false;
                }
            }
        }

        public delegate void StringScannerIteratorMethod(StringScanner scanner);

        public sealed class StringScannerSaveIndexToken : IDisposable
        {
            private bool disposedValue = false;
            private StringScanner parentScanner;

            public void Dispose()
            {
                if (!disposedValue)
                    parentScanner.SavedIndexTokenDisposed(this);

                disposedValue = true;
            }

            internal StringScannerSaveIndexToken(StringScanner parentScanner)
            {
                this.parentScanner = parentScanner;

                parentScanner.SavedIndexTokenCreated(this);
            }
        }

        public class StringScanner
        {
            /// <summary>
            /// The text content being scanned.
            /// </summary>
            public string Content { get; private set; }

            /// <summary>
            /// The scan position inside the string.
            /// </summary>
            public int CurrentIndex { get; private set; }

            /// <summary>
            /// The scan position at which the scan will start.
            /// </summary>
            public int StartIndex { get; private set; }

            private Dictionary<StringScannerSaveIndexToken, int> savedIndexTokens = new Dictionary<StringScannerSaveIndexToken, int> { };

            /// <summary>
            /// Saves the current index and returns the saved index token.
            /// </summary>
            /// <returns>The saved index token object.</returns>
            public StringScannerSaveIndexToken SaveIndex()
            {
                return new StringScannerSaveIndexToken(this);
            }

            /// <summary>
            /// Gets the index of the specified saved index token.
            /// </summary>
            /// <param name="token">The saved index token object.</param>
            internal int GetIndex(StringScannerSaveIndexToken token)
            {
                return savedIndexTokens[token];
            }

            /// <summary>
            /// Gets the index relative to the specified saved index token.
            /// </summary>
            /// <param name="token">The saved index token object.</param>
            internal int GetRelativeIndex(StringScannerSaveIndexToken token)
            {
                return CurrentIndex - GetIndex(token);
            }

            /// <summary>
            /// Restores the saved index from the specified saved index token.
            /// </summary>
            /// <param name="token">The saved index token object.</param>
            public void RestoreIndex(StringScannerSaveIndexToken token)
            {
                CurrentIndex = GetIndex(token);
            }

            /// <summary>
            /// Gets the string, starting at the specified saved index token, trimming start and end by the specified amount.
            /// </summary>
            /// <param name="token">The saved index token object.</param>
            /// <param name="trimStart">The amount to trim the start of the string.</param>
            /// <param name="trimEnd">The amount to trim the end of the string.</param>
            /// <returns></returns>
            public string GetString(StringScannerSaveIndexToken token, int trimStart = 0, int trimEnd = 0)
            {
                int start = GetIndex(token) + trimStart;

                return Content.Substring(start, CurrentIndex - start - trimEnd);
            }

            internal void SavedIndexTokenCreated(StringScannerSaveIndexToken token)
            {
                savedIndexTokens.Add(token, CurrentIndex);
            }

            internal void SavedIndexTokenDisposed(StringScannerSaveIndexToken token)
            {
                savedIndexTokens.Remove(token);
            }

            /// <summary>
            /// Increments the scan position by 1.
            /// </summary>
            public void Increment()
            {
                CurrentIndex++;
            }

            /// <summary>
            /// Decrements the scan position by 1.
            /// </summary>
            public void Decrement()
            {
                CurrentIndex++;
            }

            /// <summary>
            /// Gets the current char being scanned
            /// </summary>
            public char CurrentChar
            {
                get
                {
                    if (CurrentIndex < Content.Length)
                        return Content[CurrentIndex];
                    else
                        return (char)0;
                }
            }

            /// <summary>
            /// Creates an instance of a string scanner.
            /// </summary>
            /// <param name="content">The content that is being scanned through.</param>
            /// <param name="startIndex">The index at which to start scanning.</param>
            public StringScanner(string content, int startIndex = 0)
            {
                Content = content;
                CurrentIndex = startIndex;
            }
        }

        public sealed class StringWriterSaveContentToken : IDisposable
        {
            private bool disposedValue = false;
            private StringWriter parentWriter;

            void IDisposable.Dispose()
            {
                if (!disposedValue)
                    parentWriter.SavedContentTokenDisposed(this);

                disposedValue = true;
            }

            internal StringWriterSaveContentToken(StringWriter parentWriter)
            {
                this.parentWriter = parentWriter;

                parentWriter.SavedContentTokenCreated(this);
            }
        }

        public class StringWriter
        {
            /// <summary>
            /// Gets or sets the current content string.
            /// </summary>
            public string Content;

            private Dictionary<StringWriterSaveContentToken, string> savedContentTokens =
                new Dictionary<StringWriterSaveContentToken, string> { };

            /// <summary>
            /// Saves a copy of the current content and returns a saved content token.
            /// </summary>
            /// <returns>The resulting saved content token object.</returns>
            public StringWriterSaveContentToken SaveContent()
            {
                return new StringWriterSaveContentToken(this);
            }

            /// <summary>
            /// Gets the saved content from the specified saved content token.
            /// </summary>
            /// <param name="token">The saved content token object.</param>
            /// <returns>The saved string.</returns>
            public string GetSavedContent(StringWriterSaveContentToken token)
            {
                return savedContentTokens[token];
            }

            /// <summary>
            /// Restores the saved content from the specified saved content token.
            /// </summary>
            /// <param name="token">The saved content token object.</param>
            public void RestoreContent(StringWriterSaveContentToken token)
            {
                Content = GetSavedContent(token);
            }

            internal void SavedContentTokenCreated(StringWriterSaveContentToken token)
            {
                savedContentTokens.Add(token, Content);
            }

            internal void SavedContentTokenDisposed(StringWriterSaveContentToken token)
            {
                savedContentTokens.Remove(token);
            }

            public StringWriter(string initialContent)
            {
                Content = initialContent;
            }
        }
    }
}