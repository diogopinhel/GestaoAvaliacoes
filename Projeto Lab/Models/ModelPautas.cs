using System;
using System.Collections.Generic;
using System.Linq;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Models
{
    /// <summary>
    /// Model de negócio para gestão de pautas
    /// Segue o mesmo padrão dos outros models do projeto
    /// </summary>
    public class ModelPautas
    {
        // Referências aos outros models (dependências)
        private readonly ModelAlunos modelAlunos;
        private readonly ModelGrupos modelGrupos;
        private readonly ModelTarefas modelTarefas;
        private readonly ModelNotas modelNotas;

        // Eventos para comunicação com as Views
        public event MetodosSemParametros DadosCarregados;
        public event MetodosSemParametros DadosAlterados;
        public event MetodosComErro ErroOcorrido;

        public ModelPautas(ModelAlunos modelAlunos, ModelGrupos modelGrupos,
                           ModelTarefas modelTarefas, ModelNotas modelNotas)
        {
            this.modelAlunos = modelAlunos;
            this.modelGrupos = modelGrupos;
            this.modelTarefas = modelTarefas;
            this.modelNotas = modelNotas;

            // Inscrever eventos dos models dependentes
            InscriverEventos();
        }

        #region Inscrição de Eventos

        private void InscriverEventos()
        {
            if (modelAlunos != null)
            {
                modelAlunos.DadosAlterados += () => DadosAlterados?.Invoke();
                modelAlunos.AlunoModificado += (numero, operacao) => DadosAlterados?.Invoke();
            }

            if (modelGrupos != null)
            {
                modelGrupos.DadosAlterados += () => DadosAlterados?.Invoke();
                modelGrupos.GrupoModificado += (id, operacao) => DadosAlterados?.Invoke();
            }

            if (modelTarefas != null)
            {
                modelTarefas.DadosAlterados += () => DadosAlterados?.Invoke();
                modelTarefas.TarefaModificada += (id, operacao) => DadosAlterados?.Invoke();
            }

            if (modelNotas != null)
            {
                modelNotas.DadosAlterados += () => DadosAlterados?.Invoke();
                modelNotas.NotaModificada += (id, operacao) => DadosAlterados?.Invoke();
            }
        }

        #endregion

        #region Métodos de Negócio

        /// <summary>
        /// Obtém TODOS os alunos (avaliados e não avaliados) com suas respectivas notas
        /// Agora suporta TODAS as tarefas criadas dinamicamente
        /// </summary>
        public List<PautaViewModel> ObterPautaCompleta()
        {
            try
            {
                var pautas = new List<PautaViewModel>();

                if (modelAlunos?.Lista == null) return pautas;

                
                var tarefas = modelTarefas?.ObterTodasTarefas()?.OrderBy(t => t.Id).ToList() ?? new List<Tarefa>();

                foreach (var aluno in modelAlunos.Lista.Values.OrderBy(a => a.Nome))
                {
                    var pauta = new PautaViewModel
                    {
                        NumeroAluno = aluno.Numero,
                        NomeCompleto = aluno.Nome,
                        Email = aluno.Email
                    };

                    // Gerar iniciais
                    pauta.GerarIniciais();

                    // Encontrar grupo do aluno
                    var grupo = modelGrupos?.EncontrarGrupoDoAluno(aluno.Numero);
                    if (grupo != null)
                    {
                        pauta.GrupoId = grupo.Id;
                        pauta.NomeGrupo = grupo.Nome;
                    }
                    else
                    {
                        // Aluno sem grupo
                        pauta.GrupoId = string.Empty;
                        pauta.NomeGrupo = "Sem grupo";
                    }

                    // Criar lista dinâmica de notas para TODAS as tarefas
                    pauta.NotasTarefas = new List<decimal?>();

                    // Buscar notas para TODAS as tarefas
                    foreach (var tarefa in tarefas)
                    {
                        decimal? nota = ObterNotaAlunoTarefa(aluno.Numero, pauta.GrupoId, tarefa.Id);
                        pauta.NotasTarefas.Add(nota);
                    }

                    // Calcular nota final (só se tiver pelo menos uma nota)
                    CalcularNotaFinal(pauta, tarefas);

                    // Atualizar status das notas
                    pauta.AtualizarStatus();

                    // Adicionar à lista independentemente de ter notas ou não
                    pautas.Add(pauta);
                }

                return pautas;
            }
            catch (Exception ex)
            {
                ErroOcorrido?.Invoke($"Erro ao obter pauta completa: {ex.Message}");
                return new List<PautaViewModel>();
            }
        }

        /// <summary>
        /// NOVO: Método para obter nota de um aluno numa tarefa específica
        /// </summary>
        private decimal? ObterNotaAlunoTarefa(string numeroAluno, string grupoId, int tarefaId)
        {
            if (modelNotas?.Lista == null) return null;

            // Prioridade: nota individual > nota de grupo
            var notaIndividual = modelNotas.Lista.Values.FirstOrDefault(n =>
                n.TarefaId == tarefaId &&
                n.NumeroAluno == numeroAluno &&
                !n.ENotaGrupo);

            if (notaIndividual != null)
            {
                return notaIndividual.ValorNota;
            }

            var notaGrupo = modelNotas.Lista.Values.FirstOrDefault(n =>
                n.TarefaId == tarefaId &&
                n.GrupoId == grupoId &&
                n.ENotaGrupo);

            return notaGrupo?.ValorNota;
        }

        /// <summary>
        /// Obtém todas as tarefas para exibição dinâmica na pauta
        /// </summary>
        public List<Tarefa> ObterTodasTarefasParaPauta()
        {
            return modelTarefas?.ObterTodasTarefas()?.OrderBy(t => t.Id).ToList() ?? new List<Tarefa>();
        }

        /// <summary>
        /// Calcula estatísticas gerais da pauta
        /// Baseado em dados reais das avaliações
        /// </summary>
        public EstatisticasPauta ObterEstatisticas()
        {
            try
            {
                var pautas = ObterPautaCompleta();

                var stats = new EstatisticasPauta
                {
                    TotalAlunos = pautas.Count,
                    AlunosAvaliados = pautas.Count(p => p.NotaFinal.HasValue),
                    MediaGeral = 0
                };

                stats.AlunosPorAvaliar = stats.TotalAlunos - stats.AlunosAvaliados;

                if (stats.AlunosAvaliados > 0)
                {
                    var notasFinais = pautas
                        .Where(p => p.NotaFinal.HasValue)
                        .Select(p => p.NotaFinal.Value);

                    stats.MediaGeral = Math.Round(notasFinais.Average(), 1);
                }

                return stats;
            }
            catch (Exception ex)
            {
                ErroOcorrido?.Invoke($"Erro ao calcular estatísticas: {ex.Message}");
                return new EstatisticasPauta();
            }
        }

        /// <summary>
        /// Calcula distribuição REAL para histograma - só conta notas que realmente existem
        /// </summary>
        public List<int> ObterDistribuicaoHistograma()
        {
            try
            {
                var pautas = ObterPautaCompleta();

                // Só contar alunos que TÊM nota final calculada
                var notasFinais = pautas
                    .Where(p => p.NotaFinal.HasValue) // Só alunos com nota final
                    .Select(p => p.NotaFinal.Value)
                    .ToList();

                // Se só há 3 alunos com nota 16, só mostra 3 no intervalo correspondente
                var distribuicao = new List<int>
                {
                    notasFinais.Count(n => n >= 0 && n <= 2),     // 0-2
                    notasFinais.Count(n => n >= 3 && n <= 5),     // 3-5
                    notasFinais.Count(n => n >= 6 && n <= 8),     // 6-8
                    notasFinais.Count(n => n >= 9 && n < 10),     // 9
                    notasFinais.Count(n => n >= 10 && n <= 11),   // 10-11
                    notasFinais.Count(n => n >= 12 && n <= 13),   // 12-13
                    notasFinais.Count(n => n >= 14 && n <= 15),   // 14-15
                    notasFinais.Count(n => n >= 16 && n <= 17),   // 16-17 (aqui aparecerão os 3 alunos)
                    notasFinais.Count(n => n >= 18 && n <= 19),   // 18-19
                    notasFinais.Count(n => n == 20)               // 20
                };

                return distribuicao;
            }
            catch (Exception ex)
            {
                ErroOcorrido?.Invoke($"Erro ao calcular histograma: {ex.Message}");
                return new List<int>(new int[10]); // Array com 10 zeros
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Calcula nota final considerando TODAS as tarefas dinamicamente
        /// </summary>
        private void CalcularNotaFinal(PautaViewModel pauta, List<Tarefa> tarefas)
        {
            if (pauta.NotasTarefas == null || pauta.NotasTarefas.Count == 0) return;

            decimal somaNotasPonderadas = 0;
            int somaPesos = 0;

            for (int i = 0; i < Math.Min(tarefas.Count, pauta.NotasTarefas.Count); i++)
            {
                if (pauta.NotasTarefas[i].HasValue)
                {
                    var peso = tarefas[i].PesoNumerico;
                    somaNotasPonderadas += pauta.NotasTarefas[i].Value * peso;
                    somaPesos += peso;
                }
            }

            if (somaPesos > 0)
            {
                pauta.NotaFinal = Math.Round(somaNotasPonderadas / somaPesos, 1);
            }
        }

        #endregion
    }

    #region Classes de Apoio

    /// <summary>
    /// ViewModel para representar um aluno na pauta
    /// Suporte dinâmico para qualquer número de tarefas
    /// </summary>
    public class PautaViewModel
    {
        // Informações do Aluno
        public string NumeroAluno { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Iniciais { get; set; } = string.Empty;

        // Informações do Grupo
        public string GrupoId { get; set; } = string.Empty;
        public string NomeGrupo { get; set; } = string.Empty;

        // Sistema dinâmico de notas
        public List<decimal?> NotasTarefas { get; set; } = new List<decimal?>();
        public decimal? NotaFinal { get; set; }

        // Status dinâmico para qualquer número de tarefas
        public List<string> StatusTarefas { get; set; } = new List<string>();

        // Propriedades específicas para compatibilidade (se necessário no XAML)
        public decimal? NotaT1 => NotasTarefas.Count > 0 ? NotasTarefas[0] : null;
        public decimal? NotaT2 => NotasTarefas.Count > 1 ? NotasTarefas[1] : null;
        public decimal? NotaT3 => NotasTarefas.Count > 2 ? NotasTarefas[2] : null;
        public decimal? NotaT4 => NotasTarefas.Count > 3 ? NotasTarefas[3] : null;
        public decimal? NotaT5 => NotasTarefas.Count > 4 ? NotasTarefas[4] : null;
        public decimal? NotaT6 => NotasTarefas.Count > 5 ? NotasTarefas[5] : null;

        // Propriedades dinâmicas para binding das colunas geradas
        public string StatusT1 => StatusTarefas.Count > 0 ? StatusTarefas[0] : "SemNota";
        public string StatusT2 => StatusTarefas.Count > 1 ? StatusTarefas[1] : "SemNota";
        public string StatusT3 => StatusTarefas.Count > 2 ? StatusTarefas[2] : "SemNota";
        public string StatusT4 => StatusTarefas.Count > 3 ? StatusTarefas[3] : "SemNota";
        public string StatusT5 => StatusTarefas.Count > 4 ? StatusTarefas[4] : "SemNota";
        public string StatusT6 => StatusTarefas.Count > 5 ? StatusTarefas[5] : "SemNota";
        public string StatusFinal { get; set; } = "SemNota";

        public void GerarIniciais()
        {
            if (string.IsNullOrWhiteSpace(NomeCompleto))
            {
                Iniciais = "??";
                return;
            }

            var palavras = NomeCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (palavras.Length == 1)
            {
                Iniciais = palavras[0].Length >= 2 ? palavras[0].Substring(0, 2).ToUpper() : palavras[0].ToUpper();
            }
            else
            {
                Iniciais = (palavras.First().Substring(0, 1) + palavras.Last().Substring(0, 1)).ToUpper();
            }
        }

        public static string CalcularStatus(decimal? nota)
        {
            if (!nota.HasValue) return "SemNota";
            return nota.Value >= 10 ? "Aprovado" : "Reprovado";
        }

        public void AtualizarStatus()
        {
            // Atualizar status dinâmico para todas as tarefas na lista
            StatusTarefas = new List<string>();
            foreach (var nota in NotasTarefas)
            {
                StatusTarefas.Add(CalcularStatus(nota));
            }

            // Atualizar status final
            StatusFinal = CalcularStatus(NotaFinal);
        }

    }

    /// <summary>
    /// Classe para estatísticas da pauta
    /// </summary>
    public class EstatisticasPauta
    {
        public int TotalAlunos { get; set; }
        public int AlunosAvaliados { get; set; }
        public int AlunosPorAvaliar { get; set; }
        public decimal MediaGeral { get; set; }
    }

    #endregion
}