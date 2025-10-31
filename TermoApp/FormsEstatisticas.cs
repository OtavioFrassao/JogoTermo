using System;
using System.Windows.Forms;

namespace TermoApp
{
    public partial class FormsEstatisticas : Form
    {
        public FormsEstatisticas(int jogos, int vitorias, int seqAtual, int seqMax)
        {
            InitializeComponent();

            double percVitorias = (jogos > 0) ? ((double)vitorias / jogos) * 100 : 0;

            lblJogosJogados.Text = jogos.ToString();
            lblPercentVitorias.Text = percVitorias.ToString("F1") + "%";
            lblSequenciaAtual.Text = seqAtual.ToString();
            lblMelhorSequencia.Text = seqMax.ToString();
        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

 
    }
}