using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenCAD.OpenCADFormat.Measures
{
    public interface IMeasure
    {
        object[] GetSupportedUnits();
    }

    public interface IQuantity<M> where M : IMeasure
    {
        double StandardAmount { get; }
    }

    public class Quantity<M> : IQuantity<M> where M : IMeasure, new()
    {
        static public bool TryGetUnitBySymbol(string symbol, out Unit<M> result)
        {
            object[] supportedObj = new M().GetSupportedUnits();

            for (int i = 0; i < supportedObj.Length; i++)
            {
                Unit<M> unit = supportedObj[i] as Unit<M>;

                if (unit != null && unit.Symbol == symbol)
                {
                    result = unit;
                    return true;
                }
            }

            result = null;
            return false;
        }

        static public Unit<M> GetUnitBySymbol(string symbol)
        {
            Unit<M> result;

            if (TryGetUnitBySymbol(symbol, out result))
                return result;
            else
                throw new ArgumentOutOfRangeException("symbol", "No supported unit matches the specified symbol.");
        }

        public double StandardAmount { get; private set; }

        public Quantity(double standardAmount)
        {
            StandardAmount = standardAmount;
        }
    }

    public interface IUnit<M> where M : IMeasure
    {
        string Symbol { get; }
        IQuantity<M> Quantity { get; }
    }

    public class Unit<M> : IUnit<M> where M : IMeasure, new()
    {
        public string Symbol { get; private set; }
        public IQuantity<M> Quantity { get; private set; }

        public Unit(string symbol, double standardAmount)
        {
            Symbol = symbol;
            Quantity = new Quantity<M>(standardAmount);
        }

        public Unit(string symbol, double conversionFactor, IUnit<M> originalUnit)
        {
            Symbol = symbol;
            Quantity = new Quantity<M>(originalUnit.Quantity.StandardAmount * conversionFactor);
        }
    }

    public class UnitsCollection<M> : ReadOnlyCollection<IUnit<M>> where M : IMeasure
    {
        public IUnit<M> FindBySymbol(string symbol)
        {
            foreach (var unit in this)
                if (unit.Symbol == symbol)
                    return unit;

            return null;
        }

        public UnitsCollection(IList<IUnit<M>> list) : base(list) { }
    }

    public interface IMeasurement<M> where M : IMeasure
    {
        double Amount { get; set; }
        IUnit<M> Unit { get; }

        IMeasurement<M> ConvertTo(IUnit<M> outputUnit);
    }

    public class Measurement<M> : IMeasurement<M> where M : IMeasure, new()
    {
        static public Measurement<M> Parse(string s)
        {
            const string AMOUNT_PATTERN = @"^\d+";

            string amountStr = Regex.Match(s, AMOUNT_PATTERN).Value;
            string symbol = s.Substring(amountStr.Length, s.Length - amountStr.Length);

            double amount;

            if (!double.TryParse(amountStr, out amount))
                throw new InvalidOperationException("String does not contain valid amount information.");

            var unit = Quantity<M>.GetUnitBySymbol(symbol);

            return new Measurement<M>(amount, unit);
        }

        static public double ConvertTo(double inputAmount, IUnit<M> inputUnit, IUnit<M> outputUnit)
        {
            return new Measurement<M>(inputAmount, inputUnit).ConvertTo(outputUnit).Amount;
        }

        public double Amount { get; set; }
        public IUnit<M> Unit { get; private set; }

        public IMeasurement<M> ConvertTo(IUnit<M> outputUnit)
        {
            return new Measurement<M>(Amount * Unit.Quantity.StandardAmount / outputUnit.Quantity.StandardAmount, outputUnit);
        }

        public Measurement(double amount, IUnit<M> unit)
        {
            Amount = amount;
            Unit = unit;
        }
    }

    static class ScreenDpi
    {
        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        static public System.Drawing.SizeF GetScreenDpi()
        {
            IntPtr dc = GetDC(IntPtr.Zero);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHdc(dc))
                return new System.Drawing.SizeF(g.DpiX, g.DpiY);

        }
    }

    namespace Quantities
    {
        public class Length : IMeasure
        {
            static public IUnit<Length> PixelX { get { return new Unit<Length>("px-x", 1 / ScreenDpi.GetScreenDpi().Width, Inch); } }
            static public IUnit<Length> PixelY { get { return new Unit<Length>("px-y", 1 / ScreenDpi.GetScreenDpi().Height, Inch); } }

            static public IUnit<Length> Kilometer = new Unit<Length>("km", 1000, Meter);
            static public IUnit<Length> Meter = new Unit<Length>("m", 1);
            static public IUnit<Length> Centimeter = new Unit<Length>("cm", .01, Meter);
            static public IUnit<Length> Millimeter = new Unit<Length>("mm", .001, Meter);
            static public IUnit<Length> Mile = new Unit<Length>("mi", 1760, Yard);
            static public IUnit<Length> Furlong = new Unit<Length>("fur", 220, Yard);
            static public IUnit<Length> Chain = new Unit<Length>("ch", 22, Yard);
            static public IUnit<Length> Yard = new Unit<Length>("yd", 3, Foot);
            static public IUnit<Length> Foot = new Unit<Length>("ft", .001, Inch);
            static public IUnit<Length> Inch = new Unit<Length>("in", 2.54, Centimeter);
            static public IUnit<Length> Mil = new Unit<Length>("mil", .001, Inch);

            public object[] GetSupportedUnits()
            {
                return new object[] { PixelX, PixelY, Kilometer, Meter, Centimeter, Millimeter, Mile, Furlong,
                    Chain, Yard, Foot, Inch, Mile };
            }
        }

        public class PlaneAngle : IMeasure
        {
            static public IUnit<PlaneAngle> Degree = new Unit<PlaneAngle>("\x00B0", 1.0);
            static public IUnit<PlaneAngle> Radian = new Unit<PlaneAngle>("rad", 1.0 / 180.0 * Math.PI, Degree);
            static public IUnit<PlaneAngle> Gradian = new Unit<PlaneAngle>("grad", 9.0 / 10.0, Degree);
            static public IUnit<PlaneAngle> Minute = new Unit<PlaneAngle>("'", 1.0 / 60.0, Degree);
            static public IUnit<PlaneAngle> Second = new Unit<PlaneAngle>("\"", 1.0 / 60.0, Minute);

            static public UnitsCollection<PlaneAngle> SupportedUnits;

            static PlaneAngle()
            {
                SupportedUnits = new UnitsCollection<PlaneAngle>(new List<IUnit<PlaneAngle>> { Degree, Radian, Gradian,
                    Minute, Second });
            }

            public object[] GetSupportedUnits()
            {
                return new object[] { Degree, Radian, Gradian, Minute, Second };
            }
        }

        public class Frequency : IMeasure
        {
            static public IUnit<Frequency> Hertz = new Unit<Frequency>("Hz", 1.0);
            static public IUnit<Frequency> Millihertz = new Unit<Frequency>("mHz", 0.001, Hertz);
            static public IUnit<Frequency> Microhertz = new Unit<Frequency>("\x03BCHz", 1e-6, Hertz);
            static public IUnit<Frequency> Nanohertz = new Unit<Frequency>("nHz", 1e-9, Hertz);
            static public IUnit<Frequency> Picohertz = new Unit<Frequency>("pHz", 1e-12, Hertz);
            static public IUnit<Frequency> Femtohertz = new Unit<Frequency>("fHz", 1e-15, Hertz);
            static public IUnit<Frequency> Attohertz = new Unit<Frequency>("aHz", 1e-18, Hertz);
            static public IUnit<Frequency> Kilohertz = new Unit<Frequency>("kHz", 1000, Hertz);
            static public IUnit<Frequency> Megahertz = new Unit<Frequency>("MHz", 1e+6, Hertz);
            static public IUnit<Frequency> Gigahertz = new Unit<Frequency>("GHz", 1e+9, Hertz);
            static public IUnit<Frequency> Terahertz = new Unit<Frequency>("THz", 1e+12, Hertz);
            static public IUnit<Frequency> Petahertz = new Unit<Frequency>("PHz", 1e+15, Hertz);
            static public IUnit<Frequency> Exahertz = new Unit<Frequency>("EHz", 1e+18, Hertz);
            static public IUnit<Frequency> Zettahertz = new Unit<Frequency>("ZHz", 1e+21, Hertz);
            static public IUnit<Frequency> Yottahertz = new Unit<Frequency>("YHz", 1e+24, Hertz);

            public object[] GetSupportedUnits()
            {
                return new object[] { Hertz, Millihertz, Microhertz, Nanohertz, Picohertz, Femtohertz, Attohertz, Kilohertz,
                    Megahertz, Gigahertz, Terahertz, Petahertz, Exahertz, Zettahertz, Yottahertz };
            }
        }

        public class Charge : IMeasure
        {
            static public IUnit<Charge> Coulomb = new Unit<Charge>("C", 1.0);
            static public IUnit<Charge> Millicoulomb = new Unit<Charge>("mC", 0.001, Coulomb);
            static public IUnit<Charge> Microcoulomb = new Unit<Charge>("\x03BCC", 1e-6, Coulomb);
            static public IUnit<Charge> Nanocoulomb = new Unit<Charge>("nC", 1e-9, Coulomb);
            static public IUnit<Charge> Picocoulomb = new Unit<Charge>("pC", 1e-12, Coulomb);
            static public IUnit<Charge> Femtocoulomb = new Unit<Charge>("fC", 1e-15, Coulomb);
            static public IUnit<Charge> Attocoulomb = new Unit<Charge>("aC", 1e-18, Coulomb);
            static public IUnit<Charge> Kilocoulomb = new Unit<Charge>("kC", 1000, Coulomb);
            static public IUnit<Charge> Megacoulomb = new Unit<Charge>("MC", 1e+6, Coulomb);
            static public IUnit<Charge> Gigacoulomb = new Unit<Charge>("GC", 1e+9, Coulomb);
            static public IUnit<Charge> Teracoulomb = new Unit<Charge>("TC", 1e+12, Coulomb);
            static public IUnit<Charge> Petacoulomb = new Unit<Charge>("PC", 1e+15, Coulomb);
            static public IUnit<Charge> Exacoulomb = new Unit<Charge>("EC", 1e+18, Coulomb);
            static public IUnit<Charge> Zettacoulomb = new Unit<Charge>("ZC", 1e+21, Coulomb);
            static public IUnit<Charge> Yottacoulomb = new Unit<Charge>("YC", 1e+24, Coulomb);

            public object[] GetSupportedUnits()
            {
                return new object[] { Coulomb, Millicoulomb, Microcoulomb, Nanocoulomb, Picocoulomb, Femtocoulomb, Attocoulomb,
                    Kilocoulomb, Megacoulomb, Gigacoulomb, Teracoulomb, Petacoulomb, Exacoulomb, Zettacoulomb, Yottacoulomb };
            }
        }

        public class Current : IMeasure
        {
            static public IUnit<Current> Ampere = new Unit<Current>("A", 1.0);
            static public IUnit<Current> Milliampere = new Unit<Current>("mA", 0.001, Ampere);
            static public IUnit<Current> Microampere = new Unit<Current>("\x03BCA", 1e-6, Ampere);
            static public IUnit<Current> Nanoampere = new Unit<Current>("nA", 1e-9, Ampere);
            static public IUnit<Current> Picoampere = new Unit<Current>("pA", 1e-12, Ampere);
            static public IUnit<Current> Femtoampere = new Unit<Current>("fA", 1e-15, Ampere);
            static public IUnit<Current> Attoampere = new Unit<Current>("aA", 1e-18, Ampere);
            static public IUnit<Current> Kiloampere = new Unit<Current>("kA", 1000, Ampere);
            static public IUnit<Current> Megaampere = new Unit<Current>("MA", 1e+6, Ampere);
            static public IUnit<Current> Gigaampere = new Unit<Current>("GA", 1e+9, Ampere);
            static public IUnit<Current> Teraampere = new Unit<Current>("TA", 1e+12, Ampere);
            static public IUnit<Current> Petaampere = new Unit<Current>("PA", 1e+15, Ampere);
            static public IUnit<Current> Exaampere = new Unit<Current>("EA", 1e+18, Ampere);
            static public IUnit<Current> Zettaampere = new Unit<Current>("ZA", 1e+21, Ampere);
            static public IUnit<Current> Yottaampere = new Unit<Current>("YA", 1e+24, Ampere);

            public object[] GetSupportedUnits()
            {
                return new object[] { Ampere, Milliampere, Microampere, Nanoampere, Picoampere, Femtoampere, Attoampere,
                    Kiloampere, Megaampere, Gigaampere, Teraampere, Petaampere, Exaampere, Zettaampere, Yottaampere };
            }
        }

        public class Time : IMeasure
        {
            static public IUnit<Time> Second = new Unit<Time>("s", 1.0);
            static public IUnit<Time> Millisecond = new Unit<Time>("ms", 0.001, Second);
            static public IUnit<Time> Microsecond = new Unit<Time>("\x03BCs", 1e-6, Second);
            static public IUnit<Time> Nanosecond = new Unit<Time>("ns", 1e-9, Second);
            static public IUnit<Time> Picosecond = new Unit<Time>("ps", 1e-12, Second);
            static public IUnit<Time> Femtosecond = new Unit<Time>("fs", 1e-15, Second);
            static public IUnit<Time> Attosecond = new Unit<Time>("as", 1e-18, Second);
            static public IUnit<Time> Minute = new Unit<Time>("min", 60, Second);
            static public IUnit<Time> Hour = new Unit<Time>("h", 60, Minute);
            static public IUnit<Time> Day = new Unit<Time>("d", 24, Hour);
            static public IUnit<Time> Week = new Unit<Time>("week", 7, Day);
            static public IUnit<Time> Month = new Unit<Time>("month", 30, Day);
            static public IUnit<Time> Year = new Unit<Time>("year", 12, Month);
            static public IUnit<Time> Decade = new Unit<Time>("decade", 10, Year);
            static public IUnit<Time> Century = new Unit<Time>("century", 100, Year);
            static public IUnit<Time> Millenium = new Unit<Time>("millenium", 100, Year);

            public object[] GetSupportedUnits()
            {
                return new object[] { Second, Millisecond, Microsecond, Nanosecond, Picosecond, Femtosecond, Attosecond, Minute,
                    Hour, Day, Week, Month, Year, Decade, Century, Millenium };
            }
        }
    }
}
