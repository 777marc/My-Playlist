
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
using Telerik.Windows.Controls;

namespace My_Playlist
{
    public partial class MusicHub : PhoneApplicationPage
    {
        public MusicHub()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            DataContext = App.ViewModel;

            artistLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
            albumLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
            GenreLongListSelector.Visibility = System.Windows.Visibility.Collapsed;

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

            ApplicationBarIconButton appBarSearch = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.search.png", UriKind.Relative));
            appBarSearch.Text = "Home";
            appBarSearch.Click += new EventHandler(search_song);
            ApplicationBar.Buttons.Add(appBarSearch);


        }

        private void search_song(object sender, EventArgs e)
        {

            RadInputPrompt.Show("Select the name of the song to search for:", closedHandler: (args) =>
            {
                string songname = args.Text;

                if (songname == "")
                {
                    MessageBox.Show("Please enter a name to search.");
                    return;
                }

                NavigationService.Navigate(new Uri("/SelectSongs.xaml?value=" + HttpUtility.HtmlEncode(songname) + "&type=SongSearch", UriKind.Relative));
            });

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
            {
                DataContext = App.ViewModel;
                artistLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                albumLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                GenreLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                songLongListSelector.Visibility = System.Windows.Visibility.Visible;
            }

            if (mainPan.SelectedIndex == 1)
            {
                DataContext = App.ViewModel2;
                artistLongListSelector.Visibility = System.Windows.Visibility.Visible;
                albumLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                GenreLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                songLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (mainPan.SelectedIndex == 2)
            { 
                DataContext = App.ViewModel3;
                artistLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                albumLongListSelector.Visibility = System.Windows.Visibility.Visible;
                GenreLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                songLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (mainPan.SelectedIndex == 3)
            { 
                DataContext = App.ViewModel4;
                artistLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                albumLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
                GenreLongListSelector.Visibility = System.Windows.Visibility.Visible;
                songLongListSelector.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}