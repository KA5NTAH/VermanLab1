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


    class Program
    {
        static void Main(string[] args)
        {

            //titile parent son
            State mainMenu = new ConcreteState("MainMenu", "MainMenu");
            ConcreteState news = new ConcreteState("NEWS", "see the latest news");
            ConcreteState explore = new ConcreteState("EXPLORE", "expore our library");
            ConcreteState search = new ConcreteState("SEARCH", "search for specific things");
            mainMenu.become_state_parent(news);
            mainMenu.become_state_parent(explore);
            mainMenu.become_state_parent(search);
            Context app = new Context(mainMenu);
            app.run();
        }
    }
}
