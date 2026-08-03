using System;
using System.Linq;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Clear results from the previous scan
            dataGridView1.Rows.Clear();
            textBox2.Clear();

            // Read the Tiny source code from the input textbox
            string code = textBox1.Text;

            // Start lexical analysis
            JASON_Compiler.Start_Compiling(code);

            // Display tokens and lexical errors
            PrintTokens();
            PrintErrors();
        }

        private void PrintTokens()
        {
            for (int i = 0; i < JASON_Compiler.TokenStream.Count; i++)
            {
                Token token = JASON_Compiler.TokenStream.ElementAt(i);

                dataGridView1.Rows.Add(
                    token.lex,
                    token.token_type
                );
            }
        }

        private void PrintErrors()
        {
            for (int i = 0; i < Errors.Error_List.Count; i++)
            {
                textBox2.AppendText(
                    Errors.Error_List[i] + Environment.NewLine
                );
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Clear input, tokens, and errors
            textBox1.Clear();
            textBox2.Clear();
            dataGridView1.Rows.Clear();

            JASON_Compiler.TokenStream.Clear();
            JASON_Compiler.Jason_Scanner.Tokens.Clear();
            Errors.Error_List.Clear();
        }

        private void dataGridView1_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }
    }
}