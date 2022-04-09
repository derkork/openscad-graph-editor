/*
 * Grammar for parsing OpenScad files. This may be a bit more lenient than the built-in parser
 * but this is only used to find out which functions, modules and variables are defined in 
 * the file.
 */
parser grammar OpenSCAD;

options { tokenVocab=OpenSCADLexer; }

scadFile:
   moduleContent
    ;
    
// Everything that can be inside of a module
moduleContent:
    (variableDeclaration 
    | functionDeclaration 
    | moduleDeclaration 
    | includeDeclaration
    | useDeclaration
    | block)*;



parameterList:  ( OPEN_PAREN parameterDeclaration (COMMA parameterDeclaration)* CLOSE_PAREN | OPEN_PAREN CLOSE_PAREN ) ;
parameterDeclaration: identifier (ASSIGNMENT_OPERATOR expression)?;

variableDeclaration: identifier ASSIGNMENT_OPERATOR expression STATEMENT_TERMINATOR;
functionDeclaration: FUNCTION identifier parameterList ASSIGNMENT_OPERATOR expression STATEMENT_TERMINATOR;
moduleDeclaration: MODULE identifier parameterList BLOCK_START moduleContent BLOCK_END;

invocationParameterList: (OPEN_PAREN (invocationParameter (COMMA invocationParameter)*)? CLOSE_PAREN | OPEN_PAREN CLOSE_PAREN );
invocationParameter: (identifier ASSIGNMENT_OPERATOR expression | expression);

moduleInvocation: treeModifier identifier invocationParameterList block;
treeModifier:
    (MODULUS | HASH | NOT | MULTIPLY)?;

conditionalBlock: IF OPEN_PAREN expression CLOSE_PAREN block (ELSE block)?; 

forLoop: FOR invocationParameterList block;
intersectionForLoop: INTERSECTION_FOR invocationParameterList block;
    
block:
    BLOCK_START moduleContent BLOCK_END
    | moduleInvocation
    | conditionalBlock
    | forLoop
    | intersectionForLoop
    | STATEMENT_TERMINATOR
    ;
    
    
    
includeDeclaration: INCLUDE PATH_STRING STATEMENT_TERMINATOR;
useDeclaration: USE PATH_STRING STATEMENT_TERMINATOR;
 
expression
    : simpleExpression
    | vectorExpression
    | functionInvocation
    | unaryOperator expression
    | expression binaryOperator expression
    | OPEN_PAREN expression CLOSE_PAREN
    | variableReference
    | expression TERNARY expression COLON expression
    | rangeExpression
    ;

vectorExpression: (VECTOR_START expression (COMMA expression)* VECTOR_END) | (VECTOR_START VECTOR_END);
rangeExpression: VECTOR_START expression COLON expression (COLON expression)? VECTOR_END;
functionInvocation: identifier invocationParameterList;
variableReference: identifier;



unaryOperator:
    SUBTRACT
    | NOT
    ;
     
binaryOperator:
    | ADD
    | SUBTRACT
    | MULTIPLY
    | DIVIDE
    | MODULUS
    | EXPONENTIATE
    | LESS_THAN
    | LESS_THAN_OR_EQUAL
    | GREATER_THAN
    | GREATER_THAN_OR_EQUAL
    | EQUAL
    | NOT_EQUAL
    | AND
    | OR
    ;
    

simpleExpression
    : STRING 
    | NUMBER
    | BOOLEAN
    | UNDEF
    ;


identifier:
    IDENTIFIER;




