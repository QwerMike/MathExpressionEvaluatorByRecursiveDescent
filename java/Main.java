public class Main {
    public static void main(String[] args) {

        Variable x = new Variable(0.0);
        Variable y = new Variable(0.0);
        Expression expression =
                new ExpressionBuilder("-x^y")
                        .addVariable("x", x)
                        .addVariable("y", y)
                        .build();
        x.set(1);
        y.set(2);
        System.out.println(
            expression.eval());
    }
}
