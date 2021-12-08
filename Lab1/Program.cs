using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Lab1
{
    class QueryEventArgs : EventArgs
    {
        public DateTime queryTime { get; }
        public string query { get; }
        public string stateTitle { get; }
        public QueryEventArgs(DateTime queryTime, string query, string stateTitle)
        {
            this.queryTime = queryTime;
            this.query = query;
            this.stateTitle = stateTitle;
        }
    }


    // ----------------- APP BLOCK -----------------
    class Context<T> where T : MusContent
    {
        public State<T> current_state { get; private set; }
        public GenericLibrary<T> Context_library { get; set; }

        public Context(State<T> current_state)
        {
            this.transitionToState(current_state);
        }

        public void transitionToState(State<T> state_to_set)
        {
            current_state = state_to_set;
            current_state.Context = this;
        }

        private void draw()
        {
            this.current_state.draw();
            Console.WriteLine("type EXIT to exit");
        }

        private void handle(string user_input)
        {
            this.current_state.handle(user_input);
        }

        public void run()
        {
            while (true)
            {
                this.draw();
                string user_input = Console.ReadLine();
                if (user_input == "EXIT") break;
                this.handle(user_input);
            }
        }
    }

    abstract class State<T> where T : MusContent
    {
        public event EventHandler<QueryEventArgs> raiseQueryEvent;
        protected string title;
        protected const string title_frame = "======"; // string to be drawn from both sides of the title
        protected string state_description;
        protected List<State<T>> children = new List<State<T>>(); // states that are accessible from this state 
        public State<T> Parent { get; set; } = null; // state that can lead to this state
        public Context<T> Context { get; set; }

        public void become_state_parent(State<T> child)
        {
            this.children.Add(child);
            child.Parent = this;
        }

        public bool handle_navigation(string user_input)
        {
            foreach (State<T> child in this.children)
            {
                if (user_input == child.title)
                {
                    Context.transitionToState(child);
                    return true;
                }
            }

            if (user_input == "BACK" && this.Parent != null)
            {
                this.Context.transitionToState(this.Parent);
                return true;
            }
            return false;
        }

        public void draw_title_string()
        {
            Console.WriteLine(State<T>.title_frame + this.title + State<T>.title_frame);
        }

        public void draw_navigation_guide()
        {
            this.draw_title_string();
            foreach (State<T> child in this.children)
            {
                Console.WriteLine("type " + child.title + " to " + child.state_description);
            }
            if (this.Parent != null)
            {
                Console.WriteLine("type BACK to return to the previous state");
            }
        }

        public abstract void handle(string user_input);
        public abstract void draw();
        protected virtual void OnRaiseCustomEvent(QueryEventArgs e)
        {
            EventHandler<QueryEventArgs> raiseEvent = this.raiseQueryEvent;
            raiseEvent?.Invoke(this, e);
        }
    }

    class NavigationState<T> : State<T> where T : MusContent
    {
        public NavigationState(string title, string description)
        {
            this.title = title;
            this.state_description = description;
        }

        public override void handle(string user_input)
        {
            this.handle_navigation(user_input);
        }

        public override void draw()
        {
            this.draw_navigation_guide();
        }
    }

    class SearchByNameState<T> : State<T> where T : MusContent
    {
        public SearchByNameState(string title, string description)
        {
            this.title = title;
            this.state_description = description;
        }

        public override void handle(string user_input)
        {            
            bool user_navigated = this.handle_navigation(user_input);
            if (user_navigated) return;
            OnRaiseCustomEvent(new QueryEventArgs(DateTime.Now, user_input, this.title));
            if (this.Context.current_state != this)
            {
                return;
            }
            List<T> search_result = this.Context.Context_library.search_by_name(user_input);
            Console.WriteLine("\n");            
            Console.WriteLine("Found " + search_result.Count);
            foreach (T found_content in search_result)
            {
                (found_content as MusContent).show_info();
                Console.WriteLine("----");
                Console.WriteLine("\n");
            }
        }

        public override void draw()
        {
            this.draw_navigation_guide();
            Console.WriteLine("Type the name to search for");
        }        
    }

    class SearchByYearState<T> : State<T> where T : MusContent
    {
        public SearchByYearState(string title, string description)
        {
            this.title = title;
            this.state_description = description;
        }

        public override void handle(string user_input)
        {
            bool user_navigated = this.handle_navigation(user_input);
            if (user_navigated) return;
            OnRaiseCustomEvent(new QueryEventArgs(DateTime.Now, user_input, this.title));
            if (this.Context.current_state != this)
            {
                return;
            }
            List<T> search_result = this.Context.Context_library.search_by_year(user_input);
            Console.WriteLine("\n");
            Console.WriteLine("Found " + search_result.Count);
            foreach (MusContent found_content in search_result)
            {
                found_content.show_info();
                Console.WriteLine("----");
                Console.WriteLine("\n");
            }
        }

        public override void draw()
        {
            this.draw_navigation_guide();
            Console.WriteLine("Type the START and the FINISH of year span to search in");
        }
    }

    class SearchByGenreState<T> : State<T> where T : MusContent
    {
        public SearchByGenreState(string title, string description)
        {
            this.title = title;
            this.state_description = description;
        }

        public override void handle(string user_input)
        {
            bool user_navigated = this.handle_navigation(user_input);
            if (user_navigated) return;
            OnRaiseCustomEvent(new QueryEventArgs(DateTime.Now, user_input, this.title));
            if (this.Context.current_state != this)
            {
                return;
            }
            List<T> search_result = this.Context.Context_library.search_by_genre(user_input);
            Console.WriteLine("\n");
            Console.WriteLine("Found " + search_result.Count);
            foreach (T found_content in search_result)
            {
                found_content.show_info();
                Console.WriteLine("----");
                Console.WriteLine("\n");
            }
        }

        public override void draw()
        {
            this.draw_navigation_guide();
            Console.WriteLine("Type the GENRE NAME to search for");
        }
    }
    // ----------------- APP BLOCK -----------------


    // ----------------- MUSIC BLOCK ----------------- 
    abstract class MusContent : IEquatable<MusContent>
    {
        public string Name { get; protected set; }
        public DateTime Release_date { get; protected set; }
        public string Genre { get; protected set; }
        public Performer Performer { get; set; }
        public abstract void show_info();

        public bool Equals(MusContent other)
        {
            Console.WriteLine("INSIDE MUS CONTENT CHECK");
            return (this.Name == other.Name &&
                this.Release_date == other.Release_date &&
                this.Genre == other.Genre &&
                this.Name == other.Name &&
                this.Performer == other.Performer);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class Album : MusContent
    {
        private List<Song> songs;
        private const string song_enumeration_indent = "    ";

        public Album(string name, string album_genre, DateTime release_date)
        {
            this.Name = name;
            this.songs = new List<Song>();
            this.Genre = album_genre;
            this.Release_date = release_date;
        }

        public void add_song_to_album(Song song)
        {
            this.songs.Add(song);
            song.Song_album = this;
        }
        public override void show_info()
        {
            Console.WriteLine("Album: " + this.Name);
            Console.WriteLine("Release year: " + this.Release_date.Year);
            Console.WriteLine("Genre: " + this.Genre);
            if (this.Performer == null)
            {
                Console.WriteLine("No information about the performer yet");
            }
            else
            {
                Console.WriteLine("Performed by: " + this.Performer.Name);
            }
            if (this.songs.Count > 0)
            {
                Console.WriteLine("Songs list:");
                foreach (Song song in songs)
                {
                    Console.WriteLine(Album.song_enumeration_indent + song.Name);
                }
            }
            else
            {
                Console.WriteLine("No information about the songs yet");
            }
        }
    }

    class Performer : IEquatable<Performer>
    {
        public string Name {get; private set;}
        public DateTime Formation_date { get; private set; }
        public string Performer_genre { get; private set; }
        private List<Album> albums;
        private List<Song> songs;

        public Performer(string name, string genre, DateTime formation_date)
        {
            this.Name = name;
            this.Formation_date = formation_date;
            this.Performer_genre = genre;
            this.albums = new List<Album>();
            this.songs = new List<Song>();
        }

        public void register_song(Song song)
        {
            this.songs.Add(song);
            song.Performer = this;
        }

        public void register_album(Album album)
        {
            this.albums.Add(album);
            album.Performer = this;
        }

        // Equatable contract 
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(Performer other)
        {
            Console.WriteLine("INSIDE Performer EQUALITY CHECK");
            bool albums_equality = this.albums.Count == other.albums.Count;
            if (albums_equality)
            {
                for (int i = 0; i < this.albums.Count; i++)
                {
                    albums_equality = albums_equality && this.albums[i] == other.albums[i];
                    if (!albums_equality) break;
                }
            }
            return (this.Name == other.Name &&
                this.Formation_date == other.Formation_date &&
                albums_equality);
        }

    }

    class Song : MusContent
    {
        public Album Song_album { get; set; }

        public Song(string name, string mus_genre, DateTime release_date)
        {
            this.Name = name;
            this.Genre = mus_genre;
            this.Release_date = release_date;
        }

        public override void show_info()
        {
            Console.WriteLine("Song: " + this.Name);
            Console.WriteLine("Genre: " + this.Genre);
            Console.WriteLine("Release year: " + this.Release_date.Year);
            if (this.Performer != null)
            {
                Console.WriteLine("Performed by: " + this.Performer.Name);
            }
            if (this.Song_album != null)
            {
                Console.WriteLine("Album: " + this.Song_album.Name);
            }

        }
    }

    class MusicEnumerator<T> : IEnumerator<T> where T : MusContent
    {
        private GenericLibrary<T> musCollection;
        private int curIndex;
        private T curContentPiece;

        public MusicEnumerator(GenericLibrary<T> library)
        {
            this.musCollection = library;
            this.curIndex = -1;
            this.curContentPiece = default(T);
        }

        public bool MoveNext()
        {
            if (++this.curIndex >= this.musCollection.Count)
                return false;
            else
                this.curContentPiece = this.musCollection[this.curIndex];
            return true;
        }

        public void Reset()
        {
            this.curIndex = -1;
        }

        void IDisposable.Dispose() { }

        public T Current
        {
            get { return this.curContentPiece; }
        }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }
    }

    // todo make generic library publish events
    class GenericLibrary<T> : ICollection<T> where T : MusContent
    {        
        private List<T> innerCollection = new List<T>();

        public GenericLibrary()
        {
            this.innerCollection = new List<T>();
        }

        public List<MusContent> get_news()
        {
            List<MusContent> res = new List<MusContent>();
            return res;
        }

        public List<T> search_by_name(string query_name)
        {
            query_name = query_name.ToLower();
            List<T> found_content = new List<T>();
            foreach (T contentPiece in this.innerCollection)
            {
                if (contentPiece.Name.ToLower() == query_name)
                {
                    found_content.Add(contentPiece);
                }
            }
            return found_content;
        }

        public List<T> search_by_genre(string query_name)
        {
            query_name = query_name.ToLower();
            List<T> found_content = new List<T>();
            foreach (T thing in this.innerCollection)
            {
                if (thing.Genre.ToLower() == query_name)
                    found_content.Add(thing);
            }
            return found_content;
        }

        public List<T> search_by_year(string query_name)
        {
            List<T> found_content = new List<T>();
            int interval_start = 0;
            int interval_finish = 0;
            try
            {
                string[] interval = query_name.Split(' ');
                if (interval.Length != 2)
                {
                    Console.WriteLine("Invalid interval");
                    return found_content;
                }
                interval_start = int.Parse(interval[0]);
                interval_finish = int.Parse(interval[1]);
                if (interval_start > interval_finish)
                {
                    Console.WriteLine("Interval length should be more than 0");
                    return found_content;
                }
            }
            catch
            {
                Console.WriteLine("Invalid interval");
                return found_content;
            }
            foreach (T contentPiece in this.innerCollection)
            {
                int year = contentPiece.Release_date.Year;
                if (year >= interval_start && year <= interval_finish)
                {
                    found_content.Add(contentPiece);
                }
            }
            return found_content;
        }

        // Collection interface implementation
        public IEnumerator<T> GetEnumerator()
        {
            return new MusicEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MusicEnumerator<T>(this);
        }        

        public T this[int index]
        {
            get { return (T)this.innerCollection[index]; }
            set { this.innerCollection[index] = value; }
        }

        public bool Contains(T item)
        {
            Console.WriteLine("Start search in constains method");
            bool found = false;
            foreach (T contentPiece in this.innerCollection)
            {
                contentPiece.show_info(); // fixme remove
                if (item.Equals(contentPiece)) found = true;
            }
            return found;
        }

        public bool Contains(T item, EqualityComparer<T> comparator)
        {
            bool found = false;
            foreach (T contentPiece in this.innerCollection)
            {
                if (comparator.Equals(item, contentPiece)) found = true;
            }
            return found;
        }

        public void Add(T value)
        {
            this.innerCollection.Add(value);
        }

        public void Clear()
        {
            this.innerCollection.Clear();
        }

        public void CopyTo(T[] targetArray, int targetIndex)
        {
            if (targetArray == null)
                throw new ArgumentNullException("target array is null");
            if (targetIndex < 0)
                throw new ArgumentOutOfRangeException("target index valu");
            if (Count > targetArray.Length - targetIndex + 1)
                throw new ArgumentException("not enough space for elements to be copied");
            for (int i = 0; i < innerCollection.Count; i++)
                targetArray[i + targetIndex] = this.innerCollection[i];
        }

        public int Count
        {
            get
            {
                return innerCollection.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            bool result = false;
            for (int i = 0; i < innerCollection.Count; i++)
            {
                T curContent = (T)this.innerCollection[i];
                if (curContent.Equals(item))
                {
                    this.innerCollection.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void sort(IComparer<T> comparer)
        {
            this.innerCollection.Sort(comparer);
        }
    }
    // ----------------- MUSIC BLOCK ----------------- 

    // ----------------- LAB4 BLOCK ----------------- 
    enum LoggerMode
    {
        Console,
        TextFile
    }

    class Logger<T> where T : MusContent
    {
        private LoggerMode mode; 
        private string logs_path = "logs.txt";
        public Logger(LoggerMode mode)
        {
            this.mode = mode;
        }
        
        public void SubscribeOnState(State<T> publisher)
        {
            publisher.raiseQueryEvent += LogEvent;
        }

        private string FormStringFromEventArgs(QueryEventArgs args)
        {
            string eventDescription = "";
            eventDescription += ("QueryTime = " + args.queryTime.ToString());
            eventDescription += "; ";
            eventDescription += ("AppMode = " + args.stateTitle);
            eventDescription += "; ";
            eventDescription += ("Query = " + args.query);
            return eventDescription;
        }


        private void LogEvent(object sender, QueryEventArgs args)
        {
            string eventDescription = this.FormStringFromEventArgs(args);
            if (this.mode == LoggerMode.Console)
                Console.WriteLine(eventDescription);
            else
            {
                if (!File.Exists(this.logs_path))
                {
                    using (StreamWriter sw = File.CreateText(logs_path))
                        sw.WriteLine(eventDescription);                        
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(logs_path))
                        sw.WriteLine(eventDescription);
                }                
            }
        }

    }
    // ----------------- LAB4 BLOCK ----------------- 

    // ----------------- LAB3 BLOCK ----------------- 
    class MusicalContentComparer<T> : IComparer<T> where T : MusContent
    {
        private Func<T, T, int> sort_method;
        public MusicalContentComparer(Func<T, T, int> user_sort_logic)
        {
            this.sort_method = user_sort_logic; 
        }

        public int Compare(T first, T second)
        {
            int sort_result = this.sort_method(first, second);
            return sort_result;
        }
    }
    // ----------------- LAB3 BLOCK ----------------- 


    class Program
    {        
        static GenericLibrary<MusContent> build_generics_library()
        {
            GenericLibrary<MusContent> lib = new GenericLibrary<MusContent>();

            // generate content
            string heavy_metal = "heavy metal";
            Performer iron_maiden = new Performer("Iron Maiden", heavy_metal, new DateTime(1975, 1, 1));

            // ============= SEVENTH OF THE SEVENTH SON ALBUM =============
            DateTime seventh_son_release = new DateTime(1988, 1, 1);
            Song moonchild = new Song("Moonchild", heavy_metal, seventh_son_release);
            Song infinite_drems = new Song("Infinite Dreams", heavy_metal, seventh_son_release);
            Song can_i_play_with_madness = new Song("Can I Play with Madness", heavy_metal, seventh_son_release);
            Song the_evil_that_men_do = new Song("The Evil That Men Do", heavy_metal, seventh_son_release);
            Song seventh_son_of_a_seventh_son = new Song("Seventh Son of a Seventh Son", heavy_metal, seventh_son_release);
            Song the_prophecy = new Song("The Prophecy", heavy_metal, seventh_son_release);
            Song the_clairvoyant = new Song("The Clairvoyant", heavy_metal, seventh_son_release);
            Song only_the_good_die_young = new Song("Only the Good Die Young", heavy_metal, seventh_son_release);
            List<Song> seventh_son_songs = new List<Song> { moonchild, infinite_drems, can_i_play_with_madness, the_evil_that_men_do,
            seventh_son_of_a_seventh_son, the_prophecy, the_clairvoyant, only_the_good_die_young};

            Album seventh_son_album = new Album("Seventh Son of a Seventh Son", heavy_metal, seventh_son_release);
            foreach (Song seventh_son_song in seventh_son_songs)
            {
                seventh_son_album.add_song_to_album(seventh_son_song);
                iron_maiden.register_song(seventh_son_song);
                lib.Add(seventh_son_song);
            }
            iron_maiden.register_album(seventh_son_album);
            lib.Add(seventh_son_album);


            // ============= PIECE OF MIND ALBUM =============
            DateTime piece_of_mind_release = new DateTime(1983, 1, 1);
            Song where_eagles_dare = new Song("Where Eagles Date", heavy_metal, piece_of_mind_release);
            Song revelations = new Song("Revelations", heavy_metal, piece_of_mind_release);
            Song flight_of_icarus = new Song("Flight of Icarus", heavy_metal, piece_of_mind_release);
            Song die_with_your_boots_on = new Song("Die with Your Boots On", heavy_metal, piece_of_mind_release);
            Song the_trooper = new Song("The Trooper", heavy_metal, piece_of_mind_release);
            Song still_life = new Song("Still Life", heavy_metal, piece_of_mind_release);
            Song quest_for_fire = new Song("Quest fir Fire", heavy_metal, piece_of_mind_release);
            Song sun_and_steel = new Song("Sun and Steel", heavy_metal, piece_of_mind_release);
            Song to_tame_a_land = new Song("To Tame A Land", heavy_metal, piece_of_mind_release);
            List<Song> piece_of_mind_songs = new List<Song> { where_eagles_dare, revelations, flight_of_icarus, die_with_your_boots_on,
            the_trooper, still_life, quest_for_fire, sun_and_steel, to_tame_a_land};

            Album piece_of_mind_album = new Album("Piece of Mind", heavy_metal, piece_of_mind_release);
            foreach (Song piece_of_mind_song in piece_of_mind_songs)
            {
                piece_of_mind_album.add_song_to_album(piece_of_mind_song);
                iron_maiden.register_song(piece_of_mind_song);
                lib.Add(piece_of_mind_song);
            }
            iron_maiden.register_album(piece_of_mind_album);
            lib.Add(piece_of_mind_album);

            // ============= THE STARWHEEL ALBUM =============
            string dark_ambient = "dark ambient";
            Performer kammarheit = new Performer("Kammarheit", dark_ambient, new DateTime(2000, 1, 1));
            DateTime the_starwheel_release = new DateTime(2005, 1, 1);
            Song hypnagoga = new Song("Hypnagoga", dark_ambient, the_starwheel_release);
            Song spatium = new Song("Spatium", dark_ambient, the_starwheel_release);
            Song starwheel_clockwise = new Song("The Starwheel(Clockwise)", dark_ambient, the_starwheel_release);
            Song klockstapeln = new Song("Klockstapeln", dark_ambient, the_starwheel_release);
            Song starwheel_counterclockwise = new Song("The Starwheel(Counter Clockwise)", dark_ambient, the_starwheel_release);
            Song room_between_the_rooms = new Song("A Room Between the Rooms", dark_ambient, the_starwheel_release);
            Song sleep_after_toyle = new Song("Sleep After Toyle, Port After Stormie Seas", dark_ambient, the_starwheel_release);
            Song all_quiet_frozen_scenes = new Song("All Quiet in the Land of Frozen Scenes", dark_ambient, the_starwheel_release);

            Album the_starwheel_album = new Album("The Starwheel", dark_ambient, the_starwheel_release);
            List<Song> starwheel_songs = new List<Song> { hypnagoga, spatium, starwheel_clockwise, klockstapeln, starwheel_counterclockwise,
            room_between_the_rooms, sleep_after_toyle, all_quiet_frozen_scenes };

            foreach (Song starwheel_song in starwheel_songs)
            {
                the_starwheel_album.add_song_to_album(starwheel_song);
                kammarheit.register_song(starwheel_song);
                lib.Add(starwheel_song);
            }
            kammarheit.register_album(the_starwheel_album);
            lib.Add(the_starwheel_album);
            return lib;
        }

        static void covar_contravar_examples()
        {
            // covariance example 
            Performer iron_maiden = new Performer("Iron Maiden", "heavy_metal", new DateTime(1975, 1, 1));
            GenericLibrary<Song> song_lib = new GenericLibrary<Song>();            
            DateTime seventh_son_release = new DateTime(1988, 1, 1);
            Song s1 = new Song("Moonchild", "heavy_metal", seventh_son_release);
            iron_maiden.register_song(s1);
            song_lib.Add(s1);

            IEnumerable<Song> song_enumerable = (IEnumerable<Song>)song_lib;
            IEnumerable<MusContent> musical_enumerable = song_enumerable;
            IEnumerator<Song> song_enum = song_enumerable.GetEnumerator();
            IEnumerator<MusContent> mus_enum = musical_enumerable.GetEnumerator();
            song_enum.MoveNext();
            mus_enum.MoveNext();
            Console.WriteLine(song_enum.Current.Name);
            Console.WriteLine(mus_enum.Current.Name);
        }


        static int sort_by_name(MusContent first, MusContent second)
        {
            return first.Name.CompareTo(second.Name);
    
        }

        static int sort_by_genre(MusContent first, MusContent second)
        {
            return first.Genre.CompareTo(second.Genre);
        }


        static void sort_example()
        {
            GenericLibrary<MusContent> generic_lib = build_generics_library();
            generic_lib.sort(new MusicalContentComparer<MusContent>(sort_by_name));
            foreach (MusContent gc in generic_lib) Console.WriteLine(gc.Name);
        }

        static void Main(string[] args)
        {
            sort_example();
            //covar_contravar_examples();
            GenericLibrary<MusContent> generic_lib = build_generics_library();
            State<MusContent> mainMenu = new NavigationState<MusContent>("MainMenu", "MainMenu");
            NavigationState<MusContent> news = new NavigationState<MusContent>("NEWS", "see the latest news");
            NavigationState<MusContent> search = new NavigationState<MusContent>("SEARCH", "search for specific things");
            SearchByNameState<MusContent> search_by_name = new SearchByNameState<MusContent>("SEARCH BY NAME", "search by name");
            SearchByYearState<MusContent> search_by_year = new SearchByYearState<MusContent>("SEARCH BY YEAR", "search by year");
            SearchByGenreState<MusContent> search_by_genre = new SearchByGenreState<MusContent>("SEARCH BY GENRE", "search by genre");

            // Set up event logging
            //Logger<MusContent> logger = new Logger<MusContent>(LoggerMode.Console);
            Logger<MusContent> logger = new Logger<MusContent>(LoggerMode.TextFile);
            logger.SubscribeOnState(search_by_year);
            logger.SubscribeOnState(search_by_name);
            logger.SubscribeOnState(search_by_genre);

            search.become_state_parent(search_by_name);
            search.become_state_parent(search_by_year);
            search.become_state_parent(search_by_genre);
            mainMenu.become_state_parent(news);
            mainMenu.become_state_parent(search);
            Context<MusContent> app = new Context<MusContent>(mainMenu);
            app.Context_library = generic_lib;
            app.run();
        }
    }
}
