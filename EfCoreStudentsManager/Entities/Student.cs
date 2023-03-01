using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfCoreStudentsManager.Entities
{
    public class Student
    {
        public Guid Id { get; init; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public string Email { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
