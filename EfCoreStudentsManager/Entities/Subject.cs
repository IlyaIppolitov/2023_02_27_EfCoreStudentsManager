using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfCoreStudentsManager.Entities
{
    public class Subject
    {
        public Guid Id { get; init; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
