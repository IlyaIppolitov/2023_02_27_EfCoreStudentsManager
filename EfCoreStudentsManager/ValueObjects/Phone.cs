using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System;

namespace EfCoreStudentsManager.ValueObjects
{
    [Owned]
    public class Phone
    {
        public string Value { get; private set; }

        protected Phone() { }
        public Phone(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (!IsPhone(value))
            {
                throw new ArgumentException("Некорретный номер телефона");
            }
            Value = value;
        }

        public override string ToString() => Value;

        private static bool IsPhone(string value)
        {
            var ruPhoneRegex = new Regex(@"\+7\d{3}\d{7}");
            return ruPhoneRegex.IsMatch(value);
        }

        protected bool Equals(Phone other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Phone)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Phone? left, Phone? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Phone? left, Phone? right)
        {
            return !Equals(left, right);
        }
    }
}