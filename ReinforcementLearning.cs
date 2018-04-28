#define BOLTZMANN
//#define EPSILON

using System;

namespace MazeSolving1
{

    public partial class MainPage
    {
        double Alpha = 0.1;     // Learning rate
        double Gamma = 0.9;     // Discount factor
        double Epsilon = 0.3;   // Random action probability
        double TRate = 0.0001;  // Time rate of Sigmoid funcion

        // Update Q-value by Q-Learning Algorithm
        double QLearning(Action action)
        {
            return action.Qvalue
                      + Alpha * (action.NextState.Reward
                                + Gamma * action.NextState.MaxQvalueAction.Qvalue
                                - action.Qvalue);
        }


        // Update Q-value by Sarsa Algorithm
        double Sarsa(Action action, Action nextAction)
        {
            return action.Qvalue
                      + Alpha * (action.NextState.Reward
                                + Gamma * nextAction.Qvalue
                                - action.Qvalue);
        }


        // One Action and Update Q value by Q-Learning
        void QLearningAction()
        {
            CurrentAction = GetAction(CurrentState, t);

            // Update Current State
            PreviousState = CurrentState;
            CurrentState = CurrentAction.NextState;
            if (PreviousState != CurrentState) CurrentState.Visited++;

            // Learning Algorithm
            CurrentAction.Qvalue = QLearning(CurrentAction);
        }


        // One Action and Update Q value by Sarsa
        void SarsaAction()
        {
            // Select the 1st Action: Epsilon Greedy or Boltzmann Exploration
            if (CurrentState == StartState)
            {
                CurrentAction = GetAction(CurrentState, t);
                //CurrentAction.NextState.Visited++;
            }

            // Select the next Action
            Action NextAction = GetAction(CurrentAction.NextState, t + 1);

            // Learning Algorithm
            CurrentAction.Qvalue = Sarsa(CurrentAction, NextAction);

            // Update Currenet State and Action
            PreviousState = CurrentState;
            CurrentState = CurrentAction.NextState;
            if (PreviousState != CurrentState) CurrentState.Visited++;
            CurrentAction = NextAction;
        }



        // One Loop from Start to Goal by Q-Learning
        void QLearningLoop(int N)
        {
            for (int i = 0; i < N; i++)
            {
                CurrentState = StartState;

                while (!IsGoal())                
                //while(!IsWall(CurrentState))
                {
                    QLearningAction();
                }

                if (t == Iterations) break;
                t++;
            }
        }

        // One Loop from Start to Goal by Sarsa
        void SarsaLoop(int N)
        {
            for (int i = 0; i < N; i++)
            {
                CurrentState = StartState;

                while (!IsGoal())
                {
                    SarsaAction();
                }

                if (t == Iterations) break;
                t++;
            }
        }


        // Return the next Action according to the policy
        Action GetAction(State state, int time)
        {
            // Epsilon Greedy or Boltzmann Exploration
#if EPSILON
            return EpsilonGreedy(state);
#endif

#if BOLTZMANN
            return BoltzmannExploration(state, time);
#endif
        }


        // Epsilon Greedy Strategy
        Action EpsilonGreedy(State state)
        {
            int n = CurrentState.Actions.Count;  // The number of Actions of the CurrentState
            //Action currentAction = new Action();

            double d = rnd.NextDouble();
            int i = rnd.Next(n);


            if (d < Epsilon)    // Random Action
            {
                //CurrentState = CurrentState.Actions[i].NextState;
                return state.Actions[i];
            }
            else                // Select the action with max Qvalue
            {
                //CurrentState = CurrentState.MaxQvalueNextState;
                return state.MaxQvalueAction;
            }


        }


        // Boltzmann Exploration Strategy
        Action BoltzmannExploration(State state, int time)
        {
            double d = rnd.NextDouble();
            double d1 = d;

            double T = 1 / Math.Log(TRate * time + 1.1); // Sigmoid Function


            double ExpSum = 0.0;
            foreach (Action action in state.Actions)
            {
                ExpSum += Math.Exp(action.Qvalue / T);
            }

            Action currentAction = new Action();
            foreach (Action action in state.Actions)
            {
                double p = Math.Exp(action.Qvalue / T) / ExpSum;

                if (d < p)
                {
                    currentAction = action;
                    break;
                }
                d -= p;
            }

            return currentAction;
        }

    }
}
