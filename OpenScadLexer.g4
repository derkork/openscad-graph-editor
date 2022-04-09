lexer grammar OpenScadLexer;


INCLUDE: 'include' [\t ]* '<' -> pushMode(FILE_IMPORT_MODE);
USE: 'use' [\t ]* '<' -> pushMode(FILE_IMPORT_MODE);

// operators
ADD : '+';
SUBTRACT : '-';
MULTIPLY : '*';
DIVIDE : '/';
MODULUS : '%';
NOT : '!';
EXPONENTIATE : '^';
EQUAL : '==';
NOT_EQUAL : '!=';
LESS_THAN : '<';
LESS_THAN_OR_EQUAL : '<=';
GREATER_THAN : '>';
GREATER_THAN_OR_EQUAL : '>=';
AND : '&&';
OR : '||';
TERNARY: '?';
COLON: ':';
DOT: '.';

STATEMENT_TERMINATOR: ';'; 
ASSIGNMENT_OPERATOR: '=';

BLOCK_START: '{';
BLOCK_END: '}';
VECTOR_START: '[' ;
VECTOR_END: ']' ;
OPEN_PAREN: '(';
CLOSE_PAREN: ')';
COMMA: ',';
HASH: '#';

// keywords
BOOLEAN : ('true' | 'false');
UNDEF: 'undef';
IF: 'if';
ELSE: 'else';
FUNCTION: 'function';
MODULE: 'module';
FOR: 'for';
LET: 'let';
EACH: 'each';
INTERSECTION_FOR: 'intersection_for';


IDENTIFIER: '$'?[a-zA-Z_][a-zA-Z0-9_]*;
 

WS  : (' '|'\t'|'\r'|'\n')+ -> skip;
 
STRING : '"' .*? '"';

NUMBER  
    : ( [0-9]* '.' )? [0-9]+;
    
   
BLOCK_COMMENT
    :   '/*' .*? '*/' -> skip
    ;

LINE_COMMENT
    :   '//' ~[\r\n]* -> skip
    ;
    
mode FILE_IMPORT_MODE;
    END_IMPORT: '>' -> popMode, skip;
    PATH_STRING: ~[>]+;

