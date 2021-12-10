using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Generated;
using LLVMSharp.Interop;
using ProyectoCompiladores.Contextual;

namespace ProyectoCompiladores
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int MainMethod();
        [STAThread]
        private void button1_Click(object sender, EventArgs e)
        {
            
            String linea = richTextBox3.Text;
            if (linea.Length == 0)
            {
                MessageBox.Show("No hay codigo para procesar");
                return;
            }
            AntlrInputStream input = new AntlrInputStream(linea);
            MiniGoLexer lexer = new MiniGoLexer(input);
            CommonTokenStream tokens = new CommonTokenStream(lexer);
            MiniGoParser parser = new MiniGoParser(tokens);

            IParseTree tree = parser.root();
            
            
            LLVMModuleRef module = LLVMModuleRef.CreateWithName("AlphaObj");
            LLVMBuilderRef builder = LLVMBuilderRef.Create(module.Context);

            ContextualCode visitor = new ContextualCode(module, builder);
            LLVMValueRef main = (LLVMValueRef)visitor.Visit(tree);

            if (!module.TryVerify(LLVMVerifierFailureAction.LLVMPrintMessageAction, out var error))
            {
                MessageBox.Show($"Error: {error}");
            }

            LLVM.LinkInMCJIT();
            LLVM.InitializeX86TargetMC();
            LLVM.InitializeX86Target();
            LLVM.InitializeX86TargetInfo();
            LLVM.InitializeX86AsmParser();
            LLVM.InitializeX86AsmPrinter();

            LLVMMCJITCompilerOptions options = new LLVMMCJITCompilerOptions { NoFramePointerElim = 1 };
            if (!module.TryCreateMCJITCompiler(out var engine, ref options, out error))
            {
                MessageBox.Show($"Error: {error}");
            }

            var mainMethod = (Form1.MainMethod)Marshal.GetDelegateForFunctionPointer(engine.GetPointerToGlobal(main), typeof(Form1.MainMethod));
            mainMethod();

            if (module.WriteBitcodeToFile("sum.bc") != 0)
            {
                MessageBox.Show("Error writing bitcode to file, skipping");
            }

            module.PrintToFile("sum.ll");
            richTextBox2.Text = module.ToString();

            module.Dump();
            builder.Dispose();
            engine.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string fileName = @"D:\ProyectoCompiladores\Codigo.txt";

            StreamWriter escribir = new StreamWriter(fileName);
            String linea = richTextBox3.Text;
            escribir.WriteLine(linea);
            escribir.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string fileName = @"D:\ProyectoCompiladores\Codigo.txt";
            StreamReader reader = new StreamReader(fileName);
            richTextBox3.Text = reader.ReadToEnd();
            reader.Close();
            button3.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox3.Clear();
            var rtb = sender as System.Windows.Forms.RichTextBox;
        }

        private void EXIT_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
