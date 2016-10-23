using System;
using System.Collections.Generic;

namespace Evaluator
{
    class Variable
    {
        public double Value { get; set; }

        public Variable(double value)
        {
            this.Value = value;
        }
        
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    class ExpressionBuilder
    {

        private int pos = -1, ch;
        private string str;
        private Dictionary<string, Variable> variables;

        public ExpressionBuilder(string toParse)
        {
            str = toParse;
            variables = new Dictionary<string, Variable>();
        }

        public ExpressionBuilder AddVariable(string name, Variable var)
        {
            variables.Add(name, var);
            return this;
        }

        public Func<double> Build()
        {
            nextChar();
            Func<double> x = parseExpression();
            if (pos < str.Length)
            {
                throw new Exception("Unexpected: " + (char)ch);
            }

            return x;
        }

        public string ExressionString
        {
            get
            {
                return str;
            }
        }

        private void nextChar()
        {
            ch = (++pos < str.Length) ? str[pos] : -1;
        }

        private bool eat(int charToEat)
        {
            while (ch == ' ') nextChar();
            if (ch == charToEat)
            {
                nextChar();
                return true;
            }
            return false;
        }

        // Grammar:
        // expression = term | expression `+` term | expression `-` term
        // term = factor | term `*` factor | term `/` factor
        // factor = `+` factor | `-` factor | `(` expression `)`
        //        | number | functionName factor | factor `^` factor

        private Func<double> parseExpression()
        {
            Func<double> x = parseTerm();
            while (true)
            {
                if (eat('+'))
                { // addition
                    Func<double> a = x, b = parseTerm();
                    x = (()=>a() + b());
                }
                else if (eat('-'))
                { // subtraction
                    Func<double> a = x, b = parseTerm();
                    x = (()=>a() - b());
                }
                else return x;
            }
        }

        private Func<double> parseTerm()
        {
            Func<double> x = parseFactor();
            while (true)
            {
                if (eat('*'))
                { // multiplication
                    Func<double> a = x, b = parseFactor();
                    x = (()=>a() * b());
                }
                else if (eat('/'))
                { // division
                    Func<double> a = x, b = parseFactor();
                    x = (()=>a() / b());
                }
                else return x;
            }
        }

        private Func<double> parseFactor()
        {
            if (eat('+')) return parseFactor(); // unary plus
            if (eat('-'))
            {
                Func<double> a = parseFactor();
                return ()=> - a(); // unary minus
            }

            Func<double> x;
            int startPos = this.pos;

            if (eat('('))
            { // parentheses
                x = parseExpression();
                eat(')');
            }
            else if ((ch >= '0' && ch <= '9') || ch == '.')
            { // numbers
                while ((ch >= '0' && ch <= '9') || ch == '.')
                    nextChar();
                double number = Convert.ToDouble(
                    str.Substring(startPos, pos - startPos));
                x = ()=>number;
            }
            else if (ch >= 'a' && ch <= 'z')
            { // functions & variables
                while (ch >= 'a' && ch <= 'z')
                    nextChar();
                String func = str.Substring(startPos, pos - startPos);
                if (variables.ContainsKey(func))
                {
                    x = ()=>variables[func].Value;
                }
                else
                {
                    x = parseFactor();
                    Func<double> a = x;
                    
                    switch (func)
                    {
                        case "sqrt":
                            x = ()=>Math.Sqrt(a());
                            break;
                        case "sin":
                            x = ()=>Math.Sin(toRadians(a()));
                            break;
                        case "cos":
                            x = ()=>Math.Cos(toRadians(a()));
                            break;
                        case "tg":
                            x = ()=>Math.Tan(toRadians(a()));
                            break;
                        case "ctg":
                            x = ()=> 1 / Math.Tan(toRadians(a()));
                            break;
                        case "ln":
                            x = ()=>Math.Log(a());
                            break;
                        default:
                            throw new Exception("Unknown function: " + func);
                    }
                }
            }
            else
            {
                throw new Exception("Unexpected: " + (char)ch);
            }

            if (eat('^'))
            { // exponentiation
                Func<double> a = x, b = parseFactor();
                x = ()=>Math.Pow(a(), b());
            }
            return x;
        }

        private double toRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }

}
