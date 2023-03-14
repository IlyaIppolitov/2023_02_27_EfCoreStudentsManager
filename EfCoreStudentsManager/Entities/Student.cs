using EfCoreStudentsManager.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EfCoreStudentsManager.Entities
{
    [Index(nameof(Name), nameof(Phone), nameof(Email))]
    public class Student
    {
        public Guid Id { get; init; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public Email? Email { get; set; }
        public Phone? Phone { get; set; }
        public Passport? Passport { get; set; }
        public Group? Group { get; set; }
        public List<Visit>? Visits { get; set; }

        public int? VisitsCount => Visits?.Count;
        public override string ToString()
        {
            return Name;
        }
    }
}