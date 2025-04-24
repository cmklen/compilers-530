using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTClass
{
    public partial class TerminalAST : AST
    {
        public TerminalAST() : base() { name = "Terminal"; }
        public TerminalAST(AST parent) : base(parent) { name = "Terminal"; }
        public TerminalAST(AST parent, string name, string value) : base(parent)
        {
            this.name = name;
            val = value;
        }

        protected string val = "";

        public string Value
        {
            get { return val; }
            set { val = value; }
        }

        public override void Dump(StreamWriter sw)
        {
            sw.Write(UniqueName + "[label = \"" + Name);

            if (Name != "") sw.Write(" |");
            sw.Write(val);
            if (Name != "") sw.Write("|");

            sw.Write("\"]\r\n");

            foreach (AST child in children)
            {
                sw.WriteLine(UniqueName + " -> " + child.UniqueName);
                child.Dump(sw);
            }
        }

    }
}
