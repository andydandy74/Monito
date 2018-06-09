using Dynamo.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Monito
{
    /// <summary>
    /// Interaction logic for InCanvasSearchWindow.xaml
    /// </summary>
    public partial class InCanvasSearchWindow : Window
    {

        public InCanvasSearchWindow()
        {
            InitializeComponent();
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            clickedGUID.Text = "" + ((Button)sender).Tag;
        }
    }
}