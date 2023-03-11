using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EfCoreStudentsManager
{
    public class Debouncer
    {
        // Конструктор с указанием параметров
        public Debouncer (int time = 1000)
        {
            this.waitingTime = time;
            this.queryVersion = 0;
        }

        // Проверка прекращения внесения изменений
        public async Task<bool> IsDebounced()
        {
            // Увеличение значения текущего запроса
            this.queryVersion++;
            // Фиксация значения текущего запроса
            var savedVersion = this.queryVersion;
            // Пауза блокировки многократного исполнения
            await Task.Delay(TimeSpan.FromMilliseconds(waitingTime));
            // была ли получена новая команда?
            var result = savedVersion == queryVersion;
            // сброс значения текущего запроса
            if (result) this.queryVersion = 0;

            return result;
        }

        // номер нажатия
        int queryVersion;
        // время ожидания повторного нажатия в мс
        int waitingTime;

    }
}
