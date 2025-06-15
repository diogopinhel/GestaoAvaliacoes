using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Models
{
    public class ModelTarefas
    {
        // Estrutura de dados principal - Dictionary para acesso rápido por ID
        public Dictionary<int, Tarefa> Lista { get; private set; }

        // Contador para gerar IDs únicos
        private int proximoId;

        // Eventos para comunicação com as Views
        public event MetodosSemParametros DadosCarregados;
        public event MetodosSemParametros DadosAlterados;
        public event MetodosSemParametros ImportacaoTerminada;
        public event MetodosSemParametros ExportacaoTerminada;
        public event MetodosComErro ErroOcorrido;
        public event TarefaAlterada TarefaModificada;

        public ModelTarefas()
        {
            Lista = new Dictionary<int, Tarefa>();
            proximoId = 1;
        }
        public void AdicionarTarefa(string titulo, string descricao, string dataHoraInicio, string dataHoraFim, string peso)
        {
            try
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(titulo))
                    throw new OperacaoInvalidaException("Título da tarefa não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(dataHoraInicio))
                    throw new OperacaoInvalidaException("Data/hora de início não pode estar vazia.");

                if (string.IsNullOrWhiteSpace(dataHoraFim))
                    throw new OperacaoInvalidaException("Data/hora de fim não pode estar vazia.");

                if (string.IsNullOrWhiteSpace(peso))
                    throw new OperacaoInvalidaException("Peso da tarefa não pode estar vazio.");

                // Validar formato das datas
                ValidarFormatoDataHora(dataHoraInicio, "Data/hora de início");
                ValidarFormatoDataHora(dataHoraFim, "Data/hora de fim");

                // Validar peso
                ValidarPeso(peso);

                // Validar se data fim é posterior à data início
                ValidarOrdemDatas(dataHoraInicio, dataHoraFim);

                // VALIDAÇÃO CRÍTICA: Verificar se o peso total não excede 100%
                ValidarPesoTotal(peso, null);

                // Criar e adicionar a tarefa
                var novaTarefa = new Tarefa(proximoId, titulo, descricao ?? string.Empty, dataHoraInicio, dataHoraFim, peso);
                Lista.Add(proximoId, novaTarefa);
                proximoId++;

                // Notificar alteração
                DadosAlterados?.Invoke();
                TarefaModificada?.Invoke(novaTarefa.Id, "Adicionada");
            }
            catch (OperacaoInvalidaException)
            {
                throw; // Re-throw para manter a exceção específica
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao adicionar tarefa: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza uma tarefa existente
        /// </summary>
        public void AtualizarTarefa(int id, string titulo, string descricao, string dataHoraInicio, string dataHoraFim, string peso)
        {
            try
            {
                if (!Lista.ContainsKey(id))
                    throw new OperacaoInvalidaException($"Não existe uma tarefa com ID {id}.");

                // Validações básicas
                if (string.IsNullOrWhiteSpace(titulo))
                    throw new OperacaoInvalidaException("Título da tarefa não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(dataHoraInicio))
                    throw new OperacaoInvalidaException("Data/hora de início não pode estar vazia.");

                if (string.IsNullOrWhiteSpace(dataHoraFim))
                    throw new OperacaoInvalidaException("Data/hora de fim não pode estar vazia.");

                if (string.IsNullOrWhiteSpace(peso))
                    throw new OperacaoInvalidaException("Peso da tarefa não pode estar vazio.");

                // Validar formato das datas
                ValidarFormatoDataHora(dataHoraInicio, "Data/hora de início");
                ValidarFormatoDataHora(dataHoraFim, "Data/hora de fim");

                // Validar peso
                ValidarPeso(peso);

                // Validar se data fim é posterior à data início
                ValidarOrdemDatas(dataHoraInicio, dataHoraFim);

                // VALIDAÇÃO: Verificar se o peso total não excede 100% (excluindo a tarefa atual)
                ValidarPesoTotal(peso, id);

                // Criar tarefa atualizada
                var tarefaAtualizada = new Tarefa(id, titulo, descricao ?? string.Empty, dataHoraInicio, dataHoraFim, peso);
                Lista[id] = tarefaAtualizada;

                // Notificar alteração
                DadosAlterados?.Invoke();
                TarefaModificada?.Invoke(id, "Atualizada");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao atualizar tarefa: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove uma tarefa da lista
        /// </summary>
        public void RemoverTarefa(int id)
        {
            try
            {
                if (!Lista.ContainsKey(id))
                    throw new OperacaoInvalidaException($"Não existe uma tarefa com ID {id}.");

                Lista.Remove(id);

                // Notificar alteração
                DadosAlterados?.Invoke();
                TarefaModificada?.Invoke(id, "Removida");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao remover tarefa: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém uma tarefa pelo ID
        /// </summary>
        public Tarefa ObterTarefa(int id)
        {
            Lista.TryGetValue(id, out Tarefa tarefa);
            return tarefa;
        }

        /// <summary>
        /// Obtém todas as tarefas como lista
        /// </summary>
        public List<Tarefa> ObterTodasTarefas()
        {
            return Lista.Values.OrderBy(t => t.Id).ToList();
        }

        /// <summary>
        /// Pesquisa tarefas por título ou ID
        /// </summary>
        public List<Tarefa> PesquisarTarefas(string termoPesquisa)
        {
            if (string.IsNullOrWhiteSpace(termoPesquisa))
                return ObterTodasTarefas();

            var termo = termoPesquisa.ToLower();
            return Lista.Values.Where(t =>
                // Pesquisa por ID (exato - só números que começam com o texto)
                t.Id.ToString().StartsWith(termo) ||
                // Pesquisa por Título (início das palavras)
                (t.Titulo != null && PesquisaInteligenteTexto(t.Titulo, termo))
            ).OrderBy(t => t.Id).ToList();
        }

        /// <summary>
        /// Obtém tarefas ativas (em andamento)
        /// </summary>
        public List<Tarefa> ObterTarefasAtivas()
        {
            return Lista.Values.Where(t => t.EstaAtiva).OrderBy(t => t.Id).ToList();
        }

        /// <summary>
        /// Obtém tarefas vencidas
        /// </summary>
        public List<Tarefa> ObterTarefasVencidas()
        {
            return Lista.Values.Where(t => t.EstaVencida).OrderBy(t => t.Id).ToList();
        }

        /// <summary>
        /// Calcula o peso total de todas as tarefas
        /// </summary>
        public int CalcularPesoTotal()
        {
            return Lista.Values.Sum(t => t.PesoNumerico);
        }

        /// <summary>
        /// Calcula quanto peso ainda está disponível (100% - peso total atual)
        /// </summary>
        public int CalcularPesoDisponivel()
        {
            return 100 - CalcularPesoTotal();
        }

        /// <summary>
        /// Verifica se o peso total das tarefas é válido (≤ 100%)
        /// </summary>
        public bool PesoTotalValido()
        {
            return CalcularPesoTotal() <= 100;
        }

        /// <summary>
        /// Obtém um relatório do peso das tarefas
        /// </summary>
        public string ObterRelatorioPesos()
        {
            var pesoTotal = CalcularPesoTotal();
            var pesoDisponivel = CalcularPesoDisponivel();
            var status = pesoTotal <= 100 ? "✅ Válido" : "❌ Excedido";

            return $"Peso Total: {pesoTotal}% | Disponível: {pesoDisponivel}% | Status: {status}";
        }

        /// <summary>
        /// Limpa todos os dados
        /// </summary>
        public void LimparDados()
        {
            Lista.Clear();
            proximoId = 1;
            DadosAlterados?.Invoke();
        }

        /// <summary>
        /// Conta total de tarefas
        /// </summary>
        public int ContarTarefas()
        {
            return Lista.Count;
        }

        #region Métodos de Validação

        private void ValidarFormatoDataHora(string dataHora, string campo)
        {
            if (string.IsNullOrWhiteSpace(dataHora))
                throw new OperacaoInvalidaException($"{campo} não pode estar vazia.");

            // Aceitar formatos: dd/MM/yyyy HH:mm ou dd/MM/yyyy
            bool formatoValido = DateTime.TryParseExact(dataHora, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out _) ||
                                DateTime.TryParseExact(dataHora, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _);

            if (!formatoValido)
                throw new OperacaoInvalidaException($"{campo} deve estar no formato dd/MM/yyyy HH:mm ou dd/MM/yyyy.");
        }

        private void ValidarPeso(string peso)
        {
            if (string.IsNullOrWhiteSpace(peso))
                throw new OperacaoInvalidaException("Peso não pode estar vazio.");

            string pesoLimpo = peso.Replace("%", "").Trim();

            if (!int.TryParse(pesoLimpo, out int pesoNumerico))
                throw new OperacaoInvalidaException("Peso deve ser um número válido.");

            if (pesoNumerico <= 0 || pesoNumerico > 100)
                throw new OperacaoInvalidaException("Peso deve estar entre 1 e 100.");
        }

        /// <summary>
        /// Valida se o peso total das tarefas não excede 100%
        /// </summary>
        /// <param name="novoPeso">Peso da nova tarefa ou tarefa editada</param>
        /// <param name="idTarefaEditada">ID da tarefa sendo editada (null se for nova tarefa)</param>
        private void ValidarPesoTotal(string novoPeso, int? idTarefaEditada)
        {
            try
            {
                // Extrair valor numérico do novo peso
                string pesoLimpo = novoPeso.Replace("%", "").Trim();
                if (!int.TryParse(pesoLimpo, out int pesoNumerico))
                    return; // Se não conseguir converter, deixa passar (será validado em ValidarPeso)

                // Calcular peso total atual (excluindo a tarefa sendo editada, se aplicável)
                int pesoTotalAtual = 0;
                foreach (var tarefa in Lista.Values)
                {
                    // Se estivermos editando uma tarefa, excluir seu peso atual do cálculo
                    if (idTarefaEditada.HasValue && tarefa.Id == idTarefaEditada.Value)
                        continue;

                    pesoTotalAtual += tarefa.PesoNumerico;
                }

                // Verificar se o novo peso causaria excesso
                int pesoTotalComNovo = pesoTotalAtual + pesoNumerico;

                if (pesoTotalComNovo > 100)
                {
                    string operacao = idTarefaEditada.HasValue ? "atualizar" : "adicionar";
                    throw new OperacaoInvalidaException(
                        $"Não é possível {operacao} esta tarefa.\n\n" +
                        $"Peso atual total das tarefas: {pesoTotalAtual}%\n" +
                        $"Peso da tarefa: {pesoNumerico}%\n" +
                        $"Total resultante: {pesoTotalComNovo}%\n\n" +
                        $"O peso total de todas as tarefas não pode exceder 100%.\n" +
                        $"Peso disponível: {100 - pesoTotalAtual}%");
                }
            }
            catch (OperacaoInvalidaException)
            {
                throw; // Re-lançar exceções de validação
            }
            catch (Exception)
            {
                // Se houver erro na validação, deixa passar
                // O erro será capturado nas outras validações
            }
        }

        private void ValidarOrdemDatas(string dataInicio, string dataFim)
        {
            try
            {
                DateTime inicio, fim;

                // Tentar converter as datas
                bool inicioConvertido = DateTime.TryParseExact(dataInicio, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out inicio) ||
                                       DateTime.TryParseExact(dataInicio, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out inicio);

                bool fimConvertido = DateTime.TryParseExact(dataFim, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out fim) ||
                                    DateTime.TryParseExact(dataFim, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fim);

                if (inicioConvertido && fimConvertido && fim < inicio)
                    throw new OperacaoInvalidaException("A data de fim deve ser posterior à data de início.");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch
            {
                // Se não conseguir converter, deixa passar (será validado na validação de formato)
            }
        }

        private bool PesquisaInteligenteTexto(string textoCompleto, string textoPesquisa)
        {
            if (string.IsNullOrEmpty(textoCompleto))
                return false;

            var textoLower = textoCompleto.ToLower();

            // Verifica se o texto completo começa com a pesquisa
            if (textoLower.StartsWith(textoPesquisa))
                return true;

            // Lista de palavras a ignorar (artigos, preposições, etc.)
            var palavrasIgnorar = new HashSet<string> { "de", "do", "da", "dos", "das", "para", "com", "em", "no", "na", "nos", "nas", "um", "uma", "uns", "umas", "o", "a", "os", "as" };

            // Verifica se alguma palavra significativa dentro do texto começa com a pesquisa
            var palavras = textoLower.Split(new char[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            return palavras.Where(palavra => !palavrasIgnorar.Contains(palavra))
                          .Any(palavra => palavra.StartsWith(textoPesquisa));
        }

        #endregion
    }
}