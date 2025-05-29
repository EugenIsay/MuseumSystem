using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MuseumSystem.Models;
using System.Collections.Generic;

namespace MuseumSystem;

public partial class CommentWindow : Window
{
    public CommentWindow()
    {
        InitializeComponent();
    }
    public CommentWindow(List<ExhibitReview> reviews)
    {
        InitializeComponent();
        reviews.Reverse();
        CommentLB.ItemsSource = reviews;
    }
    public CommentWindow(List<EventReview> reviews)
    {
        InitializeComponent();
        reviews.Reverse();
        CommentLB.ItemsSource = reviews;
    }
    private void BackButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
}