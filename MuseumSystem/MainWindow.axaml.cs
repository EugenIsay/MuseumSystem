using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using MuseumSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Tmds.DBus.Protocol;

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

            TicketIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/Ticket.png");
            TicketAmount.Text = Helper.HowMainTickets.ToString();
            VisitIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/User.png");
            VisitAmount.Text = Helper.HowManyVisitors.ToString();
            EventIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/Mesuem.png");
            EventAmount.Text = Helper.HowManyEvents.ToString();
            ExhibitIcon.Source = new Bitmap(Environment.CurrentDirectory + "/Icons/Exhibit.png");
            ExhibitAmount.Text = Helper.HowManyEhxibitions.ToString();

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
                ExhibitLB.ItemsSource = Helper.Exhibits;
                ExhibitSelectedText.Text = "Экспонаты";
            }
            else
            {
                ExhibitLB.ItemsSource = Helper.Exhibits.Where(e => e.CategoryId == (ExibitTypeCB.SelectedItem as Category).Id);
                ExhibitSelectedText.Text = Helper.Categories.FirstOrDefault(c => c.Id == (ExibitTypeCB.SelectedItem as Category).Id).Name;
            }

            MainTab.SelectedIndex = Helper.Page;
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
        public Image BorderChild
        {
            get => currentHoverExhibit.Child as Image;
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
    }
}