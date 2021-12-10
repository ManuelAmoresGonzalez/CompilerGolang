using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Generated;
using LLVMSharp.Interop;
using ProyectoCompiladores.Contextual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoCompiladores
{
    static class Program
    {
       
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
