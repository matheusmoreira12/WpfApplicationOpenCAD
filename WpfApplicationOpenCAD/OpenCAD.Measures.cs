using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenCAD.Measures
{
    namespace Quantities
    {
        static class Length
        {
            static public Cureos.Measures.Unit<Cureos.Measures.Quantities.Length> Mil =
                new Cureos.Measures.ConstantConverterUnit<Cureos.Measures.Quantities.Length>
                ("mil", Cureos.Measures.Quantities.Length.Inch.ConvertAmountToStandardUnit(.001));
        }

        namespace Collections
        {
            static class SupportedUnitsCollection
            {
                static private List<Cureos.Measures.Unit<Cureos.Measures.Quantities.Length>> _Collection;
                static IEnumerable<Cureos.Measures.Unit<Cureos.Measures.Quantities.Length>> Collection { get { return _Collection; } }

                static public Cureos.Measures.Unit<Cureos.Measures.Quantities.Length> FindBySymbol(string symbol)
                {
                    foreach (var unit in _Collection)
                        if (unit.Symbol == symbol)
                            return unit;

                    return null;
                }

                static SupportedUnitsCollection()
                {
                    _Collection.Add(Length.Mil);
                    _Collection.Add(Cureos.Measures.Quantities.Length.Inch);
                    _Collection.Add(Cureos.Measures.Quantities.Length.MilliMeter);
                }
            }
        }
    }
}
