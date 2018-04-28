using System;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MazeSolving1
{    
    public partial class MainPage
    {

        // Route class to set Maze Data
        class Route
        {
            public int From { get; set; }
            public int To { get; set; }
            public double Reward { get; set; }
        }

        // Set Route Data
        List<Route> Maze_5by4 = new List<Route>()
        {
            new Route(){ From=0 , To=5,  Reward=0 },
            new Route(){ From=5 , To=6,  Reward=0 },
            new Route(){ From=5 , To=10,  Reward=0 },
            new Route(){ From=6 , To=7,  Reward=0 },
            new Route(){ From=7 , To=2,  Reward=0 },
            new Route(){ From=7 , To=12,  Reward=0 },
            new Route(){ From=2 , To=3,  Reward=0 },
            new Route(){ From=10 , To=15,  Reward=0 },
            new Route(){ From=15 , To=16,  Reward=0 },
            new Route(){ From=12 , To=13,  Reward=0 },
            new Route(){ From=13 , To=14,  Reward=0 },
            new Route(){ From=14 , To=9,  Reward=0 },
            new Route(){ From=14 , To=19,  Reward=10 },
            new Route(){ From=19 , To=19,  Reward=10 }, // Goal
            new Route(){ From=3 , To=3,  Reward=0 },    // Goal
            new Route(){ From=16 , To=16,  Reward=0 },  // Goal
            new Route(){ From=9 , To=9,  Reward=0 }     // Goal
        };


        // BoxViews and Labels to display Actions dynamically
        List<BoxView> BlinkBoxes = new List<BoxView>();
        List<Label> MazeCellLabels = new List<Label>();
        List<Button> StateButtons = new List<Button>();

        double GridCellSize;


        // Get QvalueGrid size and calculate optimal Grid cell size
        void SetGridCellSize(object sender, EventArgs e)
        {
            double w = QvalueGrid.Width / GridColumns;
            double h = QvalueGrid.Height / GridRows;

            GridCellSize = Math.Min(w, h);


            foreach(ColumnDefinition column in QvalueGrid.ColumnDefinitions)
            {
                column.Width = GridCellSize;
            }
            foreach(RowDefinition row in QvalueGrid.RowDefinitions)
            {
                row.Height = GridCellSize;
            }
        }


        // Set Grids to Display Qvalue Update
        void SetStateGrid()
        //void SetStateGrid(object sender, EventArgs e)
        {
            // Initialize Grid Columns and Rows
            for (int i = 0; i < GridColumns; i++)
                QvalueGrid.ColumnDefinitions.Add(
                    new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            for (int j = 0; j < GridRows; j++)
                QvalueGrid.RowDefinitions.Add(
                    new RowDefinition() { Height=new GridLength(1, GridUnitType.Star) });
            
            // Add Background BoxViews to all Maze Cells
            for (int i = 0; i < GridColumns; i++)
            {
                for (int j = 0; j < GridRows; j++)
                {
                    QvalueGrid.Children.Add(new BoxView() , i, j);
                }
            }

            // Create BlinkBoxes and MazeCellLabels to show Actions dynamically
            for (int i = 0; i < GridColumns; i++)
            {
                for (int j = 0; j < GridRows; j++)
                {
                    MazeCellLabels.Add(new Label()
                    {
                        FontSize = 12
                    });

                    BlinkBoxes.Add(new BoxView()
                    {
                        Color = Color.Yellow,
                        IsVisible = false
                    });

                    StateButtons.Add(new Button()
                    {
                        BackgroundColor = Color.Transparent
                    });
                }
            }

            // Set BlinkBoxes and MazeCellLabels in Grid Cell
            for (int i = 0; i < GridColumns; i++)
            {
                for (int j = 0; j < GridRows; j++)
                {
                    QvalueGrid.Children.Add(MazeCellLabels[i + j * GridColumns], i, j);
                    QvalueGrid.Children.Add(BlinkBoxes[i + j * GridColumns], i, j);
                    QvalueGrid.Children.Add(StateButtons[i + j * GridColumns], i, j);
                    StateButtons[i + j * GridColumns].Clicked += DisplayStateProperty;
                }
            }


            // Set the color of the Walls and the cells with Reward
            foreach(BoxView box in BlinkBoxes)
            {
                if(IsWall(States[BlinkBoxes.IndexOf(box)]))
                {
                    box.Color = Color.Black;
                    box.IsVisible = true;
                }

                if(States[BlinkBoxes.IndexOf(box)].Reward > 0)
                {
                    box.Color = Color.Red;
                    box.IsVisible = true;
                }
            }


            // Set Start State
            StartState = States[0]; // Set the starting state
            BlinkBoxes[0].Color = Color.Yellow;
            BlinkBoxes[0].IsVisible = true;
            QvalueGrid.Children.Add(new Label
            {
                Text = "Start",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }, 0, 0);

            // Add "Goal" label
            QvalueGrid.Children.Add(new Label
            {
                Text = "Goal",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }, 4, 3);

        }



        void SetMazeData(List<Route> routes)
        {
            int maxFrom = routes.OrderByDescending((a) => a.From).First().From;
            int maxTo = routes.OrderByDescending((a) => a.To).First().To;

            Nstate = Math.Max(maxFrom, maxTo) + 1;
            MaxReward = routes.OrderByDescending((a) => a.Reward).First().Reward;

            for (int i = 0; i < Nstate; i++)
            {
                States.Add(new State());
            }

            // Create Route States and Actions
            foreach (Route route in routes)
            {
                SetAction(route.From, route.To, route.Reward);
            }
        }


        // Set States, Actions, and Rewards
        void SetAction(int crntState, int nextState, double reward)
        {
            Action action = new Action()
            {
                CrntState = States[crntState],
                NextState = States[nextState],
                Qvalue = rnd.NextDouble()   // Initialize Qvalue
            };
            States[crntState].Actions.Add(action);
            States[nextState].Reward = reward;

            Qvalues.Add(action.Qvalue);     // Initialize Qvalues for Convergence Test
        }


        // If the state doesn't any actions, it's a Wall
        bool IsWall(State state)
        {
            return state.Actions.Count == 0;
        }


        // Check if it has arrived at the Goal
        bool IsGoal()
        {
            // If Loop in the same state, it's Goal
            return PreviousState == CurrentState;

            /*
            // If there is a loop action in the state, it is the Goal
            foreach (Action action in CurrentState.Actions)
            {
                return action.CrntState == action.NextState;
            }
            // If no loop action, the state isn't the Goal
            return false;
            */
        }


        void DisplayStateProperty(object sender, EventArgs args)
        {
            Button button = (Button)sender;
            int index = StateButtons.IndexOf(button);

            string StateIndex = string.Format("State{0}", index);

            string StateProperty = "Q values:" + "\n";
            foreach(Action action in States[index].Actions)
            {
                StateProperty += 
                    "->" + States.IndexOf(action.NextState).ToString() 
                    + "  Q value=" + ((int)(action.Qvalue*10)).ToString() + "\n";

            }


            DisplayAlert(StateIndex, StateProperty, "OK");
        }

    }
}
