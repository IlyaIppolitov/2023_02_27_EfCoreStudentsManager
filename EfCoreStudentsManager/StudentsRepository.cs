using Dapper;
using EfCoreStudentsManager.Entities;
using EfCoreStudentsManager.ValueObjects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EfCoreStudentsManager
{
    
    public class StudentsRepository : IDisposable
    {


        public StudentsRepository(string connectionString = _connectionString)
        {
            this.connection = new SqliteConnection(connectionString);
        }

        // Получить всех студентов
        public async Task<IReadOnlyList<Student>> GetStudentsAsync()
        {
            var sqlQuery = "SELECT Id, Name, Birthday, Email, Phone, Passport_Value as Passport FROM Students";
            var students = await connection.QueryAsync<Student>(sqlQuery);
            return students.ToList();
        }

        // Добавить студента
        // @return Id
        public async Task<Guid> AddStudentsAsync(Student student)
        {
            var newId = Guid.NewGuid();
            var newStrId = newId.ToString().ToUpper();
            var sqlQuery = @"INSERT INTO Students (Id, Name, Birthday, Email, Phone, Passport_Value)
               VALUES (@Id, @Name, @Birthday, @Email, @Phone, @Passport_Value)";
            await connection.QueryAsync(sqlQuery, new { 
                Id = newStrId, 
                Name = student.Name,
                Birthday = student.Birthday,
                Email = student.Email.Value,
                Phone = student.Phone.Value,
                Passport_Value = student.Passport.Value});
            return newId;
        }

        // Получить студента по его Id
        public async Task<Student> GetStudentByIdAsync(Guid id)
        {
            var searchId = "%" + EncodeForLike(id.ToString().ToUpper()) + "%";
            var sqlQuery = @"SELECT Id, Name, Birthday, Email, Phone, Passport_Value as Passport From Students WHERE Id like @searchId";
            var student = await connection.QueryAsync<Student>(sqlQuery, new { searchId });
            return student.First();
        }

        // Получить студентов по имени
        public async Task<IReadOnlyList<Student>> GetStudentsByNameAsync(string name)
        {
            var searchName = "%" + EncodeForLike(name) + "%";
            var sqlQuery = @"SELECT Id, Name, Birthday, Email, Phone, Passport_Value as Passport From Students WHERE Name like @searchName";
            var students = await  connection.QueryAsync<Student>(sqlQuery, new { searchName });
            return students.ToList();
        }

        // Удалить студента по Id
        public async Task<int> DeleteStudentById(Guid id)
        {
            var searchId = "%" + EncodeForLike(id.ToString().ToUpper()) + "%";
            var sqlQuery = "DELETE FROM Students WHERE Id like @searchId";
            return await connection.ExecuteAsync(sqlQuery, new { searchId });
        }

        // Обновит студента по Id
        public async Task<int> UpdateStudentNameById(Guid id, string newName = "newName")
        {
            var searchId = id.ToString().ToUpper();

            string sqlQuery = "UPDATE Students set Name='" + newName + "' WHERE Id like '%" +  searchId + "%'";
            return await connection.ExecuteAsync(sqlQuery, new { searchId });
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        private const string _directory = "D:\\ITStep\\CSharp\\EFCore\\2023_02_27_EfCoreStudentsManager\\EfCoreStudentsManager\\StudentVisit.db";
        private const string _connectionString = $"Data Source = {_directory}";
        private SqliteConnection connection { get; init; }

        public string EncodeForLike(string value)
        {
            return value.Replace("[", "[[]").Replace("%", "[%]");
        }
    }



    public class MySqlGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid guid)
        {
            parameter.Value = guid.ToString("N");
        }

        public override Guid Parse(object value)
        {
            return new Guid((string)value);
        }
    }

    public class MySqlEmailTypeHandler : SqlMapper.TypeHandler<Email>
    {
        public override void SetValue(IDbDataParameter parameter, Email email)
        {
            parameter.Value = email.ToString();
        }

        public override Email Parse(object value)
        {
            return new Email((string)value);
        }
    }

    public class MySqlPhoneTypeHandler : SqlMapper.TypeHandler<Phone>
    {
        public override void SetValue(IDbDataParameter parameter, Phone phone)
        {
            parameter.Value = phone.ToString();
        }

        public override Phone Parse(object value)
        {
            return new Phone((string)value);
        }
    }

    public class MySqlPassportTypeHandler : SqlMapper.TypeHandler<Passport>
    {
        public override void SetValue(IDbDataParameter parameter, Passport pass)
        {
            parameter.Value = pass.ToString();
        }

        public override Passport Parse(object value)
        {
            return new Passport((string)value);
        }
    }

}
