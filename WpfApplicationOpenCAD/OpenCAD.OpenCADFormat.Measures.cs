using OpenCAD.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenCAD.OpenCADFormat.Measures
{
    public static class Utils
    {
        internal static double GetMetricPrefixValue(IMetricPrefix prefix)
        {
            return prefix == null ? 1 : prefix.Multiplier;
        }

        internal static double GetAbsoluteAmount<M>(IMeasurement<M> measurement) where M : IPhysicalQuantity, new()
        {
            return GetMetricPrefixValue(measurement.PrefixedUnit.Prefix) * measurement.PrefixedUnit.Unit.Quantity.StandardAmount
                * measurement.Amount;
        }

        internal static double ConvertAmount<M>(IMeasurement<M> measurement, IPrefixedUnit<M> outPrefixedUnit)
             where M : IPhysicalQuantity, new()
        {
            return GetAbsoluteAmount(measurement) / outPrefixedUnit.Unit.Quantity.StandardAmount /
                GetMetricPrefixValue(outPrefixedUnit.Prefix);
        }

        private static IEnumerable<U> getAllSuppotedMatchingFields<T, U>(System.Reflection.BindingFlags bindingAttr)
        {
            var staticFields = typeof(T).GetFields(bindingAttr);

            foreach (var field in staticFields)
            {
                object value = field.GetValue(null);

                if (field is U)
                    yield return (U)value;
            }
        }

        public static IEnumerable<IUnit<M>> GetSupportedUnits<M>() where M : IPhysicalQuantity, new()
        {
            return getAllSuppotedMatchingFields<M, IUnit<M>>(System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.GetField);
        }

        public static IEnumerable<IMetricPrefix> GetSupportedMetricPrefixes()
        {
            return getAllSuppotedMatchingFields<MetricPrefix, IMetricPrefix>(System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.GetField);
        }

        private static IEnumerable<string> combineAllPrefixesAndUnits<M>() where M : IPhysicalQuantity, new()
        {
            IUnit<M>[] supportedUnits = GetSupportedUnits<M>().ToArray();
            IMetricPrefix[] supportedPrefixes = GetSupportedMetricPrefixes().ToArray();

            foreach (var unit in supportedUnits)
            {
                yield return $"{unit.Symbol}";

                foreach (var prefix in supportedPrefixes)
                    yield return $"{prefix.Symbol}{unit.Symbol}";
            }
        }

        public static string ToString<M>(IMeasurement<M> measurement) where M : IPhysicalQuantity, new()
        {
            return $"{measurement.Amount}{measurement.PrefixedUnit.ToString()}";
        }
    }

    public interface IMetricPrefix
    {
        double Multiplier { get; }
        string Symbol { get; }
    }

    public sealed class MetricPrefix : IMetricPrefix
    {
        public double Multiplier { get; private set; }
        public string Symbol { get; private set; }

        public MetricPrefix(double multiplier, string symbol)
        {
            Multiplier = multiplier;
            Symbol = symbol;
        }
    }

    public static class MetricPrefixes
    {
        public static IMetricPrefix Deci = new MetricPrefix(0.1, "d");
        public static IMetricPrefix Centi = new MetricPrefix(0.01, "c");
        public static IMetricPrefix Milli = new MetricPrefix(0.001, "m");
        public static IMetricPrefix Micro = new MetricPrefix(1e-6, "μ");
        public static IMetricPrefix Nano = new MetricPrefix(1e-9, "n");
        public static IMetricPrefix Pico = new MetricPrefix(1e-12, "p");
        public static IMetricPrefix Femto = new MetricPrefix(1e-15, "f");
        public static IMetricPrefix Atto = new MetricPrefix(1e-18, "a");
        public static IMetricPrefix Deca = new MetricPrefix(10, "da");
        public static IMetricPrefix Hecto = new MetricPrefix(100, "h");
        public static IMetricPrefix Kilo = new MetricPrefix(1000, "k");
        public static IMetricPrefix Mega = new MetricPrefix(1e+6, "M");
        public static IMetricPrefix Giga = new MetricPrefix(1e+9, "G");
        public static IMetricPrefix Tera = new MetricPrefix(1e+12, "T");
        public static IMetricPrefix Peta = new MetricPrefix(1e+15, "P");
        public static IMetricPrefix Exa = new MetricPrefix(1e+18, "E");
    }

    public interface IPhysicalQuantity
    {
        string Symbol { get; }
    }

    public interface IQuantity<M> where M : IPhysicalQuantity, new()
    {
        double StandardAmount { get; }
    }

    public sealed class Quantity<M> : IQuantity<M> where M : IPhysicalQuantity, new()
    {
        public double StandardAmount { get; private set; }

        public Quantity(double standardAmount)
        {
            StandardAmount = standardAmount;
        }
    }

    public interface IPrefixedUnit<M> where M : IPhysicalQuantity, new()
    {
        IMetricPrefix Prefix { get; }
        IUnit<M> Unit { get; }
    }

    public sealed class PrefixedUnit<M> : IPrefixedUnit<M> where M : IPhysicalQuantity, new()
    {
        public IUnit<M> Unit { get; private set; }
        public IMetricPrefix Prefix { get; private set; }

        /// <summary>
        /// Returns a string that represents the current prefix and unit.
        /// </summary>
        public override string ToString()
        {
            string prefixSymbol = Prefix == null ? "" : Prefix.Symbol;

            return $"{prefixSymbol}{Unit.Symbol}";
        }

        public PrefixedUnit(IUnit<M> unit, IMetricPrefix prefix)
        {
            Unit = unit;
            Prefix = prefix;
        }
    }

    public interface IUnit<M> where M : IPhysicalQuantity, new()
    {
        string Symbol { get; }
        Quantity<M> Quantity { get; }
    }

    public sealed class Unit<M> : IUnit<M> where M : IPhysicalQuantity, new()
    {
        public string Symbol { get; private set; }
        public Quantity<M> Quantity { get; private set; }

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

    public sealed class UnitsCollection<M> : ReadOnlyCollection<IUnit<M>> where M : IPhysicalQuantity, new()
    {
        public IUnit<M> FindBySymbol(string symbol)
        {
            foreach (var unit in this)
                if (unit.Symbol == symbol)
                    return unit;

            throw new ArgumentOutOfRangeException($"Could not find by symbol. No unit matches the symbol \"{symbol}\".");
        }

        public bool TryFindBySymbol(string symbol, out IUnit<M> result)
        {
            try
            {
                result = FindBySymbol(symbol);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public UnitsCollection(IList<IUnit<M>> original) : base(original) { }
        public UnitsCollection(IEnumerable<IUnit<M>> original) : base(original.ToList()) { }
    }

    public interface IMeasurement<M> where M : IPhysicalQuantity, new()
    {
        double Amount { get; }
        IPrefixedUnit<M> PrefixedUnit { get; }

        IMeasurement<M> ConvertTo(IUnit<M> outUnit, IMetricPrefix outPrefix = null);
        IMeasurement<M> ConvertTo(IPrefixedUnit<M> outPrefixedUnit);
    }

    public sealed class Measurement<M> : IMeasurement<M> where M : IPhysicalQuantity, new()
    {
        public static Measurement<M> Parse(string s)
        {
            bool isFloatingPoint,
                hasExponent;

            string amountStr = null;

            StringScanner scanner = new StringScanner(s);
            StringUtils.ReadDecimalString(scanner, out amountStr, out isFloatingPoint, out hasExponent);

            string symbol = s.Substring(amountStr.Length, s.Length - amountStr.Length);

            double amount;

            if (!double.TryParse(amountStr, Conventions.STANDARD_NUMBER_STYLE, Conventions.STANDARD_CULTURE, out amount))
                throw new InvalidOperationException("String does not contain valid amount information.");

            return null;
        }

        public static double ConvertAmountTo(double inAmount, IPrefixedUnit<M> inPrefixedUnit, IPrefixedUnit<M> outPrefixedUnit)
        {
            return new Measurement<M>(inAmount, inPrefixedUnit).ConvertTo(outPrefixedUnit).Amount;
        }

        /// <summary>
        /// Returns a string that represents the current measurement.
        /// </summary>
        public override string ToString()
        {
            return $"{Amount}{PrefixedUnit}";
        }

        public double Amount { get; set; }
        public IPrefixedUnit<M> PrefixedUnit { get; private set; }

        public IMeasurement<M> ConvertTo(IPrefixedUnit<M> outPrefixedUnit)
        {
            return new Measurement<M>(Utils.ConvertAmount(this, outPrefixedUnit), outPrefixedUnit);
        }

        public IMeasurement<M> ConvertTo(IUnit<M> outUnit, IMetricPrefix outPrefix = null)
        {
            return ConvertTo(new PrefixedUnit<M>(outUnit, outPrefix));
        }

        public Measurement(double amount, IUnit<M> unit, IMetricPrefix prefix = null)
        {
            Amount = amount;
            PrefixedUnit = new PrefixedUnit<M>(unit, prefix);
        }

        public Measurement(double amount, IPrefixedUnit<M> prefixedUnit)
        {
            Amount = amount;
            PrefixedUnit = prefixedUnit;
        }
    }

    static class ScreenDpi
    {
        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        public static System.Drawing.SizeF GetScreenDpi()
        {
            IntPtr dc = GetDC(IntPtr.Zero);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHdc(dc))
                return new System.Drawing.SizeF(g.DpiX, g.DpiY);

        }
    }

    namespace Quantities
    {
        public sealed class Length : IPhysicalQuantity
        {
            public static IUnit<Length> PixelX
            {
                get
                {
                    return new Unit<Length>("px-x", 1 / ScreenDpi.GetScreenDpi().Width, Inch);
                }
            }
            public static IUnit<Length> PixelY
            {
                get
                {
                    return new Unit<Length>("px-y", 1 / ScreenDpi.GetScreenDpi().Height, Inch);
                }
            }

            public static readonly IUnit<Length> Meter = new Unit<Length>("m", 1);
            public static readonly IUnit<Length> Mile = new Unit<Length>("mi", 1760, Yard);
            public static readonly IUnit<Length> Furlong = new Unit<Length>("fur", 220, Yard);
            public static readonly IUnit<Length> Chain = new Unit<Length>("ch", 22, Yard);
            public static readonly IUnit<Length> Yard = new Unit<Length>("yd", 3, Foot);
            public static readonly IUnit<Length> Foot = new Unit<Length>("ft", .001, Inch);
            public static readonly IUnit<Length> Inch = new Unit<Length>("in", .254, Meter);
            public static readonly IUnit<Length> Mil = new Unit<Length>("mil", .001, Inch);

            public string Symbol { get; } = "L";

            public UnitsCollection<Length> SupportedUnits { get; } = new UnitsCollection<Length>(
                Utils.GetSupportedUnits<Length>());
        }

        public sealed class PlaneAngle : IPhysicalQuantity
        {
            public static readonly IUnit<PlaneAngle> Degree = new Unit<PlaneAngle>("\x00B0", 1.0);
            public static readonly IUnit<PlaneAngle> Radian = new Unit<PlaneAngle>("rad", 1.0 / 180.0 * Math.PI, Degree);
            public static readonly IUnit<PlaneAngle> Gradian = new Unit<PlaneAngle>("grad", 9.0 / 10.0, Degree);
            public static readonly IUnit<PlaneAngle> Minute = new Unit<PlaneAngle>("'", 1.0 / 60.0, Degree);
            public static readonly IUnit<PlaneAngle> Second = new Unit<PlaneAngle>("\"", 1.0 / 60.0, Minute);

            public string Symbol { get; } = "L";

            public UnitsCollection<PlaneAngle> SupportedUnits { get; } = new UnitsCollection<PlaneAngle>(
                Utils.GetSupportedUnits<PlaneAngle>());
        }

        public sealed class Frequency : IPhysicalQuantity
        {
            public static readonly IUnit<Frequency> Hertz = new Unit<Frequency>("Hz", 1.0);

            public string Symbol { get; } = "f";

            public UnitsCollection<Frequency> SupportedUnits { get; } = new UnitsCollection<Frequency>(
                Utils.GetSupportedUnits<Frequency>());
        }

        public sealed class Charge : IPhysicalQuantity
        {
            public static readonly IUnit<Charge> Coulomb = new Unit<Charge>("C", 1.0);

            public string Symbol { get; } = "";

            public UnitsCollection<Charge> SupportedUnits { get; } = new UnitsCollection<Charge>(
                Utils.GetSupportedUnits<Charge>());
        }

        public sealed class Current : IPhysicalQuantity
        {
            public static readonly IUnit<Current> Ampere = new Unit<Current>("A", 1.0);

            public string Symbol { get; } = "I";

            public UnitsCollection<Current> SupportedUnits { get; } = new UnitsCollection<Current>(
                Utils.GetSupportedUnits<Current>());
        }

        public sealed class Time : IPhysicalQuantity
        {
            public static readonly IUnit<Time> Second = new Unit<Time>("s", 1.0);
            public static readonly IUnit<Time> Minute = new Unit<Time>("min", 60, Second);
            public static readonly IUnit<Time> Hour = new Unit<Time>("h", 60, Minute);
            public static readonly IUnit<Time> Day = new Unit<Time>("d", 24, Hour);
            public static readonly IUnit<Time> Week = new Unit<Time>("week", 7, Day);
            public static readonly IUnit<Time> Month = new Unit<Time>("month", 30, Day);
            public static readonly IUnit<Time> Year = new Unit<Time>("year", 12, Month);
            public static readonly IUnit<Time> Decade = new Unit<Time>("decade", 10, Year);
            public static readonly IUnit<Time> Century = new Unit<Time>("century", 100, Year);
            public static readonly IUnit<Time> Millenium = new Unit<Time>("millenium", 100, Year);

            public string Symbol { get; } = "t";

            public UnitsCollection<Time> SupportedUnits { get; } = new UnitsCollection<Time>(
                Utils.GetSupportedUnits<Time>());
        }
    }
}