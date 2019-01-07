using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforceLearningSloth
{
    class ConsoleOutput : AbstractOutput
    {
        public override void Output(string message)
        {
            Console.WriteLine(message);
        }
    }
}
