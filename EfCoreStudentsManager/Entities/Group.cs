using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EfCoreStudentsManager.Entities
{
    [Index(nameof(Name))]
    public class Group
    {
        public Guid Id { get; init; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Student>? Students { get; set; }
        public int? StudentCount => Students?.Count;
        public override string ToString()
        {
            return ($"{Name} - {StudentCount}");
        }
    }
}
