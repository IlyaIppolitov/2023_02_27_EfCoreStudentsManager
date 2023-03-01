using EfCoreStudentsManager.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EfCoreStudentsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDbContext _db = new AppDbContext();
        public MainWindow()
        {
            InitializeComponent();
        }

        // Финализатор - вызывается Garbage collector
        ~MainWindow()
        {
            _db.Dispose();
        }

        // Загрузка всех dataGrid по факту загрузки основного окна
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshAllTables();
        }

        // Инициализация текущего выбранного студента/предмета/посещения в datagrid
        public Student? CurrentStudent => datagridStudents.SelectedItem as Student;
        public Subject? CurrentSubject => datagridSubjects.SelectedItem as Subject;
        public Visit? CurrentVisit => datagridVisits.SelectedItem as Visit;

        // Обработчик события доболения нового посещения
        private async void datagridVisits_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (CurrentStudent is null )
            {
                MessageBox.Show("Сначала выберете студента!");
                return;
            }
            if (CurrentSubject is null)
            {
                MessageBox.Show("Сначала выберете предмет!");
                return;
            }
            var visit = new Visit()
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                Student = (Student) datagridStudents.SelectedItem,
                Subject = (Subject) datagridSubjects.SelectedItem
            };

            await _db.Visits.AddAsync(visit);
            await SaveChangesToDb();
            e.NewItem = visit;
        }

        // Обработчик события добавления нового Студента
        private async void datagridStudents_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var student = new Student()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Email = ""
            };

            await _db.Students.AddAsync(student);
            await SaveChangesToDb();
        }

        // Обработчик события добавления нового предмета
        private async void datagridSubjects_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var subject = new Subject()
            {
                Id = Guid.NewGuid(),
                Name = ""
            };

            await _db.Subjects.AddAsync(subject);
            await SaveChangesToDb();
        }

        // Обработчик удаления строки в datagridVisits
        private async void datagridVisits_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == DataGrid.DeleteCommand)
            {
                if (CurrentVisit is null)
                {
                    MessageBox.Show("Выберете посещение перед тем как удалять!");
                    return;
                }
                _db.Remove(CurrentVisit);
                await SaveChangesToDb();
                e.CanExecute = true;
            }
        }

        // Обработчик удаления строки в datagridStudents
        private async void datagridStudent_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == DataGrid.DeleteCommand)
            {
                if (CurrentStudent is null)
                {
                    MessageBox.Show("Выберете студента перед тем как удалять!");
                    return;
                }

                foreach (var visit in _db.Visits)
                {
                    if (visit.Student != null)
                        if (visit.Student.Id == CurrentStudent.Id) _db.Remove(visit);
                }

                _db.Remove(CurrentStudent);
                await SaveChangesToDb();
                e.CanExecute = true;
                await PutVisitsToDataGrid();
            }
        }

        // Обработчик удаления строки в datagridSubjects
        private async void datagridSubject_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == DataGrid.DeleteCommand)
            {
                if (CurrentSubject is null)
                {
                    MessageBox.Show("Выберете студента перед тем как удалять!");
                    return;
                }

                foreach (var visit in _db.Visits) 
                {
                    if (visit.Subject != null)
                        if (visit.Subject.Id == CurrentSubject.Id) _db.Remove(visit);                   
                }                

                _db.Remove(CurrentSubject);
                await SaveChangesToDb();
                e.CanExecute = true;
                await PutVisitsToDataGrid();
            }
        }

        // Обрабочики изменения ячейки в каждой из таблиц
        private async void datagridStudents_CurrentCellChanged(object sender, EventArgs e)
        {
            await SaveChangesToDb();
            await PutVisitsToDataGrid();
        }

        private async void datagridSubjects_CurrentCellChanged(object sender, EventArgs e)
        {
            await SaveChangesToDb();
            await PutVisitsToDataGrid();
        }

        private async void datagridVisits_CurrentCellChanged(object sender, EventArgs e)
        {
            await SaveChangesToDb();
        }


        /// Сохранение изменений в базу данных
        private async Task SaveChangesToDb()
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) { MessageBox.Show("Ошибка! Данные были изменены с момента их загрузки в память!" + ex.Message); }
            catch (DbUpdateException ex) { MessageBox.Show("Ошибка сохранения в базу данных: " + ex.Message); }
            
        }

        /// Отправка перечня студентов в DataGrid
        private async Task PutStudentsToDataGrid()
        {
            try
            {
                datagridStudents.ItemsSource = await _db.Students.ToListAsync();
            }
            catch (SqliteException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
            catch (ArgumentNullException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
        }

        /// Отправка перечня предметов в DataGrid
        private async Task PutSubjectsToDataGrid()
        {
            try
            {
                datagridSubjects.ItemsSource = await _db.Subjects.ToListAsync();
            }
            catch (SqliteException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
            catch (ArgumentNullException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
        }

        /// Отправка перечня посещений в DataGrid
        private async Task PutVisitsToDataGrid()
        {
            try
            {
                datagridVisits.ItemsSource = await _db.Visits
                    .Include(v => v.Student)
                    .Include(v => v.Subject)
                    .ToListAsync();                    
            }
            catch (SqliteException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
            catch (ArgumentNullException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
            foreach (var column in datagridVisits.Columns)
            {
                if (column.Header.ToString() != ("Date")) column.IsReadOnly = true;
            }
        }

        private async Task RefreshAllTables()
        {
            await PutStudentsToDataGrid();
            await PutSubjectsToDataGrid();
            await PutVisitsToDataGrid();
        }
    }
}
