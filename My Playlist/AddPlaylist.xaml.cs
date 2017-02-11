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
using Telerik.Windows.Data;
using My_Playlist.Model;
using System.Collections.ObjectModel;

namespace My_Playlist
{
    public partial class AddPlaylist : PhoneApplicationPage
    {
        private MPDB.MyPlaylistDataContext myPlaylistDB;

        private ObservableCollection<MPDB.LocalLibrary> _localLibraries;
        public ObservableCollection<MPDB.LocalLibrary> LocalLibraries
        {
            get
            {
                return _localLibraries;
            }
            set
            {
                if (_localLibraries != value)
                {
                    _localLibraries = value;
                }
            }
        }
        
        public AddPlaylist()
        {
            myPlaylistDB = new MPDB.MyPlaylistDataContext(MPDB.MyPlaylistDataContext.DBConnectionString);

            InitializeComponent();
            loadList();
            BuildLocalizedApplicationBar();
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

        private void loadList()
        {
            List<string> searchType = new List<string>() { "Artist", "Album", "Genre", "Songs" };

            rlpType.ItemsSource = searchType;

        }

        private void popGrouplist()
        {
            try
            {
                loadSelections("Artist");
            }
            catch (Exception ex)
            {
                NavigationService.Navigate(new Uri("/Player.xaml", UriKind.Relative));
            }
        }

        private void rjlMainList_ItemTap(object sender, Telerik.Windows.Controls.ListBoxItemTapEventArgs e)
        {
            NavigationService.Navigate(new Uri("/SelectSongs.xaml?value=" + HttpUtility.HtmlEncode(rjlMainList.SelectedValue.ToString()) + "&type=" + rlpType.SelectedValue.ToString(), UriKind.Relative));
        }

        private void rlpType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadSelections(rlpType.SelectedValue.ToString());
        }

        private void loadSelections(string searchType)
        {
            MediaLibrary library = new MediaLibrary();

            switch (searchType)
            {
                case "Artist":

                    var artistItems = from MPDB.LocalLibrary ll in myPlaylistDB.LocalLibraries
                                       where ll.ArtistName != ""
                                       orderby ll.ArtistName ascending
                                       select ll;

                    LocalLibraries = new ObservableCollection<MPDB.LocalLibrary>(artistItems);

                    List<string> artistnames = new List<string>();

                    foreach (var an in LocalLibraries)
                    {
                        if (!artistnames.Contains(an.ArtistName))
                        {
                            artistnames.Add(an.ArtistName);
                        }
                    }

                    rjlMainList.ItemsSource = artistnames;
                    break;

                case "Album":

                    var albumItems = from MPDB.LocalLibrary ll in myPlaylistDB.LocalLibraries
                                       where ll.AlbumName != ""
                                        orderby ll.AlbumName ascending
                                       select ll;

                    LocalLibraries = new ObservableCollection<MPDB.LocalLibrary>(albumItems);

                    List<string> albumnames = new List<string>();

                    foreach (var aln in LocalLibraries)
                    {
                        if (!albumnames.Contains(aln.AlbumName))
                        {
                            albumnames.Add(aln.AlbumName);
                        }
                    }

                    rjlMainList.ItemsSource = albumnames;
                    break;

                case "Genre":

                    var genreItems = from MPDB.LocalLibrary ll in myPlaylistDB.LocalLibraries
                                       where ll.Genre != ""
                                        orderby ll.Genre ascending
                                       select ll;

                    LocalLibraries = new ObservableCollection<MPDB.LocalLibrary>(genreItems);

                    List<string> genrenames = new List<string>();

                    foreach (var gn in LocalLibraries)
                    {
                        if (!genrenames.Contains(gn.Genre))
                        {
                            genrenames.Add(gn.Genre);
                        }
                    }

                    rjlMainList.ItemsSource = genrenames;
                    break;

                case "Songs":

                    var songItems = from MPDB.LocalLibrary ll in myPlaylistDB.LocalLibraries
                                     where ll.SongName != ""
                                    orderby ll.SongName ascending
                                     select ll;

                    LocalLibraries = new ObservableCollection<MPDB.LocalLibrary>(songItems);

                    List<string> songNames = new List<string>();

                    foreach (var sn in LocalLibraries)
                    {
                        if (!songNames.Contains(sn.SongName))
                        {
                            songNames.Add(sn.SongName);
                        }
                    }

                    rjlMainList.ItemsSource = songNames;
                    break;

                default:

                    var artistItems1 = from MPDB.LocalLibrary ll in myPlaylistDB.LocalLibraries
                                       where ll.ArtistName != ""
                                       orderby ll.ArtistName ascending
                                       select ll;

                    LocalLibraries = new ObservableCollection<MPDB.LocalLibrary>(artistItems1);

                    List<string> artistnames1 = new List<string>();

                    foreach (var an in LocalLibraries)
                    {
                        if (!artistnames1.Contains(an.ArtistName))
                        {
                            artistnames1.Add(an.ArtistName);
                        }
                    }

                    rjlMainList.ItemsSource = artistnames1;
                    break;

            }
        }
        
    }
}