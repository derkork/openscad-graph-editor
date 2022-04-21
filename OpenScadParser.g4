/*
 * Grammar for parsing OpenScad files. This may be a bit more lenient than the built-in parser
 * but this is only used to find out which functions, modules and variables are defined in 
 * the file.
 */
parser grammar OpenScadParser;

options { tokenVocab=OpenScadLexer; }

scadFile:
   moduleContent
    ;
    
// Everything that can be inside of a module
moduleContent
    : (variableDeclaration | functionDeclaration | moduleDeclaration | includeDeclaration | useDeclaration | block)*;


// the scad parser seems to be lenient about commas, so where ever a comma can be there can also be
// multiple of them and they will just be treated as a single comma. Also trailing commas seem to be no
// problem either.
comma
    : COMMA+
    ;
    
commaMaybe
    : COMMA*
    ;    

parameterList
    : OPEN_PAREN parameterDeclaration (comma parameterDeclaration)* commaMaybe CLOSE_PAREN  
    | OPEN_PAREN commaMaybe CLOSE_PAREN ;

parameterDeclaration
    : identifier (ASSIGNMENT_OPERATOR expression)?;

variableDeclaration
    : identifier ASSIGNMENT_OPERATOR expression STATEMENT_TERMINATOR;

functionDeclaration
    : FUNCTION identifier parameterList ASSIGNMENT_OPERATOR expression STATEMENT_TERMINATOR;

moduleDeclaration
    : MODULE identifier parameterList block;

invocationParameterList
    : OPEN_PAREN (invocationParameter (comma invocationParameter)*)? commaMaybe CLOSE_PAREN 
    | OPEN_PAREN commaMaybe CLOSE_PAREN;

invocationParameter
    : (identifier ASSIGNMENT_OPERATOR expression | expression);

moduleInvocation
    : treeModifier identifier invocationParameterList block;

treeModifier
    : (MODULUS | HASH | NOT | MULTIPLY)?;

conditionalBlock
    : IF OPEN_PAREN expression CLOSE_PAREN block (ELSE block)?; 

letBlock
    : LET invocationParameterList block;

forLoop
    : FOR invocationParameterList block;

intersectionForLoop
    : INTERSECTION_FOR invocationParameterList block;

assertionBlock
    : ASSERT invocationParameterList block;
    
echoInvocation
    : ECHO invocationParameterList STATEMENT_TERMINATOR;    
    
block
    : BLOCK_START moduleContent BLOCK_END
    | moduleInvocation
    | conditionalBlock
    | forLoop
    | letBlock
    | intersectionForLoop
    | assertionBlock
    | echoInvocation
    | STATEMENT_TERMINATOR
    ;
    
    
    
includeDeclaration: INCLUDE PATH_STRING;
useDeclaration: USE PATH_STRING;
 
expression
    : simpleExpression
    | LET invocationParameterList expression
    | vectorExpression
    | functionInvocation
    | unaryOperator expression
    | parenthesizedExpression
    | variableReference
    | expression TERNARY expression COLON expression
    | rangeExpression
    | vectorIndexExpression
    | expression binaryOperator expression
    | lambdaExpression
    | assertionExpression
    | echoExpression
    ;

assertionExpression
    : ASSERT invocationParameterList expression?;
    

echoExpression
    : ECHO invocationParameterList expression?;

lambdaExpression
    : FUNCTION parameterList expression;

parenthesizedExpression
    : OPEN_PAREN expression CLOSE_PAREN;

vectorIndexExpression
    : ( identifier | parenthesizedExpression | vectorExpression | functionInvocation ) (DOT identifier | VECTOR_START expression VECTOR_END)+;

vectorExpression
    : (VECTOR_START vectorInner (comma vectorInner)* commaMaybe VECTOR_END) 
    | (VECTOR_START commaMaybe VECTOR_END);
    
    
comprehensionForLoop
    : FOR OPEN_PAREN invocationParameter (comma invocationParameter)* ( STATEMENT_TERMINATOR  expression STATEMENT_TERMINATOR invocationParameter (comma invocationParameter)* commaMaybe)? CLOSE_PAREN;

vectorInner
    : IF OPEN_PAREN expression CLOSE_PAREN vectorInner (ELSE vectorInner)?
    | LET invocationParameterList vectorInner
    | EACH vectorInner
    | comprehensionForLoop vectorInner 
    | parenthesizedVectorInner
    | expression;  
    
parenthesizedVectorInner
    : OPEN_PAREN vectorInner CLOSE_PAREN;    

rangeExpression
    : VECTOR_START expression COLON expression (COLON expression)? VECTOR_END;

functionInvocation
    : identifier invocationParameterList;

variableReference
    : identifier;



unaryOperator
    : SUBTRACT
    | ADD
    | NOT
    ;
     
binaryOperator
    : ADD
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




