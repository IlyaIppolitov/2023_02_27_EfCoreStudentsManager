using System.Text.RegularExpressions;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EfCoreStudentsManager.ValueObjects
{

    [Owned]
    public class Email
    {
        public string Value { get; private set; }

        protected Email() { }
        public Email(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (!IsEmail(value))
            {
                throw new ArgumentException("Некорретный адрес электронной почты!");
            }
            Value = value;
        }

        public override string ToString() => Value;

        private static bool IsEmail(string value)
        {
            return new EmailAddressAttribute().IsValid(value);
        }

        protected bool Equals(Email other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Email)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Email? left, Email? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Email? left, Email? right)
        {
            return !Equals(left, right);

        }
    }
}