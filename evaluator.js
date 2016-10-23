class Expression {
  constructor(f) {
    this.f = f;
  }

  eval() { 
    return this.f()
  };
}

class Variable {
  constructor(value) {
    this.value = value;
  }

  get() {
    return this.value;
  }

  set(value) {
    this.value = value;
  }

  toString() {
    return value.toString();
  }
}

class ExpressionBuilder {
  constructor(toParse) {
    this.pos = -1;
    this.ch = null;
    this.str = toParse;
    this.variables = {};
  }

  addVariable(name, variable) {
    this.variables[name]= variable;
    return this;
  }

  build() {
    this.nextChar();
    let x = this.parseExpression();
    if (this.pos < this.str.length) {
      throw "Unexpected: " + this.ch;
    }
    return x;
  }

  get expression() {
    return this.str;
  }
  
  nextChar() {
    this.ch = (++this.pos < this.str.length) ? this.str.charAt(this.pos) : -1;
  }

  eat(charToEat) {
    while (this.ch == ' ') this.nextChar();
    if (this.ch == charToEat) {
      this.nextChar();
      return true;
    }
    return false;
  }

    // Grammar:
    // expression = term | expression `+` term | expression `-` term
    // term = factor | term `*` factor | term `/` factor
    // factor = `+` factor | `-` factor | `(` expression `)`
    //        | number | functionName factor | factor `^` factor

  parseExpression() {
    let x = this.parseTerm();
    while (true) {
      if (this.eat('+')) { // addition
        let a = x, b = this.parseTerm();
        x = new Expression(() => a.eval() + b.eval());
      } else if (this.eat('-')) { // subtraction
        let a = x, b = this.parseTerm();
        x = new Expression(() => a.eval() - b.eval());
      }
      else return x;
    }
  }

  parseTerm() {
    let x = this.parseFactor();
    while (true) {
      if (this.eat('*')) { // multiplication
        let a = x, b = this.parseFactor();
        x = new Expression(() => a.eval() * b.eval());
      } else if (this.eat('/')) { // division
        a = x, b = this.parseFactor();
        x = new Expression(() => a.eval() / b.eval());
      }
      else return x;
    }
  }

  parseFactor() {
    if (this.eat('+')) return this.parseFactor(); // unary plus
    if (this.eat('-')) {
      let a = this.parseFactor();
      return new Expression(() => -a.eval());
    } // unary minus

    let x = null;
    let startPos = this.pos;

    if (this.eat('(')) { // parentheses
      x = this.parseExpression();
      this.eat(')');
    } else if ((this.ch >= '0' && this.ch <= '9') || this.ch == '.') { // numbers
      while ((this.ch >= '0' && this.ch <= '9') || this.ch == '.')
        this.nextChar();
      let number = parseFloat(this.str.substring(startPos, this.pos));
      x = new Expression(() => number);
    } else if (this.ch >= 'a' && this.ch <= 'z') { // functions & variables
      while (this.ch >= 'a' && this.ch <= 'z')
        this.nextChar();
      let func = this.str.substring(startPos, this.pos);
      if (this.variables[func]) {
        x = new Expression(() => this.variables[func].get());
      } else {
        x = this.parseFactor();
        let a = x;
        switch (func) {
          case "sqrt":
            x = new Expression(() => Math.sqrt(a.eval()));
            break;
          case "sin":
            x = new Expression(() => Math.sin(Math.toRadians(a.eval())));
            break;
          case "cos":
            x = new Expression(() => Math.cos(Math.toRadians(a.eval())));
            break;
          case "tg":
            x = new Expression(() => Math.tan(Math.toRadians(a.eval())));
            break;
          case "ctg":
            x = new Expression(() => 1 / Math.tan(Math.toRadians(a.eval())));
            break;
          case "ln":
            x = new Expression(() => Math.log(a.eval()));
            break;
          default:
            throw "Unknown function: " + func;
        }
      }
    } else {
      throw "Unexpected: " + this.ch;
    }

    if (this.eat('^')) { // exponentiation
      let a = x, b = this.parseFactor();
      x = new Expression(() => Math.pow(a.eval(), b.eval()));
    }
    return x;
  }
}
