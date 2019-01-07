using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforceLearningSloth
{
    class DistrubutionV2 : AbstractDistribution
    {
        //http://www.vcskicks.com/random-element.php
        public override string SelectionPath()
        {
            string choice = "";
            Random rand = new Random();
            double picked = rand.NextDouble();
            double cumulative = 0.0;
            foreach(string symbol in PSymbols.Keys)
            {
                cumulative += PSymbols[symbol];
                if(picked < cumulative)
                {
                    choice = symbol;
                    break;
                }
            }

            return choice;
        }

        //take the positive and negative number to chance the percentage chance of the symbol and affect the other chances of the other symbols
        public override void ChangeSymbolChance(string symbol, float amount)
        {
            if(PSymbols.ContainsKey(symbol))
            {
                TotalWeight += amount;
                foreach(string pSymbol in PSymbols.Keys)
                {
                    if(pSymbol.Equals(symbol))
                    {
                        PSymbols[symbol] = (PSymbols[symbol] + amount) / TotalWeight;
                    }
                    else
                    {
                        float split = amount / (PSymbols.Count - 1);
                        PSymbols[symbol] = (PSymbols[symbol] - split / TotalWeight);
                    }
                }
            }
        }
    }
}
