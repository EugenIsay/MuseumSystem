using Avalonia.Controls;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using LibVLCSharp.Shared;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using MuseumSystem.Context;
using MuseumSystem.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using LiveChartsCore.Kernel.Sketches;

namespace MuseumSystem
{
    public static class Helper
    {
        public static int Page = 0;

        static Random random = new Random();

        static User3Context DBContext = new User3Context();

        //Глобальное отслеживание польователя
        static int currentUserId = 0;
        public static User currentUser
        {
            get => Users.FirstOrDefault(u => u.Id == currentUserId);
        }

        //Таблицы с связаные с пользователями
        public static List<Gender> Genders
        {
            get => DBContext.Genders.ToList();
        }
        public static List<User> Users
        {
            get => DBContext.Users.Include(u => u.Gender).Include(u => u.Role).Include(u => u.Tickets).ToList();
        }


        //Таблицы связаные с мероприятиями
        public static List<Event> Events
        {
            get => DBContext.Events.Include(e => e.Type).Include(e => e.Organizer).Include(e => e.IncludedItems).ThenInclude(i => i.Exhibit).Include(e => e.EventRegistrations).ToList();
        }
        public static List<Event> ShownEvents
        {
            get => Events.Where(e => !e.IsOld).OrderBy(e => e.StartDatetime).Concat(Events.Where(e => e.IsOld).OrderBy(e => e.StartDatetime)).ToList();
        }
        public static List<EventType> EventTypes
        {
            get => DBContext.EventTypes.ToList();
        }

        // Таблицы связаные с билетами
        public static List<Ticket> AllTickets
        {
            get => DBContext.Tickets.Include(t => t.User).Include(t => t.EventRegistrations).ThenInclude(t => t.Event).Include(t => t.Type).ToList();
        }
        public static List<Ticket> Tickets
        {
            get => DBContext.Tickets.Where(t => t.UserId == currentUser.Id || IsEmployee).Include(t => t.User).Include(t => t.EventRegistrations).ThenInclude(t => t.Event).Include(t => t.Type).ToList();
        }
        public static List<TicketType> TicketTypes
        {
            get => DBContext.TicketTypes.ToList();
        }

        // Таблицы связаные экспонатами
        public static List<Exhibit> Exhibits
        {
            get => DBContext.Exhibits.Include(e => e.AtachedMedia).Include(e => e.Category).Include(e => e.ExhibitReviews).ToList();
        }
        public static List<Category> Categories
        {
            get => DBContext.Categories.ToList();
        }
        public static List<AtachedMedium> atachedMedia
        {
            get => DBContext.AtachedMedia.ToList();
        }

        // Метод для проверки при входе
        public static bool IsExist(string FirsRow, string Password)
        {
            if (Users.FirstOrDefault(u => (u.Login == FirsRow || u.Email == FirsRow) && u.Password == Password && u.IsActive == true) != null)
            {
                currentUserId = Users.FirstOrDefault(u => (u.Login == FirsRow || u.Email == FirsRow) && u.Password == Password && u.IsActive == true).Id!;
                return true;
            }
            return false;
        }


        public static List<EventRegistration> registrations
        {
            get => DBContext.EventRegistrations.Include(r => r.Event).ToList();
        }

        public static decimal HowMuchMoney
        {
            get => DBContext.Tickets.Where(t => t.PurchaseDate.Value.Year == DateTime.Now.Year).Select(s => s.Price).Sum();
        }
        public static int HowManyUsers
        {
            get => DBContext.Users.Where(u => u.RegistrationDate.Value.Year == DateTime.Now.Year).Count();
        }

        public static int HowManyEhxibitions
        {
            get => DBContext.Exhibits.Count();
        }
        public static int HowManyEvents
        {
            get => DBContext.Exhibits.Count();
        }

        public static int HowManyVisitors
        {
            get => DBContext.Users.Where(u => u.RoleId == 3).Count();
        }

        public static int HowMainTickets
        {
            get => DBContext.Tickets.Count();
        }

        public static List<Event> UsersEvents
        {
            get => registrations.Where(u => u.Ticket.UserId == currentUser.Id).Select(r => r.Event).ToList();
        }

        public static void EditExhibitComment(ExhibitReview review)
        {
            if (currentUser.ExhibitReviews.FirstOrDefault(e => e.ExhibitId == review.ExhibitId) != null)
            {
                DBContext.ExhibitReviews.Update(review);
            }
            else
            {
                DBContext.ExhibitReviews.Add(review);
            }
            DBContext.SaveChanges();
        }
        public static void EditEventComment(EventReview review)
        {
            if (currentUser.EventReviews.FirstOrDefault(e => e.EventId == review.EventId) != null)
            {
                DBContext.EventReviews.Update(review);
            }
            else
            {
                DBContext.EventReviews.Add(review);
            }
            DBContext.SaveChanges();
        }
        public static void ChangeUserBool(User User)
        {
            User.IsActive = !User.IsActive;
            DBContext.Users.Update(User);
            DBContext.SaveChanges();
        }

        // Метод для добавления билета
        public static bool AddTickets(Ticket Ticket)
        {
            DBContext.Add(Ticket);
            try
            {
                return DBContext.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        // Метод для редактирования экспонатов
        public static bool EditExhibits(Exhibit Exhibit)
        {
            if (Exhibit.Id == 0)
            {
                Exhibit.Id = Helper.Exhibits.OrderBy(s => s.Id).Last().Id + 1;
                DBContext.Add(Exhibit);
            }
            else
            {
                DBContext.Update(Exhibit);
            }
            try
            {
                return DBContext.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public static void AddMedia(AtachedMedium medium)
        {
            try
            {
                medium.Id = atachedMedia.Select(s => s.Id).Order().Last() + 1;
                DBContext.Add(medium);
                DBContext.SaveChanges();
            }
            catch
            {

            }

        }
        public static void RemoveMedia(AtachedMedium medium)
        {
            try
            {
                DBContext.Remove(medium);
                DBContext.SaveChanges();
            }
            catch
            {

            }

        }

        public static bool EventEdit(Event @event, Window Window)
        {
            if (string.IsNullOrEmpty(@event.Title))
            {
                CallMessageBox("Введите название меропртиятия", Window);
                return false;
            }
            if (string.IsNullOrEmpty(@event.Addres))
            {
                CallMessageBox("Введите место проведения мероприятия", Window);
                return false;
            }
            if (@event.OrganizerId < 1)
            {
                CallMessageBox("Укажите организатора", Window);
                return false;
            }
            if (@event.TypeId < 1)
            {
                CallMessageBox("Укажите тип мероприятия", Window);
                return false;
            }
            if (@event.TypeId < 1)
            {
                CallMessageBox("Укажите тип мероприятия", Window);
                return false;
            }
            if (@event.StartDatetime == DateTime.MinValue)
            {
                CallMessageBox("Укажите начальную дату", Window);
                return false;
            }
            else if (@event.StartDatetime > DateTime.Now)
            {
                CallMessageBox("Начальная дата доллжна быть позже чем сейчас", Window);
                return false;
            }
            else
            {
                if (@event.EndDatetime != DateTime.MinValue && @event.StartDatetime > @event.EndDatetime)
                {
                    CallMessageBox("Начальная дата не может быть позже конечной", Window);
                    return false;
                }
            }
            if (@event.MaxAttendees <= 0)
            {
                CallMessageBox("Укажите максимум посетителей", Window);
                return false;
            }
            if (@event.Price < 0)
            {
                CallMessageBox("Укажите цену", Window);
                return false;
            }
            if (@event.Id == 0)
            {
                @event.Id = Events.Select(e => e.Id).Order().Last() + 1;
                DBContext.Events.Add(@event);
            }
            else
            {
                DBContext.Events.Update(@event);
            }
            try
            {

                return DBContext.SaveChanges() > 0;
            }
            catch
            {
                CallMessageBox("Что-то пошло не так, пожалуйста поождите", Window);
                return false;
            }

        }


        public static void AddEventEhibits(IncludedItem includedItem)
        {
            DBContext.IncludedItems.Add(includedItem);
            DBContext.SaveChanges();
        }
        public static void RemoveEventEhibits(IncludedItem includedItem)
        {
            includedItem = DBContext.IncludedItems.FirstOrDefault(i => i.EventId == includedItem.EventId && i.ExhibitId == includedItem.ExhibitId)!;
            DBContext.IncludedItems.Remove(includedItem);

            DBContext.SaveChanges();
        }

        public static void AddEventReg(EventRegistration registration)
        {
            DBContext.EventRegistrations.Add(registration);
            DBContext.SaveChanges();
        }

        public static async Task<bool> CanRegister(User User, Window Window)
        {
            if (string.IsNullOrEmpty(User.Login))
            {
                CallMessageBox("Придумайте себе логин", Window);
                return false;
            }
            if (DBContext.Users.Select(u => u.Login).Contains(User.Login))
            {
                CallMessageBox("Данный логин уже занят", Window);
                return false;
            }
            if (string.IsNullOrEmpty(User.FirstName) || string.IsNullOrEmpty(User.LastName))
            {
                CallMessageBox("Укажите имя и фамилию", Window);
                return false;
            }
            if (string.IsNullOrEmpty(User.Email) || !IsValidEmail(User.Email) || string.IsNullOrEmpty(User.PhoneNumber) || User.PhoneNumber.Contains("_"))
            {
                CallMessageBox("Укажите правильные контактные данные", Window);
                return false;
            }
            if (DBContext.Users.Select(u => u.Email).Contains(User.Email))
            {
                CallMessageBox("Данная почта уже занята", Window);
                return false;
            }
            if (User.Birthday == null || User.Birthday > DateOnly.FromDateTime(DateTime.Now.AddYears(-12)))
            {
                CallMessageBox("Укажите свою дату рождения", Window);
                return false;
            }
            if (User.GenderId == 0)
            {
                CallMessageBox("Укажите свой пол", Window);
                return false;
            }
            if (string.IsNullOrEmpty(User.Password) || User.Password.Count() < 4)
            {
                CallMessageBox("Подберите более надёжный пароль", Window);
                return false;
            }
            try
            {
                var result = await EmailTask(User.Email, Window);
                if (!result)
                {
                    CallMessageBox("Вы не прошли проверку кода. Попробуте ещё раз", Window);
                    return false;
                }
                if (User.Id == 0)
                {
                    User.Id = DBContext.Users.Select(s => s.Id).Order().Last() + 1;
                    DBContext.Users.Add(User);
                }
                else
                {
                    DBContext.Users.Update(User);
                }
                DBContext.SaveChanges();
                currentUserId = User.Id;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static async Task<bool> EmailTask(string email, Window window)
        {
            string code = $"{random.Next(100000, 999999)}";
            // с какого аккаунта будет отправленно письмо
            MailAddress from = new MailAddress("museum.system.00@gmail.com", "Код для регистрации");
            // кому отправляем
            MailAddress to = new MailAddress($"{email}");
            // создаем объект сообщения
            MailMessage m = new MailMessage(from, to);
            // тема письма
            m.Subject = "Пароль для потвердждения регистрации!";
            // текст письма
            string mail = $"<h2>{code}</h2>\r\n";
            m.Body = mail;
            // письмо представляет код html
            m.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо (если почта с которой ты отправляешь gmail, то оставляй так)
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            // логин и пароль
            smtp.Credentials = new NetworkCredential("museum.system.00@gmail.com", "vtoy zmwb iiob zqkn");
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(m);

            var box = MsBox.Avalonia.MessageBoxManager.GetMessageBoxCustom(
                new MessageBoxCustomParams()
                {
                    ContentTitle = "Вам на почвту отправленно сообщение",
                    Width = 400,
                    Height = 120,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ButtonDefinitions = new List<ButtonDefinition>
                    {
                        new ButtonDefinition { Name = "Потвердить", }
                    },
                    InputParams = new InputParams()
                    {
                        Label = "введите сюда код из сообщения",
                    }
                });
            await box.ShowWindowDialogAsync(window);
            if (code == box.InputValue)
                return true;
            else
                return false;

        }
        static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        public static void CallMessageBox(string Message, Window Window)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", Message).ShowWindowDialogAsync(Window);
        }
        public static bool IsAdmin
        {
            get => currentUser.RoleId == 1;
        }
        public static bool IsEmployee
        {
            get => currentUser.RoleId != 3;
        }
        public static bool PrintTicket(Ticket Ticket, Window window)
        {
            try
            {
                string outputPath = "C:\\output.pdf";
                string arialFontPath = "C:\\Windows\\Fonts\\arial.ttf";
                PdfFont arial = PdfFontFactory.CreateFont(arialFontPath, PdfEncodings.IDENTITY_H);
                PdfWriter writer = new PdfWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\{Ticket.Number}.pdf");
                PdfDocument pdfDocument = new PdfDocument(writer.SetSmartMode(true));
                Document d = new Document(pdfDocument, iText.Kernel.Geom.PageSize.LETTER);
                d.SetFont(arial);
                d.Add(new Paragraph("Билет").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
                d.Add(new Paragraph($"{Ticket.Number}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
                d.Add(new Paragraph($"Дата покупки {Ticket.PurchaseDate}").SetTextAlignment(TextAlignment.CENTER).SetFontSize(14));
                ImageData imageData = ImageDataFactory.Create(Ticket.qrBytes);
                d.Add(new iText.Layout.Element.Image(imageData).ScaleToFit(300, 300).SetHorizontalAlignment(HorizontalAlignment.CENTER));
                LineSeparator ls = new LineSeparator(new SolidLine());
                d.Add(ls);
                d.Add(new Paragraph($"Покупатель {Ticket.User.FullName}").SetFontSize(14));
                d.Add(new Paragraph($"Билет действителен с {Ticket.ValidFrom}, до {Ticket.ValidTo}").SetFontSize(14));
                d.Add(ls);
                // Создаем таблицу (3 колонки: №, Услуга, Цена)
                Table table = new Table(3, true)
                    .SetWidth(UnitValue.CreatePercentValue(100));
                table.AddHeaderCell("№");
                table.AddHeaderCell("Услуга");
                table.AddHeaderCell("Цена");

                table.AddCell($"{1}");
                table.AddCell(Ticket.Type.Name);
                table.AddCell($"{Ticket.Type.Price} руб.");
                if (Ticket.EventRegistrations?.Count > 0)
                {
                    // Заполняем данные
                    for (int i = 0; i < Ticket.EventRegistrations.Count; i++)
                    {
                        var service = Ticket.EventRegistrations.ToList()[i];
                        table.AddCell($"{i + 2}");
                        table.AddCell(service.Event.Title);
                        table.AddCell($"{service.Event.Price} руб.");
                    }
                }

                table.SetBorder(new SolidBorder(0));
                d.Add(table);
                d.Add(ls);
                d.Add(new Paragraph($"Итого: {Ticket.Price}").SetFontSize(14));
                d.Add(ls);
                d.Close();
                return true;
            }
            catch
            {
                CallMessageBox($"Билет \"{Ticket.Number}\" не распечатался", window);
                return false;
            }

        }
        public static void MakeReport(DateTime startDate, DateTime endDate, Window window)
        {
            try
            {
                List<Event> reportEvents = Events.Where(e => (startDate <= e.StartDatetime && endDate >= e.StartDatetime) || (startDate <= e.EndDatetime && endDate >= e.EndDatetime)).ToList();
                string outputPath = "C:\\output.pdf";
                string arialFontPath = "C:\\Windows\\Fonts\\arial.ttf";
                PdfFont arial = PdfFontFactory.CreateFont(arialFontPath, PdfEncodings.IDENTITY_H);
                PdfWriter writer = new PdfWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\{DateTime.Now.ToString("yyyy_MM_dd-HH_mm")}.pdf");
                PdfDocument pdfDocument = new PdfDocument(writer.SetSmartMode(true));
                Document d = new Document(pdfDocument, iText.Kernel.Geom.PageSize.LETTER);
                LineSeparator ls = new LineSeparator(new SolidLine());
                d.SetFont(arial);
                d.Add(new Paragraph("Отчёт").SetFontSize(20)).SetTextAlignment(TextAlignment.CENTER);
                d.Add(new Paragraph($"C \"{startDate.Day}\" {startDate.ToString("MMMM yyyy")} по \"{endDate.Day}\" {endDate.ToString("MMMM yyyy")}").SetFontSize(14)).SetTextAlignment(TextAlignment.CENTER);
                d.Add(ls);

                // Общая статистика
                d.Add(new Paragraph("Общая статистика").SetFontSize(14)).SetTextAlignment(TextAlignment.CENTER);
                Table table = new Table(2, true)
                    .SetWidth(UnitValue.CreatePercentValue(100));
                table.AddHeaderCell("Переменная");
                table.AddHeaderCell("Значение");
                table.AddCell("Количетсво эксонатов");
                table.AddCell($"{Exhibits.Count}");

                table.AddCell("Количетсво мероприятий");
                table.AddCell($"{Events.Count}");

                table.AddCell("Количетсво пользователей");
                table.AddCell($"{Users.Count}");
                d.Add(table);

                d.Add(ls);

                // Мроприятия
                d.Add(new Paragraph("Статистика по мероприятиям").SetFontSize(14)).SetTextAlignment(TextAlignment.CENTER);
                Table tableEvent = new Table(2, true)
    .SetWidth(UnitValue.CreatePercentValue(100));
                tableEvent.AddCell("Количество мероприятий за период");
                tableEvent.AddCell($"{reportEvents.Count()}");
                tableEvent.AddCell("Самое востребованное мероприятия");
                tableEvent.AddCell($"{Events.FirstOrDefault(e => e.RegistrationCount == Events.Select(s => s.RegistrationCount).Max()).Title}");

                d.Add(tableEvent);
                d.Add(ls);


                SKImage chartImageColumn = GenerateCartChart(reportEvents.Where(e => e.RegistrationCount != 0).ToList());
                SKData dataColumn = chartImageColumn.Encode(SKEncodedImageFormat.Png, 100);
                byte[] imageBytesColumn = dataColumn.ToArray();
                ImageData imageDataColumn = ImageDataFactory.Create(imageBytesColumn);
                d.Add(new iText.Layout.Element.Image(imageDataColumn).ScaleToFit(300, 300).SetHorizontalAlignment(HorizontalAlignment.CENTER));
                // Финансовая отчётность

                d.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                d.Add(new Paragraph("Статистика по финансам").SetFontSize(14)).SetTextAlignment(TextAlignment.CENTER);
                Table tableFin = new Table(2, true)
    .SetWidth(UnitValue.CreatePercentValue(100));

                tableFin.AddCell("Выручка за период");
                tableFin.AddCell($"{AllTickets.Where(t => startDate < t.PurchaseDate && endDate > t.PurchaseDate).Select(t => t.Price).Sum()}");

                tableFin.AddCell("По типам билетов");
                tableFin.AddCell($" ");

                foreach (var tType in TicketTypes)
                {
                    tableFin.AddCell($"{tType.Name}");
                    tableFin.AddCell($"{AllTickets.Where(e => e.TypeId == tType.Id).Where(t => startDate < t.PurchaseDate && endDate > t.PurchaseDate).Select(t => t.Price).Sum()}");
                }

                tableFin.AddCell("Выручка за мероприятия");
                tableFin.AddCell($"{reportEvents.Select(e => e.EventRegistrations.Count() * e.Price).Sum()}");

                tableFin.AddCell("По мероприятиям (5 лучших)");
                tableFin.AddCell($" ");

                foreach (var eType in reportEvents.Where(e => e.EventRegistrations.Count() != 0).OrderBy(s => s.EventRegistrations.Count() * s.Price).Take(5))
                {
                    tableFin.AddCell($"{eType.Title}");
                    tableFin.AddCell($"{eType.EventRegistrations.Count * eType.Price}");
                }

                d.Add(tableFin);
                d.Add(ls);

                SKImage chartImage = GeneratePieChart(reportEvents.Where(e => e.RegistrationCount != 0).ToList());
                SKData data = chartImage.Encode(SKEncodedImageFormat.Png, 100);
                byte[] imageBytes = data.ToArray();
                ImageData imageData = ImageDataFactory.Create(imageBytes);
                d.Add(new iText.Layout.Element.Image(imageData).ScaleToFit(300, 300).SetHorizontalAlignment(HorizontalAlignment.CENTER));

                int n = pdfDocument.GetNumberOfPages();
                for (int i = 1; i <= n; i++)
                {
                    d.ShowTextAligned(new Paragraph(System.String
                       .Format("стр." + i + " из " + n)),
                        559, 806, i, TextAlignment.RIGHT,
                        VerticalAlignment.TOP, 0);
                }
                d.Close();
                MessageBoxManager.GetMessageBoxStandard("Готово", $"Отчёт успешно файлов").ShowWindowDialogAsync(window);
            }
            catch
            {
                CallMessageBox("Ошибка, что-то пошло нет так", window);
            }
        }
        private static SKImage GeneratePieChart(List<Event> events)
        {
            var pieSeries = new List<PieSeries<decimal>>();
            foreach (var ev in events)
            {
                pieSeries.Add(new PieSeries<decimal> { Name = ev.Title, Values = new List<decimal> { ev.EventRegistrations.Count() * (decimal)ev.Price } });

            }
            var chart = new SKPieChart
            {
                Width = 600,
                Height = 400,
                Series = pieSeries,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right
            };

            return chart.GetImage();
        }
        private static SKImage GenerateCartChart(List<Event> events)
        {
            var columnSeries = new List<ColumnSeries<int>>();
            foreach (var ev in events)
            {
                columnSeries.Add(new ColumnSeries<int> { Name = ev.Title, Values = new List<int> { ev.EventRegistrations.Count } });

            }

            var chart = new SKCartesianChart
            {
                Width = 600,
                Height = 400,
                Series = columnSeries,
                XAxes = new List<Axis> { new Axis { IsVisible = false } },
                YAxes = new List<Axis> { new Axis { MinStep = 1,  MinLimit = 0} },
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right
            };

            return chart.GetImage();
        }
    }

}
