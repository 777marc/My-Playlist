using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using My_Playlist.Model;
using System.Collections.ObjectModel;
using My_Playlist;

namespace My_Playlist
{
    public class AlbumItemViewModel
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

        private List<MPDB.LocalLibrary> _albumItems;

        public AlbumItemViewModel()
        {

            myPlaylistDB = new MPDB.MyPlaylistDataContext(MPDB.MyPlaylistDataContext.DBConnectionString);
            
            this.AlbumItems = new List<MPDB.LocalLibrary>();

            this.LoadData();
        }

        /// <summary>
        /// A collection for Person objects.
        /// </summary>
        public List<MPDB.LocalLibrary> AlbumItems
        {
            get
            {
                return _albumItems;
            }
            private set
            {
                _albumItems = value;
                NotifyPropertyChanged();                
            }
        }

        /// <summary>
        /// A collection for Person objects grouped by their first character.
        /// </summary>
        public List<AlphaKeyGroup<MPDB.LocalLibrary>> GroupedPeople
        {
            get
            {
                return AlphaKeyGroup<MPDB.LocalLibrary>.CreateGroups(
                    AlbumItems,
                    (MPDB.LocalLibrary s) => { return s.AlbumName; },
                    true);
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few Person objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            var rawList= myPlaylistDB.LocalLibraries.Where(l => l.LibraryItemId > 0).ToList<MPDB.LocalLibrary>();


            List<string> albumnames = new List<string>();

            foreach (MPDB.LocalLibrary itm in rawList)
            {
                if (!albumnames.Contains(itm.AlbumName))
                {
                    albumnames.Add(itm.AlbumName);
                    AlbumItems.Add(itm);
                }
            }

            albumnames = null;

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
