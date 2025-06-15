using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class GestaoNotasView : UserControl
    {
        private App app;
        private ModelNotas modelNotas;
        private ModelTarefas modelTarefas;
        private ModelGrupos modelGrupos;

        private Tarefa tarefaSelecionada;
        private List<GrupoAvaliacaoViewModel> gruposAvaliacao;

        public GestaoNotasView()
        {
            InitializeComponent();
            InicializarView();
        }

        #region Inicialização

        private void InicializarView()
        {
            try
            {
                // Obter referências dos models
                app = App.Current as App;
                modelNotas = app.Model_Notas;
                modelTarefas = app.Model_Tarefas;
                modelGrupos = app.Model_Grupos;

                // Subscrever TODOS os eventos necessários
                SubscreverEventos();

                // Carregar dados iniciais
                CarregarDadosIniciais();
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao inicializar gestão de notas: {ex.Message}");
            }
        }

        // Método dedicado para subscrever eventos
        private void SubscreverEventos()
        {
            try
            {
                // Eventos do ModelNotas
                if (modelNotas != null)
                {
                    modelNotas.DadosCarregados += OnDadosAlterados;
                    modelNotas.DadosAlterados += OnDadosAlterados;
                    modelNotas.NotaModificada += OnNotaModificada;
                }

                // Eventos do ModelTarefas
                if (modelTarefas != null)
                {
                    modelTarefas.DadosCarregados += OnTarefasAlteradas;
                    modelTarefas.DadosAlterados += OnTarefasAlteradas;
                    modelTarefas.TarefaModificada += OnTarefaModificada;
                }

                // Eventos do ModelGrupos
                if (modelGrupos != null)
                {
                    modelGrupos.DadosCarregados += OnGruposAlterados;
                    modelGrupos.DadosAlterados += OnGruposAlterados;
                    modelGrupos.GrupoModificado += OnGrupoModificado;
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao subscrever eventos: {ex.Message}");
            }
        }

        private void CarregarDadosIniciais()
        {
            CarregarTarefasNoComboBox();
            AtualizarEstatisticasGerais();
            LimparSelecaoTarefa();
        }

        private void CarregarTarefasNoComboBox()
        {
            try
            {
                // Sempre buscar dados atualizados do model
                var tarefas = modelTarefas?.ObterTodasTarefas() ?? new List<Tarefa>();

                // Preservar seleção atual se possível
                var tarefaSelecionadaId = tarefaSelecionada?.Id;

                cbTarefas.ItemsSource = null; // Limpar primeiro
                cbTarefas.ItemsSource = tarefas;

                // Restaurar seleção se a tarefa ainda existir
                if (tarefaSelecionadaId.HasValue)
                {
                    var tarefaExistente = tarefas.FirstOrDefault(t => t.Id == tarefaSelecionadaId.Value);
                    if (tarefaExistente != null)
                    {
                        cbTarefas.SelectedItem = tarefaExistente;
                        tarefaSelecionada = tarefaExistente;
                    }
                    else
                    {
                        // A tarefa foi removida, limpar seleção
                        cbTarefas.SelectedIndex = -1;
                        tarefaSelecionada = null;
                        LimparSelecaoTarefa();
                    }
                }
                else
                {
                    cbTarefas.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao carregar tarefas: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers - Models

        // Handler específico para alterações em tarefas
        private void OnTarefasAlteradas()
        {
            Dispatcher.Invoke(() =>
            {
                CarregarTarefasNoComboBox();
                AtualizarEstatisticasGerais();

                // Se há uma tarefa selecionada, atualizar suas informações
                if (tarefaSelecionada != null)
                {
                    // Buscar a tarefa atualizada
                    var tarefaAtualizada = modelTarefas?.ObterTarefa(tarefaSelecionada.Id);
                    if (tarefaAtualizada != null)
                    {
                        tarefaSelecionada = tarefaAtualizada;
                        AtualizarInformacoesTarefa();
                    }
                }
            });
        }

        // Handler específico para modificação de tarefa individual
        private void OnTarefaModificada(int idTarefa, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                CarregarTarefasNoComboBox();
                AtualizarEstatisticasGerais();

                if (operacao == "Removida" && tarefaSelecionada?.Id == idTarefa)
                {
                    // A tarefa selecionada foi removida
                    tarefaSelecionada = null;
                    LimparSelecaoTarefa();
                }
                else if (tarefaSelecionada?.Id == idTarefa)
                {
                    // A tarefa selecionada foi atualizada
                    var tarefaAtualizada = modelTarefas?.ObterTarefa(idTarefa);
                    if (tarefaAtualizada != null)
                    {
                        tarefaSelecionada = tarefaAtualizada;
                        AtualizarInformacoesTarefa();
                    }
                }
            });
        }

        private void OnDadosAlterados()
        {
            Dispatcher.Invoke(() =>
            {
                AtualizarEstatisticasGerais();
                if (tarefaSelecionada != null)
                {
                    CarregarGruposDaTarefa();
                }
            });
        }

        private void OnNotaModificada(int idNota, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                AtualizarEstatisticasGerais();
                if (tarefaSelecionada != null)
                {
                    AtualizarInformacoesTarefa();
                    CarregarGruposDaTarefa();
                }
            });
        }

        // ✅ NOVO: Handlers para grupos
        private void OnGruposAlterados()
        {
            Dispatcher.Invoke(() =>
            {
                if (tarefaSelecionada != null)
                {
                    CarregarGruposDaTarefa();
                    AtualizarInformacoesTarefa();
                }
            });
        }

        private void OnGrupoModificado(string idGrupo, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                if (tarefaSelecionada != null)
                {
                    CarregarGruposDaTarefa();
                    AtualizarInformacoesTarefa();
                }
            });
        }

        #endregion

        #region Event Handlers - UI

        private void CbTarefas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                tarefaSelecionada = cbTarefas.SelectedItem as Tarefa;

                if (tarefaSelecionada != null)
                {
                    AtualizarInformacoesTarefa();
                    CarregarGruposDaTarefa();

                    // Mostrar tabela e esconder mensagem vazia
                    dgGrupos.Visibility = Visibility.Visible;
                    spMensagemVazia.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LimparSelecaoTarefa();
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao selecionar tarefa: {ex.Message}");
            }
        }

        private void BtnAcaoGrupo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var grupoViewModel = button?.Tag as GrupoAvaliacaoViewModel;

                if (grupoViewModel == null || tarefaSelecionada == null)
                    return;

                if (grupoViewModel.Estado == "PENDENTE")
                {
                    AvaliarGrupo(grupoViewModel);
                }
                else
                {
                    EditarAvaliacaoGrupo(grupoViewModel);
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro na ação do grupo: {ex.Message}");
            }
        }

        #endregion

        #region Métodos de Atualização da Interface

        private void AtualizarEstatisticasGerais()
        {
            try
            {
                var totalTarefas = modelTarefas?.ContarTarefas() ?? 0;
                var tarefasComNotas = ObterTarefasComNotas();
                var porAvaliar = totalTarefas - tarefasComNotas;

                tbTotalTarefas.Text = totalTarefas.ToString();
                tbPorAvaliar.Text = porAvaliar.ToString();
                tbConcluidas.Text = tarefasComNotas.ToString();
            }
            catch (Exception)
            {
                tbTotalTarefas.Text = "0";
                tbPorAvaliar.Text = "0";
                tbConcluidas.Text = "0";
            }
        }

        private void AtualizarInformacoesTarefa()
        {
            if (tarefaSelecionada == null) return;

            try
            {
                // Sempre buscar a tarefa mais atualizada
                var tarefaAtualizada = modelTarefas?.ObterTarefa(tarefaSelecionada.Id);
                if (tarefaAtualizada != null)
                {
                    tarefaSelecionada = tarefaAtualizada;
                }

                tbTarefaTitulo.Text = $"📋 {tarefaSelecionada.Titulo}";
                tbTarefaDescricao.Text = string.IsNullOrEmpty(tarefaSelecionada.Descricao)
                    ? "Sem descrição"
                    : tarefaSelecionada.Descricao;

                // Atualizar informações meta
                tbTarefaData.Text = $"📅 {tarefaSelecionada.DataHoraInicio} → {tarefaSelecionada.DataHoraFim}";
                tbTarefaPeso.Text = $"⚖️ Peso: {tarefaSelecionada.Peso}";

                var totalGrupos = modelGrupos?.ContarGrupos() ?? 0;
                tbTarefaGrupos.Text = $"👥 {totalGrupos} grupos";

                var gruposAvaliados = ContarGruposAvaliadosNaTarefa(tarefaSelecionada.Id);
                tbTarefaProgresso.Text = $"📊 {gruposAvaliados}/{totalGrupos} avaliados";

                spTarefaInfo.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                tbTarefaTitulo.Text = "📋 Erro ao carregar tarefa";
                tbTarefaDescricao.Text = "Erro ao carregar informações";
                spTarefaInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void CarregarGruposDaTarefa()
        {
            if (tarefaSelecionada == null) return;

            try
            {
                var todosGrupos = modelGrupos?.ObterTodosGrupos() ?? new List<Grupo>();
                gruposAvaliacao = new List<GrupoAvaliacaoViewModel>();

                var icones = new[] { "🔵", "🟢", "🟡", "🟠", "🟣", "🔴", "⚫", "⚪" };
                int iconIndex = 0;

                foreach (var grupo in todosGrupos)
                {
                    var estado = VerificarEstadoGrupoTarefa(tarefaSelecionada.Id, grupo.Id);
                    var membros = ObterMembrosDoGrupo(grupo.Id);
                    var membrosTexto = $"{string.Join(", ", membros)} ({membros.Count} membros)";

                    gruposAvaliacao.Add(new GrupoAvaliacaoViewModel
                    {
                        GrupoId = grupo.Id,
                        NomeGrupo = grupo.Nome,
                        Estado = estado,
                        MembrosTexto = membrosTexto,
                        IconeGrupo = icones[iconIndex % icones.Length]
                    });

                    iconIndex++;
                }

                dgGrupos.ItemsSource = null; // Limpar primeiro
                dgGrupos.ItemsSource = gruposAvaliacao;
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao carregar grupos: {ex.Message}");
                dgGrupos.ItemsSource = null;
            }
        }

        private void LimparSelecaoTarefa()
        {
            tbTarefaTitulo.Text = "📋 Selecione uma tarefa";
            tbTarefaDescricao.Text = "Escolha uma tarefa na lista para ver os detalhes";
            spTarefaInfo.Visibility = Visibility.Collapsed;

            dgGrupos.ItemsSource = null;
            dgGrupos.Visibility = Visibility.Collapsed;
            spMensagemVazia.Visibility = Visibility.Visible;

            gruposAvaliacao = null;
        }

        #endregion

        #region Métodos de Ação

        private void AvaliarGrupo(GrupoAvaliacaoViewModel grupoViewModel)
        {
            try
            {
                var grupo = modelGrupos?.ObterGrupo(grupoViewModel.GrupoId);
                if (grupo == null)
                {
                    MostrarAviso("Grupo não encontrado.");
                    return;
                }

                var window = new AvaliarGrupoWindow(tarefaSelecionada, grupo);
                window.Owner = Window.GetWindow(this);

                if (window.ShowDialog() == true)
                {
                    AtualizarInformacoesTarefa();
                    CarregarGruposDaTarefa();
                    MostrarSucesso($"Grupo '{grupo.Nome}' avaliado com sucesso!");
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao avaliar grupo: {ex.Message}");
            }
        }

        private void EditarAvaliacaoGrupo(GrupoAvaliacaoViewModel grupoViewModel)
        {
            try
            {
                var nota = BuscarNotaGrupoTarefa(tarefaSelecionada.Id, grupoViewModel.GrupoId);

                if (nota == null)
                {
                    MostrarAviso($"Não foi encontrada uma nota para editar no grupo '{grupoViewModel.NomeGrupo}'.");
                    return;
                }

                var window = new AvaliarGrupoWindow(nota);
                window.Owner = Window.GetWindow(this);

                if (window.ShowDialog() == true)
                {
                    AtualizarInformacoesTarefa();
                    CarregarGruposDaTarefa();
                    MostrarSucesso($"Avaliação do grupo '{grupoViewModel.NomeGrupo}' atualizada com sucesso!");
                }
            }
            catch (Exception ex)
            {
                MostrarErro($"Erro ao editar avaliação: {ex.Message}");
            }
        }

        #endregion

        #region Métodos Auxiliares

        private int ObterTarefasComNotas()
        {
            try
            {
                if (modelNotas?.Lista == null) return 0;

                return modelNotas.Lista.Values
                    .Select(n => n.TarefaId)
                    .Distinct()
                    .Count();
            }
            catch
            {
                return 0;
            }
        }

        private string VerificarEstadoGrupoTarefa(int tarefaId, string grupoId)
        {
            try
            {
                if (modelNotas?.Lista == null) return "PENDENTE";

                bool temNotas = modelNotas.Lista.Values
                    .Any(n => n.TarefaId == tarefaId && n.GrupoId == grupoId);

                return temNotas ? "CONCLUÍDO" : "PENDENTE";
            }
            catch
            {
                return "PENDENTE";
            }
        }

        private int ContarGruposAvaliadosNaTarefa(int tarefaId)
        {
            try
            {
                if (modelNotas?.Lista == null) return 0;

                return modelNotas.Lista.Values
                    .Where(n => n.TarefaId == tarefaId)
                    .Select(n => n.GrupoId)
                    .Distinct()
                    .Count();
            }
            catch
            {
                return 0;
            }
        }

        private List<string> ObterMembrosDoGrupo(string grupoId)
        {
            try
            {
                var grupo = modelGrupos?.ObterGrupo(grupoId);
                if (grupo == null) return new List<string>();

                return modelGrupos.ObterNomesAlunosDoGrupo(grupoId) ?? new List<string>();
            }
            catch
            {
                return new List<string> { "Erro ao carregar membros" };
            }
        }

        private NotaViewModel BuscarNotaGrupoTarefa(int tarefaId, string grupoId)
        {
            try
            {
                var nota = modelNotas?.Lista?.Values
                    .FirstOrDefault(n => n.TarefaId == tarefaId && n.GrupoId == grupoId);

                if (nota == null) return null;

                return new NotaViewModel
                {
                    Id = nota.Id,
                    TarefaId = nota.TarefaId,
                    GrupoId = nota.GrupoId,
                    ValorNota = nota.ValorNota,
                    TipoNota = nota.TipoNota
                };
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Métodos de UI Helper

        private void MostrarErro(string mensagem)
        {
            MessageBox.Show(mensagem, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MostrarAviso(string mensagem)
        {
            MessageBox.Show(mensagem, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MostrarSucesso(string mensagem)
        {
            MessageBox.Show(mensagem, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Cleanup

        // Método melhorado para limpeza
        public void LimparEventos()
        {
            try
            {
                if (modelNotas != null)
                {
                    modelNotas.DadosCarregados -= OnDadosAlterados;
                    modelNotas.DadosAlterados -= OnDadosAlterados;
                    modelNotas.NotaModificada -= OnNotaModificada;
                }

                if (modelTarefas != null)
                {
                    modelTarefas.DadosCarregados -= OnTarefasAlteradas;
                    modelTarefas.DadosAlterados -= OnTarefasAlteradas;
                    modelTarefas.TarefaModificada -= OnTarefaModificada;
                }

                if (modelGrupos != null)
                {
                    modelGrupos.DadosCarregados -= OnGruposAlterados;
                    modelGrupos.DadosAlterados -= OnGruposAlterados;
                    modelGrupos.GrupoModificado -= OnGrupoModificado;
                }
            }
            catch
            {
                // Ignorar erros na limpeza
            }
        }

        #endregion
    }

    #region ViewModel Classes

    public class GrupoAvaliacaoViewModel
    {
        public string GrupoId { get; set; }
        public string NomeGrupo { get; set; }
        public string Estado { get; set; }
        public string MembrosTexto { get; set; }
        public string IconeGrupo { get; set; }
    }

    #endregion
}