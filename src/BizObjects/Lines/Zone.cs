using LogObjects;
using System;
using System.Collections.Generic;

namespace BizObjects.Lines
{
    public class Zone : Line, IEquatable<Zone>
    {
        public static Zone Unknown = new Zone(null, "Unknown");

        public Zone(LogDatum logLine, string zoneName) : base(logLine)
        {
            Name = zoneName;
        }
        public string Name { get; }

        #region Equality

        public override bool Equals(object obj)
        {
            return Equals(obj as Zone);
        }

        public bool Equals(Zone other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public static bool operator ==(Zone left, Zone right)
        {
            return EqualityComparer<Zone>.Default.Equals(left, right);
        }

        public static bool operator !=(Zone left, Zone right)
        {
            return !(left == right);
        }

        #endregion

        private string DebuggerDisplay => string.Format($"{Name}");

        public override string ToString()
        {
            return ToString("G");
        }

        public string ToString(string format)
        {
            switch (format)
            {
                case "G":
                default:
                    return DebuggerDisplay;
            }
        }
    }
}
