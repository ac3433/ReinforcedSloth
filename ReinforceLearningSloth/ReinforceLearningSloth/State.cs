using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforceLearningSloth
{
    class State
    {
        public int StateID { get; private set; }

        private Dictionary<string, Transition> SymbolTransition;

        private AbstractOutput Output;

        public State(int id, AbstractOutput output)
        {
            StateID = id;
            SymbolTransition = new Dictionary<string, Transition>();
            Output = output;
        }

        public State GetTransitionState(string symbol)
        {
            Transition transition = GetTransition(symbol);
            if (transition != null)
                return transition.EndTransition;

            return null;
        }

        public Transition GetTransition(string symbol)
        {
            Transition transition;

            SymbolTransition.TryGetValue(symbol, out transition);

            return transition;
        }

        public void AddTransition(string symbol, Transition transition)
        {
            if (!SymbolTransition.ContainsKey(symbol))
                SymbolTransition.Add(symbol, transition);
        }

        public List<Transition> GetPossibleTransitions()
        {
            return SymbolTransition.Values.ToList();
        }

    }
}
