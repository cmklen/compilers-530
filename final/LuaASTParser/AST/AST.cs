using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace ASTClass
{
    public enum Operator
    {
        // ======================= Binary Operators =====================
        Add, Subtract, Multiply, Divide, Exponent,

        // ======================= Unary Operators ======================
        Inc, Dec,

        // ======================= Assignment Operators ======================
        Assign,

        NoOp
    };

    public partial class AST
    {

        public static int id = 0;   // To generate unique id for each AST object

        #region Fields

        protected List<AST> children = new List<AST>();
        protected List<Operator> operators = new List<Operator>();
        protected AST parent = null;
        protected int oid;          // Each AST object has a unique id
        protected string name = "un_defined";

        #endregion Fields

        #region Constructors and properties

        public AST() { oid = id++; }
        public AST(AST p)
        {
            parent = p;
            oid = id++;

            if (parent != null)
                parent.AddChild(this);
        }

        public AST(AST p, string n)
        {
            parent = p;
            oid = id++;
            name = n;

            if (parent != null)
                parent.AddChild(this);
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual string UniqueName
        {
            get { return Name + oid.ToString(); }
        }

        public List<AST> Children
        {
            get { return children; }
        }

        public List<Operator> Operators
        {
            get { return operators; }
        }

        public AST Parent { get { return parent; } }

        #endregion Constructors and properties

        #region Basic AST utilities

        public virtual AST Clone()
        {
            AST ast = new AST();
            ast.operators = operators;
            foreach (AST c in children)
                ast.children.Add(c.Clone());
            return ast;
        }

        public void SetParent(AST p)
        {
            parent = p;
        }
        public void SetParent2(AST p)
        {
            if (this.parent == p)       // Its parent is p; no need to change
                return;

            parent = p;
            if (p != null) p.AddChild(this);
        }
        public void AddChild(AST child)
        {
            children.Add(child);
        }
        public void RemoveChild(AST child)
        {
            children.Remove(child);
        }
        public AST GetChild(int index)
        {
            if (index < children.Count)
                return children[index];

            return null;
        }
        public bool HasOperators()
        {
            if (operators.Count > 0) return true;
            return false;
        }
        public bool HasChildren()
        {
            if (children.Count > 0) return true;
            return false;
        }
        public int ChildCount() { return children.Count; }
        public void ReparentChildren(AST parent)
        {
            foreach (AST c in children)
                c.SetParent(parent);

            children.Clear();
        }
        public void AddOp(Operator op)
        {
            operators.Add(op);
        }

        #endregion Basic AST utilities

        #region AST simplification

        public void Simplify()  // Top-down simplification
        {
            for (int i = 0; i < ChildCount(); i++)
            {
                AST c = children[i];
                if (!c.HasOperators() && c.ChildCount() == 1)   // [1] Remove internal nodes, which have no operator
                {
                    children[i] = c.GetChild(0);
                    children[i].SetParent(this);
                    c.SetParent(null);
                    --i;

                }
                else if (c.ChildCount() == 0)                   // [2] Remove nonterminal leaf node and empty (NULL) leaf node
                {
                    TerminalAST t = c as TerminalAST;
                    if (t == null || ((t.Name == "NULL" || t.Name == "") && (t.Value == null || t.Value == "")))
                    {
                        c.SetParent(null);
                        RemoveChild(c);
                    }
                }
            }

            foreach (AST c in children)
                c.Simplify();
        }

        public void Simplify2() // Bottom-up simplification
        {
            foreach (AST c in children)
                c.Simplify2();

            for (int i = 0; i < ChildCount(); i++)
            {
                AST c = children[i];
                if (!c.HasOperators() && c.ChildCount() == 1)   // [1] Remove internal nodes, which have no operator
                {
                    children[i] = c.GetChild(0);
                    children[i].SetParent(this);
                    c.SetParent(null);
                    --i;
                }
                else if (c.ChildCount() == 0)                   // [2] Remove nonterminal leaf node and empty (null) leaf node
                {
                    TerminalAST t = c as TerminalAST;
                    if (t == null || ((t.Name == "NULL" || t.Name == "") && (t.Value == null || t.Value == "")))
                    {
                        RemoveChild(c);
                        c.parent.Simplify();
                        c.SetParent(null);
                    }
                }
            }
        }

        #endregion AST simplification

        #region  Methods to make graphviz dot file for AST display

        public bool MakeDotFile(string fileName, string title)
        {
            FileStream file;
            StreamWriter stream = null;

            try
            {
                file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            }
            catch
            {
                return false;
            }
            stream = new StreamWriter(file);

            stream.WriteLine("digraph G {");
            stream.WriteLine("Title [label=\"" + title + "\",shape=box]");
            Dump(stream);
            stream.Write("}");

            if (stream != null)
            {
                stream.Close();
            }

            return true;
        }

        public virtual void Dump(StreamWriter sw)
        {
            sw.Write(UniqueName + "[label = \"" + Name);

            int count = operators.Count;
            if (count > 0)
            {
                sw.Write(" |");

                for (int i = 0; i < count - 1; i++)
                    sw.Write(operators[i].ToString() + ",");
                // Last operator
                sw.Write(operators[count - 1].ToString());

                sw.Write("|");
            }
            sw.Write("\"]\r\n");
            foreach (AST child in children)
            {
                sw.WriteLine(UniqueName + " -> " + child.UniqueName);
                child.Dump(sw);
            }
        }

        #endregion  Methods to make graphviz dot file for AST display

    }
}