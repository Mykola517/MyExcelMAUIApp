grammar LabCalculator;

compileUnit : expression EOF;

expression :
      LPAREN expression RPAREN                                        #ParenthesizedExpr
    | INC  LPAREN expression RPAREN                                   #IncExpr
    | DEC  LPAREN expression RPAREN                                   #DecExpr
    | MAX  LPAREN expression COMMA expression RPAREN                  #MaxExpr
    | MIN  LPAREN expression COMMA expression RPAREN                  #MinExpr
    | MMAX LPAREN paramlist=arglist RPAREN                            #MmaxExpr
    | MMIN LPAREN paramlist=arglist RPAREN                            #MminExpr
    | expression operatorToken=(MULTIPLY | DIVIDE) expression         #MultiplicativeExpr
    | expression operatorToken=(ADD | SUBTRACT) expression            #AdditiveExpr
    | NUMBER                                                          #NumberExpr
    | IDENTIFIER                                                      #IdentifierExpr
    ;

arglist : expression (COMMA expression)* ;

INC  : 'inc';
DEC  : 'dec';
MAX  : 'max';
MIN  : 'min';
MMAX : 'mmax';
MMIN : 'mmin';

NUMBER : INT;
INT : ('0'..'9')+;

IDENTIFIER : [a-zA-Z]+[1-9][0-9]*;

MULTIPLY : '*';
DIVIDE   : '/';
SUBTRACT : '-';
ADD      : '+';
LPAREN   : '(';
RPAREN   : ')';
COMMA    : ',';

WS : [ \t\r\n] -> channel(HIDDEN);

