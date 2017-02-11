using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.ApplicationModel.Store;

namespace My_Playlist
{
    public partial class Donate : PhoneApplicationPage
    {
        public Donate()
        {
            InitializeComponent();
            buildAppBar();
        }

        private void buildAppBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton btnHome = new ApplicationBarIconButton(new Uri("/Assets/AppBar/back.png", UriKind.Relative));
            btnHome.Text = "home";
            btnHome.Click += new EventHandler(go_home);
            ApplicationBar.Buttons.Add(btnHome);
        }

        private void go_home(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PivMain.xaml", UriKind.Relative));
        }

        private async void cmdDonate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string result = await CurrentApp.RequestProductPurchaseAsync("PL_RemoveAds", false);
                txtMessage.Text = "Thanks for your puchase!";
                App.SHOWADS = false;
            }
            catch (Exception)
            {
                txtMessage.Text = "There was a problem processing your request. Please try again.";
            }
            
        }

    }
}