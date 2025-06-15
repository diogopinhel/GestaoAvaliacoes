using System;
using System.Collections.Generic;
using System.Linq;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Models
{
    public class ModelNotas
    {
        // Estrutura de dados principal
        public Dictionary<int, Nota> Lista { get; private set; }

        // Referências aos outros models
        private readonly ModelAlunos modelAlunos;
        private readonly ModelGrupos modelGrupos;
        private readonly ModelTarefas modelTarefas;

        // Contador para gerar IDs únicos
        private int proximoId;

        // Eventos para comunicação com as Views
        public event MetodosSemParametros DadosCarregados;
        public event MetodosSemParametros DadosAlterados;
        public event MetodosComErro ErroOcorrido;
        public event NotaAlterada NotaModificada;

        public ModelNotas(ModelAlunos modelAlunos, ModelGrupos modelGrupos, ModelTarefas modelTarefas)
        {
            Lista = new Dictionary<int, Nota>();
            this.modelAlunos = modelAlunos;
            this.modelGrupos = modelGrupos;
            this.modelTarefas = modelTarefas;
            proximoId = 1;

            CarregarDadosExemplo();
        }

        /// <summary>
        /// Carrega dados de exemplo para demonstração
        /// </summary>
        private void CarregarDadosExemplo()
        {
            // Verificar se há dados nos outros models antes de criar exemplos
            if (modelTarefas?.Lista?.Count > 0 && modelGrupos?.Lista?.Count > 0)
            {
                var primeiroGrupo = modelGrupos.Lista.First().Key;
                var primeiraTarefa = modelTarefas.Lista.First().Key;

                var notasExemplo = new List<Nota>
                {
                    new Nota(1, primeiraTarefa, primeiroGrupo, 16.0m),
                    new Nota(2, primeiraTarefa, primeiroGrupo, 17.0m)
                };

                Lista.Clear();
                foreach (var nota in notasExemplo)
                {
                    Lista.Add(nota.Id, nota);
                    if (nota.Id >= proximoId)
                        proximoId = nota.Id + 1;
                }
            }

            DadosCarregados?.Invoke();
        }

        /// <summary>
        /// Adiciona uma nota de grupo
        /// </summary>
        public void AdicionarNotaGrupo(int tarefaId, string grupoId, decimal valorNota)
        {
            try
            {
                ValidarDadosComuns(tarefaId, grupoId, valorNota);

                // Verificar se já existe nota de grupo para esta tarefa e grupo
                var notaExistente = Lista.Values.FirstOrDefault(n =>
                    n.TarefaId == tarefaId &&
                    n.GrupoId == grupoId &&
                    n.ENotaGrupo);

                if (notaExistente != null)
                    throw new OperacaoInvalidaException($"Já existe uma nota de grupo para a tarefa {tarefaId} do grupo {grupoId}.");

                // Criar nova nota de grupo
                var novaNota = new Nota(proximoId, tarefaId, grupoId, valorNota);
                Lista.Add(proximoId, novaNota);
                proximoId++;

                DadosAlterados?.Invoke();
                NotaModificada?.Invoke(novaNota.Id, "Adicionada");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao adicionar nota de grupo: {ex.Message}");
            }
        }

        /// <summary>
        /// Adiciona uma nota individual
        /// </summary>
        public void AdicionarNotaIndividual(int tarefaId, string grupoId, string numeroAluno, decimal valorNota)
        {
            try
            {
                ValidarDadosComuns(tarefaId, grupoId, valorNota);

                // Validar aluno
                if (string.IsNullOrWhiteSpace(numeroAluno))
                    throw new OperacaoInvalidaException("Número do aluno não pode estar vazio.");

                if (!modelAlunos.Lista.ContainsKey(numeroAluno))
                    throw new OperacaoInvalidaException($"Aluno com número {numeroAluno} não existe.");

                // Verificar se o aluno pertence ao grupo
                var grupo = modelGrupos.ObterGrupo(grupoId);
                if (grupo == null || !grupo.ContemAluno(numeroAluno))
                    throw new OperacaoInvalidaException($"Aluno {numeroAluno} não pertence ao grupo {grupoId}.");

                // Verificar se já existe nota individual para este aluno nesta tarefa
                var notaExistente = Lista.Values.FirstOrDefault(n =>
                    n.TarefaId == tarefaId &&
                    n.NumeroAluno == numeroAluno &&
                    !n.ENotaGrupo);

                if (notaExistente != null)
                    throw new OperacaoInvalidaException($"Já existe uma nota individual para o aluno {numeroAluno} na tarefa {tarefaId}.");

                // Criar nova nota individual
                var novaNota = new Nota(proximoId, tarefaId, grupoId, numeroAluno, valorNota);
                Lista.Add(proximoId, novaNota);
                proximoId++;

                DadosAlterados?.Invoke();
                NotaModificada?.Invoke(novaNota.Id, "Adicionada");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao adicionar nota individual: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza uma nota existente
        /// </summary>
        public void AtualizarNota(int id, decimal novoValor)
        {
            try
            {
                if (!Lista.ContainsKey(id))
                    throw new OperacaoInvalidaException($"Não existe uma nota com ID {id}.");

                ValidarNota(novoValor);

                var notaExistente = Lista[id];
                Nota notaAtualizada;

                if (notaExistente.ENotaGrupo)
                {
                    notaAtualizada = new Nota(id, notaExistente.TarefaId, notaExistente.GrupoId, novoValor);
                }
                else
                {
                    notaAtualizada = new Nota(id, notaExistente.TarefaId, notaExistente.GrupoId, notaExistente.NumeroAluno, novoValor);
                }

                Lista[id] = notaAtualizada;

                DadosAlterados?.Invoke();
                NotaModificada?.Invoke(id, "Atualizada");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao atualizar nota: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove uma nota
        /// </summary>
        public void RemoverNota(int id)
        {
            try
            {
                if (!Lista.ContainsKey(id))
                    throw new OperacaoInvalidaException($"Não existe uma nota com ID {id}.");

                Lista.Remove(id);

                DadosAlterados?.Invoke();
                NotaModificada?.Invoke(id, "Removida");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao remover nota: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém todas as notas para exibição na DataGrid
        /// </summary>
        public List<NotaViewModel> ObterNotasParaExibicao()
        {
            var notasViewModel = new List<NotaViewModel>();

            foreach (var nota in Lista.Values.OrderBy(n => n.TarefaId).ThenBy(n => n.GrupoId))
            {
                var tarefa = modelTarefas?.ObterTarefa(nota.TarefaId);
                var grupo = modelGrupos?.ObterGrupo(nota.GrupoId);

                string nomeTarefa = tarefa?.Titulo ?? $"Tarefa {nota.TarefaId}";
                string nomeGrupo = grupo?.Nome ?? $"Grupo {nota.GrupoId}";
                string alunos = "";

                if (nota.ENotaGrupo)
                {
                    // Para nota de grupo, mostrar todos os alunos do grupo
                    if (grupo != null)
                    {
                        var nomesAlunos = modelGrupos.ObterNomesAlunosDoGrupo(nota.GrupoId);
                        alunos = string.Join(", ", nomesAlunos);
                    }
                }
                else
                {
                    // Para nota individual, mostrar apenas o aluno específico
                    var aluno = modelAlunos?.ObterAluno(nota.NumeroAluno);
                    alunos = aluno?.Nome ?? nota.NumeroAluno;
                }

                notasViewModel.Add(new NotaViewModel
                {
                    Id = nota.Id,
                    TarefaId = nota.TarefaId,
                    NomeTarefa = nomeTarefa,
                    GrupoId = nota.GrupoId,
                    NomeGrupo = nomeGrupo,
                    Alunos = alunos,
                    ValorNota = nota.ValorNota,
                    TipoNota = nota.TipoNota
                });
            }

            return notasViewModel;
        }

        /// <summary>
        /// Obtém todas as tarefas disponíveis
        /// </summary>
        public List<Tarefa> ObterTarefasDisponiveis()
        {
            return modelTarefas?.ObterTodasTarefas() ?? new List<Tarefa>();
        }

        /// <summary>
        /// Obtém todos os grupos disponíveis
        /// </summary>
        public List<Grupo> ObterGruposDisponiveis()
        {
            return modelGrupos?.ObterTodosGrupos() ?? new List<Grupo>();
        }

        /// <summary>
        /// Obtém alunos de um grupo específico
        /// </summary>
        public List<Aluno> ObterAlunosDoGrupo(string grupoId)
        {
            return modelGrupos?.ObterAlunosDoGrupo(grupoId) ?? new List<Aluno>();
        }

        /// <summary>
        /// Pesquisa notas
        /// </summary>
        public List<NotaViewModel> PesquisarNotas(string termoPesquisa)
        {
            if (string.IsNullOrWhiteSpace(termoPesquisa))
                return ObterNotasParaExibicao();

            var todasNotas = ObterNotasParaExibicao();
            var termo = termoPesquisa.ToLower();

            return todasNotas.Where(n =>
                n.NomeTarefa.ToLower().Contains(termo) ||
                n.NomeGrupo.ToLower().Contains(termo) ||
                n.Alunos.ToLower().Contains(termo) ||
                n.TipoNota.ToLower().Contains(termo) ||
                n.ValorNota.ToString().Contains(termo)
            ).ToList();
        }

        /// <summary>
        /// Conta total de notas
        /// </summary>
        public int ContarNotas()
        {
            return Lista.Count;
        }

        #region Métodos de Validação

        private void ValidarDadosComuns(int tarefaId, string grupoId, decimal valorNota)
        {
            // Validar tarefa
            if (modelTarefas == null || !modelTarefas.Lista.ContainsKey(tarefaId))
                throw new OperacaoInvalidaException($"Tarefa com ID {tarefaId} não existe.");

            // Validar grupo
            if (string.IsNullOrWhiteSpace(grupoId))
                throw new OperacaoInvalidaException("ID do grupo não pode estar vazio.");

            if (modelGrupos == null || !modelGrupos.Lista.ContainsKey(grupoId))
                throw new OperacaoInvalidaException($"Grupo com ID {grupoId} não existe.");

            // Validar nota
            ValidarNota(valorNota);
        }

        private void ValidarNota(decimal valorNota)
        {
            if (valorNota < 0 || valorNota > 20)
                throw new OperacaoInvalidaException("A nota deve estar entre 0 e 20 valores.");
        }

        #endregion
    }

    // ViewModel para exibição na DataGrid
    public class NotaViewModel
    {
        public int Id { get; set; }
        public int TarefaId { get; set; }
        public string NomeTarefa { get; set; }
        public string GrupoId { get; set; }
        public string NomeGrupo { get; set; }
        public string Alunos { get; set; }
        public decimal ValorNota { get; set; }
        public string TipoNota { get; set; }
    }
}