# Weather Analyzer

Тестовое задание: получение текущей погоды по списку городов через API [wttr.in](https://wttr.in).

Реализовано на **C# (.NET 8)** с использованием только стандартных библиотек (`System.Net.Http`, `System.Text.Json`).

---

## Что делает приложение

1. Загружает список городов из файла `Cities.txt`.
2. Для каждого города выполняет GET-запрос к API:
   https://wttr.in/{City}?format=j1
3. Формирует модель `WeatherData` с полями:
   - **City** — город.
   - **Temperature** — текущая температура в °C.
   - **Country** — страна.
4. Выводит информацию по каждому городу в формате:
   Tokyo, Japan +18 °C
5. Группирует города по странам и выводит статистику:
   Japan - 3 cities, avg: +19 °C, min: +16 °C, max: +22 °C

---

## Требования

- Windows / Linux / macOS
- .NET 8 SDK

---

## Установка и запуск

1. Клонирование репозитория:
   git clone https://github.com/ВАШ_ЛОГИН/WeatherAnalyzer.git
   cd WeatherAnalyzer

2. Сборка:
   dotnet build

3. Запуск:
   dotnet run

---

## Структура проекта

WeatherAnalyzer/
├── Program.cs                 # Основной код
├── WeatherAnalyzer.csproj     # Конфигурация проекта
├── Cities.txt                 # Список городов (по одному на строку)
├── README.md
└── .gitignore

---

## Пример вывода

Формат вывода:

--- Weather by City ---
<Город>, <Страна> <температура> °C
...

--- Statistics by Country ---
<Страна> - <N> cities, avg: <средняя> °C, min: <мин> °C, max: <макс> °C
...

---

## Ограничения и особенности реализации

### Соблюдение ограничений задания

- Только стандартные библиотеки. Использованы:
  - System.Net.Http — HTTP-запросы.
  - System.Text.Json — десериализация JSON.
  - System.IO, System.Linq, System.Collections.Generic — работа с файлами и коллекциями.
- Внешние NuGet-пакеты не подключались.

### Обработка edge cases

- BOM в Cities.txt. Файл может содержать BOM (Byte Order Mark) в начале, что привело бы к некорректному имени первого города. Учтено через TrimStart('\uFEFF').
- Пустые строки и пробелы. Каждая строка очищается методом Trim(), пустые строки пропускаются.
- URL-кодирование. Названия городов с пробелами или спецсимволами (например, New York, Saint-Petersburg) кодируются через Uri.EscapeDataString, чтобы не сломать URL.
- User-Agent. API wttr.in может блокировать запросы без заголовка User-Agent. В HttpClient установлен Mozilla/5.0.
- Обработка ошибок. Если API не отвечает, возвращает некорректный JSON или город не найден, приложение выводит сообщение об ошибке и продолжает работу с остальными городами.
- Культура. Парсинг температуры выполняется через double.TryParse — безопасно для любых форматов.

---

## Модель данных

public class WeatherData
{
    public string City { get; set; }
    public double Temperature { get; set; }
    public string Country { get; set; }
}

---

## Формат Cities.txt

Каждый город — на отдельной строке:

Moscow
Khabarovsk
Saint-Petersburg
Vienna
Izhevsk
Perm
NhaTrang
Villach

Файл сохраняется в кодировке UTF-8 без BOM.

---

## Примечания

- Приложение не изменяет никакие системные файлы и не требует прав администратора.
- Все запросы выполняются к публичному API wttr.in.
- Скорость работы зависит от ответа API (обычно 1–3 секунды на город).

## Примечание о тестировании

На момент разработки API wttr.in был недоступен из региона проживания автора
(геоблокировка). Код написан и проверен на уровне синтаксиса и логики:
- Проект успешно компилируется (`dotnet build` → Build succeeded).
- Логика работы с HttpClient, System.Text.Json и LINQ проверена.
- Структура классов соответствует ответу API wttr.in (см. документацию).

Для запуска достаточно выполнить `dotnet run` в регионе, где API доступен.