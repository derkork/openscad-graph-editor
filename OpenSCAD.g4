grammar OpenSCAD;

query
    : expression
    ;
 
expression
    : STRING
    | NUMBER
    | expression 'AND' expression
    | expression 'OR' expression
    ;
 
WS  : (' '|'\t'|'\r'|'\n')+ -> skip;
 
STRING : '"' .*? '"';
SIGN
   : ('+' | '-')
   ;
NUMBER  
    : SIGN? ( [0-9]* '.' )? [0-9]+;