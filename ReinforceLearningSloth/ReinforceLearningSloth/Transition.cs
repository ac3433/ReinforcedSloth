using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforceLearningSloth
{
    class Transition
    {
        public State StartTransition { get; private set; }
        public State EndTransition { get; private set; }

        public Dictionary<Transition, float> Expectations { get; set; }

        public string Name { get; private set; }
        public float Confidence { get; set; }

        public bool IsTemporary { get; set; }
        public AbstractDistribution Distribution { get; set; }

        public Transition(State start, State end, string symbol, AbstractOutput output, AbstractDistribution distribution)
        {
            StartTransition = start;
            EndTransition = end;
            Expectations = new Dictionary<Transition, float>();
            Name = symbol;
            Distribution = distribution;
            IsTemporary = false;
            Confidence = .1f;
        }

        public void AddExpectation(Transition t, float value)
        {
            if (!Expectations.ContainsKey(t))
                Expectations.Add(t, value);
        }

        //Todo?: create a deep cloner
        public void CopyTransition(Transition transition)
        {
            Distribution = transition.Distribution;
            Confidence = transition.Confidence;
        }

        public void IncreaseExpectation(Transition t)
        {
            if(Expectations.ContainsKey(t))
            {
                float increase = .5f * Expectations[t];
                Expectations[t] += increase;
            }
        }

        public void DecreaseExpectation(Transition t)
        {
            if (Expectations.ContainsKey(t))
            {
                float decrease = .7f * Expectations[t];
                Expectations[t] += decrease;
            }
        }

    }
}
