using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Lab1
{

    class Context
    {
        State current_state;

        public Context(State current_state)
        {
            this.transitionToState(current_state);
        }

        public void transitionToState(State state_to_set)
        {
            current_state = state_to_set;
            current_state.Context = this;
        }

        private void draw()  // todo make private
        {
            this.current_state.draw();
            Console.WriteLine("type EXIT to exit");
        }

        private void handle(string user_input)
        {
            Console.WriteLine("Handle your input...");
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


    abstract class State
    {
        protected string title;
        protected const string title_frame = "======"; // string to be drawn from both sides of the title
        protected string state_description;
        protected List<State> children = new List<State>(); // states that are accessible from this state 
        public State Parent { get; set; } = null; // state that can lead to this state
        public Context Context { get; set; }

        public void become_state_parent(State child)
        {
            this.children.Add(child);
            child.Parent = this;
        }

        public void handle_navigation(string user_input)
        {
            foreach (State child in this.children)
            {
                if (user_input == child.title)
                {
                    Context.transitionToState(child);
                }
            }

            if (user_input == "BACK" && this.Parent != null)
            {
                this.Context.transitionToState(this.Parent);
            }
        }

        public void draw_navigation_guide()
        {
            Console.WriteLine(State.title_frame + this.title + State.title_frame);
            foreach (State child in this.children)
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
    }


    class ConcreteState : State
    {
        public ConcreteState(string title, string description)
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


    abstract class MusContent
    {
        public string Name { get; protected set; }
        public abstract void show_info();
    }


    class Album : MusContent
    {
        private List<Song> songs;
        public DateTime Release_date { get; private set; }
        public Genre Album_genre { get; private set; }
        public Performer Album_performer { get; set; }
        private const string song_enumeration_indent = "    ";

        public Album(string name, Genre album_genre, DateTime release_date)
        {
            this.Name = name;
            this.songs = new List<Song>();
            this.Album_genre = album_genre;
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
            Console.WriteLine("Genre: " + this.Album_genre.Name);
            if (this.Album_performer == null)
            {
                Console.WriteLine("No information about the performer yet");
            }
            else
            {
                Console.WriteLine("Performed by: " + this.Album_performer.Name);
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


    class Performer : MusContent
    {
        public DateTime Formation_date { get; private set; }
        public Genre Performer_genre { get; private set; }
        private List<Album> albums;
        private List<Song> songs;

        public Performer(string name, DateTime formation_date)
        {
            this.Name = name;
            this.Formation_date = formation_date;
            this.albums = new List<Album>();
            this.songs = new List<Song>();
        }

        public override void show_info()
        {
            Console.WriteLine("Musical performer: " + this.Name);
            Console.WriteLine("Genre: " + this.Performer_genre.Name);
            Console.WriteLine("Formation date: " + this.Formation_date.ToShortDateString());
            if (this.albums.Count > 0)
            {
                Console.WriteLine("Albums by " + this.Name);
                foreach (Album a in this.albums)
                {
                    Console.WriteLine(a.Name);
                }
            }
            else
            {
                Console.WriteLine("No information aobout the albums");
            }
            
        }

        public void register_song(Song song)
        {
            this.songs.Add(song);
            song.Song_performer = this;
        }

        public void register_album(Album album)
        {
            this.albums.Add(album);
            album.Album_performer = this;
        }
    }


    class Song : MusContent
    {
        public DateTime Release_date { get; private set; }
        public Genre Mus_genre { get; private set; }
        public Performer Song_performer { get; set; }
        public Album Song_album { get; set; }

        public Song(string name, Genre mus_genre, DateTime release_date)
        {
            this.Name = name;
            this.Mus_genre = mus_genre;
            this.Release_date = release_date;
        }

        public override void show_info()
        {
            Console.WriteLine("Song: " + this.Name);
            Console.WriteLine("Genre: " + this.Mus_genre.Name);
            Console.WriteLine("Release year: " + this.Release_date.Year);
            if (this.Song_performer != null)
            {
                Console.WriteLine("Performed by: " + this.Song_performer.Name);
            }
            if (this.Song_album != null)
            {
                Console.WriteLine("Album: " + this.Song_album.Name);
            }
           
        }
    }


    class Genre : MusContent
    {
        public Genre(string name)
        {
            this.Name = name;
        }

        public override void show_info()
        {
            Console.WriteLine("Genre: " + this.Name);
        }
    }


    class Library
    {
        private List<MusContent> content;

        public Library()
        {
            content = new List<MusContent>();
        }

        public void add_content(MusContent content)
        {
            this.content.Add(content);
        }

        public List<MusContent> get_news()
        {
            List<MusContent> res = new List<MusContent>();
            return res;
        }

        public List<MusContent> search_by_name(string query_name)
        {
            List<MusContent> found_content = new List<MusContent>();
            foreach (MusContent thing in this.content)
            {
                if (thing.Name == query_name)
                {
                    found_content.Add(thing);
                }
            }
            return found_content;
        }
    }

    class Program
    {
        
        static Library build_library()
        {
            // todo build seven son album
            Library lib = new Library();

            // generate content
            Genre heavy_metal = new Genre("heavy metal");
            Performer iron_maiden = new Performer("Iron Maiden", new DateTime(1975, 1, 1));

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
                lib.add_content(seventh_son_song);
            }
            iron_maiden.register_album(seventh_son_album);
            lib.add_content(seventh_son_album);
            lib.add_content(iron_maiden);
            return lib;
        }

        static Context build_app()
        {
            State mainMenu = new ConcreteState("MainMenu", "MainMenu");
            ConcreteState news = new ConcreteState("NEWS", "see the latest news");
            ConcreteState search = new ConcreteState("SEARCH", "search for specific things");
            mainMenu.become_state_parent(news);
            mainMenu.become_state_parent(search);
            Context app = new Context(mainMenu);
            return app;
        }

        static void Main(string[] args)
        {
            Library lib = build_library();
            //titile parent son
            Context app = build_app();
            //app.run();
        }
    }
}
