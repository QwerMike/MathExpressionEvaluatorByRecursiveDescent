import java.util.HashMap;
import java.util.Map;

interface Expression {
    double eval();
}

class Variable {
    private double value;

    public Variable(double value) {
        this.value = value;
    }

    public double get() {
        return value;
    }

    public void set(double value) {
        this.value = value;
    }

    @Override
    public String toString() {
        return Double.toString(value);
    }
}

public class ExpressionBuilder {

    private int pos = -1, ch;
    private String str;
    private Map<String,Variable> variables;

    public ExpressionBuilder(final String toParse) {
        str = toParse;
        variables = new HashMap<>();
    }

    public ExpressionBuilder addVariable(String name, Variable var) {
        variables.put(name, var);
        return this;
    }

    public Expression build() {
        nextChar();
        Expression x = parseExpression();
        if (pos < str.length()) {
            throw new RuntimeException("Unexpected: " + (char) ch);
        }

        return x;
    }

    public String getExressionString() {
        return str;
    }

    private void nextChar() {
        ch = (++pos < str.length()) ? str.charAt(pos) : -1;
    }

    private boolean eat(int charToEat) {
        while (ch == ' ') nextChar();
        if (ch == charToEat) {
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

    private Expression parseExpression() {
        Expression x = parseTerm();
        while (true) {
            if (eat('+')) { // addition
                Expression a = x, b = parseTerm();
                x = (() -> a.eval() + b.eval());
            } else if (eat('-')) { // subtraction
                Expression a = x, b = parseTerm();
                x = (() -> a.eval() - b.eval());
            }
            else return x;
        }
    }

    private Expression parseTerm() {
        Expression x = parseFactor();
        while (true) {
            if (eat('*')) { // multiplication
                Expression a = x, b = parseFactor();
                x = (() -> a.eval() * b.eval());
            } else if (eat('/')) { // division
                Expression a = x, b = parseFactor();
                x = (() -> a.eval() / b.eval());
            }
            else return x;
        }
    }

    private Expression parseFactor() {
        if (eat('+')) return parseFactor(); // unary plus
        if (eat('-')) {
            Expression a = parseFactor();
            return () -> -a.eval(); // unary minus
        }

        Expression x;
        int startPos = this.pos;

        if (eat('(')) { // parentheses
            x = parseExpression();
            eat(')');
        } else if ((ch >= '0' && ch <= '9') || ch == '.') { // numbers
            while ((ch >= '0' && ch <= '9') || ch == '.')
                nextChar();
            double number = Double.parseDouble(str.substring(startPos, this.pos));
            x = () -> number;
        } else if (ch >= 'a' && ch <= 'z') { // functions & variables
            while (ch >= 'a' && ch <= 'z')
                nextChar();
            String func = str.substring(startPos, this.pos);
            if (variables.containsKey(func)) {
                x = () -> variables.get(func).get();
            } else {
                x = parseFactor();
                Expression a = x;
                switch (func) {
                    case "sqrt":
                        x = () -> Math.sqrt(a.eval());
                        break;
                    case "sin":
                        x = () -> Math.sin(Math.toRadians(a.eval()));
                        break;
                    case "cos":
                        x = () -> Math.cos(Math.toRadians(a.eval()));
                        break;
                    case "tg":
                        x = () -> Math.tan(Math.toRadians(a.eval()));
                        break;
                    case "ctg":
                        x = () -> 1 / Math.tan(Math.toRadians(a.eval()));
                        break;
                    case "ln":
                        x = () -> Math.log(a.eval());
                        break;
                    default:
                        throw new RuntimeException("Unknown function: " + func);
                }
            }
        } else {
            throw new RuntimeException("Unexpected: " + (char)ch);
        }

        if (eat('^')) { // exponentiation
            Expression a = x, b = parseFactor();
            x = () -> Math.pow(a.eval(), b.eval());
        }
        return x;
    }
}
