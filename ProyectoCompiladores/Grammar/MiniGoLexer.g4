lexer grammar MiniGoLexer;
                         //Reservadas
                         Package : 'package';
                         Import : 'import';
                         Var: 'var';
                         Type: 'type';
                         Struct: 'struct';
                         Append: 'append';
                         Len: 'len';
                         Cap: 'cap';
                         Print: 'print';
                         PrintLn: 'println';
                         Return: 'return';
                         Break: 'break';
                         Continue: 'continue';
                         If: 'if';
                         Func: 'func';
                         Else: 'else';
                         For: 'for';
                         Switch: 'switch';
                         Case: 'case';
                         Default: 'default';
                         
                         //Simbolos
                         Comilla: '\'';
                         LPAREN : '(';
                         RPAREN : ')';
                         SEMICOLON : ';';
                         Asign: '=';
                         Coma: ',';
                         Puntos: ':';
                         CorcheteD: '[';
                         CorcheteI: ']';
                         LlaveD: '{';
                         LlaveI: '}';
                         Not :'!';
                         Asterisco: '*';
                         Slash: '/';
                         Porcentaje: '%';
                         CorrimientoIzquierda: '<<';
                         CorrimientoDerecha: '>>';
                         And: '&';
                         AndNot: '&^';
                         Mas: '+';
                         Menos: '-';
                         Or: '|';
                         DisyuncionExclusiva: '^' ;
                         Equal: '==';
                         NotEqual: '!=';
                         Mayor: '>';
                         Menor: '<';
                         MenorIgual: '<=';
                         MayorIgual: '>=';
                         Land: '&&';
                         LOR: '||';
                         Punto: '.';
                         Incremento: '++';
                         Decremento: '--';
                         Definir: ':=';
                         SumaIzquierda: '+=';
                         RestaIzquierda: '-=';
                         AndValores: '&=';
                         OrValores: '|=';
                         MultiIzquierda: '*=';
                         AsignacionExclusiva: '^=';
                         AsignCorrimientoIzquierda: '<<=';
                         AsignCorrimientoDerecha: '>>=';
                         AndExclusivo: '&^=';
                         RestarPorcentaje: '%=';
                         DivisionIzquierda: '/=';
                         Lower: '_';
                         
                         NewLine : [\r\n]+ -> skip;
                         WS  :   [ \t]+ -> skip ;
                         Comments : ('//' .*? [\r\n] | '/*' .*?  '*/') -> skip;
                                                                           
                                                  
                         
                         Identifier : Letter ( Letter | Unicode_Digit )*;                         
                         Letter    :  (Unicode_Letter | Lower);
                                                  
                         Int_lit         : Decimal_lit | Binary_lit | Octal_lit | Hex_lit;
                         Decimal_lit     : Unicode_Digit ((Lower)? Decimal_digits)? ;
                         Binary_lit      : '0' ( 'b' | 'B' )  (Lower)? Binary_digits;
                         Octal_lit       : '0' ( 'o' | 'O' )?  (Lower)? Octal_digits;
                         Hex_lit         : '0' ( 'x' | 'X' )  (Lower)? Hex_digits;
                         
                         Decimal_digits : Decimal_Digit ((Lower)?  Decimal_Digit)*;
                         Binary_digits  : Binary_digit ((Lower)? Binary_digit )* ;
                         Octal_digits   : Octal_digit  ((Lower)? Octal_digit )* ;
                         Hex_digits     : Hex_digit ((Lower)? Hex_digit )*;
                         

                         
                         //
                         Float_Lit        : Decimal_Float_Lit | Hex_Float_List ;
                         Decimal_Float_Lit : Decimal_digits Punto ( Decimal_digits ) ( Decimal_Exponent )? |
                                              Decimal_digits Decimal_Exponent |
                                              Punto Decimal_digits ( Decimal_Exponent )? ;
                         Decimal_Exponent  : ( 'e' | 'E' ) ( '+' | '-' )?  Decimal_digits;
                          
                         Hex_Float_List    : '00' ( 'x' | 'X' ) Hex_Mantissa Hex_Exponent ;
                         Hex_Mantissa    : (Lower)? Hex_digits Punto ( Hex_digits )? |
                                              (Lower)? Hex_digits |
                                              Punto Hex_digits ;
                         Hex_Exponent     : ( 'p' | 'P' ) ( '+' | '-' )? Decimal_digits ;
                         
                         //
                        
                         Raw_String_Lit         : '`' (Unicode_Char | NewLine )* '`' ;
                         Interpreted_String_Lit : '"' ( Unicode_Value | Byte_Value )* '"';
                         Rune_Lit         : '\'' ( Unicode_Value | Byte_Value ) '\'' ;
                         Unicode_Value    : Unicode_Char | Little_U_Value | Big_U_Value | Escaped_Char|' ' ;                       
                         Byte_Value       : Octal_Byte_Value | Hex_Byte_Value ;
                         Octal_Byte_Value : '\\' Octal_digit Octal_digit Octal_digit ;
                         Hex_Byte_Value   : '\\' 'x' Hex_digit Hex_digit  ;
                         Little_U_Value   : '\\' 'u' Hex_digit Hex_digit Hex_digit Hex_digit ;
                         Big_U_Value       : '\\' 'U' Hex_digit Hex_digit Hex_digit Hex_digit
                                             Hex_digit Hex_digit Hex_digit Hex_digit ;
                         String_Lit             : Raw_String_Lit | Interpreted_String_Lit ;                         
                         Unicode_Digit  : Decimal_Digit(Decimal_Digit)* | Decimal_Digit(Decimal_Digit)*Punto(Decimal_Digit)*;

                   
                         
                         fragment Binary_digit   : '0' | '1'  ;
                         fragment Octal_digit    : '0' .. '7' ;    
                         fragment Unicode_Char		: '\u0000'..'\u0009' | '\u000B'..'\u{10FFFF}'; 
                         
                         Unicode_Letter : 'a'..'z'|'A'..'Z';
                         fragment Escaped_Char     : '\\' ( 'a' | 'b' | 'f' | 'n' | 'r' | 't' | 'v' | '\\' | '\'' | '"' );
                         fragment Hex_digit      : '0' .. '9' | 'A' .. 'F' | 'a' .. 'f';
                         fragment Decimal_Digit : '0' .. '9';
                         
                         //Comment : '/*' .*? '*/' -> skip;  