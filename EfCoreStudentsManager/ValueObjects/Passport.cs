using System.Text.RegularExpressions;
using System;
using Microsoft.EntityFrameworkCore;

namespace EfCoreStudentsManager.ValueObjects
{
    [Owned]
    public class Passport
    {
        public string Value { get; private set; }

        protected Passport() { }
        public Passport(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (!IsPassport(value))
            {
                throw new ArgumentException("Некорретный номер паспорта!");
            }
            Value = value;
        }

        public override string ToString() => Value;

        private static bool IsPassport(string value)
        {
            var ruPassportRegex = new Regex(@"(\d{4})\s*-?\s*(\d{6})");
            return ruPassportRegex.IsMatch(value);
        }

        protected bool Equals(Passport other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Passport)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Passport? left, Passport? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Passport? left, Passport? right)
        {
            return !Equals(left, right);
        }
    }
}