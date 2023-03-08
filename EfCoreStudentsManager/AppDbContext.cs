using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EfCoreStudentsManager.Entities;
using System.IO;
using System.Security.AccessControl;

namespace EfCoreStudentsManager
{
    public class AppDbContext : DbContext
    {
        private const string directory = "D:\\ITStep\\CSharp\\EFCore\\2023_02_27_EfCoreStudentsManager\\EfCoreStudentsManager\\StudentVisit.db";
        private const string ConnectionString = $"Data Source = {directory}";
        private const string _logFile = "D:\\123.log";

        object locker = new ();

        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite(ConnectionString)
                .LogTo(line => { 
                    lock (locker)
                    {
                        using (var fs = File.Open(_logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)) 
                        { 
                            using (var sw = new StreamWriter(fs))
                            {
                                sw.WriteLine(line + Environment.NewLine);
                            }
                        }
                    }
                    // Просто так вылетает ошибка о том, что файл пытается открыть второй поток
                    //File.AppendAllText(_logFile, line + Environment.NewLine); 
                })
                .EnableSensitiveDataLogging();
            ;
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Visit> Visits => Set<Visit>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<Group> Groups => Set<Group>();
    }
}
