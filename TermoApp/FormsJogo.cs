using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using TermoLib;
using TermoApp.Properties;
using System.Media;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace TermoApp
{
    public partial class FormsJogo : Form
    {
        public Termo termo;
        int coluna = 1;

        public FormsJogo()
        {
            InitializeComponent();
            termo = new Termo();
            btnReiniciar.Enabled = false;
        }

        private void FormsJogo_Load(object sender, EventArgs e)
        {

        }

        private void PlaySoundFromResource(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string fullResourceName = $"TermoApp.efeitos_sonoros.{resourceName}";

                using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                {
                    if (stream != null)
                    {
                        using (SoundPlayer player = new SoundPlayer(stream))
                        {
                            player.Play();
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"AVISO: Recurso de som não encontrado: {fullResourceName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao tocar som '{resourceName}': {ex.Message}");
            }
        }

        private void btnTeclado_Click(object sender, EventArgs e)
        {
            PlaySoundFromResource("clickjogo.wav");
            if (coluna > 5) return;

            var button = (Button)sender;
            var linha = termo.palavraAtual;
            var nomeButton = $"btn{linha}{coluna}";
            var buttonTabuleiro = RetornaBotao(nomeButton);
            if (buttonTabuleiro != null)
            {
                buttonTabuleiro.Text = button.Text;
                coluna++;
            }
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            if (coluna <= 5)
            {
                PlaySoundFromResource("enter_invalido.wav");
                MessageBox.Show("Palavra com menos de 5 letras, por favor complete!",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var palavra = string.Empty;

            for (int i = 1; i <= 5; i++)
            {
                var nomeBotao = $"btn{termo.palavraAtual}{i}";
                var botao = RetornaBotao(nomeBotao);
                if (botao != null)
                {
                    palavra += botao.Text;
                }
                else
                {
                    PlaySoundFromResource("enter_invalido.wav");
                    MessageBox.Show("Erro ao ler a palavra do tabuleiro.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            bool palavraValida = termo.ChecaPalavra(palavra);

            if (!palavraValida)
            {
                PlaySoundFromResource("enter_invalido.wav");
                MessageBox.Show("Palavra não válida, tente outra!",
                                "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PlaySoundFromResource("enter_valido.wav");

            AtualizaTabuleiro();
            coluna = 1;

            if (termo.JogoFinalizado)
            {
                MessageBox.Show("Parabens, palavra Correta",
                                "Jogo Termo", MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                FinalizarJogo();
            }
            else if (termo.TentativasEsgotadas)
            {
                MessageBox.Show($"Você perdeu! A palavra era: {termo.palavraSorteada.ToUpper()}",
                                "Jogo Termo", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                FinalizarJogo();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            PlaySoundFromResource("deletejogo.wav");
            if (coluna == 1) return;

            coluna--;
            var linha = termo.palavraAtual;
            var nomeButton = $"btn{linha}{coluna}";
            var buttonTabuleiro = RetornaBotao(nomeButton);
            if (buttonTabuleiro != null)
            {
                buttonTabuleiro.Text = "";
            }
        }

        private void btnTabuleiro_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var buttonName = button.Name;

            if (buttonName.Length == 5 && buttonName.StartsWith("btn"))
            {
                try
                {
                    int clickedRow = int.Parse(buttonName.Substring(3, 1));
                    int clickedCol = int.Parse(buttonName.Substring(4, 1));

                    if (clickedRow == termo.palavraAtual)
                    {
                        coluna = clickedCol;
                        if (coluna > 6) coluna = 7;
                    }
                }
                catch (FormatException)
                {

                }
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData >= Keys.A && keyData <= Keys.Z)
            {
                string letra = keyData.ToString();
                var botaoTeclado = RetornaBotao($"btn{letra}");
                if (botaoTeclado != null)
                {
                    btnTeclado_Click(botaoTeclado, EventArgs.Empty);
                }
                return true;
            }
            else if (keyData == Keys.Enter)
            {
                btnEnter_Click(btnEnter, EventArgs.Empty);
                return true;
            }
            else if (keyData == Keys.Back)
            {
                btnDelete_Click(btnDelete, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnEstatisticas_Click(object sender, EventArgs e)
        {
            string nomeRecursoImagem = "TermoApp.imagens.backgorund_estatistica.jpg";
            PlaySoundFromResource("estatisticajogo.wav");

            int jogos = Properties.Settings.Default.JogosJogados;
            int vitorias = Properties.Settings.Default.TotalVitorias;
            int seqAtual = Properties.Settings.Default.SequenciaAtual;
            int seqMax = Properties.Settings.Default.MelhorSequencia;

            FormsEstatisticas formStats = new FormsEstatisticas(jogos, vitorias, seqAtual, seqMax);
            formStats.ShowDialog(this);
        }

        private void btnReiniciar_Click(object sender, EventArgs e)
        {
            termo.Reiniciar();
            ReiniciarTabuleiroVisual();
            PlaySoundFromResource("reiniciarjogo.wav");
        }

        private Button RetornaBotao(string name)
        {
            var controls = Controls.Find(name, true);
            if (controls.Length > 0)
            {
                return (Button)controls[0];
            }
            return null;
        }

        private void AtualizaTabuleiro()
        {
            int linhaParaAtualizar = termo.palavraAtual - 2;

            if (linhaParaAtualizar < 0) return;

            for (int col = 1; col <= 5; col++)
            {
                if (linhaParaAtualizar < termo.tabuleiro.Count &&
                    col - 1 < termo.tabuleiro[linhaParaAtualizar].Count)
                {
                    var letra = termo.tabuleiro[linhaParaAtualizar][col - 1];
                    var nomeBotao = $"btn{termo.palavraAtual - 1}{col}";
                    var botaoTab = RetornaBotao(nomeBotao);

                    if (botaoTab != null)
                    {
                        if (letra.Cor == 'A')
                        {
                            botaoTab.BackColor = Color.Yellow;
                            botaoTab.ForeColor = Color.Black;
                        }
                        else if (letra.Cor == 'V')
                        {
                            botaoTab.BackColor = Color.Green;
                            botaoTab.ForeColor = Color.White;
                        }
                        else
                        {
                            botaoTab.BackColor = Color.Gray;
                            botaoTab.ForeColor = Color.White;
                        }
                    }
                }
            }

            foreach (var letraInfo in termo.teclado)
            {
                var nomeBotaoKey = $"btn{letraInfo.Key}";
                var botaoKey = RetornaBotao(nomeBotaoKey);

                if (botaoKey != null)
                {
                    if (letraInfo.Value == 'V')
                    {
                        botaoKey.BackColor = Color.Green;
                        botaoKey.ForeColor = Color.White;
                    }
                    else if (letraInfo.Value == 'A')
                    {
                        botaoKey.BackColor = Color.Yellow;
                        botaoKey.ForeColor = Color.Black;
                    }
                    else if (letraInfo.Value == 'P')
                    {
                        botaoKey.BackColor = Color.Gray;
                        botaoKey.ForeColor = Color.White;
                    }

                }
            }

            groupBox1.Refresh();
            groupBox2.Refresh();
            this.Refresh();
        }


        private void FinalizarJogo()
        {
            bool vitoria = termo.JogoFinalizado;
            var settings = Properties.Settings.Default;

            if (vitoria)
            {
                PlaySoundFromResource("ganharjogo.wav");
            }
            else
            {
                PlaySoundFromResource("perderjogo.wav");
            }

            settings.JogosJogados++;

            if (vitoria)
            {
                settings.TotalVitorias++;
                settings.SequenciaAtual++;
                if (settings.SequenciaAtual > settings.MelhorSequencia)
                {
                    settings.MelhorSequencia = settings.SequenciaAtual;
                }
            }
            else
            {
                settings.SequenciaAtual = 0;
            }

            settings.Save();

            groupBox1.Enabled = false;
            btnEnter.Enabled = false;
            btnDelete.Enabled = false;

            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    var btnTab = RetornaBotao($"btn{i}{j}");
                    if (btnTab != null) btnTab.Enabled = false;
                }
            }

            btnReiniciar.Enabled = true;
        }

        private void ReiniciarTabuleiroVisual()
        {
            for (int i = 1; i <= 6; i++)
            {
                for (int j = 1; j <= 5; j++)
                {
                    var btnTab = RetornaBotao($"btn{i}{j}");
                    if (btnTab != null)
                    {
                        btnTab.Text = "";
                        btnTab.BackColor = Color.Transparent;
                        btnTab.ForeColor = Color.White;
                        btnTab.Enabled = true;
                    }
                }
            }

            foreach (var letraInfo in termo.teclado)
            {
                var nomeBotaoKey = $"btn{letraInfo.Key}";
                var botaoKey = RetornaBotao(nomeBotaoKey);

                if (botaoKey != null)
                {
                    botaoKey.BackColor = Color.Transparent;
                    botaoKey.ForeColor = Color.White;
                }
            }

            groupBox1.Enabled = true;
            btnEnter.Enabled = true;
            btnDelete.Enabled = true;
            btnReiniciar.Enabled = false;

            coluna = 1;
            this.Refresh();
        }

        private void btnFechar_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}