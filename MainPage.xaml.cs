#define QLEARNING
//#define SARSA

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

namespace MazeSolving1
{
    public partial class MainPage : ContentPage
    {
        int Iterations = 1000;  // The number of Learning Iteration
        int Interval = 10;      // Update Result Display every Interval steps
        int t = 0;              // Learning Time(Step)

        int Nstate;             // The number of Actions
        double MaxReward;       // Maximum Value of Rewards

        int GridRows = 4;
        int GridColumns = 5;

        Random rnd = new Random();

        // State class to calculate Qvalue
        class State
        {
            public double Reward { get; set; }  // Reward of this state

            public List<Action> Actions = new List<Action>();
            // Possible action at this state

            public Action MaxQvalueAction
            {      // Maximum Qvalue of the next Actions
                get { return Actions.OrderByDescending((q) => q.Qvalue).First(); }
            }

            public int Visited { get; set; }    // The number of times this sate was visted

        }
        State StartState, CurrentState, PreviousState;
        List<State> States = new List<State>();


        // Action class to calculate Qvalue
        class Action
        {
            public State CrntState { get; set; }    // Current State
            public State NextState { get; set; }    // Next State
            public double Qvalue { get; set; }      // Q value of this action
        }
        Action CurrentAction;


        // Qvalue List used for Convergence Test in IntervalTuning()
        List<double> Qvalues = new List<double>();


        // Learning Result item to be added ListView
        class Result
        {
            public string Result1 { get; set; }
            public string Result2 { get; set; }
        }
        static ObservableCollection<Result> Results;


        public MainPage()
        {
            InitializeComponent();

            Initialize();   // Initialize Display and Reinforcement Learning
        }


        // Set Maze Data and Initialization
        void Initialize()
        {
            SetMazeData(Maze_5by4); // Set Maze Data

            SetStateGrid();         // Initialize Maze Display

            // Initialize the ListView for displaying results
            Results = new ObservableCollection<Result>()
            {
                new Result(){Result1="Learning Start:"}
            };
            ResultsList.ItemsSource = Results;
            DisplayResults();   // Display initial Qvalues;
        }


        // Reinforcement Learning
        void Learning()
        {
            Label3.Text = "Learning";
            // Learning Loop: Iterate over Learning Interval times every 1 seconds
            // The number of Interval steps is changed by IntervalTuning()
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                Label1.Text = "timer start";
                // Interval times Learning with Action Blink
                if (!Pause) IntervalLearningWithActionBlink();


                bool NextTimerStart = t < Iterations;   // Keep Timer running while t<Iterations
                if (!NextTimerStart)     // Executed when Timer Stop
                {
                    StatusLabel.Text = "Status: Complete";
                    Label3.Text = "End";
                }

                return NextTimerStart;  // True: Next Timer Start
                                        // false: Timer Stop
            });
        }


        // "Interval" times Learning with one loop blinking action
        void IntervalLearningWithActionBlink()
        {
            Label2.Text = "Interval Learning";
            CurrentState = StartState;

            Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
            {
                
#if QLEARNING
                QLearningAction();
#elif SARSA
                SarsaAction();
#endif

                // Blink Current State
                int index = States.IndexOf(CurrentState);
                BlinkBoxes[index].IsVisible = true;


                // End, if arrive Goal
                bool NextTimerStart = !IsGoal();
                if (!NextTimerStart)
                {
                    
#if QLEARNING
                    QLearningLoop(Interval);
#elif SARSA
                    SarsaLoop(Interval);
#endif

                    // Update Interval according to the Convergence Test
                    Interval = IntervalTuning();

                    // Display Results
                    DisplayResults();
                    Label1.Text = t.ToString();

                    // Clear (invisible) Blinked States
                    foreach (BoxView box in BlinkBoxes)
                    {
                        if( !IsWall(States[BlinkBoxes.IndexOf(box)]) 
                           && BlinkBoxes.IndexOf(box)!=0 )
                            box.IsVisible = false;
                    }

                }

                return NextTimerStart;

            });
        }



        // Enlarge Result Display Interval according to the Qvalue Convergence
        int IntervalTuning()
        {

            List<double> currentQvalues = new List<double>();
            double d = 0;

            foreach (State state in States)
            {
                foreach (Action action in state.Actions)
                {
                    if (action.CrntState != action.NextState)
                    {
                        currentQvalues.Add(action.Qvalue);
                    }
                }
            }

            // Calculate the difference between new Qvalue and old one
            for(int i=0; i<currentQvalues.Count; i++)
            {
                d += Math.Abs(currentQvalues[i] - Qvalues[i]);
            }

            Qvalues = currentQvalues;   // Update Qvalues

            Label3.Text = d.ToString();

            // Change the Interval according to the Qvalue Convergence
            if (d > 20) return 10;
            else if (d > 10) return 50;
            else if (d > 5) return 100;
            else if (d > 1) return 200;
            else return 2000;  // Stop Learning
        }


        // Start, Stop, and Reset Learning
        bool Pause = false;
        void StartLearning(object sender, EventArgs args)
        {
            Button button = (Button)sender;

            if (button.Text == "Start")
            {
                t = 0;
                Learning();     // Execute Learning
                button.Text = "Pause";
                StatusLabel.Text = "Status: Learning";
                return;
            }
            if (button.Text == "Pause")
            {
                // Pause Learning
                Pause = true;
                button.Text = "ReStart";
                StatusLabel.Text = "Status: Waiting";
                return;
            }
            if (button.Text == "ReStart")
            {
                Pause = false;
                button.Text = "Pause";
                StatusLabel.Text = "Status: Learning";
                return;
            }

        }
        void StopLearning(object sender, EventArgs args)
        {
            if (StartButton.Text != "Start") StartButton.Text = "Start";
            t = Iterations;
            StatusLabel.Text = "Status: Stop";
        }

        void ResetLearning(object sender, EventArgs args)
        {
            Results.Clear();
            StatusLabel.Text = "Status: Waiting";
        }


        // Display Results
        void DisplayResults()
        {
            string s1 = t.ToString()+": "; // Step Number "t"
            string s2 = "";

            foreach(State state in States)
            {
                foreach(Action action in state.Actions)
                {
                    if(action.CrntState!=action.NextState){
                        s1 += string.Format("{0, 3} ", (int)(action.Qvalue*10));
                        s2 += States.IndexOf(action.CrntState).ToString() 
                                    + ">" + States.IndexOf(action.NextState).ToString() + " ";
                    }                        
                }
            }


            for (int i = 0; i < GridColumns; i++)
            {
                for (int j = 0; j < GridRows; j++)
                {
                    // Convert the result values to the color intensity
                    // and Normalize the intensity according to the Maximum of the values
                    int color = (int)(States[i + j*GridColumns].Visited/(double)Iterations*256);
                    if (color > 256) color = 255;

                    MazeCellLabels[i + j * GridColumns].Text = (States[i + j * GridColumns].Visited).ToString();
                    MazeCellLabels[i + j * GridColumns].BackgroundColor = Color.FromRgb(255, 255 - color, 255 - color);
                }
            }


            Result result = new Result()
            {
                Result1 = s1,
                Result2 = s2
            };
            Results.Insert(0, result);

            Label2.Text = s1;

        }

    }   // End of public partial class MainPage : ContentPage
}       // End of namespace MazeSolving1
