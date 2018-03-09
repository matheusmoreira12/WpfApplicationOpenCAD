using OpenCAD.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenCAD
{
    //Future-proof unicode support
    namespace Unicode
    {
        static class Conversions
        {
            const uint CHAR_MASK = 0xFFFF;

            const int BYTES_PER_CHAR = 2;
            const int BITS_PER_BYTE = 8;

            public static uint CharArrayToInt(char[] c)
            {
                uint result = 0;

                for (int i = 0; i < c.Length; i++)
                    result = result | (uint)c[i] << (BITS_PER_BYTE * BYTES_PER_CHAR * i);

                return result;
            }

            public static char[] IntToCharArray(uint i)
            {
                List<char> result = new List<char> { };

                while (i > 0)
                {
                    uint val = i & CHAR_MASK;

                    result.Add((char)val);

                    i >>= (BITS_PER_BYTE * BYTES_PER_CHAR);
                }

                return result.ToArray();
            }

            public static char[] UnicodePointArrayToCharArray(UnicodeCodePoint[] a)
            {
                int length = a.Length,
                    outLength = length * 2;

                char[] result = new char[outLength];

                return result;
            }
        }

        public struct UnicodeString
        {
            public static UnicodeString FromString(string s)
            {
                return new UnicodeString(s);
            }

            public new string ToString()
            {
                string result = "";

                for (int i = 0; i < content.Length; i++)
                    result += (string)content[i];

                return result;
            }

            private UnicodeCodePoint[] content;

            private UnicodeString(UnicodeCodePoint[] content)
            {
                this.content = content;
            }

            private UnicodeString(string content)
            {
                this.content = null;
            }
        }

        public struct UnicodeCodePoint
        {
            const string ERROR_INVALID_SURROGATE_PAIR = @"Cannot create code point from the supplied surrogates.
The surrogate pair is invalid.";

            public static UnicodeCodePoint[] GetRange(int start, int end)
            {
                int count = end - start + 1;

                return Enumerable.Range(start, count).Select(i => (UnicodeCodePoint)i).ToArray();
            }

            public static UnicodeCodePoint[] GetRange(UnicodeCodePoint start, UnicodeCodePoint end)
            {
                return GetRange((int)start, (int)end);
            }

            #region Explicit Operators

            public static explicit operator string(UnicodeCodePoint me)
            {
                return new string(me.surrogatePair);
            }

            public static explicit operator UnicodeCodePoint(string value)
            {
                return new UnicodeCodePoint(value.ToArray());
            }

            public static explicit operator int(UnicodeCodePoint me)
            {
                return (int)Conversions.CharArrayToInt(me.surrogatePair.ToArray());
            }

            public static explicit operator UnicodeCodePoint(int value)
            {
                return new UnicodeCodePoint(value);
            }

            #endregion

            private char[] surrogatePair;

            public override string ToString()
            {
                return (string)this;
            }

            private UnicodeCodePoint(char[] surrogatePair)
            {
                this.surrogatePair = surrogatePair;
            }

            private UnicodeCodePoint(int utf32)
            {
                surrogatePair = Conversions.IntToCharArray((uint)utf32);
            }
        }

        public struct UnicodeCategory
        {
            public static readonly UnicodeCodePoint[] BASIC_LATIN = UnicodeCodePoint.GetRange(0x0020, 0x007F);
            public static readonly UnicodeCodePoint[] LATIN_1_SUPPLEMENT = UnicodeCodePoint.GetRange(0x00A0, 0x00FF);
            public static readonly UnicodeCodePoint[] LATIN_EXTENDED_A = UnicodeCodePoint.GetRange(0x0100, 0x017F);
            public static readonly UnicodeCodePoint[] LATIN_EXTENDED_B = UnicodeCodePoint.GetRange(0x0180, 0x024F);
            public static readonly UnicodeCodePoint[] IPA_EXTENSIONS = UnicodeCodePoint.GetRange(0x0250, 0x02AF);
            public static readonly UnicodeCodePoint[] SPACING_MODIFIER_LETTERS = UnicodeCodePoint.GetRange(0x02B0, 0x02FF);
            public static readonly UnicodeCodePoint[] COMBINING_DIACRITICAL_MARKS = UnicodeCodePoint.GetRange(0x0300, 0x036F);
            public static readonly UnicodeCodePoint[] GREEK_AND_COPTIC = UnicodeCodePoint.GetRange(0x0370, 0x03FF);
            public static readonly UnicodeCodePoint[] CYRILLIC = UnicodeCodePoint.GetRange(0x0400, 0x04FF);
            public static readonly UnicodeCodePoint[] CYRILLIC_SUPPLEMENTARY = UnicodeCodePoint.GetRange(0x0500, 0x052F);
            public static readonly UnicodeCodePoint[] ARMENIAN = UnicodeCodePoint.GetRange(0x0530, 0x058F);
            public static readonly UnicodeCodePoint[] HEBREW = UnicodeCodePoint.GetRange(0x0590, 0x05FF);
            public static readonly UnicodeCodePoint[] ARABIC = UnicodeCodePoint.GetRange(0x0600, 0x06FF);
            public static readonly UnicodeCodePoint[] SYRIAC = UnicodeCodePoint.GetRange(0x0700, 0x074F);
            public static readonly UnicodeCodePoint[] THAANA = UnicodeCodePoint.GetRange(0x0780, 0x07BF);
            public static readonly UnicodeCodePoint[] DEVANAGARY = UnicodeCodePoint.GetRange(0x0900, 0x097F);
            public static readonly UnicodeCodePoint[] BENGALI = UnicodeCodePoint.GetRange(0x0980, 0x09FF);
            public static readonly UnicodeCodePoint[] GURMUKHI = UnicodeCodePoint.GetRange(0x0A00, 0x0A7F);
            public static readonly UnicodeCodePoint[] GUJARATI = UnicodeCodePoint.GetRange(0x0A80, 0x0AFF);
            public static readonly UnicodeCodePoint[] ORIYA = UnicodeCodePoint.GetRange(0x0B00, 0x0B7F);
            public static readonly UnicodeCodePoint[] TAMIL = UnicodeCodePoint.GetRange(0x0B80, 0x0BFF);
            public static readonly UnicodeCodePoint[] TELUGU = UnicodeCodePoint.GetRange(0x0C00, 0x0C7F);
            public static readonly UnicodeCodePoint[] KANNADA = UnicodeCodePoint.GetRange(0x0C80, 0x0CFF);
            public static readonly UnicodeCodePoint[] MALAYALAM = UnicodeCodePoint.GetRange(0x0D00, 0x0D7F);
            public static readonly UnicodeCodePoint[] SINHALA = UnicodeCodePoint.GetRange(0x0D80, 0x0DFF);
            public static readonly UnicodeCodePoint[] THAI = UnicodeCodePoint.GetRange(0x0E00, 0x0E7F);
            public static readonly UnicodeCodePoint[] LAO = UnicodeCodePoint.GetRange(0x0E80, 0x0EFF);
            public static readonly UnicodeCodePoint[] TIBETAN = UnicodeCodePoint.GetRange(0x0F00, 0x0FFF);
            public static readonly UnicodeCodePoint[] MYANMAR = UnicodeCodePoint.GetRange(0x1000, 0x109F);
            public static readonly UnicodeCodePoint[] GEORGIAN = UnicodeCodePoint.GetRange(0x10A0, 0x10FF);
            public static readonly UnicodeCodePoint[] HANGUL_JAMO = UnicodeCodePoint.GetRange(0x1100, 0x11FF);
            public static readonly UnicodeCodePoint[] ETHIOPIC = UnicodeCodePoint.GetRange(0x1200, 0x137F);
            public static readonly UnicodeCodePoint[] CHEROKEE = UnicodeCodePoint.GetRange(0x13A0, 0x13FF);
            public static readonly UnicodeCodePoint[] UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS = UnicodeCodePoint.GetRange(0x1400, 0x167F);
            public static readonly UnicodeCodePoint[] OGHAM = UnicodeCodePoint.GetRange(0x1680, 0x169F);
            public static readonly UnicodeCodePoint[] RUNIC = UnicodeCodePoint.GetRange(0x16A0, 0x16FF);
            public static readonly UnicodeCodePoint[] TAGALOG = UnicodeCodePoint.GetRange(0x1700, 0x171F);
            public static readonly UnicodeCodePoint[] HANUNOO = UnicodeCodePoint.GetRange(0x1720, 0x173F);
            public static readonly UnicodeCodePoint[] BUHID = UnicodeCodePoint.GetRange(0x1740, 0x175F);
            public static readonly UnicodeCodePoint[] TAGBANWA = UnicodeCodePoint.GetRange(0x1760, 0x177F);
            public static readonly UnicodeCodePoint[] KHMER = UnicodeCodePoint.GetRange(0x1780, 0x17FF);
            public static readonly UnicodeCodePoint[] MONGOLIAN = UnicodeCodePoint.GetRange(0x1800, 0x18AF);
            public static readonly UnicodeCodePoint[] LIMBU = UnicodeCodePoint.GetRange(0x1900, 0x194F);
            public static readonly UnicodeCodePoint[] TAI_LE = UnicodeCodePoint.GetRange(0x1950, 0x197F);
            public static readonly UnicodeCodePoint[] KHMER_SYMBOLS = UnicodeCodePoint.GetRange(0x19E0, 0x19FF);
            public static readonly UnicodeCodePoint[] PHONETIC_EXTENSIONS = UnicodeCodePoint.GetRange(0x1D00, 0x1D7F);
            public static readonly UnicodeCodePoint[] LATIN_EXTENDED_ADDITIONAL = UnicodeCodePoint.GetRange(0x1E00, 0x1EFF);
            public static readonly UnicodeCodePoint[] GREEK_EXTENDED = UnicodeCodePoint.GetRange(0x1F00, 0x1FFF);
            public static readonly UnicodeCodePoint[] GENERAL_PUNCTUATION = UnicodeCodePoint.GetRange(0x2000, 0x206F);
            public static readonly UnicodeCodePoint[] SUPERSCRIPTS_AND_SUBSCRIPTS = UnicodeCodePoint.GetRange(0x2070, 0x209F);
            public static readonly UnicodeCodePoint[] CURRENCY_SYMBOLS = UnicodeCodePoint.GetRange(0x20A0, 0x20CF);
            public static readonly UnicodeCodePoint[] COMBINING_DIACRITICAL_MARKS_FOR_SYMBOLS = UnicodeCodePoint.GetRange(0x20D0, 0x20FF);
            public static readonly UnicodeCodePoint[] LETTERLIKE_SYMBOLS = UnicodeCodePoint.GetRange(0x2100, 0x214F);
            public static readonly UnicodeCodePoint[] NUMBER_FORMS = UnicodeCodePoint.GetRange(0x2150, 0x218F);
            public static readonly UnicodeCodePoint[] ARROWS = UnicodeCodePoint.GetRange(0x2190, 0x21FF);
            public static readonly UnicodeCodePoint[] MATHEMATICAL_OPERATORS = UnicodeCodePoint.GetRange(0x2200, 0x22FF);
            public static readonly UnicodeCodePoint[] MISCELLANEOUS_TECHNICAL = UnicodeCodePoint.GetRange(0x2300, 0x23FF);
            public static readonly UnicodeCodePoint[] CONTROL_PICTURES = UnicodeCodePoint.GetRange(0x2400, 0x243F);
            public static readonly UnicodeCodePoint[] OPTICAL_CHARACTER_RECOGNITION = UnicodeCodePoint.GetRange(0x2400, 0x243F);
            public static readonly UnicodeCodePoint[] ENCLOSED_ALPHANUMERICS = UnicodeCodePoint.GetRange(0x2440, 0x24FF);
            public static readonly UnicodeCodePoint[] BOX_DRAWING = UnicodeCodePoint.GetRange(0x2500, 0x257F);
            public static readonly UnicodeCodePoint[] BLOCK_ELEMENTS = UnicodeCodePoint.GetRange(0x2580, 0x259F);
            public static readonly UnicodeCodePoint[] GEOMETRIC_SHAPES = UnicodeCodePoint.GetRange(0x25A0, 0x25FF);
            public static readonly UnicodeCodePoint[] MISCELLANEOUS_SYMBOLS = UnicodeCodePoint.GetRange(0x2600, 0x26FF);
            public static readonly UnicodeCodePoint[] DINGBATS = UnicodeCodePoint.GetRange(0x2700, 0x27BF);
            public static readonly UnicodeCodePoint[] MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A = UnicodeCodePoint.GetRange(0x27C0, 0x27EF);
            public static readonly UnicodeCodePoint[] SUPPLEMENTAL_ARROWS_A = UnicodeCodePoint.GetRange(0x27F0, 0x27FF);
            public static readonly UnicodeCodePoint[] BRAILLE_PATTERNS = UnicodeCodePoint.GetRange(0x2800, 0x28FF);
            public static readonly UnicodeCodePoint[] SUPPLEMENTAL_ARROWS_B = UnicodeCodePoint.GetRange(0x2900, 0x297F);
            public static readonly UnicodeCodePoint[] MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B = UnicodeCodePoint.GetRange(0x2980, 0x29FF);
            public static readonly UnicodeCodePoint[] SUPLLEMENTAL_MATHEMATICAL_OPERATORS = UnicodeCodePoint.GetRange(0x2A00, 0x2AFF);
            public static readonly UnicodeCodePoint[] MISCELLANEOUS_SYMBOLS_AND_ARROWS = UnicodeCodePoint.GetRange(0x2B00, 0x2BFF);
            public static readonly UnicodeCodePoint[] CJK_RADICALS_SUPPLEMENT = UnicodeCodePoint.GetRange(0x2E00, 0x2EFF);
            public static readonly UnicodeCodePoint[] KANGXI_RADICALS = UnicodeCodePoint.GetRange(0x2F00, 0x2FDF);
            public static readonly UnicodeCodePoint[] IDEOGRAPHIC_DESCRIPTION_CHARACTERS = UnicodeCodePoint.GetRange(0x2FF0, 0x2FFF);
            public static readonly UnicodeCodePoint[] CJK_SYMBOLS_AND_PUCTUATION = UnicodeCodePoint.GetRange(0x3000, 0x303F);
            public static readonly UnicodeCodePoint[] HIRAGANA = UnicodeCodePoint.GetRange(0x3040, 0x309F);
            public static readonly UnicodeCodePoint[] KATAKANA = UnicodeCodePoint.GetRange(0x30A0, 0x30FF);
            public static readonly UnicodeCodePoint[] BOPOMOFO = UnicodeCodePoint.GetRange(0x3100, 0x312F);
            public static readonly UnicodeCodePoint[] HANGUL_COMPATIBILITY_JAMO = UnicodeCodePoint.GetRange(0x3130, 0x318F);
            public static readonly UnicodeCodePoint[] KANBUN = UnicodeCodePoint.GetRange(0x3190, 0x319F);
            public static readonly UnicodeCodePoint[] BOPOMOFO_EXTENDED = UnicodeCodePoint.GetRange(0x31A0, 0x31BF);
            public static readonly UnicodeCodePoint[] KATAKANA_PHONETIC_EXTENSIONS = UnicodeCodePoint.GetRange(0x31F0, 0x31FF);
            public static readonly UnicodeCodePoint[] ENCLOSED_CJK_LETTERS_AND_MONTHS = UnicodeCodePoint.GetRange(0x3200, 0x32FF);
            public static readonly UnicodeCodePoint[] CJK_COMPATIBILITY = UnicodeCodePoint.GetRange(0x3300, 0x33FF);
            public static readonly UnicodeCodePoint[] CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A = UnicodeCodePoint.GetRange(0x3400, 0x4DBF);
            public static readonly UnicodeCodePoint[] YIJING_HEXAGRAM_SYMBOLS = UnicodeCodePoint.GetRange(0x4DC0, 0x4DFF);
            public static readonly UnicodeCodePoint[] CJK_UNIFIED_IDEOGRAPHS = UnicodeCodePoint.GetRange(0x4E00, 0x9FFF);
            public static readonly UnicodeCodePoint[] YI_SYLLABLES = UnicodeCodePoint.GetRange(0xA000, 0xA48F);
            public static readonly UnicodeCodePoint[] YI_RADICALS = UnicodeCodePoint.GetRange(0xA490, 0xA4CF);
            public static readonly UnicodeCodePoint[] HANGUL_SYLLABLES = UnicodeCodePoint.GetRange(0xAC00, 0xD7AF);
            public static readonly UnicodeCodePoint[] HIGH_SURROGATES = UnicodeCodePoint.GetRange(0xD800, 0xDB7F);
            public static readonly UnicodeCodePoint[] HIGH_PRIVATE_USE_SURROGATES = UnicodeCodePoint.GetRange(0xDB80, 0xDBFF);
            public static readonly UnicodeCodePoint[] LOW_SURROGATES = UnicodeCodePoint.GetRange(0xDC00, 0xDFFF);
            public static readonly UnicodeCodePoint[] PRIVATE_USE_AREA = UnicodeCodePoint.GetRange(0xE000, 0xF8FF);
            public static readonly UnicodeCodePoint[] CJK_COMPATIBILITY_IDEOGRAPHS = UnicodeCodePoint.GetRange(0xF900, 0xFAFF);
            public static readonly UnicodeCodePoint[] ALPHABETIC_PRESENTATION_FORMS = UnicodeCodePoint.GetRange(0xFB00, 0xFB4F);
            public static readonly UnicodeCodePoint[] ARABIC_PRESENTATION_FORMS_A = UnicodeCodePoint.GetRange(0xFB50, 0xFDFF);
            public static readonly UnicodeCodePoint[] VARIATION_SELECTORS = UnicodeCodePoint.GetRange(0xFE00, 0xFE0F);
            public static readonly UnicodeCodePoint[] COMBINING_HALF_MARKS = UnicodeCodePoint.GetRange(0xFE20, 0xFE2F);
            public static readonly UnicodeCodePoint[] CJK_COMPATIBILITY_FORMS = UnicodeCodePoint.GetRange(0xFE30, 0xFE4F);
            public static readonly UnicodeCodePoint[] SMALL_FORM_VARIANTS = UnicodeCodePoint.GetRange(0xFE50, 0xFE6F);
            public static readonly UnicodeCodePoint[] ARABIC_PRESENTATION_FORMS_B = UnicodeCodePoint.GetRange(0xFE70, 0xFEFF);
            public static readonly UnicodeCodePoint[] HALFWIDTH_AND_FULLWIDTH_FORMS = UnicodeCodePoint.GetRange(0xFF00, 0xFFEF);
            public static readonly UnicodeCodePoint[] SPECIALS = UnicodeCodePoint.GetRange(0xFFF0, 0xFFFF);
            public static readonly UnicodeCodePoint[] LINEAR_B_SYLLABARY = UnicodeCodePoint.GetRange(0x10000, 0x1007F);
            public static readonly UnicodeCodePoint[] LINEAR_B_IDEOGRAMS = UnicodeCodePoint.GetRange(0x10080, 0x100FF);
            public static readonly UnicodeCodePoint[] AEGEAN_NUMBERS = UnicodeCodePoint.GetRange(0x10100, 0x1013F);
            public static readonly UnicodeCodePoint[] OLD_ITALIC = UnicodeCodePoint.GetRange(0x10300, 0x1032F);
            public static readonly UnicodeCodePoint[] GOTHIC = UnicodeCodePoint.GetRange(0x10330, 0x1034F);
            public static readonly UnicodeCodePoint[] UGARITIC = UnicodeCodePoint.GetRange(0x10380, 0x1039F);
            public static readonly UnicodeCodePoint[] DESERET = UnicodeCodePoint.GetRange(0x10400, 0x1044F);
            public static readonly UnicodeCodePoint[] SHAVIAN = UnicodeCodePoint.GetRange(0x10450, 0x1047F);
            public static readonly UnicodeCodePoint[] OSMANYA = UnicodeCodePoint.GetRange(0x10480, 0x104AF);
            public static readonly UnicodeCodePoint[] CYPRIOT_SYLLABARY = UnicodeCodePoint.GetRange(0x10800, 0x1083F);
            public static readonly UnicodeCodePoint[] BYZANTINE_MUSICAL_SYMBOLS = UnicodeCodePoint.GetRange(0x1D000, 0x1D0FF);
            public static readonly UnicodeCodePoint[] MUSICAL_SYMBOLS = UnicodeCodePoint.GetRange(0x1D100, 0x1D1FF);
            public static readonly UnicodeCodePoint[] TAI_XUAN_JING_SYMBOLS = UnicodeCodePoint.GetRange(0x1D300, 0x1D35F);
            public static readonly UnicodeCodePoint[] MATHEMATICAL_ALPHANUMERIC_SYMBOLS = UnicodeCodePoint.GetRange(0x1D400, 0x1D7FF);
            public static readonly UnicodeCodePoint[] CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B = UnicodeCodePoint.GetRange(0x20000, 0x2A6DF);
            public static readonly UnicodeCodePoint[] CJK_COMPATIBILITY_IDEOGRAPHS_SUPPLEMENT_TAGS = UnicodeCodePoint.GetRange(0x2F800, 0x2FA1F);
            public static readonly UnicodeCodePoint[] TAGS = UnicodeCodePoint.GetRange(0xE0000, 0xE007F);

            public string Name { get; private set; }
            public UnicodeCodePoint[] CodePoints { get; private set; }

            private static string getCategoryName(string s)
            {
                TextInfo info = CultureInfo.CurrentCulture.TextInfo;

                s = info.ToTitleCase(s.ToLower().Replace('_', ' '));

                return s;
            }

            private static UnicodeCategory[] getAllCategories()
            {
                var staticFields = typeof(UnicodeCategory).GetFields(System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Static);
                List<UnicodeCategory> result = new List<UnicodeCategory> { };

                foreach (var field in staticFields)
                    if (field.FieldType == typeof(UnicodeCodePoint[]) && field.IsInitOnly)
                        result.Add(new UnicodeCategory(getCategoryName(field.Name), (UnicodeCodePoint[])field.GetValue(null)));

                return result.ToArray();
            }

            public static UnicodeCategory[] AllCategories { get { return getAllCategories(); } }

            public UnicodeCategory(string name, UnicodeCodePoint[] codePoints)
            {
                Name = name;
                CodePoints = codePoints;
            }
        }
    }
}