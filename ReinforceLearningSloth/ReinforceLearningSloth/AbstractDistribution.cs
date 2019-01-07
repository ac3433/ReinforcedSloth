using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforceLearningSloth
{
    
    abstract class AbstractDistribution
    {
        protected Dictionary<string, float> PSymbols;
        protected float TotalWeight;

        public AbstractDistribution()
        {
            PSymbols = new Dictionary<string, float>();
            TotalWeight = 0f;
        }

        public abstract string SelectionPath();

        public abstract void ChangeSymbolChance(string symbol, float amount);

        public void SetCustomizeDistribution(Dictionary<string, float> customize)
        {
            PSymbols.Clear();
            foreach (string symbol in customize.Keys)
            {
                PSymbols.Add(symbol, customize[symbol]);
                TotalWeight += customize[symbol];
            }
        }
    }
}
