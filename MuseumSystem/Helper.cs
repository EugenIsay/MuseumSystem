using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MuseumSystem.Context;
using MuseumSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseumSystem
{
    public static class Helper
    {
        static User3Context DBContext = new User3Context();
        static User currentUser = null;
        public static List<Gender> Genders
        {
            get => DBContext.Genders.ToList();
        }
        public static List<User> Users
        {
            get => DBContext.Users.ToList();
        }

        public static List<Event> Events
        {
            get => DBContext.Events.Include(e => e.Type).Include(e => e.Organizer).Include(e => e.IncludedItems).Include(e => e.EventRegistrations).ToList();
        }

        public static List<Ticket> Tickets
        {
            get => DBContext.Tickets.Where(t => t.UserId == currentUser.Id).Include(t => t.User).Include(t => t.EventRegistrations).Include(t => t.Type).ToList();
        }

        public static List<Exhibit> Exhibits
        {
            get => DBContext.Exhibits.Include(e => e.AtachedMedia).Include(e => e.Category).ToList();
        }

        public static bool IsExist(string FirsRow, string Password)
        {
            currentUser = DBContext.Users.FirstOrDefault(u => (u.Login == FirsRow || u.Email == FirsRow) && u.Password == Password);
            return currentUser != null;
        }
        public static bool CanRegister(User User, AuthorizationWindow Window)
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
                currentUser = User;
                return true;
            }
            catch
            {
                return false;
            }
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
