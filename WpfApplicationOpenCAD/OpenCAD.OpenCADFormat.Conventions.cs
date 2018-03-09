using System.Globalization;

namespace OpenCAD
{
    namespace OpenCADFormat
    {
        public static class Conventions
        {
            public static CultureInfo STANDARD_CULTURE { get; } = CultureInfo.InvariantCulture;

            public const NumberStyles STANDARD_NUMBER_STYLE = NumberStyles.Float;
        }
    }
}