using EfCoreStudentsManager.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

        // Объявление коллекций для хранения и изменения данных даблиц базы данных
        private ObservableCollection<Student>? _students;
        private ObservableCollection<Group>? _groups;
        private ObservableCollection<Visit>? _visits;
        private ObservableCollection<Subject>? _subjects;

        // Объявление и инициализация Семафора - используется для блокировки доступа к базе данных
        static SemaphoreSlim sem = new SemaphoreSlim(1, 1);

        //static Debouncer debouncer = new Debouncer();
        DebounceDispatcher searchDebouncer = new();


        // Флаг загруженных исходных данный
        bool _defaultData;

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
            await UpdateComboBox();
        }

        // Инициализация текущего выбранного студента/предмета/посещения в datagrid
        public Student? CurrentStudent => datagridStudents.SelectedItem as Student;
        public Subject? CurrentSubject => datagridSubjects.SelectedItem as Subject;
        public Visit? CurrentVisit => datagridVisits.SelectedItem as Visit;
        public Group? CurrentGroup => datagridGroups.SelectedItem as Group;

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
                Name = ""
            };

            await _db.Students.AddAsync(student);
            await SaveChangesToDb();
            e.NewItem = student;
        }

        // Обработчик события добавления новой Группы
        private async void datagridGroups_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            var group = new Group()
            {
                Id = Guid.NewGuid(),
                Name = "",
                CreatedAt = DateTime.Now
            };

            await _db.Groups.AddAsync(group);
            await SaveChangesToDb();
            e.NewItem = group;
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
            e.NewItem = subject;
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
                datagridVisits.Items.Refresh();
            }
        }

        // Обработчик удаления строки в datagridStudents
        private async void datagridGroup_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == DataGrid.DeleteCommand)
            {
                if (CurrentGroup is null)
                {
                    MessageBox.Show("Выберете группу перед тем как удалять!");
                    return;
                }

                foreach (var student in _db.Students)
                {
                    if (student.Group != null)
                        if (student.Group.Id == CurrentGroup.Id) _db.Remove(student);
                }

                _db.Remove(CurrentGroup);
                await SaveChangesToDb();
                e.CanExecute = true;
                datagridSubjects.Items.Refresh();
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
                datagridVisits.Items.Refresh();
            }
        }

        // Обрабочики изменения ячейки в каждой из таблиц
        private async void CurrentCellChanged(object sender, EventArgs e)
        {
            await SaveChangesToDb();
        }


        /// Сохранение изменений в базу данных
        private async Task SaveChangesToDb()
        {
            await Task.Run(async () =>
            {
                try
                {
                    await sem.WaitAsync();
                    await _db.SaveChangesAsync();
                    sem.Release();
                }
                catch (DbUpdateConcurrencyException ex) { MessageBox.Show("Ошибка! Данные были изменены с момента их загрузки в память!" + ex.Message); }
                catch (DbUpdateException ex) { MessageBox.Show("Ошибка сохранения в базу данных: " + ex.Message); }
            });
        }

        /// Отправка перечня студентов в DataGrid
        private async Task PutStudentsToDataGrid()
        {
            await Task.Run(async () =>
            {
                try
                {
                    await sem.WaitAsync();
                    var students = await _db.Students
                    .Include(s => s.Visits)
                    .Include(s => s.Group)
                    .ToListAsync();
                    sem.Release();
                    _students = new ObservableCollection<Student>(students);
                    Dispatcher.Invoke(() =>  datagridStudents.ItemsSource = _students);
                }
                catch (SqliteException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
                catch (ArgumentNullException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }

            });
        }

        /// Отправка перечня предметов в DataGrid
        private async Task PutSubjectsToDataGrid()
        {
            await Task.Run(async () =>
            {
                try
                {
                    await sem.WaitAsync();
                    var subjects = await _db.Subjects.ToListAsync();
                    sem.Release();
                    _subjects = new ObservableCollection<Subject>(subjects);
                    Dispatcher.Invoke(() => datagridSubjects.ItemsSource = _subjects);
                }
                catch (SqliteException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
                catch (ArgumentNullException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }

            });
        }

        /// Отправка перечня посещений в DataGrid
        private async Task PutVisitsToDataGrid()
        {
            await Task.Run(async () =>
            {
                try
                {
                    await sem.WaitAsync();
                    var visits = await _db.Visits
                        .Include(v => v.Student)
                        .Include(v => v.Subject)
                        .ToListAsync();
                    sem.Release();
                    _visits = new ObservableCollection<Visit>(visits);
                    Dispatcher.Invoke(() => datagridVisits.ItemsSource = _visits);
                }
                catch (SqliteException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
                catch (ArgumentNullException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
            });
        }

        /// Отправка перечня посещений в DataGrid
        private async Task PutGroupsToDataGrid()
        {
            await Task.Run(async () =>
            {
                try
                {
                    await sem.WaitAsync();
                    var groups = await _db.Groups
                    .Include(g => g.Students)
                    .ToListAsync();
                    sem.Release();
                    _groups = new ObservableCollection<Group>(groups);
                    Dispatcher.Invoke(() => datagridGroups.ItemsSource = _groups);
                }
                catch (SqliteException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); }
                catch (ArgumentNullException ex) { MessageBox.Show("Отсутствует необходимая таблица для отображения\n" + ex.Message); } 
            });
        }

        private async Task RefreshAllTables()
        {
            if (_defaultData == true) return;

            await PutSubjectsToDataGrid();
            await PutVisitsToDataGrid();
            await PutStudentsToDataGrid();
            await PutGroupsToDataGrid();
            _defaultData = true;
        }

        private async void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            await RefreshAllTables();
        }
        
        //
        private async void datagridGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (CurrentGroup is not null)
            {
                tboxGroupName.Text = CurrentGroup.Name;
                datepickerGroup.SelectedDate = CurrentGroup.CreatedAt;

                //ленивая загрузка студентов группы!
                await _db.Entry(CurrentGroup).Collection(it => it.Students).LoadAsync();
                datagridStudents.ItemsSource = CurrentGroup.Students;
            }
            _defaultData = false;
        }

        private async void datagridStudents_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Группа")
            {
                var groupName = ((TextBox) e.EditingElement).Text;
                if (_db.Groups.Any(g => g.Name == groupName))
                {
                    if (CurrentStudent is not null)
                        CurrentStudent.Group = await _db.Groups
                            .FindAsync(new Guid(_db.Groups.Where(g => g.Name == groupName)
                            .First().Id.ToString()));
                }
                await SaveChangesToDb();
            }
            _defaultData = false;
        }

        // Отображение данных о студентах, посещавших занятия в феврале 2023г.
        private async void buttonStudentsInFebruary_Click(object sender, RoutedEventArgs e)
        {
            DateTime startTime = new DateTime(2023, 02, 1);
            DateTime endTime = new DateTime(2023, 03, 1);

            // SQL style:
            //if (_db.Students is not null && _db.Visits is not null)
            //    datagridStudents.ItemsSource = await _db.Visits
            //        .Where(v => v.Date >= new DateTime(2023, 02, 01) && v.Date <= new DateTime(2023, 02, 28))
            //        .Join(_db.Students,v => v.Student.Id,s => s.Id, (v,s) => new {Id = s.Id, Name = s.Name, Email = s.Email, Birthday = s.Birthday} )
            //        .Distinct()
            //        .ToListAsync();
            
            if (_db.Students is not null && _db.Visits is not null)
                datagridStudents.ItemsSource = await _db.Students
                    .Where(s => s.Visits.Any(v => v.Date >= startTime && v.Date < endTime))
                    .ToListAsync();

            _defaultData = false;
        }

        // Кнопка - Добавление нового студента
        private async void btnAddNewStudent_Click(object sender, RoutedEventArgs e)
        {
            var student = new Student()
            {
                Id = Guid.NewGuid(),
                Name = tboxStudentName.Text,
                Email = new ValueObjects.Email(""),
                Birthday = datepickerStudent.SelectedDate.GetValueOrDefault(),
                Group = (Group) comboboxStudentGroup.SelectedItem,
            };

            await _db.Students.AddAsync(student);
            await SaveChangesToDb();
            _students.Add(student);
            datagridStudents.SelectedItem = student;
            //await PutStudentsToDataGrid();
            // Хочется так, но так нельзя!
            //(datagridStudents.ItemsSource as List<Student>).Add(student);

            //var students = (ObservableCollection<Student>)datagridStudents.ItemsSource;
            //students.Add(student);
        }

        // Обновление перечня групп в Combobox
        private async Task UpdateComboBox()
        {
            comboboxStudentGroup.Items.Clear();
            comboboxStudentGroup.ItemsSource = await _db.Groups.ToListAsync();
            //comboboxStudentGroup.SelectedValuePath = "Id";
            comboboxStudentGroup.DisplayMemberPath = "Name";
        }

        // Обработчик изменения выбранного студента
        private async void datagridStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentStudent is not null)
            {
                tboxStudentName.Text = CurrentStudent.Name;
                tboxStudentEmail.Text = CurrentStudent.Email.Value;
                tboxStudentPhone.Text = CurrentStudent.Phone.Value;
                tboxStudentPassport.Text = CurrentStudent.Passport?.Value ?? "";
                //tboxStudentPassport.Text = CurrentStudent.Passport is null ? "" : CurrentStudent.Passport.Value;
                datepickerStudent.SelectedDate = CurrentStudent.Birthday;
                comboboxStudentGroup.SelectedItem = CurrentStudent.Group;

                datagridVisits.ItemsSource = await _db.Visits
                    .Where(v => (v.Student.Id == CurrentStudent.Id)).ToListAsync();
            }
            _defaultData = false;
        }

        // Кнопка - Сохраненние изменений в данных существующего студента
        private async void btnSaveChangesToStudent_Click(object sender, RoutedEventArgs e)
        {
            if (_students is null)
            {
                MessageBox.Show("У нас нет студентов! Выбирать не из кого!");
                return;
            }
            if (CurrentStudent is null)
            {
                MessageBox.Show("Выберете студента для редактирования его данных!");
                return;
            }
            if (tboxStudentName.Text is null)
            {
                MessageBox.Show("Введите имя студента!");
                return;
            }
            if (datepickerStudent.SelectedDate is null)
            {
                MessageBox.Show("Введите дату рождения студента!");
                return;
            }
            if (comboboxStudentGroup.SelectedItem is null)
            {
                MessageBox.Show("Выберете группу для студента!");
                return;
            }
            try
            {
                CurrentStudent.Name = tboxStudentName.Text;
                CurrentStudent.Email = new ValueObjects.Email(tboxStudentEmail.Text);
                CurrentStudent.Phone = new ValueObjects.Phone(tboxStudentPhone.Text);
                CurrentStudent.Passport = new ValueObjects.Passport(tboxStudentPassport.Text);
                CurrentStudent.Birthday = datepickerStudent.SelectedDate.Value;
                CurrentStudent.Group = (Group)comboboxStudentGroup.SelectedItem;
            }
            catch (ArgumentException exc) { MessageBox.Show(exc.Message);}

            await SaveChangesToDb();
            datagridStudents.Items.Refresh();
            datagridVisits.Items.Refresh();
        }

        // Кнопка - Отображение всех студентов
        private void btnShowAllStudents_Click(object sender, RoutedEventArgs e)
        {
            datagridStudents.ItemsSource = _students;
            datagridStudents.Items.Refresh();
        }

        // Кнопка - Создание новой группы
        private async void btnAddNewGroup_Click(object sender, RoutedEventArgs e)
        {
            if (tboxGroupName.Text is null)
            {
                MessageBox.Show("Введите название группы!");
                return;
            }
                      
            var group = new Group()
            {
                Id = Guid.NewGuid(),
                Name = tboxGroupName.Text,
                CreatedAt = datepickerGroup.SelectedDate.GetValueOrDefault()
            };

            await _db.Groups.AddAsync(group);
            await SaveChangesToDb();
            _groups.Add(group);
            datagridStudents.SelectedItem = group;
        }

        // Кнопка - Редактирование существующей группы
        private async void btnSaveChangesToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (_groups is null)
            {
                MessageBox.Show("У нас нет групп! Выбирать не из чего!");
                return;
            }
            if (CurrentGroup is null)
            {
                MessageBox.Show("Выберете группу для редактирования её данных!");
                return;
            }
            if (tboxGroupName.Text is null)
            {
                MessageBox.Show("Введите имя группы!");
                return;
            }
            if (datepickerGroup.SelectedDate is null)
            {
                MessageBox.Show("Введите дату создания группы!");
                return;
            }

            CurrentGroup.Name = tboxGroupName.Text;
            CurrentGroup.CreatedAt = datepickerGroup.SelectedDate.Value;

            await SaveChangesToDb();
            datagridGroups.Items.Refresh();
        }

        // Кнопка - Отобразить все существующие группы
        private void btnShowAllGroups_Click(object sender, RoutedEventArgs e)
        {
            datagridGroups.ItemsSource = _groups;
            datagridGroups.Items.Refresh();
        }

        // Кнопка - Добавить новый предмет
        private async void btnAddNewSubject_Click(object sender, RoutedEventArgs e)
        {
            if (tboxSubjectName.Text is null)
            {
                MessageBox.Show("Введите название предмета!");
                return;
            }

            var subject = new Subject()
            {
                Id = Guid.NewGuid(),
                Name = tboxSubjectName.Text
            };

            await _db.Subjects.AddAsync(subject);
            await SaveChangesToDb();
            _subjects.Add(subject);
            datagridStudents.SelectedItem = subject;
        }

        // Редактирование существующего предмета
        private async void btnSaveChangesToSubject_Click(object sender, RoutedEventArgs e)
        {
            if (_subjects is null)
            {
                MessageBox.Show("У нас нет предметов! Выбирать не из чего!");
                return;
            }
            if (CurrentSubject is null)
            {
                MessageBox.Show("Выберете предмет для редактирования его данных!");
                return;
            }
            if (tboxSubjectName.Text is null)
            {
                MessageBox.Show("Введите имя предмета!");
                return;
            }

            CurrentSubject.Name = tboxSubjectName.Text;

            await SaveChangesToDb();
            datagridSubjects.Items.Refresh();
            datagridVisits.Items.Refresh();
        }

        // Обработчик изменения выбора объекта в окне Предметы
        private async void datagridSubjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentSubject is not null)
                tboxSubjectName.Text = CurrentSubject.Name;

            if (CurrentSubject is not null)
            {
                datagridVisits.ItemsSource = await _db.Visits
                    .Where(v => (v.Subject!.Id == CurrentSubject!.Id)).ToListAsync();
            }
            else
            {
                datagridVisits.ItemsSource = null;
            }

            _defaultData = false;
        }

        // Кнопка - добавить новое посещение
        private async void btnAddNewVisit_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentStudent is null)
            {
                MessageBox.Show("Выберете студента!");
                return;
            }
            if (CurrentSubject is null)
            {
                MessageBox.Show("Выберете предмет!");
                return;
            }

            var visit = new Visit()
            {
                Id = Guid.NewGuid(),
                Subject = CurrentSubject,
                Student = CurrentStudent,
                Date = datepickerVisit.SelectedDate.HasValue ? datepickerVisit.SelectedDate.Value : DateTime.Now

            };

            await _db.Visits.AddAsync(visit);
            await SaveChangesToDb();
            _visits!.Add(visit);
            datagridVisits.SelectedItem = visit;
        }

        // Кнопка - изменить существующее посещение
        private async void btnSaveChangesToVisit_Click(object sender, RoutedEventArgs e)
        {
            if (_visits is null)
            {
                MessageBox.Show("У нас нет посещений! Выбирать не из чего!");
                return;
            }
            if (CurrentVisit is null)
            {
                MessageBox.Show("Выберете посещение для редактирования его данных!");
                return;
            }
            if (!datepickerVisit.SelectedDate.HasValue)
            {
                MessageBox.Show("Введите дату!");
                return;
            }

            CurrentVisit.Date = datepickerVisit.SelectedDate.Value;

            await SaveChangesToDb();
            datagridVisits.Items.Refresh();
        }

        // Кнопка - отобразить все посещения
        private void btnShowAllVisits_Click(object sender, RoutedEventArgs e)
        {
            datagridVisits.ItemsSource = _visits;
            datagridVisits.Items.Refresh();
        }

        // Обработчик изменения выбора объекта в окне Посещения
        private void datagridVisits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentVisit is not null)
                datepickerVisit.SelectedDate = CurrentVisit.Date;

            _defaultData = false;
        }

        // Обработчик измененения значения строки поиска
        private async void textboxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textboxSearch.Text == "Поиск...") return;

            if (string.IsNullOrWhiteSpace(textboxSearch.Text))
            {
                await RefreshAllTables();
                return;
            }

            // Ожидаение исполнения debouncer - новая версия
            await searchDebouncer.Debounce(() => Search(textboxSearch.Text));

            // Ожидаение исполнения debouncer - старая версия
            //if (await debouncer.IsDebounced())
            //{
            //    await Search(textboxSearch.Text);
            //}
        }

        private void textboxSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            SelectAllTextBoxSearch();
        }

        private void textboxSearch_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectAllTextBoxSearch();
        }

        public void SelectAllTextBoxSearch()
        {
            if (textboxSearch.Text == "Поиск...")
            {
                textboxSearch.SelectAll();
                textboxSearch.Focus();
            }
        }

        // Поиск
        async Task Search(string textSnapshot)
        {
            // Получаем возможность поиска без учета регистра, но!:
            // 1. Потребление памяти, 2. Перфоманс - ухудшаем
            //var allStudents = await _db.Students.ToListAsync(); // O(n)
            //var matchesStudents = allStudents // O(n)
                //.Where(s => s.Name.Contains(textSnapshot, StringComparison.CurrentCultureIgnoreCase))
                //.ToList();

            var matchesStudents = await _db.Students // O(1)
                .Where(s => EF.Functions.Like(s.Name, $"%{textSnapshot}%")  || 
                            s.Phone.Value.Contains(textSnapshot)                  ||
                            s.Email.Value.Contains(textSnapshot))
                .ToListAsync();
            datagridStudents.ItemsSource = matchesStudents;

            var matchesSubjects = await _db.Subjects
                .Where(s => s.Name.Contains(textSnapshot))
                .ToListAsync();
            datagridSubjects.ItemsSource = matchesSubjects;

            var matchesGroups = await _db.Groups
                .Where(g => g.Name.Contains(textSnapshot) ||
                        g.Students!.Any(s => s.Name.Contains(textSnapshot)))
                .ToListAsync();
            datagridGroups.ItemsSource = matchesGroups;

            var matchesVisits = await _db.Visits
                .Where(v => (v.Subject!.Name.Contains(textSnapshot) ||
                v.Student!.Name.Contains(textSnapshot)))
                .ToListAsync();
            datagridVisits.ItemsSource = matchesVisits;

            _defaultData = false;
        }
    }
}
