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
using My_Playlist.Model;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using System.Windows.Resources;
using System.Windows.Media;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using Windows.ApplicationModel.Store;


namespace My_Playlist
{
    public partial class PivMain : PhoneApplicationPage
    {
        enum PlayerButtons
        {
            rewind = 0,
            play = 1,
            forward = 2
        }

        private MPDB.MyPlaylistDataContext myPlaylistDB;
        private ObservableCollection<MPDB.Playlist> _playlists;
        public ObservableCollection<MPDB.Playlist> Playlists
        {
            get
            {
                return _playlists;
            }
            set
            {
                if (_playlists != value)
                {
                    _playlists = value;
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

        int _iCount = 0;
        private Task _loadLibraryTask;
        private Song _currentSong;
        bool _playing = false;
        bool _paused = false;
        bool _playlistAvailable = false;

        DispatcherTimer _dt = new DispatcherTimer();

        IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;

        public PivMain()
        {
            InitializeComponent();

            adjustForTheme();

            clearSongInfo();

            myPlaylistDB = new MPDB.MyPlaylistDataContext(MPDB.MyPlaylistDataContext.DBConnectionString);
            
            var total = from li in myPlaylistDB.LocalLibraries
                        where li.LibraryItemId > 0
                        select li;

            MediaLibrary library = new MediaLibrary();
            int mediaTotal = library.Songs.Count();

            if (total.Count() != mediaTotal)
            {
                myPlaylistDB.LocalLibraries.DeleteAllOnSubmit(total);
                myPlaylistDB.SubmitChanges();
                loadLibrary();
            }
            else
            {
                radBusyInd.Visibility = System.Windows.Visibility.Collapsed;
                loadPlaylists();
            }

            // Timer to run the XNA internals (MediaPlayer is from XNA)
            _dt.Interval = TimeSpan.FromMilliseconds(1000);
            _dt.Tick += delegate { try { FrameworkDispatcher.Update(); } catch { } };
            _dt.Tick += onTimerTick;
            _dt.Start();

            loadConsents();

            checkPreventLock();

            if (App.SHUFFLEHISTORY == null)
            {
                App.SHUFFLEHISTORY = new List<int>();
            }

            bool isVisable = false;
            setTimerVisibility(isVisable);
            
            checkForLastPlaylist();

            if(App.CURRENTPLAYLISTNAME != null)
            {
                int ind = rlpPlaylists.Items.IndexOf(App.CURRENTPLAYLISTNAME);
                rlpPlaylists.SelectedIndex = ind;
            }

            checkRemoveAdsLicesnse();

            // rate app reminder
            RadRateApplicationReminder radRAReminder = new RadRateApplicationReminder();
            radRAReminder.RecurrencePerUsageCount = 3;
            radRAReminder.SkipFurtherRemindersOnYesPressed = true;
            radRAReminder.Notify();
        }

        private void checkForLastPlaylist()
        {
            if (_settings.Contains("LastSelectedPlaylist"))
            {
                string playlistname = _settings["LastSelectedPlaylist"].ToString();
                App.CURRENTPLAYLISTNAME = playlistname;
            }
            else
            {
                App.CURRENTPLAYLISTNAME = null;
            }
        }

        private void addPlaylistNameToStorage(string playlistname)
        {
            if (_settings.Contains("LastSelectedPlaylist"))
            {
                _settings["LastSelectedPlaylist"] = playlistname;
            }
            else
            {
                _settings.Add("LastSelectedPlaylist", playlistname);
            }
        }

        private void checkRemoveAdsLicesnse()
        {
            var licenseInformation = CurrentApp.LicenseInformation;

            if (licenseInformation.ProductLicenses["PL_RemoveAds"].IsActive)
            {
                App.SHOWADS = false;
                btnNoAds.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                App.SHOWADS = true;
                btnNoAds.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void setTimerVisibility(bool isVisable)
        {
            if (isVisable)
            {
                txtBlkElapsed.Visibility = System.Windows.Visibility.Visible;
                txtBlkRemaining.Visibility = System.Windows.Visibility.Visible;
                recTimer.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtBlkElapsed.Visibility = System.Windows.Visibility.Collapsed;
                txtBlkRemaining.Visibility = System.Windows.Visibility.Collapsed;
                recTimer.Visibility = System.Windows.Visibility.Collapsed;

            }
        }

        private void checkPreventLock()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("PreventLock"))
            {
                if ((bool)IsolatedStorageSettings.ApplicationSettings["PreventLock"])
                {
                    PhoneApplicationService.Current.UserIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Disabled;
                }
            }
            else
            {
                PhoneApplicationService.Current.UserIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Disabled;
                IsolatedStorageSettings.ApplicationSettings["PreventLock"] = true;
            }
        }

        private void BuildInitialAppBar()
        {
            ApplicationBar = new ApplicationBar();
            // menu buttons
            ApplicationBarIconButton prevBtn = new ApplicationBarIconButton(new Uri("/Assets/AppBar/transport.rew.png", UriKind.Relative));
            prevBtn.Text = "prev.";
            prevBtn.Click += new EventHandler(play_prev);
            ApplicationBar.Buttons.Add(prevBtn);

            ApplicationBarIconButton playBtn = new ApplicationBarIconButton(new Uri("/Assets/AppBar/transport.play.png", UriKind.Relative));
            playBtn.Text = "play";
            playBtn.Click += new EventHandler(play_song);
            ApplicationBar.Buttons.Add(playBtn);

            ApplicationBarIconButton nextBtn = new ApplicationBarIconButton(new Uri("/Assets/AppBar/transport.ff.png", UriKind.Relative));
            nextBtn.Text = "next";
            nextBtn.Click += new EventHandler(play_next);
            ApplicationBar.Buttons.Add(nextBtn);
        }
                        
        private void adjustForTheme()
        {
            Visibility dbgisibility = (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"];

            if (dbgisibility != Visibility.Visible)
            {
                rlpPlaylists.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                rlpPlaylists.Background = new SolidColorBrush(Colors.Black);
            }

        }
        
        private void onTimerTick(object sender, EventArgs e)
        {
            bool _playNext = false;
                                   
            if (MediaPlayer.State == MediaState.Paused || MediaPlayer.State == MediaState.Stopped)
            {
                if (!_paused)
                {
                    _playNext = true;
                }
            }
            else
            {
                _playNext = false;

                if (MainPiv.SelectedIndex == 0 && ApplicationBar != null)
                {
                    ApplicationBarIconButton btn = (ApplicationBarIconButton)ApplicationBar.Buttons[(int)PlayerButtons.play];
                    btn.Text = "pause";
                    btn.IconUri = new Uri("/Assets/AppBar/transport.pause.png", UriKind.Relative);
                    if (!_paused)
                    {
                        _playing = true;
                    }
                }
            }

            if (MediaPlayer.State == MediaState.Paused && _playing == true)
            {
                _playNext = true;
            }

            
            if (_playNext && _playing)
            {
                playsong();
            }

            if (_playing && _currentSong != null)
            {

                double end = _currentSong.Duration.TotalSeconds;
                TimeSpan songPosition = MediaPlayer.PlayPosition;
                TimeSpan remainingTime = TimeSpan.FromSeconds(end - MediaPlayer.PlayPosition.TotalSeconds);
                double increment = 0.0;
                
                if (end > 0)
                {
                    setTimerVisibility(true);
                    increment = (400 / end);

                    if (remainingTime.TotalHours > 0)
                    {
                        txtBlkRemaining.Text = remainingTime.ToString(@"mm\:ss");
                        txtBlkElapsed.Text = songPosition.ToString(@"mm\:ss");
                    }
                    else
                    {
                        txtBlkRemaining.Text = remainingTime.ToString(@"hh\:mm\:ss");
                        txtBlkElapsed.Text = songPosition.ToString(@"hh\:mm\:ss");
                    }

                    try
                    {
                        recTimer.Width = (remainingTime.TotalSeconds * increment);
                    }
                    catch (Exception)
                    {
                        // continue                        
                    }
                    

                }
                else
                {
                    setTimerVisibility(false);
                }
                

            }
        }

        private async void loadLibrary()
        {
            radBusyInd.Visibility = System.Windows.Visibility.Visible;
            //ApplicationBar.IsVisible = false;

            _loadLibraryTask = Task<Task>.Factory.StartNew(() => startLoadingSongs());
            await _loadLibraryTask;

            radBusyInd.Visibility = System.Windows.Visibility.Collapsed;
            //ApplicationBar.IsVisible = true;
            loadPlaylists();
        }

        private async Task startLoadingSongs()
        {
            await loadSongsWorker();
        }

        private async Task loadSongsWorker()
        {

            MediaLibrary library = new MediaLibrary();

            foreach (var song in library.Songs)
            {
                addSongToLibrary(song);
                _iCount = _iCount + 1;
            }

            library.Dispose();
            await _loadLibraryTask;

        }
        
        private void addSongToLibrary(Song song)
        {

            MPDB.LocalLibrary libItem = new MPDB.LocalLibrary();

            libItem.ArtistName = song.Artist.Name;
            libItem.SongName = song.Name;
            libItem.AlbumName = song.Album.Name;
            libItem.ItemDate = DateTime.Now;
            libItem.Genre = song.Genre.Name;
            
            myPlaylistDB.LocalLibraries.InsertOnSubmit(libItem);
            myPlaylistDB.SubmitChanges();

        }

        private void play_next(object sender, EventArgs e)
        {
            if (!_playlistAvailable)
                return;

            playsong();
        }

        private void play_prev(object sender, EventArgs e)
        {
            if (!_playlistAvailable)
                return;

            bool playPrevious = true;

            playsong(playPrevious);
        }

        private void play_song(object sender, EventArgs e)
        {
            if (!_playlistAvailable)
                return;

            ApplicationBarIconButton btn = (ApplicationBarIconButton)ApplicationBar.Buttons[1];

            if (btn.Text == "play")
            {
                btn.Text = "pause";
                btn.IconUri = new Uri("/Assets/AppBar/transport.pause.png", UriKind.Relative);
            }
            else if (btn.Text == "pause")
            {
                btn.Text = "play";
                btn.IconUri = new Uri("/Assets/AppBar/transport.play.png", UriKind.Relative);
            }


            if (btn.Text == "pause")
            {

                if (_paused)
                {
                    _paused = false;
                    MediaPlayer.Resume();
                }
                else
                {
                    playsong();
                }
                _playing = true;
                _dt.Start();
            }
            else
            {
                _dt.Stop();
                _playing = false;
                _paused = true;
                MediaPlayer.Pause();
            }
        }

        private void playsong(bool playPrevious = false)
        {

            MediaLibrary library = new MediaLibrary();
            int songPosition = 0;

            // get list count
            var listCount = (from MPDB.Playlist pl in myPlaylistDB.Playlists
                             where pl.PlaylistName == App.CURRENTPLAYLISTNAME
                             select pl).Count();

            // if the player is playing a song already, check if its in the list 
            if (MediaPlayer.State == MediaState.Playing && _currentSong == null)
            {

                string songName = MediaPlayer.Queue.ActiveSong.Name;

                int songPos = getSongCurrentPositionInList(songName, App.CURRENTPLAYLISTNAME);

                if (songPos != -1)
                {
                    _currentSong = MediaPlayer.Queue.ActiveSong;
                }

            }
            
            if (_currentSong == null)
            {
                var firstSong = (from MPDB.Playlist pl in myPlaylistDB.Playlists
                                 where pl.PlaylistName == App.CURRENTPLAYLISTNAME &&
                                 pl.SongID == 1
                                 select pl.SongName).Single();

                _currentSong = library.Songs.First(s => s.Name == firstSong);

                songPosition = 1;
                
            }
            else
            {
                int currentPosition;

                MPDB.Playlist currSong = myPlaylistDB.Playlists.First(s => s.PlaylistName == App.CURRENTPLAYLISTNAME
                                                                      && s.SongName == _currentSong.Name);

                currentPosition = currSong.SongID;

                if (listCount == currentPosition)
                {
                    currentPosition = 1;
                }
                else
                {

                    if (playPrevious && currentPosition > 1)
                    {
                        currentPosition = currentPosition - 1;
                    }
                    else
                    {
                        currentPosition = currentPosition + 1;
                    }
                }

                if (App.SHUFFLE)
                {
                    currentPosition = getShufflePosition(currentPosition,listCount);
                }
                                
                var nextSong = (from MPDB.Playlist pl in myPlaylistDB.Playlists
                                where pl.PlaylistName == App.CURRENTPLAYLISTNAME &&
                                pl.SongID == currentPosition
                                select pl.SongName).Single();

                _currentSong = library.Songs.First(s => s.Name == nextSong);
                songPosition = currentPosition - 1;
            }

            library.Dispose();

            MediaPlayer.Play(_currentSong);
            populateSongInfo(_currentSong);


            try
            {
                playListListBox.SelectedIndex = songPosition;
            }
            catch (Exception)
            {
                // continue
            }
            

        }

        private int getShufflePosition(int currentPosition, int listSize)
        {
            int position = -1;
            Random random;
            int randomNumber = -1;

            while (randomNumber == -1)
            {
                random = new Random();
                randomNumber = random.Next(1, listSize);

                if (!App.SHUFFLEHISTORY.Contains(randomNumber))
                {
                    if (App.SHUFFLEHISTORY.Count == 3)
                    {
                        App.SHUFFLEHISTORY.RemoveAt(0);
                    }
                    App.SHUFFLEHISTORY.Add(randomNumber);
                }
                else
                {
                    randomNumber = -1;
                }

            }

            position = randomNumber;
            return position;
        }

        private int getSongCurrentPositionInList(string songName, string playlistName)
        {

            int position = -1;
            
            try
            {
                var currentPosition = (from MPDB.Playlist pl in myPlaylistDB.Playlists
                                       where pl.PlaylistName == playlistName &&
                                       pl.SongName == songName
                                       select pl.SongID).Single();

                if (currentPosition != null)
                {
                    position = currentPosition;
                }
            }
            catch (Exception)
            {
                position = -1;
            }


            return position;
        }

        private void newPlaylist(object sender, EventArgs e)
        {

            if (_playing)
            {
                MessageBox.Show("Please pause before adding a new playlist.");
                return;
            }

            RadInputPrompt.Show("Playlist Name", closedHandler: (args) =>
            {
                string playListName = args.Text;

                if (playListName == "" | playListName == null)
                {
                    return;
                }

                App.NEWPLAYLISTNAME = playListName;
                App.CURRENTPLAYLISTNAME = null;

                NavigationService.Navigate(new Uri("/MusicHub.xaml", UriKind.Relative));

            });
        }

        private void goToSettings(object sender, EventArgs e)
        {
            if (_playing)
            {
                MessageBox.Show("Please pause before going to settings.");
                return;
            }
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void loadPlaylists()
        {

            string firstPL = "";
            _currentSong = null;
            
            var playlistsInDB = from MPDB.Playlist pl in myPlaylistDB.Playlists
                                select pl;
            Playlists = new ObservableCollection<MPDB.Playlist>(playlistsInDB);


            rlpPlaylists.Items.Clear();

            foreach (MPDB.Playlist pl in Playlists)
            {
                if (pl.PlaylistName != null)
                {

                    if (firstPL == "")
                    {
                        firstPL = pl.PlaylistName;
                    }

                    if (!rlpPlaylists.Items.Contains(pl.PlaylistName))
                    {
                        rlpPlaylists.Items.Add(pl.PlaylistName);
                    }
                }
            }

            if (Playlists.Count == 0)
            {
                _playing = false;
                clearSongInfo();
                MessageBox.Show("You don't have any playlists yet.  Please create one to start using this app.");
                _dt.Stop();
                newPlaylist(this, null);
            }
            else
            {
                _playlistAvailable = true;

                var selectedPL = from MPDB.Playlist pl in myPlaylistDB.Playlists
                                 where pl.PlaylistName == firstPL
                                 select pl;

                SelectedPlaylists = new ObservableCollection<MPDB.Playlist>(selectedPL);
                playListListBox.ItemsSource = SelectedPlaylists;

                MPDB.Playlist first_Song = myPlaylistDB.Playlists.First(s => s.PlaylistName == firstPL &&
                                                                            s.SongID == 1);                     
                                
                MediaLibrary library = new MediaLibrary();

                Song firstSong = library.Songs.First(s => s.Name == first_Song.SongName);

                populateSongInfo(firstSong);

                library.Dispose();

                if (App.CURRENTPLAYLISTNAME == null)
                {
                    App.CURRENTPLAYLISTNAME = firstPL;
                }

                playlistsInDB = from MPDB.Playlist pl in myPlaylistDB.Playlists
                                    select pl;
                Playlists = new ObservableCollection<MPDB.Playlist>(playlistsInDB);
                
                rlpPlaylists.Items.Clear();

                foreach (MPDB.Playlist pl in Playlists)
                {
                    if (pl.PlaylistName != null)
                    {

                        if (firstPL == "")
                        {
                            firstPL = pl.PlaylistName;
                        }

                        if (!rlpPlaylists.Items.Contains(pl.PlaylistName))
                        {
                            rlpPlaylists.Items.Add(pl.PlaylistName);
                        }
                    }
                }

                int ind = rlpPlaylists.Items.IndexOf(App.CURRENTPLAYLISTNAME);
                rlpPlaylists.SelectedIndex = ind;

            }
        }
        
        private void populateSongInfo(Song song)
        {
            Stream albumArtStream = song.Album.GetAlbumArt();

            if (albumArtStream == null)
            {
                StreamResourceInfo albumArtPlaceholder = Application.GetResourceStream(new Uri("Assets/concert1.png", UriKind.Relative));
                albumArtStream = albumArtPlaceholder.Stream;
            }

            BitmapImage albumArtImage = new BitmapImage();
            albumArtImage.SetSource(albumArtStream);

            bgImg.ImageSource = albumArtImage;

            txtBlkAlbumLabel.Text = "Album Name:";
            txtBlkArtistLabel.Text = "Artist Name:";
            txtBlkSongLabel.Text = "Song Name:";

            txtBlkAlbum.Text = song.Album.Name;
            txtBlkArtist.Text = song.Artist.Name;
            txtBlkSong.Text = song.Name;
        }

        private void clearSongInfo()
        {
            bgImg.ImageSource = null;

            txtBlkAlbum.Text = "";
            txtBlkArtist.Text = "";
            txtBlkSong.Text = "";

            txtBlkAlbumLabel.Text = "";
            txtBlkArtistLabel.Text = "";
            txtBlkSongLabel.Text = "";
        }

        private void MainPiv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ApplicationBar = new ApplicationBar();

            if (MainPiv.SelectedIndex == 2)
            {
                ApplicationBar = null;
            }

            if (MainPiv.SelectedIndex == 1)
            {

                ApplicationBarMenuItem menuItem1 = new ApplicationBarMenuItem();
                menuItem1.Text = "new playlist";
                menuItem1.Click += new EventHandler(newPlaylist);
                ApplicationBar.MenuItems.Add(menuItem1);

                ApplicationBarMenuItem menuItem2 = new ApplicationBarMenuItem();
                menuItem2.Text = "delete playlist";
                menuItem2.Click += new EventHandler(delete_playlist);
                ApplicationBar.MenuItems.Add(menuItem2);

                ApplicationBarMenuItem menuItem3 = new ApplicationBarMenuItem();
                menuItem3.Text = "rename playlist";
                menuItem3.Click += new EventHandler(rename_playlist);
                ApplicationBar.MenuItems.Add(menuItem3);


                ApplicationBarIconButton addsongbutton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/add.png", UriKind.Relative));
                addsongbutton.Text = "add songs";
                addsongbutton.Click += new EventHandler(add_song);
                ApplicationBar.Buttons.Add(addsongbutton);

                ApplicationBarIconButton upbutton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/up.png", UriKind.Relative));
                upbutton.Text = "move up";
                upbutton.Click += new EventHandler(moveSongUp);
                ApplicationBar.Buttons.Add(upbutton);

                ApplicationBarIconButton downbutton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/down.png", UriKind.Relative));
                downbutton.Text = "move down";
                downbutton.Click += new EventHandler(moveSongDown);
                ApplicationBar.Buttons.Add(downbutton);

                ApplicationBarIconButton deletebutton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/delete.png", UriKind.Relative));
                deletebutton.Text = "delete song";
                deletebutton.Click += new EventHandler(delete_song);
                ApplicationBar.Buttons.Add(deletebutton);

            }


            if (MainPiv.SelectedIndex == 0)
            {
                // menu items
                string shuffleText = "";
                if (App.SHUFFLE)
                {
                    shuffleText = "shuffle: on";
                }
                else
                {
                    shuffleText = "shuffle: off";
                }
                
                ApplicationBarMenuItem setShuffle = new ApplicationBarMenuItem();
                setShuffle.Text = shuffleText;
                setShuffle.Click += new EventHandler(set_Shuffle);
                ApplicationBar.MenuItems.Add(setShuffle);

                if(App.SHOWADS)
                {
                    ApplicationBarMenuItem mnuDonate = new ApplicationBarMenuItem();
                    mnuDonate.Text = "Remove Ads";
                    mnuDonate.Click += new EventHandler(go_donate);
                    ApplicationBar.MenuItems.Add(mnuDonate);
                }

                // menu buttons
                ApplicationBarIconButton appBarButton0 = new ApplicationBarIconButton(new Uri("/Assets/AppBar/transport.rew.png", UriKind.Relative));
                appBarButton0.Text = "prev.";
                appBarButton0.Click += new EventHandler(play_prev);
                ApplicationBar.Buttons.Add(appBarButton0);

                ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/transport.play.png", UriKind.Relative));
                appBarButton.Text = "play";
                appBarButton.Click += new EventHandler(play_song);
                ApplicationBar.Buttons.Add(appBarButton);

                ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Assets/AppBar/transport.ff.png", UriKind.Relative));
                appBarButton2.Text = "next";
                appBarButton2.Click += new EventHandler(play_next);
                ApplicationBar.Buttons.Add(appBarButton2);

            }

        }

        private void go_donate(object sender, EventArgs e)
        {

            if (_playing)
            {
                MessageBox.Show("Please pause before removing ads.");
                return;
            }

            NavigationService.Navigate(new Uri("/Donate.xaml", UriKind.Relative));
        }

        private void rename_playlist(object sender, EventArgs e)
        {
            string currentPlaylistName = rlpPlaylists.SelectedValue.ToString();

            RadInputPrompt.Show("Select new playlist Name:", closedHandler: (args) =>
            {
                string playListName = args.Text;

                if (playListName == "" | playListName == null)
                {
                    return;
                }

                List<MPDB.Playlist> pls = myPlaylistDB.Playlists.Where(l => l.PlaylistName == currentPlaylistName).ToList();
              
                foreach(var itm in pls)
                {
                    itm.PlaylistName = playListName;
                }

                myPlaylistDB.SubmitChanges();

                App.CURRENTPLAYLISTNAME = playListName;

                loadPlaylists();

                int ind = rlpPlaylists.Items.IndexOf(playListName);
                rlpPlaylists.SelectedIndex = ind;

            });

        }

        private void set_Shuffle(object sender, EventArgs e)
        {

            ApplicationBarMenuItem shuf = (ApplicationBarMenuItem)sender;

            if (shuf.Text == "shuffle: off")
            {
                shuf.Text = "shuffle: on";
                App.SHUFFLE = true;
            }
            else
            {
                shuf.Text = "shuffle: off";
                App.SHUFFLE = false;
            }
            
        }

        private void add_song(object sender, EventArgs e)
        {
            if (!_playlistAvailable)
                return;

            if (_playing)
            {
                MessageBox.Show("Please pause before adding songs.");
                return;
            }

            ApplicationBar = null;

            //NavigationService.Navigate(new Uri("/MusicHub.xaml", UriKind.Relative));
            NavigationService.Navigate(new Uri("/MusicHub.xaml", UriKind.Relative));
        }

        private void delete_song(object sender, EventArgs e)
        {
            if (!_playlistAvailable)
                return;

            MPDB.Playlist selectedSong = (MPDB.Playlist)playListListBox.SelectedValue;

            if (selectedSong == null)
            {
                MessageBox.Show("Please select a song to delete first by touching the song name.");
                return;
            }

            MPDB.Playlist songToDelete = (from pl in myPlaylistDB.Playlists
                                        where pl.SongName == selectedSong.SongName &&
                                        pl.PlaylistName == selectedSong.PlaylistName
                                        select pl).FirstOrDefault();

            myPlaylistDB.Playlists.DeleteOnSubmit(songToDelete);
            myPlaylistDB.SubmitChanges();

            reorderList(songToDelete.PlaylistName);

            reloadPlaylist(songToDelete.PlaylistName);

        }

        private void moveSongDown(object sender, EventArgs e)
        {
            if (!_playlistAvailable)
                return;

            int currentPosition;
            string playlistName;

            MPDB.Playlist selectedSong = (MPDB.Playlist)playListListBox.SelectedValue;

            if (selectedSong == null)
            {
                MessageBox.Show("Please select a song to move first by touching the song name.");
                return;
            }

            MPDB.Playlist songToMove = (from pl in myPlaylistDB.Playlists
                                        where pl.SongName == selectedSong.SongName &&
                                        pl.PlaylistName == selectedSong.PlaylistName
                                        select pl).FirstOrDefault();

            playlistName = songToMove.PlaylistName;
            currentPosition = songToMove.SongID;

            var playlistCount = from pl in myPlaylistDB.Playlists
                                where pl.PlaylistName == selectedSong.PlaylistName
                                select pl;


            if (currentPosition >= playlistCount.Count())
            {
                return;
            }

            songToMove.SongID = currentPosition + 1;

            var query = (from pl in myPlaylistDB.Playlists
                         where pl.SongID == (currentPosition + 1)
                         && pl.PlaylistName == playlistName
                         select pl).FirstOrDefault();


            query.SongID = currentPosition;

            myPlaylistDB.SubmitChanges();

            reloadPlaylist(playlistName);

            // set selected item (compensate for 0 base)
            playListListBox.SelectedIndex = currentPosition;
        }

        private void moveSongUp(object sender, EventArgs e)
        {

            if (!_playlistAvailable)
                return;
            
            int currentPosition;
            string playlistName;

            MPDB.Playlist selectedSong = (MPDB.Playlist)playListListBox.SelectedValue;

            if (selectedSong == null)
            {
                MessageBox.Show("Please select a song to move first by touching the song name.");
                return;
            }

            MPDB.Playlist songToMove = (from pl in myPlaylistDB.Playlists
                                        where pl.SongName == selectedSong.SongName &&
                                        pl.PlaylistName == selectedSong.PlaylistName
                                        select pl).FirstOrDefault();

            playlistName = songToMove.PlaylistName;
            currentPosition = songToMove.SongID;

            if (currentPosition == 1)
            {
                return;
            }

            // set the position of the song moving up
            songToMove.SongID = currentPosition - 1;

            var query = (from pl in myPlaylistDB.Playlists
                        where pl.SongID == (currentPosition - 1)
                        && pl.PlaylistName == playlistName
                        select pl).FirstOrDefault();

            // set the position of the swapped song
            query.SongID = currentPosition;   

            myPlaylistDB.SubmitChanges();

            reloadPlaylist(playlistName);

            // set selected item (compensate for 0 base)
            playListListBox.SelectedIndex = currentPosition -2 ;
        }

        private void reloadPlaylist(string playlistName)
        {

            reorderList(playlistName);

            var selectedPL = from MPDB.Playlist pl in myPlaylistDB.Playlists
                             where pl.PlaylistName == playlistName
                             orderby pl.SongID ascending
                             select pl;

            SelectedPlaylists = new ObservableCollection<MPDB.Playlist>(selectedPL);
            playListListBox.ItemsSource = SelectedPlaylists;

            MPDB.Playlist first_Song = myPlaylistDB.Playlists.First(s => s.PlaylistName == playlistName &&
                                                                            s.SongID == 1);

            MediaLibrary library = new MediaLibrary();

            Song firstSong = library.Songs.First(s => s.Name == first_Song.SongName);

            populateSongInfo(firstSong);

            library.Dispose();

            _currentSong = null;
            App.CURRENTPLAYLISTNAME = playlistName;

        }

        private void rlpPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rlpPlaylists.SelectedValue != null)
            {
                reloadPlaylist(rlpPlaylists.SelectedValue.ToString());
                addPlaylistNameToStorage(rlpPlaylists.SelectedValue.ToString());
            }
        }
        
        private void delete_playlist(object sender, EventArgs e)
        {
            if (!_playlistAvailable)
                return;

            if (_playing)
            {
                MessageBox.Show("Please pause before deleting this playlist.");
                return;
            }


            string playlistName = App.CURRENTPLAYLISTNAME;

            if (MessageBox.Show("Are you sure you want to delete this playlist?", "Delete Playlist?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var playlistsInDB = from MPDB.Playlist pl in myPlaylistDB.Playlists
                                    where pl.PlaylistName == playlistName
                                    select pl;

                foreach (MPDB.Playlist itm in playlistsInDB)
                {
                    myPlaylistDB.Playlists.DeleteOnSubmit((MPDB.Playlist)itm);
                }

                myPlaylistDB.SubmitChanges();

                App.CURRENTPLAYLISTNAME = "";
                App.NEWPLAYLISTNAME = "";

                playListListBox.ItemsSource = null;
                loadPlaylists();

            }
            else
            {
                return;
            }
        }

        private void reorderList(string playlistName)
        {
            int loopCount = 1;

            var currentPlaylist = from MPDB.Playlist pl in myPlaylistDB.Playlists
                                    where pl.PlaylistName == playlistName
                                    orderby pl.SongID ascending
                                    select pl;

            foreach (MPDB.Playlist pli in currentPlaylist)
            {
                pli.SongID = loopCount;
                loopCount += 1;
            }

            myPlaylistDB.SubmitChanges();

        }
        
        private void btnEmailDeveloper_Click(object sender, RoutedEventArgs e)
        {
            string emailacct = "support@alminasoftware.com";

            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Message From Playlists! App";
            emailComposeTask.Body = "insert your message here";
            emailComposeTask.To = emailacct;

            emailComposeTask.Show();
        }

        private void btnRate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rateTask = new MarketplaceReviewTask();
                rateTask.Show();
            }
            catch
            {
            }
        }

        private void chkScreenLock_Checked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.UserIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Disabled;
            IsolatedStorageSettings.ApplicationSettings["PreventLock"] = true;
        }

        private void chkScreenLock_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You must restart for this to take efect.");
            PhoneApplicationService.Current.UserIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Enabled;
            IsolatedStorageSettings.ApplicationSettings["PreventLock"] = false;
        }

        private void chkRunLock_Checked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.ApplicationIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Disabled;
            IsolatedStorageSettings.ApplicationSettings["RunLocked"] = true;
        }

        private void chkRunLock_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You must restart for this to take efect.");
            IsolatedStorageSettings.ApplicationSettings["RunLocked"] = false;
            //PhoneApplicationService.Current.ApplicationIdleDetectionMode = Microsoft.Phone.Shell.IdleDetectionMode.Enabled;
        }

        private void loadConsents()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("RunLocked"))
            {
                chkRunLock.IsChecked = (bool)IsolatedStorageSettings.ApplicationSettings["RunLocked"];
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("PreventLock"))
            {
                chkScreenLock.IsChecked = (bool)IsolatedStorageSettings.ApplicationSettings["PreventLock"];
            }

        }

        private void btnNoAds_Click(object sender, RoutedEventArgs e)
        {

            if (_playing)
            {
                MessageBox.Show("Please pause before removing ads.");
                return;
            }

            NavigationService.Navigate(new Uri("/Donate.xaml", UriKind.Relative));
            //MarketplaceDetailTask mdt = new MarketplaceDetailTask();
            //mdt.ContentIdentifier = "ae03045f-bdbd-4767-86ac-a8f906df586d";
            //mdt.Show();
        }


        private void ads3_Loaded(object sender, RoutedEventArgs e)
        {
            if(App.SHOWADS)
            {
                ads3.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ads3.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ads2_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.SHOWADS)
            {
                ads2.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ads2.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ads1_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.SHOWADS)
            {
                ads1.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ads1.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void playListListBox_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }




    }
}