using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace TermoLib
{
    public class Letra
    {
        public Letra(char caracter, char cor)
        {
            Caracter = caracter;
            Cor = cor;
        }
        public char Caracter;
        public char Cor;
    }

    public class Termo
    {
        public List<string> palavras;
        public string palavraSorteada = string.Empty;
        public List<List<Letra>> tabuleiro;
        public Dictionary<char, char> teclado;
        public int palavraAtual;
        public bool JogoFinalizado;
        public bool TentativasEsgotadas;

        public Termo()
        {
            try
            {
                string nomeDoSeuArquivo = "minhas_palavras.txt";
                string nomeCompletoRecurso = $"TermoLib.{nomeDoSeuArquivo}";

                palavras = CarregarPalavrasDoRecurso(nomeCompletoRecurso);

                if (palavras == null || palavras.Count == 0)
                {
                    throw new Exception("Lista de palavras do recurso incorporado está vazia ou não foi encontrada após filtragem.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"*** ERRO CRÍTICO AO CARREGAR RECURSO: {ex.Message} *** USANDO LISTA DE FALLBACK!");
                palavras = new List<string> { "TERMO", "LETRA", "JOGAR", "LIVRO", "MOUSE" };
            }

            SorteiaPalavra();
            palavraAtual = 1;
            tabuleiro = new List<List<Letra>>();
            teclado = new Dictionary<char, char>();
            for (int i = 65; i < 91; i++)
            {
                //C: Nao digitado / V : Posicao correta / A: Na palavra / P: Nao faz parte
                teclado.Add((char)i, 'c');
            }
            JogoFinalizado = false;
            TentativasEsgotadas = false;
        }

        private List<string> CarregarPalavrasDoRecurso(string nomeRecurso)
        {
            var assembly = Assembly.GetExecutingAssembly();
            List<string> listaPalavras = new List<string>();


            using (Stream stream = assembly.GetManifestResourceStream(nomeRecurso))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Recurso incorporado '{nomeRecurso}' não encontrado.", nomeRecurso);
                }
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string linha;
                    while ((linha = reader.ReadLine()) != null)
                    {
                        string palavra = linha.Trim().ToUpper();
                        if (palavra.Length == 5 && !string.IsNullOrWhiteSpace(palavra) && palavra.All(char.IsLetter))
                        {
                            listaPalavras.Add(palavra);
                        }
                    }
                }
            }
            return listaPalavras.Distinct().ToList();
        }

        public bool ChecaPalavra(string palavra)
        {
            string palavraNormalizada = palavra.ToUpper();

            if (!palavras.Contains(palavraNormalizada))
            {
                return false;
            }

            var palavraSorteadaUpper = palavraSorteada;

            if (palavraNormalizada == palavraSorteadaUpper)
                JogoFinalizado = true;

            var palavraTabuleiro = new List<Letra>();
            bool[] sorteadaUsada = new bool[5];

            for (int i = 0; i < palavraNormalizada.Length; i++)
            {
                if (palavraNormalizada[i] == palavraSorteadaUpper[i])
                {
                    palavraTabuleiro.Add(new Letra(palavraNormalizada[i], 'V'));
                    sorteadaUsada[i] = true;
                }
                else
                {
                    palavraTabuleiro.Add(new Letra(palavraNormalizada[i], 'P'));
                }
            }

            for (int i = 0; i < palavraNormalizada.Length; i++)
            {
                if (palavraTabuleiro[i].Cor != 'V')
                {
                    for (int j = 0; j < palavraSorteadaUpper.Length; j++)
                    {
                        if (!sorteadaUsada[j] && palavraNormalizada[i] == palavraSorteadaUpper[j])
                        {
                            palavraTabuleiro[i] = new Letra(palavraNormalizada[i], 'A');
                            sorteadaUsada[j] = true;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < palavraNormalizada.Length; i++)
            {
                var letra = palavraNormalizada[i];
                if (teclado.ContainsKey(letra))
                {
                    var corAtual = teclado[letra];
                    var corNova = palavraTabuleiro[i].Cor;

                    if (corNova == 'V')
                    {
                        teclado[letra] = 'V';
                    }
                    else if (corNova == 'A' && corAtual != 'V')
                    {
                        teclado[letra] = 'A';
                    }
                    else if (corNova == 'P' && corAtual == 'c')
                    {
                        teclado[letra] = 'P';
                    }
                }
            }

            tabuleiro.Add(palavraTabuleiro);
            palavraAtual++;

            if (palavraAtual > 6 && !JogoFinalizado)
            {
                TentativasEsgotadas = true;
            }

            return true;
        }

        public void SorteiaPalavra()
        {
            if (palavras == null || palavras.Count == 0)
            {
                palavraSorteada = "ERRO"; 
                return;
            }
            Random rdn = new Random();
            var index = rdn.Next(0, palavras.Count);
            palavraSorteada = palavras[index];
        }

        public void Reiniciar()
        {
            SorteiaPalavra();
            palavraAtual = 1;
            tabuleiro.Clear();
            teclado.Clear();
            for (int i = 65; i < 91; i++)
            {
                //C: Nao digitado / V : Posicao correta / A: Na palavra / P: Nao faz parte
                teclado.Add((char)i, 'c');
            }
            JogoFinalizado = false;
            TentativasEsgotadas = false;
        }
    } 
} 