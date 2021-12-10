using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Antlr4.Runtime;
using Generated;
using ProyectoCompiladores.Error;


namespace ProyectoCompiladores.Error
{
    class DescriptiveErrorListener : BaseErrorListener, IAntlrErrorListener<int> //nótese la segunda interface
    {
        private ArrayList errorMsgs = new ArrayList();
        private int linea = 0;
        private bool isTrue = false;
        
        public static DescriptiveErrorListener INSTANCE = new DescriptiveErrorListener();

        /* En C#, la librería ANTLR4 define dos métodos diferentes y dos interfaces que deben implementarse en la clase a diferencia de JAVA
        para poder implementar el mismo funcionamiento que vimos. Esa es la razón del por qué está dos veces sobreescrito el método SyntaxError
        Nótese que tienen parámetros diferentes*/
        
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            
            
            if (recognizer is MiniGoParser)
            {
                errorMsgs.Add("PARSER ERROR - line "+line+":"+charPositionInLine + " " + msg);
            }
                
            else if (recognizer is MiniGoLexer)
            {
                errorMsgs.Add("SCANNER ERROR - line "+line+":"+charPositionInLine + " " + msg );
            }
            else
            {
                errorMsgs.Add("Other Error");
            }
                
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            if (isTrue == false)
            {
                linea = line;
                isTrue = true;
            }
            
            if (recognizer is MiniGoParser)
                errorMsgs.Add("Error de Parser - line "+line+":"+charPositionInLine + " " + msg);
            else if (recognizer is MiniGoLexer)
                errorMsgs.Add("Error de scanner - line "+line+":"+charPositionInLine + " " + msg );
            else
                errorMsgs.Add("Other Error");
        }
        
        public void cleanArrayError()
        {
            this.errorMsgs.Clear();
            this.linea = 0;
            isTrue = false;
        }
        
        public int returnLine()
        {
            return this.linea;
        }


        public bool hasErrors ( )
        {
            return this.errorMsgs.Count > 0;
        }
        
        public ArrayList getErrorMsgs() {
            return this.errorMsgs;
        }
        
        public override String ToString ( )
        {
            if ( !hasErrors() ) return "0 errors";
            StringBuilder builder = new StringBuilder();
            foreach ( string s in errorMsgs )
            {
                builder.Append ( String.Format( "{0}\n", s ) );
            }
            return builder.ToString();
        }
    }
}