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
            this.current_state = current_state;
            this.current_state.context = this;
        }

        public void transitionToState(State state_to_set)
        {
            current_state = state_to_set;
            current_state.context = this;
        }

        public void draw()
        {
            this.current_state.draw();
            Console.WriteLine("type EXIT to exit");
        }

        public void handle(string user_input)
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


    abstract class BaseState
    {
        public abstract void handle(string user_input);
        public abstract void draw();

    }


    class State : BaseState
    {
        public string title;
        public string title_frame = "=========="; // todo make constant/define in baseclass
        public string state_description;

        public State dst_state;  // todo make more than one possible destinations
        public Dictionary<string, State> options = new Dictionary<string, State>();
        public State parent_state;
        public Context context;

        public State(string title, string description)
        {
            this.title = title;
            this.state_description = description;
            this.dst_state = null;
            this.parent_state = null;
        }

        public void becomeStateParent(State child)
        {            
            this.options.Add(child.title, child);
            child.parent_state = this;
        }

        public void setContext(Context context)
        {
            this.context = context;
        }

        public override void handle(string user_input)
        {
            // handle navigation
            foreach (KeyValuePair<string, State> option in this.options)
            {
                if (user_input == option.Key)
                {
                    context.transitionToState(option.Value);
                }
            }

            if (user_input == "BACK" && this.parent_state != null)
            {
                this.context.transitionToState(this.parent_state);
            }
        }

        public override void draw()
        {
            Console.WriteLine(title_frame + title + title_frame);
            foreach (KeyValuePair<string, State> option in this.options)
            {
                Console.WriteLine("type " + option.Key + " to " + option.Value);
            }
            if (this.parent_state != null)
            {
                Console.WriteLine("type BACK to return to the previous state");
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {

            // titile parent son
            State mainMenu = new State("MainMenu", "MainMenu");
            State news = new State("NEWS", "see the latest news");
            State explore = new State("EXPLORE", "expore our library");
            State search = new State("SEARCH", "search for specific things");
            mainMenu.becomeStateParent(news);
            mainMenu.becomeStateParent(explore);
            mainMenu.becomeStateParent(search);
            Context app = new Context(mainMenu);
            app.run();            
        }
    }
}
