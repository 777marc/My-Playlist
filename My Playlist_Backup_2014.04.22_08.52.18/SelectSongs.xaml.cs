using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.Collections.ObjectModel;
using My_Playlist.Model;
using System.Windows.Media;

namespace My_Playlist
{
    public partial class SelectSongs : PhoneApplicationPage
    {
        
        private MPDB.MyPlaylistDataContext playlistDB;
        
        private ObservableCollection<MPDB.LocalLibrary> _songs;
        public ObservableCollection<MPDB.LocalLibrary> Songs
        {
            get
            {
                return _songs;
            }
            set
            {
                if (_songs != value)
                {
                    _songs = value;
                }
            }
        }

        private ObservableCollection<MPDB.Playlist> _selectedPlaylists;
        public ObservableCollection<MPDB.Playlist> SelectedPlaylists
        {
            get
            {
                return _selectedPlaylists;
            }
            set
            {
                if (_selectedPlaylists != value)
                {
                    _selectedPlaylists = value;
                }
            }
        }

        public SelectSongs()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();

            // Connect to the database and instantiate data context.
            playlistDB = new MPDB.MyPlaylistDataContext(MPDB.MyPlaylistDataContext.DBConnectionString);

            // Data context and observable collection are children of the main page.
            this.DataContext = this;
            
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

            //// Create a new button and set the text value to the localized string from AppResources.
            //ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Assets/AppBar/check.png", UriKind.Relative));
            //appBarButton2.Text = "Playlist";
            //appBarButton2.Click += new EventHandler(goToPlaylist);
            //ApplicationBar.Buttons.Add(appBarButton2);
        }

        //private void goToPlaylist(object sender, EventArgs e)
        //{
        //    string playListName = getPlaylistName();

        //    NavigationService.Navigate(new Uri("/PivMain.xaml?playlistName=" + playListName, UriKind.Relative));
        //}

        private void go_home(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MusicHub.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            MediaLibrary library = new MediaLibrary();
            string albumName = "";
            string artistName = "";
            string searchType = "Album";
            string genre = "";
            string song_name = "";

            NavigationContext.QueryString.TryGetValue("type", out searchType);
            
            if(searchType == "Album")
            {
                if (NavigationContext.QueryString.TryGetValue("value", out albumName))
                {
                    var songs = from MPDB.LocalLibrary ll in playlistDB.LocalLibraries
                                where ll.AlbumName == albumName
                                select ll;

                    Songs = new ObservableCollection<MPDB.LocalLibrary>(songs);

                    songsListBox.ItemsSource = Songs;
                }
            }

            if (searchType == "Artist")
            {
                if (NavigationContext.QueryString.TryGetValue("value", out artistName))
                {
                    var songs = from MPDB.LocalLibrary ll in playlistDB.LocalLibraries
                                where ll.ArtistName == artistName
                                select ll;

                    Songs = new ObservableCollection<MPDB.LocalLibrary>(songs);

                    songsListBox.ItemsSource = Songs;
                }
            }

            if (searchType == "Genre")
            {
                if (NavigationContext.QueryString.TryGetValue("value", out genre))
                {
                    var songs = from MPDB.LocalLibrary ll in playlistDB.LocalLibraries
                                where ll.Genre == genre
                                select ll;

                    Songs = new ObservableCollection<MPDB.LocalLibrary>(songs);

                    songsListBox.ItemsSource = Songs;
                }
            }

            if (searchType == "Songs")
            {
                if (NavigationContext.QueryString.TryGetValue("value", out song_name))
                {
                    var songs = from MPDB.LocalLibrary ll in playlistDB.LocalLibraries
                                where ll.SongName == song_name
                                select ll;

                    Songs = new ObservableCollection<MPDB.LocalLibrary>(songs);

                    songsListBox.ItemsSource = Songs;
                }
            }


            // Call the base method.
            base.OnNavigatedTo(e);
        }

        private string getPlaylistName()
        {
            string playListName = "";

            if (App.CURRENTPLAYLISTNAME != "" && App.CURRENTPLAYLISTNAME != null)
            {
                playListName = App.CURRENTPLAYLISTNAME;
            }
            else
            {
                playListName = App.NEWPLAYLISTNAME;
            }

            return playListName;
        }

        private int getSongListPosition(string playlistName)
        {
            int listPosition = 0;

            var songsInPlaylist = from MPDB.Playlist pl in playlistDB.Playlists
                                where pl.PlaylistName == playlistName
                                select pl;

            SelectedPlaylists = new ObservableCollection<MPDB.Playlist>(songsInPlaylist);

            listPosition = SelectedPlaylists.Count + 1;

            return listPosition;
        }

        private void songsListBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            MPDB.Playlist newPlaylistItem = new MPDB.Playlist();
            MPDB.LocalLibrary selectedSong;
            string playListName = "";
            int listPosition = 0;

            if (songsListBox.SelectedValue != null)
            {
                selectedSong = (MPDB.LocalLibrary)songsListBox.SelectedValue;
            }
            else
            {
                return;
            }

            playListName = getPlaylistName();

            listPosition = getSongListPosition(playListName);

            newPlaylistItem.SongName = selectedSong.SongName;
            newPlaylistItem.SongID = listPosition;
            newPlaylistItem.PlaylistName = playListName;
            newPlaylistItem.ItemDate = DateTime.Now;

            Songs.Remove(selectedSong);

            playlistDB.Playlists.InsertOnSubmit(newPlaylistItem);
            playlistDB.SubmitChanges();
            App.CURRENTPLAYLISTNAME = playListName;

            if (Songs.Count == 0)
            {
                NavigationService.Navigate(new Uri("/MusicHub.xaml?playlistName=" + playListName, UriKind.Relative));
            }
        }

    }
}