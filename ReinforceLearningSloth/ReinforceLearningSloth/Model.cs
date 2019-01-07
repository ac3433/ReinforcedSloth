using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//updated version from previous model
namespace ReinforceLearningSloth
{
    class Model
    {
        private AbstractDistribution Distribution;
        private AbstractOutput Output;

        //parameters from the skinner box
        private const float alpha = .05f;
        private const float beta = .5f;
        private const float gamma = .001f;
        private const float zeta = .1f;
        private const float kota = .1f;
        private const float nu = .5f;
        private const float epi = .8f;
        private const float kappa = .9f;
        private const string EPISILON = " ";

        private List<State> Q; //all state
        private List<State> R; //reward
        private List<State> P; //punish
        private State q0; // inital
        private List<string> OSymbol; //output symbol
        private List<string> ISymbol; //input symbol
        private Dictionary<string, float> I; //current input list
        private Dictionary<string, float> Il; //last list

        private State c; // current
        private State lc; //last state
        private State qa; // new transition

        private string ad; //current symbol
        private float sd; // current symbol value
        private string al; //last symbol
        private string o; // current ouput symbol
        private string ol; //last output symbol
        private Transition ct; //current transition

        private Dictionary<string, float> SymbolStrength; //keep track of all inputs chance possibilities
        private Dictionary<string, Transition> HistoryData; // keep track of actions performed
        private List<AbstractDistribution> conditions; //temporary hold the conditions until the next action
        private const int finalStop = 20;

        public Model(AbstractOutput output)
        {
            Q = new List<State>();
            R = new List<State>();
            P = new List<State>();
            q0 = new State(0, output);
            OSymbol = new List<string>();
            ISymbol = new List<string>();
            I = new Dictionary<string, float>();
            Il = new Dictionary<string, float>();
            SymbolStrength = new Dictionary<string, float>();
            HistoryData = new Dictionary<string, Transition>();
            Distribution = new DistrubutionV2();
            Initalize();
        }

        //used to initalize predefine setting and be able to track it outside of this model...
        public Model(List<State> Q, List<State> Reward, List<State> Punish, State Inital, Dictionary<string, float> I, Dictionary<string, float> Il, AbstractOutput output, AbstractDistribution dist)
        {
            
            this.Q = Q;
            R = Reward;
            P = Punish;
            q0 = Inital;
            this.I = I;
            this.Il = Il;
            this.Distribution = dist;
            this.Output = output;
            SymbolStrength = new Dictionary<string, float>();
            HistoryData = new Dictionary<string, Transition>();
            SymbolStrength = new Dictionary<string, float>();
            OSymbol = new List<string>();
            ISymbol = new List<string>();
            Initalize();
        }

        //step 1
        private void Initalize()
        {
            c = q0;
            lc = q0;
            qa = q0;
            ad = EPISILON;
            sd = 0f;
            al = EPISILON;
            o = EPISILON;
            ol = EPISILON;

        }

        //step 2
        public State IterateThrough(Dictionary<string, float> inputs)
        {
            //step 3
            Il = I;
            I = inputs;
            //step 4
            FindStrongestSymbol();
            //step 5
            CreateNewTransitions();
            //step 7
            NewOutput();
            //step 8
            KeepTrackHistory();
            //step 9
            UpdateExpectation();
            //step 10
            lc = c;
            //step 11
            int QPos = Q.IndexOf(c.GetTransition(ad).EndTransition); //find position in the Q of the next Transition
            c = QPos >= 0 ? Q[QPos] : null;
            //step 12
            al = ad;

            //step 13
            //if it reached to the reward location, increase chance
            if (R.Contains(c))
                ApplyReward();
            else if (P.Contains(c))
                ApplyPunishment();
            else
            {
                conditions = new List<AbstractDistribution>();
                ApplyCondition();
            }
            Output.Output(c.StateID.ToString()); //provide action have taken.
            return c; //return current state to be used for new interation
        }

        private void FindStrongestSymbol()
        {
            string symbol = EPISILON;
            float val = 0;
            foreach(string sym in I.Keys)
            {
                if(I[sym] > val)
                {
                    symbol = sym;
                    val = I[sym];
                }
            }

            ad = symbol;
            sd = val;
        }

        private void CreateNewTransitions()
        {
            foreach(string symbol in I.Keys)
            {
                if(c.GetTransition(symbol) == null)
                {
                    State s = new State(Q.Count, Output);
                    Q.Add(s);
                    Transition t = new Transition(c, s, symbol ,Output, Distribution);
                    c.AddTransition(symbol, t);
                    Transition empty = new Transition(s, qa, EPISILON, Output, Distribution);
                    t.AddExpectation(empty, 0);


                    foreach(State st in Q)
                    {
                        Transition ts = st.GetTransition(symbol);

                        if(ts != null)
                        {
                            t.CopyTransition(ts);
                            if (Q.Contains(ts.EndTransition))
                            {
                                int position = Q.IndexOf(ts.EndTransition);
                                if (R.Contains(Q[position]))
                                    R.Add(s);
                                else if (P.Contains(Q[position]))
                                    P.Add(s);
                                break;
                            }

                        }
                       

                    }
                }
            }
            ct = c.GetTransition(ad);
        }

        //update the current output
        private void NewOutput()
        {
            Transition goToTrans = c.GetTransition(ad);
            ol = o;
            o = goToTrans.Distribution.SelectionPath();
            if (SymbolStrength.ContainsKey(o))
            {
                if(SymbolStrength.ContainsKey(ad))
                    SymbolStrength[o] = SymbolStrength[ad] * goToTrans.Confidence / (1 + goToTrans.Confidence);
            }
            else
                SymbolStrength.Add(o, goToTrans.Confidence / (1 + goToTrans.Confidence));
        }

        private void KeepTrackHistory()
        {
            if(!HistoryData.ContainsKey(o))
            {
                HistoryData.Add(o, c.GetTransition(ad));
            }
        }

        private void UpdateExpectation()
        {
            Transition lastTransition = lc.GetTransition(al);
            Transition currentTransition = c.GetTransition(ad);

            if(lastTransition != null)
            {
                if(lastTransition.Expectations.ContainsKey(currentTransition))
                {
                    lastTransition.IncreaseExpectation(currentTransition);
                    currentTransition.IncreaseExpectation(lastTransition);
                }
                else
                {
                    lastTransition.AddExpectation(currentTransition, alpha);
                    currentTransition.AddExpectation(lastTransition, alpha);
                }

                foreach(string symbol in ISymbol)
                {
                    if(!I.ContainsKey(symbol))
                    {
                        Transition transitionTo = c.GetTransition(symbol);
                        if(lastTransition.Expectations.ContainsKey(transitionTo))
                        {
                            lastTransition.DecreaseExpectation(transitionTo);
                            currentTransition.DecreaseExpectation(transitionTo);
                        }
                    }

                    foreach(string symbolB in ISymbol)
                    {
                        if(!symbol.Equals(symbolB))
                        {
                            Transition a = c.GetTransition(symbol);
                            Transition b = c.GetTransition(symbolB);

                            if(a != null)
                            {
                                if(I.ContainsKey(symbol) && I.ContainsKey(symbolB))
                                {
                                    if (a.Expectations.ContainsKey(b))
                                        a.IncreaseExpectation(b);
                                    else
                                        a.AddExpectation(b, alpha);
                                }
                            }
                            else if (ISymbol.Contains(symbolB))
                            {
                                a.DecreaseExpectation(b);
                            }
                        }
                    }

                }
            }
        }

        private void ApplyReward()
        {
            foreach(string symbol in HistoryData.Keys)
            {
                Transition st = HistoryData[symbol];
                float speedOfLearningAmount = sd * zeta / st.Confidence;
                st.Distribution.ChangeSymbolChance(symbol, speedOfLearningAmount);
                st.Confidence = speedOfLearningAmount;
                foreach(State s in Q)
                {
                    if(s.Equals(st.StartTransition))
                    {
                        Transition start = s.GetTransition(st.Name);
                        if(start != null)
                        {
                            start.Distribution.ChangeSymbolChance(symbol, speedOfLearningAmount * nu);
                            start.Confidence = speedOfLearningAmount;
                        }
                    }
                }
            }
            HistoryData.Clear();
        }

        private void ApplyPunishment()
        {
            foreach (string symbol in HistoryData.Keys)
            {
                Transition st = HistoryData[symbol];
                float speedOfLearningAmount = sd * zeta / st.Confidence;
                st.Distribution.ChangeSymbolChance(symbol, -speedOfLearningAmount);
                st.Confidence = speedOfLearningAmount;
                foreach (State s in Q)
                {
                    if (s.Equals(st.StartTransition))
                    {
                        Transition start = s.GetTransition(st.Name);
                        if (start != null)
                        {
                            start.Distribution.ChangeSymbolChance(symbol, -speedOfLearningAmount * nu);
                            start.Confidence = speedOfLearningAmount;
                        }
                    }
                }
            }
            HistoryData.Clear();
        }

        private void ApplyCondition()
        {
            if(!ol.Equals(EPISILON))
            {
                Transition lt = lc.GetTransition(al);
                foreach (State state in Q)
                {
                    foreach (string symbol in ISymbol)
                    {
                        if (state.GetTransition(symbol) != null)
                            state.GetTransition(symbol).Distribution.ChangeSymbolChance(ol, alpha * sd);

                    }
                }

                foreach (string symbol in ISymbol)
                {
                    Transition st = lc.GetTransition(symbol);
                    if(lt.Expectations.ContainsKey(st) && Il.ContainsKey(symbol))
                    {
                        st.Distribution.ChangeSymbolChance(ol, alpha * sd);
                        st.Confidence = nu * sd / st.Confidence;
                        UpdateCondition(lc, symbol, sd / st.Confidence);
                    }

                    foreach(State ss in Q)
                    {
                        Transition selectionT = ss.GetTransition(symbol);
                        if(selectionT != null && lt.Expectations.ContainsKey(st))
                        {
                            lt.Confidence = nu * sd / selectionT.Confidence;
                            UpdateCondition(ss, symbol, sd / selectionT.Confidence);
                        }
                    }

                }
            }
        }

        private void UpdateCondition(State state, string symbol, float amount)
        {
            Transition st = state.GetTransition(symbol);

            if(st != null)
            {
                foreach(string symb in I.Keys)
                {
                    Transition nextT = state.GetTransition(symb);
                    if(nextT != null)
                    {
                        if(st.Expectations.ContainsKey(nextT))
                        {
                            nextT.Distribution.ChangeSymbolChance(ol, gamma * sd / st.Confidence);
                            nextT.Confidence = gamma * sd / st.Confidence;
                            if(nextT.Confidence != 0)
                                UpdateCondition(state, symbol, amount / nextT.Confidence);
                        }
                    }
                }
            }
        }
    }
}
