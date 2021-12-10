using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Generated;
using LLVMSharp.Interop;

namespace ProyectoCompiladores.Contextual
{
    public class ContextualCode: MiniGoParserBaseVisitor<Object>
    {
       public bool isGlobal = true;
       public int typeFunction = 0;
       public int contador =0;
       public string funcionActual = "";
        private static readonly LLVMValueRef NullValue = new LLVMValueRef(IntPtr.Zero);

        private readonly LLVMModuleRef module;

        private readonly LLVMBuilderRef builder;

        private readonly Dictionary<string, LLVMValueRef> namedValues = new Dictionary<string, LLVMValueRef>();

        private readonly Stack<LLVMValueRef> valueStack = new Stack<LLVMValueRef>();
        public Stack<LLVMValueRef> ResultStack { get { return valueStack; } }

        public ContextualCode(LLVMModuleRef module, LLVMBuilderRef builder)
        {
            this.module = module;
            this.builder = builder;
        }
        public override object VisitMain(MiniGoParser.MainContext context)
        {
            Visit(context.topDeclarationList());
            return module.GetNamedFunction("main");
        }

        public override object VisitEstructuraProyecto(MiniGoParser.EstructuraProyectoContext context)
        {
           LLVMTypeRef[] param_types = new LLVMTypeRef[1] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) };
           LLVMTypeRef llvm_printf_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, param_types);
           module.AddFunction("puts", llvm_printf_type);
           param_types = new LLVMTypeRef[0] { };
           LLVMTypeRef ret_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, param_types);
           LLVMBasicBlockRef entry =  this.module.AddFunction("main", ret_type).AppendBasicBlock("entry");

           this.builder.PositionAtEnd(entry);

           builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0));
           // builder.BuildCall(print, a, "imprimir");
           foreach (IParseTree objeto in context.children)
           {
              Visit(objeto);
           }
           return null;
        }

   public override object VisitVariableDeclFormal(MiniGoParser.VariableDeclFormalContext context)
   {
      Visit(context.singleVarDecl());
      return null;
   }

   public override object VisitVariableDeclCombinada(MiniGoParser.VariableDeclCombinadaContext context)
   {
      return null;
   }

   public override object VisitVariableDeclVacia(MiniGoParser.VariableDeclVaciaContext context)
   {
      return null;
   }

   public override object VisitDeclararVariablesInternas(MiniGoParser.DeclararVariablesInternasContext context)
   {
      return null;
   }

   public override object VisitSingleVarDeclAsign(MiniGoParser.SingleVarDeclAsignContext context)
   {
      string expression;
      LLVMValueRef referencia;
      MiniGoParser.ExpressionContext[] variables = (MiniGoParser.ExpressionContext[])Visit(context.expressionList());
      String tipoVariable= (String) Visit(context.declType());
      ITerminalNode[] identificadores =(ITerminalNode[]) Visit(context.identifierList());
      String nombreVariable = (String) identificadores[0].GetText();
      if (isGlobal)
      {
         if (tipoVariable == "int")
         {
            expression = (string)Visit(variables[0]);
            int valueInt = Int32.Parse(expression);
            namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Int32, nombreVariable));
            namedValues.TryGetValue(nombreVariable, out referencia);
            referencia.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)valueInt, true);
         }

         if (tipoVariable == "float")
         {
            expression = (string)Visit(variables[0]);
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            double valueInt = Convert.ToDouble(expression, provider);
            namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Double, nombreVariable));
            namedValues.TryGetValue(nombreVariable, out referencia);
            referencia.Initializer = LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, valueInt);
         }
         
         if (tipoVariable == "rune")
         {
            namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Int8, nombreVariable));
         }
         if (tipoVariable == "bool")
         {
            expression = (string)Visit(variables[0]);
            if (expression == "true")
            {
               namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Int1, nombreVariable));
               namedValues.TryGetValue(nombreVariable, out referencia);
               referencia.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1, true);
            }
            else
            {
               namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Int1, nombreVariable));
               namedValues.TryGetValue(nombreVariable, out referencia);
               referencia.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0, true);
            }
         }
         
      }
      else
      {
         if (tipoVariable == "int")
         {
            expression = (string) Visit(variables[0]);
            int valueInt = Int32.Parse(expression);//creo valor parseado
            LLVMValueRef const1 = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)valueInt); 
            namedValues.Add(context.identifierList().GetText(), this.builder.BuildAlloca(LLVMTypeRef.Int32, context.identifierList().GetText()));
            namedValues.TryGetValue(context.identifierList().GetText(), out referencia);
            builder.BuildStore(const1, referencia);
         }

         if (tipoVariable == "float")
         {
            expression = (string)Visit(variables[0]);

            NumberFormatInfo provider = new NumberFormatInfo();

            provider.NumberDecimalSeparator = ".";
                    
            double val = Convert.ToDouble(expression, provider);
            LLVMValueRef const1 = LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, val);
            namedValues.Add(context.identifierList().GetText(), this.builder.BuildAlloca(LLVMTypeRef.Double, context.identifierList().GetText()));
            namedValues.TryGetValue(context.identifierList().GetText(), out referencia);
            builder.BuildStore(const1, referencia);
         }

         if (tipoVariable == "rune")
         {
            namedValues.Add(nombreVariable, this.builder.BuildAlloca(LLVMTypeRef.Int8, nombreVariable));
         }

         if (tipoVariable == "bool")
         {
            expression = (string) Visit(variables[0]);
            if (expression == "true")
            {
               LLVMValueRef const1 = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1);
               namedValues.Add(context.identifierList().GetText(), this.builder.BuildAlloca(LLVMTypeRef.Int1, context.identifierList().GetText()));
               namedValues.TryGetValue(context.identifierList().GetText(), out referencia);
               builder.BuildStore(const1, referencia);
            }
            else
            {
               LLVMValueRef const1 = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0);
               namedValues.Add(context.identifierList().GetText(), this.builder.BuildAlloca(LLVMTypeRef.Int1, context.identifierList().GetText()));
               namedValues.TryGetValue(context.identifierList().GetText(), out referencia);
               builder.BuildStore(const1, referencia);
            }
         }
      }
      return null;
   }

   public override object VisitSingleVarDeclAsignExpL(MiniGoParser.SingleVarDeclAsignExpLContext context)
   {
      return null;
   }

   public override object VisitSingleVarDeclNull(MiniGoParser.SingleVarDeclNullContext context)
   {
      Visit(context.singleVarDeclNoExps());
      return null;
   }

   public override object VisitAsignarVariableSinValor(MiniGoParser.AsignarVariableSinValorContext context)
   {
      String tipoVariable= (String) Visit(context.declType());
      ITerminalNode[] identificadores =(ITerminalNode[]) Visit(context.identifierList());
      String nombreVariable = (String) identificadores[0].GetText();
      if (isGlobal)
      {
         if (tipoVariable == "int")
         {
            namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Int32, nombreVariable));
         }

         if (tipoVariable == "float")
         {
            namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Double, nombreVariable));
         }
         
         if (tipoVariable == "rune")
         {
            namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Int8, nombreVariable));
         }
         if (tipoVariable == "bool")
         {
            namedValues.Add(nombreVariable,this.module.AddGlobal(LLVMTypeRef.Int1, nombreVariable));
         }
         
      }
      else
      {
         if (tipoVariable == "int")
         {
            namedValues.Add(nombreVariable,this.builder.BuildAlloca(LLVMTypeRef.Int32, nombreVariable));
         }

         if (tipoVariable == "float")
         {
            namedValues.Add(nombreVariable,this.builder.BuildAlloca(LLVMTypeRef.Double, nombreVariable));
         }
         
         if (tipoVariable == "rune")
         {
            namedValues.Add(nombreVariable,this.builder.BuildAlloca(LLVMTypeRef.Int8, nombreVariable));
         }
         if (tipoVariable == "bool")
         {
            namedValues.Add(nombreVariable,this.builder.BuildAlloca(LLVMTypeRef.Int1, nombreVariable));
         }
      }
      Visit(context.identifierList());
      Visit(context.declType());
      isGlobal = true;
      return null;
   }

   public override object VisitTypeDeclUnico(MiniGoParser.TypeDeclUnicoContext context)
   {
      return null;
   }

   public override object VisitTypeDeclVarios(MiniGoParser.TypeDeclVariosContext context)
   {
      return null;
   }

   public override object VisitTypeDeclVacio(MiniGoParser.TypeDeclVacioContext context)
   {
      return null;        
   }

   public override object VisitDeclaracionInterna(MiniGoParser.DeclaracionInternaContext context)
   {
      return null;        
   }

   public override object VisitIdentificadorTipo(MiniGoParser.IdentificadorTipoContext context)
   {
      return null;        
   }

   public override object VisitEstructuraFuncion(MiniGoParser.EstructuraFuncionContext context)
   {
      Antlr4.Runtime.Tree.TerminalNodeImpl valores =(Antlr4.Runtime.Tree.TerminalNodeImpl)Visit(context.funcFrontDecl() );
      LLVMValueRef value;
      namedValues.TryGetValue(valores.GetText(), out value);
      LLVMBasicBlockRef entry = value.AppendBasicBlock("entry");
      this.builder.PositionAtEnd(entry);
      Visit(context.block());
      if (this.typeFunction == 1)
      {
         builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0));
      }
      if (this.typeFunction == 2)
      {
         builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0));
      }
      if (this.typeFunction == 3)
      {
         builder.BuildRet(LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 0));
      }
      //Visit(context.funcFrontDecl());

      return null;        
   }
   public override object VisitDeclararFuncion(MiniGoParser.DeclararFuncionContext context)
   {
      String nombreFuncion = context.Identifier().GetText();
      String tipoFuncion = (String) Visit(context.declType());
      
      if (context.funcArgDecls() != null)
      {
         MiniGoParser.SingleVarDeclNoExpsContext[] atributos = (MiniGoParser.SingleVarDeclNoExpsContext[])Visit(context.funcArgDecls());
         List<LLVMTypeRef> param_types = new List<LLVMTypeRef>();
         for (int i = 0; i < atributos.Length; i++)
         {
            if ((atributos[i].GetText() + "").Contains("int"))
            {
               LLVMTypeRef val = (LLVMTypeRef)LLVMTypeRef.Int32;
               param_types.Add(val);
            }
         }
         LLVMTypeRef[] param_types2 = new LLVMTypeRef[param_types.Count];

         for (int i = 0; i < param_types.Count; i++)
         {
            param_types2[i] = param_types[i];
         }
         if (tipoFuncion == "int")
         {
            
            this.typeFunction = 1;
            LLVMTypeRef ret_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, param_types2);
            namedValues.Add(context.Identifier().GetText(), this.module.AddFunction(context.Identifier().GetText(), ret_type));
            funcionActual = context.Identifier().GetText();
         }
         if (tipoFuncion == "bool")
         {
            this.typeFunction = 2;
            LLVMTypeRef ret_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int1, param_types2);
            namedValues.Add(context.Identifier().GetText(), this.module.AddFunction(context.Identifier().GetText(), ret_type));
            funcionActual = context.Identifier().GetText();
         }
         if (tipoFuncion == "float")
         {
            this.typeFunction = 3;
            LLVMTypeRef ret_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Double, param_types2);
            namedValues.Add(context.Identifier().GetText(), this.module.AddFunction(context.Identifier().GetText(), ret_type));
            funcionActual = context.Identifier().GetText();
         }
         
      }
      else
      {
         var val = new LLVMTypeRef[0];
         if (tipoFuncion == "int")
         {
            this.typeFunction = 1;
            LLVMTypeRef ret_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, val);
            namedValues.Add(context.Identifier().GetText(), this.module.AddFunction(context.Identifier().GetText(), ret_type));
            funcionActual= context.Identifier().GetText();
         }
         if (tipoFuncion == "bool")
         {
            this.typeFunction = 2;
            LLVMTypeRef ret_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int1, val);
            namedValues.Add(context.Identifier().GetText(), this.module.AddFunction(context.Identifier().GetText(), ret_type));
            funcionActual = context.Identifier().GetText();
         }
         if (tipoFuncion == "float")
         {
            this.typeFunction = 3;
            LLVMTypeRef ret_type = LLVMTypeRef.CreateFunction(LLVMTypeRef.Double, val);
            namedValues.Add(context.Identifier().GetText(), this.module.AddFunction(context.Identifier().GetText(), ret_type));
            funcionActual = context.Identifier().GetText();
         }
      }
      return context.Identifier();        
   }

   public override object VisitParametrosFuncion(MiniGoParser.ParametrosFuncionContext context)
   {
      return context.singleVarDeclNoExps();        
   }

   public override object VisitDeclTypeParentesis(MiniGoParser.DeclTypeParentesisContext context)
   {
      return null;        
   }

   public override object VisitDeclTypeIdentificador(MiniGoParser.DeclTypeIdentificadorContext context)
   {
      return context.Identifier().GetText();
   }
   

   public override object VisitDeclTypeDeclaracionSimple(MiniGoParser.DeclTypeDeclaracionSimpleContext context)
   {
      return null;        }

   public override object VisitDeclTypeDeclaracionArreglo(MiniGoParser.DeclTypeDeclaracionArregloContext context)
   {
      return null;       
   }

   public override object VisitDeclTypeEstructuraDecTipo(MiniGoParser.DeclTypeEstructuraDecTipoContext context)
   {
      return null;        }

   public override object VisitDeclaracionVacia(MiniGoParser.DeclaracionVaciaContext context)
   {
      return null;        
   }

   public override object VisitDeclaracionInt(MiniGoParser.DeclaracionIntContext context)
   {
      return null;        }

   public override object VisitEstructuraLlaves(MiniGoParser.EstructuraLlavesContext context)
   {
      return null;        }

   public override object VisitPartesCodigo(MiniGoParser.PartesCodigoContext context)
   {
      return null;        }

   public override object VisitListarIdentificadores(MiniGoParser.ListarIdentificadoresContext context)
   {
      return context.Identifier();        
   }

   public override object VisitExpressionMenorIgual(MiniGoParser.ExpressionMenorIgualContext context)
   {
      return null;        }

   public override object VisitExpressionAsterisco(MiniGoParser.ExpressionAsteriscoContext context)
   {
      return null;        }

   public override object VisitExpressionCorrimientoDerecha(MiniGoParser.ExpressionCorrimientoDerechaContext context)
   {
      return null;        }

   public override object VisitExpressionLand(MiniGoParser.ExpressionLandContext context)
   {
      return null;        }

   public override object VisitExpressionMayorIgual(MiniGoParser.ExpressionMayorIgualContext context)
   {
      return null;        }

   public override object VisitNotExpression(MiniGoParser.NotExpressionContext context)
   {
      return null;        }

   public override object VisitExpressionMayor(MiniGoParser.ExpressionMayorContext context)
   {
      return null;        }

   public override object VisitExpressionEqual(MiniGoParser.ExpressionEqualContext context)
   {
      return context.expression();        
   }

   public override object VisitExpressionMas(MiniGoParser.ExpressionMasContext context)
   {
      
      string valor1 = (string)Visit(context.expression(0));
      string valor2 = (string)Visit(context.expression(1));
            

      if (valor1.Contains('.') && valor2.Contains('.')) 
      {
         NumberFormatInfo provider = new NumberFormatInfo();

         provider.NumberDecimalSeparator = ".";

         double result = 0.0;
         double expresion1 = Convert.ToDouble(valor1,provider);
         double expresion2 = Convert.ToDouble(valor2, provider);
                
         result = expresion1 + expresion2;
         string resultFinal = result.ToString().Replace(',', '.'); 
         return resultFinal;
      }
      else
      {
         int result = 0;
         int expresion1 = Int32.Parse(valor1);
         int expresion2= Int32.Parse(valor2);

         result = expresion1+expresion2;
         Console.WriteLine(result);
         return result.ToString();
      }
      
   }

   public override object VisitExpressionSlash(MiniGoParser.ExpressionSlashContext context)
   {
      return null;        
   }

   public override object VisitExpressionDisyuncionExclusi(MiniGoParser.ExpressionDisyuncionExclusiContext context)
   {
      return null;        }

   public override object VisitMasExpression(MiniGoParser.MasExpressionContext context)
   {
      return null;        }

   public override object VisitExpressionAndNOt(MiniGoParser.ExpressionAndNOtContext context)
   {
      return null;        }

   public override object VisitExpressionPrimaryExpression(MiniGoParser.ExpressionPrimaryExpressionContext context)
   {
      return Visit(context.primaryExpression());        
   }

   public override object VisitExpressionAnd(MiniGoParser.ExpressionAndContext context)
   {
      return null;        }

   public override object VisitExpressionCorrimientoIzqui(MiniGoParser.ExpressionCorrimientoIzquiContext context)
   {
      return null;        }

   public override object VisitExpressionNotEqual(MiniGoParser.ExpressionNotEqualContext context)
   {
      return null;        }

   public override object VisitMenosExpression(MiniGoParser.MenosExpressionContext context)
   {
      return null;        }

   public override object VisitDisyuncionExpression(MiniGoParser.DisyuncionExpressionContext context)
   {
      return null;        }

   public override object VisitExpressionPorcentaje(MiniGoParser.ExpressionPorcentajeContext context)
   {
      return null;        }

   public override object VisitExpressionOr(MiniGoParser.ExpressionOrContext context)
   {
      return null;        }

   public override object VisitExpressionMenor(MiniGoParser.ExpressionMenorContext context)
   {
      return null;        }

   public override object VisitExpressionMenos(MiniGoParser.ExpressionMenosContext context)
   {
      return null;        }

   public override object VisitExpressionLor(MiniGoParser.ExpressionLorContext context)
   {
      return null;        
   }

   public override object VisitExpressionListExpression(MiniGoParser.ExpressionListExpressionContext context)
   {
      for (int i = 0; i < context.expression().Length; i++)
      {
         Visit(context.expression(i));   
      }

      return context.expression();
   }

   public override object VisitPrimaryExpressionSelector(MiniGoParser.PrimaryExpressionSelectorContext context)
   {
      return null;         
   }

   public override object VisitPrimaryExpressionCap(MiniGoParser.PrimaryExpressionCapContext context)
   {
      return null;         }

   public override object VisitPrimaryExpressionOperand(MiniGoParser.PrimaryExpressionOperandContext context)
   {
      return Visit(context.operand());         
   }

   public override object VisitPrimaryExpressionAppend(MiniGoParser.PrimaryExpressionAppendContext context)
   {
      return null;         }

   public override object VisitPrimaryExpressionArguments(MiniGoParser.PrimaryExpressionArgumentsContext context)
   {
      return null;         }

   public override object VisitPrimaryExpressionIndex(MiniGoParser.PrimaryExpressionIndexContext context)
   {
      return null;         }

   public override object VisitPrimaryExpressionLenght(MiniGoParser.PrimaryExpressionLenghtContext context)
   {
      return null;         }

   public override object VisitOperandoLiteral(MiniGoParser.OperandoLiteralContext context)
   {
      return Visit(context.literal());         
   }

   public override object VisitOperandoIdentificador(MiniGoParser.OperandoIdentificadorContext context)
   {
      return context.Identifier().GetText();         
   }

   public override object VisitOperandoExpresion(MiniGoParser.OperandoExpresionContext context)
   {
      return Visit(context.expression());         
   }

   public override object VisitLiteralLit(MiniGoParser.LiteralLitContext context)
   {
      return context.Int_lit().GetText();
      
   }

   public override object VisitLiteralFloat(MiniGoParser.LiteralFloatContext context)
   {
      return context.Float_Lit().GetText();         
   }

   public override object VisitLiteralRune(MiniGoParser.LiteralRuneContext context)
   {
      return null;         
   }

   public override object VisitLiteralRaw(MiniGoParser.LiteralRawContext context)
   {
      return context.Raw_String_Lit().GetText();         }

   public override object VisitLiteralInterpreted(MiniGoParser.LiteralInterpretedContext context)
   {
      return null;         }

   public override object VisitIndexTipo(MiniGoParser.IndexTipoContext context)
   {
      return null;         }

   public override object VisitArgumentos(MiniGoParser.ArgumentosContext context)
   {
      return null;         }

   public override object VisitSelectorPunto(MiniGoParser.SelectorPuntoContext context)
   {
      return null;         }

   public override object VisitExpresionAppend(MiniGoParser.ExpresionAppendContext context)
   {
      return null;         }

   public override object VisitExpresionLen(MiniGoParser.ExpresionLenContext context)
   {
      return null;         }

   public override object VisitExpresionCap(MiniGoParser.ExpresionCapContext context)
   {
      return null; 
   }

   public override object VisitListaDeclaraciones(MiniGoParser.ListaDeclaracionesContext context)
   {
      for (int i = 0; i < context.statement().Length; i++)
      {
         Visit(context.statement(i));
      }
      
      return context.statement(); 
   }

   public override object VisitEstructuraBlock(MiniGoParser.EstructuraBlockContext context)
   {
      Visit(context.statementList());
      return null;
   }

   public override object VisitFuncionImprimir(MiniGoParser.FuncionImprimirContext context)
   {
      return base.VisitFuncionImprimir(context);
   }

   public override object VisitFuncionImprimirLn(MiniGoParser.FuncionImprimirLnContext context)
   {

      string val = context.expressionList().GetText();
      string valorcambeado = val.Substring(1, val.Length - 2);
      LLVMValueRef format = builder.BuildGlobalStringPtr(valorcambeado, "format");
      LLVMValueRef[] a = new LLVMValueRef[1] { format };
      LLVMValueRef print = this.module.GetNamedFunction("puts");
      builder.BuildCall(print, a, "imprimir");
      return null;
   }

   public override object VisitFuncionRetornar(MiniGoParser.FuncionRetornarContext context)
   {
      return base.VisitFuncionRetornar(context);
   }

   public override object VisitFuncionBreak(MiniGoParser.FuncionBreakContext context)
   {
      return base.VisitFuncionBreak(context);
   }

   public override object VisitFuncionContinuar(MiniGoParser.FuncionContinuarContext context)
   {
      return base.VisitFuncionContinuar(context);
   }

   public override object VisitExpresionSimple(MiniGoParser.ExpresionSimpleContext context)
   {
      return base.VisitExpresionSimple(context);
   }

   public override object VisitFuncionBlock(MiniGoParser.FuncionBlockContext context)
   {
      return base.VisitFuncionBlock(context);
   }

   public override object VisitFuncionSwitch(MiniGoParser.FuncionSwitchContext context)
   {
      return base.VisitFuncionSwitch(context);
   }

   public override object VisitFuncionIf(MiniGoParser.FuncionIfContext context)
   {
      this.contador++;
      Visit(context.ifStatement());
      return null;
   }

   public override object VisitFuncionLoop(MiniGoParser.FuncionLoopContext context)
   {
      return base.VisitFuncionLoop(context);
   }

   public override object VisitDeclararTipos(MiniGoParser.DeclararTiposContext context)
   {
      return base.VisitDeclararTipos(context);
   }

   public override object VisitDeclararVariables(MiniGoParser.DeclararVariablesContext context)
   {
      isGlobal = false;
      Visit(context.variableDecl());
      return null;
   }

   public override object VisitFuncionEpsilon1(MiniGoParser.FuncionEpsilon1Context context)
   {
      return null;
   }

   public override object VisitDecrementoIncremento(MiniGoParser.DecrementoIncrementoContext context)
   {
      return null;
   }

   public override object VisitDeclararAsignacion(MiniGoParser.DeclararAsignacionContext context)
   {
      Visit(context.assignmentStatement());
      return null;
   }

   public override object VisitSentenciaDefinir(MiniGoParser.SentenciaDefinirContext context)
   {
      return null;
   }

   public override object VisitPrimerAsignacion(MiniGoParser.PrimerAsignacionContext context)
   {
      var nombre= context.expressionList(0).GetText();
      var valor = context.expressionList(1).GetText();
      LLVMValueRef value;
      if (int.TryParse(valor, out var result))
      {
         LLVMValueRef const1 = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)result);
         namedValues.TryGetValue(nombre, out value);
         if (value.ToString().Contains("loca"))
         {
            builder.BuildStore(const1, value);
         }
         else
         {
            value.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)result, true);
            builder.BuildStore(value.Initializer, value);
         }
      }
      else if (double.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture,  out var result1))
      {
         LLVMValueRef const1 = LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, result1);
         namedValues.TryGetValue(nombre, out value);
         if (value.ToString().Contains("loca")) 
         {
            builder.BuildStore(const1, value);
         }
         else
         {
            value.Initializer = LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, result1);
            builder.BuildStore(value.Initializer, value);
         }
      }
      else if (bool.TryParse(valor, out var result2))
      {
         
         if (valor == "true")
         {
            LLVMValueRef const1 = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1);


            namedValues.TryGetValue(nombre, out value);
            if (value.ToString().Contains("loca"))
            {
               builder.BuildStore(const1, value);
            }
            else
            { 
               value.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1, true);
               builder.BuildStore(value.Initializer, value);
            }
         }
         else
         {
            LLVMValueRef const1 = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0);
            namedValues.TryGetValue(nombre, out value);

            if (value.ToString().Contains("loca"))
            {
               builder.BuildStore(const1, value);
            }
            else
            { 
               value.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0, true);
               builder.BuildStore(value.Initializer, value);

            }
         }
      }

      
      return null;
   }

   public override object VisitSegundaAsignacion(MiniGoParser.SegundaAsignacionContext context)
   {
      return null;
   }

   public override object VisitTercerAsignacion(MiniGoParser.TercerAsignacionContext context)
   {
      return null;
   }

   public override object VisitCuartaAsignacion(MiniGoParser.CuartaAsignacionContext context)
   {
      return null;
   }

   public override object VisitQuintaAsignacion(MiniGoParser.QuintaAsignacionContext context)
   {
      return null;
   }

   public override object VisitSextaAsignacion(MiniGoParser.SextaAsignacionContext context)
   {
      return null;
   }

   public override object VisitSetimaAsignacion(MiniGoParser.SetimaAsignacionContext context)
   {
      return null;
   }

   public override object VisitOctavaAsignacion(MiniGoParser.OctavaAsignacionContext context)
   {
      return null;
   }

   public override object VisitNovenaAsignacion(MiniGoParser.NovenaAsignacionContext context)
   {
      return null;
   }

   public override object VisitDecimaAsignacion(MiniGoParser.DecimaAsignacionContext context)
   {
      return null;
   }

   public override object VisitUndecimaAsignacion(MiniGoParser.UndecimaAsignacionContext context)
   {
      return null;
   }

   public override object VisitDuodecimaAsignacion(MiniGoParser.DuodecimaAsignacionContext context)
   {
      return null;
   }

   public override object VisitPrimerIf(MiniGoParser.PrimerIfContext context)
   {
      return base.VisitPrimerIf(context);
   }

   public override object VisitSegundoIf(MiniGoParser.SegundoIfContext context)
   {
      return base.VisitSegundoIf(context);
   }

   public override object VisitTercerIf(MiniGoParser.TercerIfContext context)
   {
      return null;
   }

   public override object VisitCuartoIf(MiniGoParser.CuartoIfContext context)
   {
      return base.VisitCuartoIf(context);
   }

   public override object VisitQuintoIf(MiniGoParser.QuintoIfContext context)
   {
      return base.VisitQuintoIf(context);
   }

   public override object VisitSextoIf(MiniGoParser.SextoIfContext context)
   {
      return base.VisitSextoIf(context);
   }

   public override object VisitPrimerFor(MiniGoParser.PrimerForContext context)
   {
      return base.VisitPrimerFor(context);
   }

   public override object VisitSegundoFor(MiniGoParser.SegundoForContext context)
   {
      return base.VisitSegundoFor(context);
   }

   public override object VisitTercerFor(MiniGoParser.TercerForContext context)
   {
      return base.VisitTercerFor(context);
   }

   public override object VisitCuartoFor(MiniGoParser.CuartoForContext context)
   {
      return base.VisitCuartoFor(context);
   }

   public override object VisitPrimerSwitch(MiniGoParser.PrimerSwitchContext context)
   {
      return base.VisitPrimerSwitch(context);
   }

   public override object VisitSegundoSwitch(MiniGoParser.SegundoSwitchContext context)
   {
      return base.VisitSegundoSwitch(context);
   }

   public override object VisitTecerSwitch(MiniGoParser.TecerSwitchContext context)
   {
      return base.VisitTecerSwitch(context);
   }

   public override object VisitCuartoSwitch(MiniGoParser.CuartoSwitchContext context)
   {
      return base.VisitCuartoSwitch(context);
   }

   public override object VisitFuncionEpsilon(MiniGoParser.FuncionEpsilonContext context)
   {
      return base.VisitFuncionEpsilon(context);
   }

   public override object VisitExpresionCasoClausula(MiniGoParser.ExpresionCasoClausulaContext context)
   {
      return base.VisitExpresionCasoClausula(context);
   }

   public override object VisitExpresionSwitch(MiniGoParser.ExpresionSwitchContext context)
   {
      return base.VisitExpresionSwitch(context);
   }

   public override object VisitExpresionCase(MiniGoParser.ExpresionCaseContext context)
   {
      return base.VisitExpresionCase(context);
   }

   public override object VisitExpresionDefault(MiniGoParser.ExpresionDefaultContext context)
   {
      return base.VisitExpresionDefault(context);
   }
   }
}