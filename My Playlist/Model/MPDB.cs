using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_Playlist.Model
{
    public class MPDB
    {
        public class MyPlaylistDataContext : DataContext
        {
            // Specify the connection string as a static, used in main page and app.xaml.
            public static string DBConnectionString = "Data Source=isostore:/MyPlaylist.sdf";

            // Pass the connection string to the base class.
            public MyPlaylistDataContext(string connectionString)
                : base(connectionString)
            { }

            // Specify a single table 
            public Table<Playlist> Playlists;
            public Table<LocalLibrary> LocalLibraries;

        }

        [Table]
        public class Playlist : INotifyPropertyChanged, INotifyPropertyChanging
        {
            // Define ID: private field, public property and database column.
            private int _playlistItemId;
            [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
            public int PlaylistItemId
            {
                get
                {
                    return _playlistItemId;
                }
                set
                {
                    if (_playlistItemId != value)
                    {
                        NotifyPropertyChanging("PlaylistItemId");
                        _playlistItemId = value;
                        NotifyPropertyChanged("PlaylistItemId");
                    }
                }
            }

            private DateTime _itemDate;
            [Column]
            public DateTime ItemDate
            {
                get
                {
                    return _itemDate;
                }
                set
                {
                    if (_itemDate != value)
                    {
                        NotifyPropertyChanging("ItemDate");
                        _itemDate = value;
                        NotifyPropertyChanged("ItemDate");
                    }
                }
            }

            private int _songID;
            [Column]
            public int SongID
            {
                get
                {
                    return _songID;
                }
                set
                {
                    if (_songID != value)
                    {
                        NotifyPropertyChanging("SongID");
                        _songID = value;
                        NotifyPropertyChanged("SongID");
                    }
                }
            }

            private string _songName;
            [Column]
            public string SongName
            {
                get
                {
                    return _songName;
                }
                set
                {
                    if (_songName != value)
                    {
                        NotifyPropertyChanging("SongName");
                        _songName = value;
                        NotifyPropertyChanged("SongName");
                    }
                }
            }

            private string _playlistName;
            [Column]
            public string PlaylistName
            {
                get
                {
                    return _playlistName;
                }
                set
                {
                    if (_playlistName != value)
                    {
                        NotifyPropertyChanging("PlaylistName");
                        _playlistName = value;
                        NotifyPropertyChanged("PlaylistName");
                    }
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            // Used to notify the page that a data context property changed
            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion

            #region INotifyPropertyChanging Members

            public event PropertyChangingEventHandler PropertyChanging;

            // Used to notify the data context that a data context property is about to change
            private void NotifyPropertyChanging(string propertyName)
            {
                if (PropertyChanging != null)
                {
                    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
                }
            }

            #endregion

        }

        [Table]
        public class LocalLibrary : INotifyPropertyChanged, INotifyPropertyChanging
        {
            private int _libraryItemId;
            [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
            public int LibraryItemId
            {
                get
                {
                    return _libraryItemId;
                }
                set
                {
                    if (_libraryItemId != value)
                    {
                        NotifyPropertyChanging("LibraryItemId");
                        _libraryItemId = value;
                        NotifyPropertyChanged("LibraryItemId");
                    }
                }
            }

            private DateTime _itemDate;
            [Column]
            public DateTime ItemDate
            {
                get
                {
                    return _itemDate;
                }
                set
                {
                    if (_itemDate != value)
                    {
                        NotifyPropertyChanging("ItemDate");
                        _itemDate = value;
                        NotifyPropertyChanged("ItemDate");
                    }
                }
            }

            private int _songID;
            [Column]
            public int SongID
            {
                get
                {
                    return _songID;
                }
                set
                {
                    if (_songID != value)
                    {
                        NotifyPropertyChanging("SongID");
                        _songID = value;
                        NotifyPropertyChanged("SongID");
                    }
                }
            }

            private string _songName;
            [Column]
            public string SongName
            {
                get
                {
                    return _songName;
                }
                set
                {
                    if (_songName != value)
                    {
                        NotifyPropertyChanging("SongName");
                        _songName = value;
                        NotifyPropertyChanged("SongName");
                    }
                }
            }

            private string _albumName;
            [Column]
            public string AlbumName
            {
                get
                {
                    return _albumName;
                }
                set
                {
                    if (_albumName != value)
                    {
                        NotifyPropertyChanging("AlbumName");
                        _albumName = value;
                        NotifyPropertyChanged("AlbumName");
                    }
                }
            }

            private string _artistName;
            [Column]
            public string ArtistName
            {
                get
                {
                    return _artistName;
                }
                set
                {
                    if (_artistName != value)
                    {
                        NotifyPropertyChanging("ArtistName");
                        _artistName = value;
                        NotifyPropertyChanged("ArtistName");
                    }
                }
            }

            private string _genre;
            [Column]
            public string Genre
            {
                get
                {
                    return _genre;
                }
                set
                {
                    if (_genre != value)
                    {
                        NotifyPropertyChanging("Genre");
                        _genre = value;
                        NotifyPropertyChanged("Genre");
                    }
                }
            }
            
            private string _artwork;
            [Column]
            public string Artwork
            {
                get
                {
                    return _artwork;
                }
                set
                {
                    if (_artwork != value)
                    {
                        NotifyPropertyChanging("Artwork");
                        _artwork = value;
                        NotifyPropertyChanged("Artwork");
                    }
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            // Used to notify the page that a data context property changed
            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion

            #region INotifyPropertyChanging Members

            public event PropertyChangingEventHandler PropertyChanging;

            // Used to notify the data context that a data context property is about to change
            private void NotifyPropertyChanging(string propertyName)
            {
                if (PropertyChanging != null)
                {
                    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
                }
            }

            #endregion

        }

    }
}
