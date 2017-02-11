using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using My_Playlist.Model;

namespace My_Playlist
{
    public partial class MusicHub : PhoneApplicationPage
    {
        public MusicHub()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            DataContext = App.ViewModel;

        }


        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("/Assets/AppBar/back.png", UriKind.Relative));
            appBarButton3.Text = "Home";
            appBarButton3.Click += new EventHandler(go_home);
            ApplicationBar.Buttons.Add(appBarButton3);

        }

        private void go_home(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PivMain.xaml", UriKind.Relative));
        }

        private void select_album(object sender, EventArgs e)
        {
            var selectedItem = albumLongListSelector.SelectedItem;

            if (selectedItem == null)
                return;

            MPDB.LocalLibrary album = (MPDB.LocalLibrary)selectedItem;

            string albumName = album.AlbumName;

            NavigationService.Navigate(new Uri("/SelectSongs.xaml?value=" + HttpUtility.HtmlEncode(albumName) + "&type=Album", UriKind.Relative));
        }

        private void select_artist(object sender, EventArgs e)
        {
            var selectedItem = artistLongListSelector.SelectedItem;
            
            if (selectedItem == null)
                return;

            MPDB.LocalLibrary artist = (MPDB.LocalLibrary)selectedItem;

            string artistName = artist.ArtistName;
            
            NavigationService.Navigate(new Uri("/SelectSongs.xaml?value=" + HttpUtility.HtmlEncode(artistName) + "&type=Artist", UriKind.Relative));
        }

        private void select_genre(object sender, EventArgs e)
        {
            var selectedItem = GenreLongListSelector.SelectedItem;

            if (selectedItem == null)
                return;

            MPDB.LocalLibrary genre = (MPDB.LocalLibrary)selectedItem;

            string genreName = genre.Genre;

            NavigationService.Navigate(new Uri("/SelectSongs.xaml?value=" + HttpUtility.HtmlEncode(genreName) + "&type=Genre", UriKind.Relative));
        }

        private void select_song(object sender, EventArgs e)
        {
            var selectedItem = songLongListSelector.SelectedItem;

            if (selectedItem == null)
                return;

            MPDB.LocalLibrary song = (MPDB.LocalLibrary)selectedItem;

            string songName = song.SongName;

            NavigationService.Navigate(new Uri("/SelectSongs.xaml?value=" + HttpUtility.HtmlEncode(songName) + "&type=Songs", UriKind.Relative));
        }

        private void MainPan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (mainPan.SelectedIndex == 0)
                DataContext = App.ViewModel;

            if (mainPan.SelectedIndex == 1)
                DataContext = App.ViewModel2;

            if (mainPan.SelectedIndex == 2)
                DataContext = App.ViewModel3;

            if (mainPan.SelectedIndex == 3)
                DataContext = App.ViewModel4;
        }
    }
}