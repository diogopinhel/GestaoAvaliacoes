using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Models
{
    public class ModelGrupos
    {
        // Estrutura de dados principal
        public Dictionary<string, Grupo> Lista { get; private set; }

        // Referência ao ModelAlunos para buscar dados dos alunos
        private readonly ModelAlunos modelAlunos;

        // Eventos para comunicação com as Views
        public event MetodosSemParametros DadosCarregados;
        public event MetodosSemParametros DadosAlterados;
        public event MetodosSemParametros ImportacaoTerminada;
        public event MetodosSemParametros ExportacaoTerminada;
        public event MetodosComErro ErroOcorrido;
        public event GrupoAlterado GrupoModificado;

        public ModelGrupos(ModelAlunos modelAlunos)
        {
            Lista = new Dictionary<string, Grupo>();
            this.modelAlunos = modelAlunos;

            // Inscrever no evento AlunoModificado para detectar quando um aluno é removido
            if (modelAlunos != null)
            {
                modelAlunos.AlunoModificado += AlunoModificadoHandler;
            }

            CarregarDadosExemplo();
        }

        // Handler para o evento AlunoModificado do ModelAlunos
        private void AlunoModificadoHandler(string numeroAluno, string operacao)
        {
            if (operacao == "Removido")
            {
                // Se um aluno foi removido, remover de todos os grupos
                RemoverAlunoDeGrupos(numeroAluno);
            }
        }

        // Método para remover um aluno de todos os grupos
        public void RemoverAlunoDeGrupos(string numeroAluno)
        {
            bool alteracaoFeita = false;

            foreach (var grupo in Lista.Values)
            {
                if (grupo.ContemAluno(numeroAluno))
                {
                    grupo.RemoverAluno(numeroAluno);
                    alteracaoFeita = true;
                    GrupoModificado?.Invoke(grupo.Id, "AlunoRemovido");
                }
            }

            if (alteracaoFeita)
            {
                DadosAlterados?.Invoke();
            }
        }

        /// <summary>
        /// Carrega dados de exemplo para demonstração
        /// </summary>
        private void CarregarDadosExemplo()
        {
            var gruposExemplo = new List<Grupo>
                    {
                        new Grupo("1", "Os Reis", "Grupo principal"),
                        new Grupo("2", "As Rainhas", "Grupo secundário"),
                        new Grupo("3", "Os Magos", "Grupo de desenvolvimento")
                    };

            // Adicionar alguns alunos aos grupos (se existirem)
            if (modelAlunos != null && modelAlunos.Lista.Count > 0)
            {
                var alunosDisponiveis = modelAlunos.Lista.Keys.ToList();

                if (alunosDisponiveis.Count >= 3)
                {
                    for (int i = 0; i < Math.Min(3, alunosDisponiveis.Count); i++)
                    {
                        gruposExemplo[0].AdicionarAluno(alunosDisponiveis[i]);
                    }
                }
            }

            Lista.Clear();
            foreach (var grupo in gruposExemplo)
            {
                Lista.Add(grupo.Id, grupo);
            }

            DadosCarregados?.Invoke();
        }

        /// <summary>
        /// Adiciona um novo grupo
        /// </summary>
        public void AdicionarGrupo(string id, string nome, string descricao = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new OperacaoInvalidaException("ID do grupo não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(nome))
                    throw new OperacaoInvalidaException("Nome do grupo não pode estar vazio.");

                if (Lista.ContainsKey(id))
                    throw new OperacaoInvalidaException($"Já existe um grupo com o ID {id}.");

                var novoGrupo = new Grupo(id, nome, descricao);
                Lista.Add(id, novoGrupo);

                DadosAlterados?.Invoke();
                GrupoModificado?.Invoke(id, "Adicionado");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao adicionar grupo: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove um grupo
        /// </summary>
        public void RemoverGrupo(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new OperacaoInvalidaException("ID do grupo não pode estar vazio.");

                if (!Lista.ContainsKey(id))
                    throw new OperacaoInvalidaException($"Não existe um grupo com ID {id}.");

                Lista.Remove(id);

                DadosAlterados?.Invoke();
                GrupoModificado?.Invoke(id, "Removido");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao remover grupo: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza um grupo existente
        /// </summary>
        public void AtualizarGrupo(string id, string novoNome, string novaDescricao = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new OperacaoInvalidaException("ID do grupo não pode estar vazio.");

                if (!Lista.ContainsKey(id))
                    throw new OperacaoInvalidaException($"Não existe um grupo com ID {id}.");

                if (string.IsNullOrWhiteSpace(novoNome))
                    throw new OperacaoInvalidaException("Nome do grupo não pode estar vazio.");

                var grupo = Lista[id];
                grupo.Nome = novoNome;
                grupo.Descricao = novaDescricao ?? string.Empty;

                DadosAlterados?.Invoke();
                GrupoModificado?.Invoke(id, "Atualizado");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao atualizar grupo: {ex.Message}");
            }
        }

        /// <summary>
        /// Adiciona um aluno a um grupo
        /// </summary>
        public void AdicionarAlunoAoGrupo(string idGrupo, string numeroAluno)
        {
            try
            {
                if (!Lista.ContainsKey(idGrupo))
                    throw new OperacaoInvalidaException($"Grupo com ID {idGrupo} não existe.");

                if (modelAlunos == null || !modelAlunos.Lista.ContainsKey(numeroAluno))
                    throw new OperacaoInvalidaException($"Aluno com número {numeroAluno} não existe.");

                // Verificar se o aluno já está em outro grupo
                var grupoAtual = EncontrarGrupoDoAluno(numeroAluno);
                if (grupoAtual != null && grupoAtual.Id != idGrupo)
                    throw new OperacaoInvalidaException($"Aluno já está no grupo '{grupoAtual.Nome}'.");

                Lista[idGrupo].AdicionarAluno(numeroAluno);

                DadosAlterados?.Invoke();
                GrupoModificado?.Invoke(idGrupo, "AlunoAdicionado");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao adicionar aluno ao grupo: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove um aluno de um grupo
        /// </summary>
        public void RemoverAlunoDoGrupo(string idGrupo, string numeroAluno)
        {
            try
            {
                if (!Lista.ContainsKey(idGrupo))
                    throw new OperacaoInvalidaException($"Grupo com ID {idGrupo} não existe.");

                Lista[idGrupo].RemoverAluno(numeroAluno);

                DadosAlterados?.Invoke();
                GrupoModificado?.Invoke(idGrupo, "AlunoRemovido");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao remover aluno do grupo: {ex.Message}");
            }
        }

        /// <summary>
        /// Encontra o grupo que contém um determinado aluno
        /// </summary>
        public Grupo EncontrarGrupoDoAluno(string numeroAluno)
        {
            return Lista.Values.FirstOrDefault(g => g.ContemAluno(numeroAluno));
        }

        /// <summary>
        /// Obtém alunos que não estão em nenhum grupo
        /// </summary>
        public List<Aluno> ObterAlunosDisponiveis()
        {
            if (modelAlunos == null)
                return new List<Aluno>();

            var alunosEmGrupos = Lista.Values
                .SelectMany(g => g.NumerosAlunos)
                .ToHashSet();

            return modelAlunos.Lista.Values
                .Where(a => !alunosEmGrupos.Contains(a.Numero))
                .ToList();
        }

        /// <summary>
        /// Obtém nomes dos alunos de um grupo
        /// </summary>
        public List<string> ObterNomesAlunosDoGrupo(string idGrupo)
        {
            if (!Lista.ContainsKey(idGrupo) || modelAlunos == null)
                return new List<string>();

            var grupo = Lista[idGrupo];
            var nomes = new List<string>();

            foreach (var numeroAluno in grupo.NumerosAlunos)
            {
                if (modelAlunos.Lista.TryGetValue(numeroAluno, out Aluno aluno))
                {
                    nomes.Add(aluno.Nome);
                }
            }

            return nomes;
        }

        /// <summary>
        /// Obtém todos os alunos de um grupo
        /// </summary>
        public List<Aluno> ObterAlunosDoGrupo(string idGrupo)
        {
            if (!Lista.ContainsKey(idGrupo) || modelAlunos == null)
                return new List<Aluno>();

            var grupo = Lista[idGrupo];
            var alunos = new List<Aluno>();

            foreach (var numeroAluno in grupo.NumerosAlunos)
            {
                if (modelAlunos.Lista.TryGetValue(numeroAluno, out Aluno aluno))
                {
                    alunos.Add(aluno);
                }
            }

            return alunos;
        }

        /// <summary>
        /// Obtém um grupo pelo ID
        /// </summary>
        public Grupo ObterGrupo(string id)
        {
            Lista.TryGetValue(id, out Grupo grupo);
            return grupo;
        }

        /// <summary>
        /// Obtém todos os grupos
        /// </summary>
        public List<Grupo> ObterTodosGrupos()
        {
            return Lista.Values.ToList();
        }

        /// <summary>
        /// Pesquisa grupos por nome ou ID
        /// </summary>
        public List<Grupo> PesquisarGrupos(string termoPesquisa)
        {
            if (string.IsNullOrWhiteSpace(termoPesquisa))
                return ObterTodosGrupos();

            var termo = termoPesquisa.ToLower();
            return Lista.Values.Where(g =>
                (g.Nome != null && g.Nome.ToLower().Contains(termo)) ||
                (g.Id != null && g.Id.ToLower().Contains(termo)) ||
                (g.Descricao != null && g.Descricao.ToLower().Contains(termo))
            ).ToList();
        }

        /// <summary>
        /// Gera próximo ID disponível
        /// </summary>
        public string GerarProximoId()
        {
            if (Lista.Count == 0)
                return "1";

            // Encontrar o maior ID numérico e somar 1
            var idsNumericos = Lista.Keys
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(0);

            return (idsNumericos.Max() + 1).ToString();
        }

        /// <summary>
        /// Conta total de grupos
        /// </summary>
        public int ContarGrupos()
        {
            return Lista.Count;
        }
    }
}