using Avalonia.Controls;
using LibVLCSharp.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using MuseumSystem.Context;
using MuseumSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

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
            get => DBContext.Tickets.Where(t => t.UserId == currentUser.Id).Include(t => t.User).Include(t => t.EventRegistrations).ThenInclude(t => t.Event).Include(t => t.Type).ToList();
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
            get => DBContext.Exhibits.Where(u => u.AddDate.Value.Year == DateTime.Now.Year).Count();
        }
        public static int HowManyEvents
        {
            get => DBContext.Exhibits.Where(u => u.AddDate.Value.Year == DateTime.Now.Year).Count();
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
            get => registrations.Where(u => u.Id == currentUser.Id).Select(r => r.Event).ToList();
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
                    ContentMessage = "введите сюда код из сообщения",
                    InputParams = new InputParams()
                    {
                        Label = "код",
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
    }
}
