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
    public class GenreItemViewModel
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

        private List<MPDB.LocalLibrary> _genreItems;

        public GenreItemViewModel()
        {

            myPlaylistDB = new MPDB.MyPlaylistDataContext(MPDB.MyPlaylistDataContext.DBConnectionString);
            
            this.GenreItems = new List<MPDB.LocalLibrary>();

            this.LoadData();
        }

        /// <summary>
        /// A collection for Person objects.
        /// </summary>
        public List<MPDB.LocalLibrary> GenreItems
        {
            get
            {
                return _genreItems;
            }
            private set
            {
                _genreItems = value;
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
                    GenreItems,
                    (MPDB.LocalLibrary s) => { return s.Genre; },
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

            List<MPDB.LocalLibrary> rawList = myPlaylistDB.LocalLibraries.Where(l => l.LibraryItemId > 0).ToList<MPDB.LocalLibrary>();
            List<string> genrenames = new List<string>();

            foreach (MPDB.LocalLibrary itm in rawList)
            {
                if (!genrenames.Contains(itm.Genre))
                {
                    genrenames.Add(itm.Genre);
                    GenreItems.Add(itm);
                }
            }

            genrenames = null;
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
