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


parameterList
    : OPEN_PAREN parameterDeclaration (COMMA parameterDeclaration)* CLOSE_PAREN 
    | OPEN_PAREN CLOSE_PAREN ;

parameterDeclaration
    : identifier (ASSIGNMENT_OPERATOR expression)?;

variableDeclaration
    : identifier ASSIGNMENT_OPERATOR expression STATEMENT_TERMINATOR;

functionDeclaration
    : FUNCTION identifier parameterList ASSIGNMENT_OPERATOR expression STATEMENT_TERMINATOR;

moduleDeclaration
    : MODULE identifier parameterList block;

invocationParameterList
    : OPEN_PAREN (invocationParameter (COMMA invocationParameter)*)? CLOSE_PAREN 
    | OPEN_PAREN CLOSE_PAREN;

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
    
    
    
includeDeclaration: INCLUDE PATH_STRING STATEMENT_TERMINATOR;
useDeclaration: USE PATH_STRING STATEMENT_TERMINATOR;
 
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
    : (VECTOR_START vectorInner (COMMA vectorInner)* COMMA* VECTOR_END) // funny enough the parser allows for trailing commas 
    | (VECTOR_START VECTOR_END);
    
    
comprehensionForLoop
    : FOR OPEN_PAREN invocationParameter (COMMA invocationParameter)* ( STATEMENT_TERMINATOR  expression STATEMENT_TERMINATOR invocationParameter (COMMA invocationParameter)* )? CLOSE_PAREN;

vectorInner
    : IF OPEN_PAREN expression CLOSE_PAREN vectorInner (ELSE vectorInner)?
    | LET invocationParameterList vectorInner
    | EACH vectorInner
    | comprehensionForLoop vectorInner 
    | expression;  

rangeExpression
    : VECTOR_START expression COLON expression (COLON expression)? VECTOR_END;

functionInvocation
    : identifier invocationParameterList;

variableReference
    : identifier;



unaryOperator
    : SUBTRACT
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




