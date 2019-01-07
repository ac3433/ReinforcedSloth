using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforceLearningSloth
{
    class Sloth
    {
        private Model model;
        private List<State> reward;
        private List<State> punish;
        private List<State> states;
        private Dictionary<string, float> inputs;
        private Dictionary<string, float> outputs;
        private AbstractOutput output;
        private AbstractDistribution dist;
        private List<Transition> possibleTrans;

        public Sloth()
        {
            reward = new List<State>();
            punish = new List<State>();
            states = new List<State>();
            possibleTrans = new List<Transition>();
            inputs = new Dictionary<string, float>();
            outputs = new Dictionary<string, float>();
            output = new ConsoleOutput();
            dist = new DistrubutionV2();

            //convert the enum and put it into the inputs/states data
            foreach(int i in Enum.GetValues(typeof(DirectionalBox)))
            {
                string name = Enum.GetName(typeof(DirectionalBox), i);
                inputs.Add(name, 0);
                states.Add(new State(i, output));
                possibleTrans.Add(new Transition(states[0], states[i], name, output, dist));
            }
            reward.Add(states[states.Count - 1]);//last is food

            foreach(Transition t in possibleTrans)
            {
                t.Confidence = 100;
                states[0].AddTransition(t.Name, t);
            }

            foreach(Movement values in Enum.GetValues(typeof(Movement)))
            {
                outputs.Add(values.ToString(), 0);
            }

            model = new Model(states, reward, punish, states[0], inputs, outputs, output, dist);


        }

        public void ActivateTestSubject(int trials)
        {
            State current = new State(2, output); ;
            for(int i = 0; i < trials; i++)
            {
                Dictionary<string, float> input = new Dictionary<string, float>();
                input.Add(current.StateID.ToString(), .24f);
                current = model.IterateThrough(input); //start the cycle and get last state to be used again for next cycle
            }
        }
    }
}
