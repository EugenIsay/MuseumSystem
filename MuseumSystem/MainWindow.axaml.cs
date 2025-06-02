using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MuseumSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tmds.DBus.Protocol;
using iText.IO.Image;
using iText.Kernel.Pdf.Canvas.Parser;

namespace MuseumSystem
{
    public partial class MainWindow : Window
    {
        int HEpage = 0;
        private DispatcherTimer _animationTimer;
        public MainWindow()
        {
            InitializeComponent();
            EventLB.ItemsSource = Helper.ShownEvents;
            TicketLB.ItemsSource = Helper.Tickets;
            ExhibitLB.ItemsSource = Helper.Exhibits;
            BDay.DisplayDateEnd = System.DateTime.Now.AddYears(-12);
            Gender.ItemsSource = Helper.Genders;
            UserLB.ItemsSource = Helper.Users.Where(u => u.RoleId != 1);
            ExibitTypeCB.ItemsSource = Helper.Categories.Concat(new List<Category>() { new Category() { Id = 0, Name = "Все категории" } }).OrderBy(c => c.Id);
            MainTab.SelectedIndex = Helper.Page;
            ReadyButton.IsVisible = false;
            MainImage.Source = new Bitmap(Environment.CurrentDirectory + "/autum.jpg");
            HelloMessage.Text = $"Добропожаловать, {Helper.currentUser.FullName}!";
            HappEventLB.ItemsSource = Helper.Events.Where(s => s.StartDatetime > DateTime.Now).OrderByDescending(e => e.StartDatetime).Take(3);
            NearlyEventLB.ItemsSource = Helper.Events.Where(s => s.StartDatetime < DateTime.Now).OrderBy(e => e.EndDatetime).Take(3);
            UsersEvents.ItemsSource = Helper.UsersEvents.Take(3);
            Leafes.Source = new Bitmap(Environment.CurrentDirectory + "/leafs.jpg");
            River.Source = new Bitmap(Environment.CurrentDirectory + "/river.jpg");
            Forest.Source = new Bitmap(Environment.CurrentDirectory + "/forest.jpg");

            TicketIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/Ticket.png");
            TicketAmount.Text = Helper.HowMainTickets.ToString();
            VisitIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/User.png");
            VisitAmount.Text = Helper.HowManyVisitors.ToString();
            EventIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/Mesuem.png");
            EventAmount.Text = Helper.HowManyEvents.ToString();
            ExhibitIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/Exhibit.png");
            ExhibitAmount.Text = Helper.HowManyEhxibitions.ToString();
            EventTypeCB.ItemsSource = Helper.EventTypes.Concat(new List<Models.EventType>() { new Models.EventType { Id = 0, Name = "Все мероприятя" } }).OrderBy(c => c.Id);
            EventTypeCB.SelectedIndex = 0;
            TicketLB.SelectedItems = new List<Ticket>();
            if (UsersEvents.ItemCount == 0)
            {
                MessageHas.IsVisible = true;
            }
            Resize();
        }

        public void FillForm()
        {
            Login.Text = Helper.currentUser.Login;
            FirstName.Text = Helper.currentUser.FirstName;
            LastName.Text = Helper.currentUser.LastName;
            Patronymic.Text = Helper.currentUser.Patronymic;
            Email.Text = Helper.currentUser.Email;
            Phone.Text = Helper.currentUser.PhoneNumber;
            BDay.SelectedDate = Helper.currentUser.Birthday.ToDateTime(new TimeOnly());
            Gender.SelectedItem = Helper.currentUser.Gender;
            Password.Text = Helper.currentUser.Password;
        }
        private void AddExhibitionButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new ExhibitWindow().Show();
            this.Close();
        }

        private void Border_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            new ExhibitWindow(ExhibitLB.SelectedItem as Exhibit).Show();
            this.Close();
        }

        private void TicketButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new TicketWindow().Show();
            this.Close();
        }

        private void EventButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new EventWindow().Show();
            this.Close();
        }

        private void Border_DoubleTapped_1(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (EventLB.SelectedItem as Event == null)
            {
                return;
            }
            new EventWindow(EventLB.SelectedItem as Event).Show();
            this.Close();
        }

        private async void ComfirmButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!DateOnly.TryParse(BDay.Text, out DateOnly _))
            {
                Helper.CallMessageBox("Укажите дату рождения в формате день месяц год через точки", this);
                return;
            }
            if (Password.Text != Helper.currentUser.Password)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Предосторежение", "Вы точно хотите изменить пароль?", ButtonEnum.YesNo);
                var result = await box.ShowAsync();
                if (!result.Equals(ButtonResult.Yes))
                {
                    return;
                }
            }
            var resultReg = await Helper.CanRegister(new User()
            {
                FirstName = FirstName.Text,
                LastName = LastName.Text,
                Patronymic = Patronymic.Text,
                Email = Email.Text,
                Login = Login.Text,
                PhoneNumber = Phone.Text,
                GenderId = (Gender.SelectedItem as Models.Gender).Id,
                Birthday = DateOnly.Parse(BDay.Text),
                Password = Password.Text,
                RoleId = 3
            }, this);
            if (resultReg)
            {
                MessageBoxManager.GetMessageBoxStandard("Готво", "Пользователь успешно сохранён").ShowWindowDialogAsync(this);
            }
        }

        private void ExitButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new AuthorizationWindow().Show();
            Helper.Page = 0;
            this.Close();
        }

        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            ReadyButton.IsVisible = true;
        }

        private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            ReadyButton.IsVisible = true;
        }

        private void TabControl_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (MainTab != null)
            {
                Helper.Page = MainTab.SelectedIndex;
                ReadyButton.IsVisible = false;
                FillForm();
            }
        }

        private void UserLB_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (UserLB.SelectedItems!.Count > 0)
                BlockButton.IsVisible = true;
            else
                BlockButton.IsVisible = false;
        }

        private async void BlockButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var box = MessageBoxManager
            .GetMessageBoxStandard("Предосторежение", "Вы точно хотите разблокировать/заблокировать данных пользователей?",
                ButtonEnum.YesNo);
            var result = await box.ShowAsync();
            if (result.Equals(ButtonResult.Yes))
            {
                var Users = UserLB.SelectedItems;
                foreach (var user in Users)
                {
                    Helper.ChangeUserBool(user as User);
                    UserLB.ItemsSource = Helper.Users.Where(u => u.RoleId != 1); ;
                }
            }
        }

        DateTime startDate = new DateTime();
        DateTime endDate = new DateTime();
        private void ReportCB_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            DateSelector.IsVisible = false;
            switch ((sender as ComboBox).SelectedIndex)
            {
                case 0:
                    startDate = DateTime.Now.AddDays(-DateTime.Now.Day);
                    endDate = DateTime.Now.AddDays(-DateTime.Now.Day).AddMonths(1);
                    break;
                case 1:
                    startDate = DateTime.Now.AddMonths(-DateTime.Now.Month);
                    endDate = DateTime.Now.AddMonths(-DateTime.Now.Month).AddYears(1);
                    break;
                case 2:
                    DateSelector.IsVisible = true;
                    break;
                default:
                    return;

            }
        }

        private void MakeReportButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ReportCB.SelectedIndex == -1)
            {
                Helper.CallMessageBox("Выберите период отчёта", this);
                return;
            }
            else if (ReportCB.SelectedIndex == 2)
            {
                if (StartDate.SelectedDate == null || StartDate.SelectedDate == null || StartDate.SelectedDate < EndDate.SelectedDate)
                {
                    Helper.CallMessageBox("Выберите корректный период отчёта", this);
                    return;
                }
                startDate = StartDate.SelectedDate.Value.DateTime;
                endDate = EndDate.SelectedDate.Value.DateTime;
            }
            Helper.MakeReport(startDate, endDate, this);
        }

        private void ExibiyHeader_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            var ctl = sender as Control;
            if (ctl != null)
            {
                FlyoutBase.ShowAttachedFlyout(ctl);
                var a = FlyoutBase.GetAttachedFlyout(ctl);
            }
        }

        private void TextBlock_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            (sender as TextBlock).TextDecorations = TextDecorations.Underline;

        }

        private void TextBlock_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            (sender as TextBlock).TextDecorations = null;
        }

        private void CategoriesLB_Changed(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            Helper.Page = 3;
            var fly = (sender as ListBox).FindAncestorOfType<FlyoutPresenter>();
            var brd = fly.Parent.Parent;
            var ctl = FlyoutBase.GetAttachedFlyout(brd as Border);
            ctl.Hide();
            if ((ExibitTypeCB.SelectedItem as Category).Id == 0)
            {
                exCategory = 0;
                ExhibitSelectedText.Text = "Экспонаты";
            }
            else
            {
                exCategory = (ExibitTypeCB.SelectedItem as Category).Id;
                ExhibitSelectedText.Text = Helper.Categories.FirstOrDefault(c => c.Id == (ExibitTypeCB.SelectedItem as Category).Id).Name;
            }

            MainTab.SelectedIndex = Helper.Page;
            UpdateExibits();
        }

        private void Window_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
        {
            Resize();
        }
        private void Resize()
        {
            if (ExhibitLB.ItemsPanelRoot as UniformGrid != null)
            {
                if ((int)Math.Ceiling(MainW.Width / 600) > 1)
                {
                    (ExhibitLB.ItemsPanelRoot as UniformGrid).Columns = (int)Math.Ceiling(MainW.Width / 480);
                }
                else
                {
                    (ExhibitLB.ItemsPanelRoot as UniformGrid).Columns = 1;
                }
            }
            if (BigList != null)
            {
                if (MainW.Width <= 800)
                {
                    BigList.Columns = 1;
                }
                else
                {
                    BigList.Columns = 2;
                }
            }
            if (MainGrid != null)
            {
                MainGrid.Width = MainW.Width;
                MainGrid.Height = MainW.Height - 200;
            }
        }



        private void Rectangle_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if ((sender as Border).Child.Name == "LeftHE" && Helper.UsersEvents.Count() > 3)
            {
                LeftHE.IsVisible = true;
            }
            else if (Helper.UsersEvents.Count() > 3)
            {
                RightHE.IsVisible = true;
            }
        }

        private void Rectangle_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if ((sender as Border).Child.Name == "LeftHE")
            {
                LeftHE.IsVisible = false;
            }
            else
            {
                RightHE.IsVisible = false;
            }
        }

        private void HEButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if ((sender as Button).Name == "BackHE")
            {
                if (HEpage == 0)
                    HEpage = (int)Math.Ceiling(Helper.UsersEvents.Count() / 3.00);
                else
                    HEpage--;
            }
            else
            {
                if ((HEpage + 1) * 3 >= Helper.UsersEvents.Count())
                    HEpage = 0;
                else
                    HEpage++;
            }
            UsersEvents.ItemsSource = Helper.UsersEvents.Skip(HEpage * 3).Take(3);
        }


        List<AtachedMedium> Images = new List<AtachedMedium>();
        Border currentHoverExhibit = null;
        int HoveredPage = 0;
        public Avalonia.Controls.Image BorderChild
        {
            get => currentHoverExhibit.Child as Avalonia.Controls.Image;
        }
        private void EIBorder_PointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (sender == null)
                return;
            if (((sender as Border).Tag as Exhibit)?.AdditionalImages?.Count == 0) return;
            currentHoverExhibit = (sender as Border);
            Images = (currentHoverExhibit.Tag as Exhibit).SpecificMdeia(1);
            BorderChild.IsVisible = true;
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Интервал переключения
            };

            _animationTimer.Tick += ImageAnimation;
            _animationTimer.Start(); ;
        }

        private void ImageAnimation(object sender, EventArgs e)
        {
            HoveredPage++;
            if (HoveredPage >= Images.Count)
            {
                HoveredPage = 0;
            }
            BorderChild.Source = Images[HoveredPage].ImageBitmap;
        }


        private void EIBorder_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (_animationTimer != null)
            {
                _animationTimer.Tick -= ImageAnimation;
                _animationTimer.Stop();
                _animationTimer = null;
                HoveredPage = 0;
                BorderChild.IsVisible = false;
                currentHoverExhibit = null;
                Images.Clear();
            }
        }

        private void PressButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            List<Ticket> selectedTickets = TicketLB.SelectedItems as List<Ticket>;

            List<bool> succes = new List<bool>();
            foreach (var OneTicket in selectedTickets)
            {
                succes.Add(Helper.PrintTicket(OneTicket, this));
            }
            MessageBoxManager.GetMessageBoxStandard("Готово", $"Создано {succes.Where(s => s == true).Count()} файлов").ShowWindowDialogAsync(this);
        }

        private void TicketLB_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (TicketLB != null)
            {
                PressButton.IsVisible = TicketLB.SelectedItems.Count > 0;
            }
        }

        int exCategory = 0;
        string exSearch = "";
        int exSort = 0;
        private void SearchExhibit_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            exSearch = (sender as TextBox).Text;
            UpdateExibits();
        }

        public void UpdateExibits()
        {
            if (ExhibitLB == null)
                return;
            string[] searchWords = exSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(w => w.ToLower())
                                     .ToArray();
            // Фильтрация
            ExhibitLB.ItemsSource = Helper.Exhibits
                .Where(e => e.CategoryId == exCategory || exCategory == 0)
                .Where(exhibit =>
            {
                // Поиск
                string fullText = $"{exhibit.Name?.ToLower()} {exhibit.Description?.ToLower()} {exhibit.InventoryNumber.ToLower()} {exhibit.Condition.ToLower()} {exhibit.PermanentlyLocated.ToLower()}";
                return searchWords.Any(word => fullText.Contains(word))
                || string.IsNullOrEmpty(exSearch);

            }).OrderByDescending<Exhibit, object>(e =>
            // Сортировка по убыванию
            exSort switch
            {
                1 => e.Name,
                2 => e.InventoryNumber,
                3 => e.AddDate,
                4 => e.ApproximateCost,
                5 => e.AvgRaiting,
                _ => null
            })
            .OrderBy<Exhibit, object>(e =>
            exSort switch
            {
                // Сортировка по возрастанию
                7 => e.Name,
                8 => e.InventoryNumber,
                9 => e.AddDate,
                10 => e.ApproximateCost,
                11 => e.AvgRaiting,
                _ => null
            }).ToList();
        }

        private void EXComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (sender == null || EXSortCB == null)
                return;
            if ((sender as ComboBox).SelectedIndex == 0)
            {
                EXSortCB.IsVisible = false;
            }
            else
            {
                EXSortCB.IsVisible = true;
            }
            exSort = (sender as ComboBox).SelectedIndex + (-EXSortCB.SelectedIndex + 1) * 6;
            UpdateExibits();
        }

        private void EXComboBox_SelectionChanged_2(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (sender == null)
                return;
            if ((sender as ComboBox).SelectedIndex == 0)
            {
                exSort += 6;
            }
            else
            {
                exSort -= 6;
            }
            UpdateExibits();
        }


        int evType = 0;
        string evSearch = "";
        int evSort = 0;
        private void EvTextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        {
            evSearch = (sender as TextBox).Text;
            UpdateEvents();
        }

        public void UpdateEvents()
        {
            if (EventLB == null)
                return;
            string[] searchWords = evSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(w => w.ToLower())
                                     .ToArray();
            // Фильтрация
            EventLB.ItemsSource = Helper.Events
                .Where(e => e.TypeId == evType || evType == 0)
                .Where(evnt =>
                {
                    // Поиск
                    string fullText = $"{evnt.Title?.ToLower()} {evnt.Description?.ToLower()} {evnt.Organizer.FullName.ToLower()} {evnt.Addres.ToLower()}";
                    return searchWords.Any(word => fullText.Contains(word))
                    || string.IsNullOrEmpty(evSearch);

                }).OrderByDescending<Event, object>(e =>
                // Сортировка по убыванию
                evSort switch
                {
                    1 => e.Title,
                    2 => e.StartDatetime,
                    3 => e.FreeSeats,
                    4 => e.Price,
                    5 => e.AvgRaiting,
                    _ => null
                })
            .OrderBy<Event, object>(e =>
            evSort switch
            {
                // Сортировка по возрастанию
                7 => e.Title,
                8 => e.StartDatetime,
                9 => e.FreeSeats,
                10 => e.Price,
                11 => e.AvgRaiting,
                _ => null
            }).ToList();
        }

        private void EVTypeComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            evType = ((sender as ComboBox).SelectedItem as Models.EventType).Id;
            UpdateEvents();
        }

        private void EvComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (sender == null || EVSortCB == null)
                return;
            if ((sender as ComboBox).SelectedIndex == 0)
            {
                EVSortCB.IsVisible = false;
            }
            else
            {
                EVSortCB.IsVisible = true;
            }
            evSort = (sender as ComboBox).SelectedIndex + (-EVSortCB.SelectedIndex + 1) * 6;
            UpdateEvents();
        }

        private void EvComboBox_SelectionChanged_2(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (sender == null)
                return;
            if ((sender as ComboBox).SelectedIndex == 0)
            {
                evSort += 6;
            }
            else
            {
                evSort -= 6;
            }
            UpdateEvents();
        }
    }
}