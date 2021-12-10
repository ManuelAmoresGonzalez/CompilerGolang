parser grammar MiniGoParser;

options{
    tokenVocab = MiniGoLexer;
}


root : Package Identifier SEMICOLON topDeclarationList                                                  #main                         ;

topDeclarationList : (  variableDecl | typeDecl | funcDecl )*                                           #estructuraProyecto           ;

variableDecl :  Var singleVarDecl SEMICOLON                                                             #variableDeclFormal           | 
                Var LPAREN innerVarDecls RPAREN SEMICOLON                                               #variableDeclCombinada        |
                Var  LPAREN RPAREN SEMICOLON                                                            #variableDeclVacia            ; 
                
innerVarDecls : singleVarDecl SEMICOLON (singleVarDecl  SEMICOLON)*                                     #declararVariablesInternas    ;

singleVarDecl : identifierList declType Asign expressionList                                            #singleVarDeclAsign           |  
			    identifierList Asign expressionList                                                     #singleVarDeclAsignExpL       |
			    singleVarDeclNoExps                                                                     #singleVarDeclNull            ;

singleVarDeclNoExps	: identifierList declType                                                           #asignarVariableSinValor      ;

typeDecl	: Type singleTypeDecl SEMICOLON                                                             #typeDeclUnico                |               
			  Type LPAREN innerTypeDecls RPAREN SEMICOLON                                               #typeDeclVarios               | 
			  Type LPAREN RPAREN SEMICOLON                                                              #typeDeclVacio                ;
			 
innerTypeDecls		: singleTypeDecl SEMICOLON (singleTypeDecl SEMICOLON)*                              #declaracionInterna           ;

singleTypeDecl		: Identifier declType                                                               #identificadorTipo            ;

funcDecl		: funcFrontDecl block SEMICOLON                                                         #estructuraFuncion            ;

funcFrontDecl		: Func Identifier LPAREN (funcArgDecls| ) RPAREN (declType| )                       #declararFuncion              ;

funcArgDecls		: singleVarDeclNoExps (Coma singleVarDeclNoExps)*                                   #parametrosFuncion            ;	
    
declType	: LPAREN declType RPAREN                                                                    #declTypeParentesis           |	
			  Identifier                                                                                #declTypeIdentificador        |
			  sliceDeclType                                                                             #declTypeDeclaracionSimple    |
			  arrayDeclType                                                                             #declTypeDeclaracionArreglo   |
			  structDeclType                                                                            #declTypeEstructuraDecTipo    ;
			
sliceDeclType  : CorcheteD CorcheteI declType                                                           #declaracionVacia             ;

arrayDeclType  : CorcheteD Int_lit CorcheteI declType                                                   #declaracionInt               ;

structDeclType : Struct LlaveD (structMemDecls| ) LlaveI                                                #estructuraLlaves             ;

structMemDecls : singleVarDeclNoExps SEMICOLON (singleVarDeclNoExps SEMICOLON)*                         #partesCodigo                 ;

identifierList : Identifier (Coma Identifier)*                                                          #listarIdentificadores        ;

expression	: primaryExpression                                                                         #expressionPrimaryExpression  |
              expression  Asterisco expression                                                          #expressionAsterisco          |
              expression Slash expression                                                               #expressionSlash              |
              expression Porcentaje expression                                                          #expressionPorcentaje         |
              expression CorrimientoIzquierda expression                                                #expressionCorrimientoIzqui   | 
              expression CorrimientoDerecha expression                                                  #expressionCorrimientoDerecha |
              expression And expression                                                                 #expressionAnd                |
              expression AndNot expression                                                              #expressionAndNOt             |
              expression Mas expression                                                                 #expressionMas                |
			  expression Menos expression	                                                            #expressionMenos              |
			  expression Or expression                                                                  #expressionOr                 |
			  expression DisyuncionExclusiva expression	                                                #expressionDisyuncionExclusi  |
			  expression Equal expression                                                               #expressionEqual              |
			  expression NotEqual expression                                                            #expressionNotEqual           |
			  expression Menor expression                                                               #expressionMenor              |
			  expression MenorIgual expression                                                          #expressionMenorIgual         |
			  expression Mayor expression                                                               #expressionMayor              |
			  expression MayorIgual expression                                                          #expressionMayorIgual         |
			  expression Land expression                                                                #expressionLand               |
			  expression LOR expression                                                                 #expressionLor                |
			  Mas expression                                                                            #masExpression                |
			  Menos expression                                                                          #menosExpression              |
			  Not expression                                                                            #notExpression                |
			  DisyuncionExclusiva expression                                                            #disyuncionExpression         ;

expressionList		: expression (Coma expression)*                                                     #expressionListExpression     ;

primaryExpression	: operand								                                            #primaryExpressionOperand     |
                      primaryExpression selector                                                        #primaryExpressionSelector    |
                      primaryExpression index                                                           #primaryExpressionIndex       |                   
                      primaryExpression arguments                                                       #primaryExpressionArguments   |
                      appendExpression                                                                  #primaryExpressionAppend      |    
                      lengthExpression                                                                  #primaryExpressionLenght      |
                      capExpression                                                                     #primaryExpressionCap         ;
			
operand		: literal                                                                                   #operandoLiteral              |									
			  Identifier                                                                                #operandoIdentificador        |
			  LPAREN expression RPAREN                                                                  #operandoExpresion            ;
			
literal		: Int_lit								                                                    #literalLit                   |
			  Float_Lit							                                                        #literalFloat                 |
			  Rune_Lit							                                                        #literalRune                  |
			  Raw_String_Lit	                                                                        #literalRaw                   |	                 				 
			  Interpreted_String_Lit                                                                    #literalInterpreted           ;
							                                                                                                            
index			: CorcheteD expression CorcheteI                                                        #indexTipo                    ;

arguments		: LPAREN (expressionList | ) RPAREN                                                     #argumentos                   ;

selector		: Punto Identifier                                                                      #selectorPunto                ;

appendExpression	: Append LPAREN expression Coma expression RPAREN                                   #expresionAppend              ;

lengthExpression	: Len LPAREN expression RPAREN                                                      #expresionLen                 ;

capExpression		: Cap LPAREN expression RPAREN                                                      #expresionCap                 ;

statementList 		: statement*                                                                        #listaDeclaraciones           ;

block 			: LlaveD statementList LlaveI                                                           #estructuraBlock              ;

statement	: Print LPAREN (expressionList | ) RPAREN SEMICOLON                                         #funcionImprimir              |
			  PrintLn LPAREN (expressionList | Raw_String_Lit) RPAREN SEMICOLON                         #funcionImprimirLn            |
			  Return (expression | ) SEMICOLON                                                          #funcionRetornar              |
			  Break SEMICOLON                                                                           #funcionBreak                 |
			  Continue SEMICOLON                                                                        #funcionContinuar             |
			  simpleStatement SEMICOLON                                                                 #expresionSimple              |
			  block SEMICOLON                                                                           #funcionBlock                 |
			  switch SEMICOLON                                                                          #funcionSwitch                |
			  ifStatement SEMICOLON                                                                     #funcionIf                    |
			  loop SEMICOLON                                                                            #funcionLoop                  |
			  typeDecl                                                                                  #declararTipos                |
			  variableDecl                                                                              #declararVariables            ;
			
simpleStatement	:                                                                                       #funcionEpsilon1              |
                     expression ( Incremento | Decremento | )                                           #decrementoIncremento         |
                     assignmentStatement                                                                #declararAsignacion           |
                     expressionList Definir expressionList                                              #sentenciaDefinir             ;
			
assignmentStatement         :expressionList Asign expressionList                                        #primerAsignacion             |
			                 expression SumaIzquierda expression                                        #segundaAsignacion            |
                             expression AndValores expression                                           #tercerAsignacion             |
                             expression RestaIzquierda expression                                       #cuartaAsignacion             | 
                             expression OrValores expression                                            #quintaAsignacion             | 
                             expression MultiIzquierda expression                                       #sextaAsignacion              |  
                             expression AsignacionExclusiva expression                                  #setimaAsignacion             |
                             expression AsignCorrimientoIzquierda expression                            #octavaAsignacion             | 
                             expression AsignCorrimientoDerecha expression                              #novenaAsignacion             |
                             expression AndExclusivo expression                                         #decimaAsignacion             |
                             expression RestarPorcentaje expression                                     #undecimaAsignacion           |
                             expression DivisionIzquierda expression                                    #duodecimaAsignacion          ;
			
ifStatement 	: If expression block                                                                   #primerIf                     |
                  If expression block Else ifStatement                                                  #segundoIf                    |
                  If expression block Else block                                                        #tercerIf                     |
                  If simpleStatement  SEMICOLON expression block                                        #cuartoIf                     |
                  If simpleStatement SEMICOLON  expression block Else ifStatement                       #quintoIf                     |
                  If simpleStatement  SEMICOLON expression block Else block                             #sextoIf                      ;
			
			
loop	    	: For block                                                                             #primerFor                    |
                  For expression block                                                                   #segundoFor                  |
                  For simpleStatement SEMICOLON expression SEMICOLON simpleStatement block              #tercerFor                    |
                  For simpleStatement SEMICOLON SEMICOLON simpleStatement block                         #cuartoFor                    ; 
			
switch		    : Switch simpleStatement SEMICOLON expression LlaveD expressionCaseClauseList LlaveI    #primerSwitch                 | 
			      Switch expression LlaveD expressionCaseClauseList LlaveI                              #segundoSwitch                |
			      Switch simpleStatement SEMICOLON LlaveD expressionCaseClauseList LlaveI               #tecerSwitch                  |
			      Switch LlaveD expressionCaseClauseList LlaveI                                         #cuartoSwitch                 ; 
			 
expressionCaseClauseList :                                                                              #funcionEpsilon               |
			                expressionCaseClause expressionCaseClauseList                               #expresionCasoClausula        ;
			
expressionCaseClause 	: expressionSwitchCase Puntos statementList                                     #expresionSwitch              ;

expressionSwitchCase :  Case expressionList                                                             #expresionCase                |
			            Default                                                                         #expresionDefault             ;






