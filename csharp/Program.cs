using System;

namespace Evaluator
{
    class Program
    {
        static void Main(string[] args)
        {
            Variable x = new Variable(0.0);
            Variable y = new Variable(0.0);
            var expression =
                    new ExpressionBuilder("-x^y")
                            .AddVariable("x", x)
                            .AddVariable("y", y)
                            .Build();
            x.Value = 1;
            y.Value = 2;
            Console.WriteLine(expression());
        }
    }
}
