using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using My_Playlist.Model;
using System.Collections.ObjectModel;

namespace My_Playlist
{
    public class SongItemViewModel : INotifyPropertyChanged
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

        private List<MPDB.LocalLibrary> _songitems;

        public SongItemViewModel()
        {

            myPlaylistDB = new MPDB.MyPlaylistDataContext(MPDB.MyPlaylistDataContext.DBConnectionString);
            
            this.SongItems = new List<MPDB.LocalLibrary>();

            this.LoadData();
        }

        /// <summary>
        /// A collection for Person objects.
        /// </summary>
        public List<MPDB.LocalLibrary> SongItems
        {
            get
            {
                return _songitems;
            }
            private set
            {
                _songitems = value;
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
                    SongItems,
                    (MPDB.LocalLibrary s) => { return s.SongName; },
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
            this.SongItems = myPlaylistDB.LocalLibraries.Where(l => l.LibraryItemId > 0).ToList<MPDB.LocalLibrary>();
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