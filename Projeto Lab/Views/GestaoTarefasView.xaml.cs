using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class GestaoTarefasView : UserControl
    {
        private App app;
        private ModelTarefas modelTarefas;

        public GestaoTarefasView()
        {
            InitializeComponent();

            // Obtém a instância do App (camada de interligação)
            app = App.Current as App;
            modelTarefas = app.Model_Tarefas;

            // Subscrever aos eventos do Model (padrão MVVM Simplificado)
            modelTarefas.DadosCarregados += Model_Tarefas_DadosCarregados;
            modelTarefas.DadosAlterados += Model_Tarefas_DadosAlterados;
            modelTarefas.ImportacaoTerminada += Model_Tarefas_ImportacaoTerminada;
            modelTarefas.ExportacaoTerminada += Model_Tarefas_ExportacaoTerminada;
            modelTarefas.ErroOcorrido += Model_Tarefas_ErroOcorrido;
            modelTarefas.TarefaModificada += Model_Tarefas_TarefaModificada;

            // Configurar eventos da interface
            ConfigurarEventosInterface();

            // Carregar dados iniciais
            AtualizarDataGrid();
        }

        private void ConfigurarEventosInterface()
        {
            // Event handlers dos botões
            btnNovaTarefa.Click += BtnNovaTarefa_Click;
            btnEditarTarefa.Click += BtnEditarTarefa_Click;
            btnRemoverTarefa.Click += BtnRemoverTarefa_Click;

            // Event handlers da DataGrid
            dgTarefas.SelectionChanged += DgTarefas_SelectionChanged;

            // Event handlers da SearchBar
            tbPesquisa.GotFocus += TbPesquisa_GotFocus;
            tbPesquisa.LostFocus += TbPesquisa_LostFocus;
            tbPesquisa.TextChanged += TbPesquisa_TextChanged;

            // Estado inicial dos botões
            btnEditarTarefa.IsEnabled = false;
            btnRemoverTarefa.IsEnabled = false;
        }

        #region Event Handlers do Model (MVVM Pattern)

        private void Model_Tarefas_DadosCarregados()
        {
            AtualizarDataGrid();
        }

        private void Model_Tarefas_DadosAlterados()
        {
            AtualizarDataGrid();
        }

        private void Model_Tarefas_ImportacaoTerminada()
        {
            MessageBox.Show($"Importação concluída com sucesso!\n\nTotal de tarefas: {modelTarefas.ContarTarefas()}",
                          "Importação realizada",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void Model_Tarefas_ExportacaoTerminada()
        {
            MessageBox.Show("Dados exportados com sucesso!",
                          "Exportação realizada",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
        }

        private void Model_Tarefas_ErroOcorrido(string mensagemErro)
        {
            MessageBox.Show(mensagemErro,
                          "Erro",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
        }

        private void Model_Tarefas_TarefaModificada(int idTarefa, string operacao)
        {
            // Atualizar interface quando tarefa é modificada
            AtualizarDataGrid();

            // Desabilitar botões se necessário
            if (operacao == "Removida")
            {
                btnEditarTarefa.IsEnabled = false;
                btnRemoverTarefa.IsEnabled = false;
            }
        }

        #endregion

        #region Event Handlers da Interface

        private void BtnNovaTarefa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Criar a janela de nova tarefa
                var janela = new NovaTarefaWindow();

                // Mostrar a janela como modal
                bool? resultado = janela.ShowDialog();

                // Se o usuário criou uma tarefa com sucesso
                if (resultado == true)
                {
                    // Os dados já foram salvos no modelo pela janela
                    // A interface será atualizada automaticamente via eventos
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir janela de criação: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BtnEditarTarefa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tarefaSelecionada = dgTarefas.SelectedItem as Tarefa;
                if (tarefaSelecionada == null)
                {
                    MessageBox.Show("Por favor, selecione uma tarefa para editar.",
                                  "Nenhuma tarefa selecionada",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                // Criar a janela de editar tarefa
                var janela = new EditarTarefaWindow(tarefaSelecionada);

                // Mostrar a janela como modal
                bool? resultado = janela.ShowDialog();

                // Se o usuário editou a tarefa com sucesso
                if (resultado == true)
                {
                    // Os dados já foram salvos no modelo pela janela
                    // A interface será atualizada automaticamente via eventos
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir janela de edição: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BtnRemoverTarefa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tarefaSelecionada = dgTarefas.SelectedItem as Tarefa;
                if (tarefaSelecionada == null)
                {
                    MessageBox.Show("Por favor, selecione uma tarefa para remover.",
                                  "Nenhuma tarefa selecionada",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                var resultado = MessageBox.Show($"Tem certeza que deseja remover a tarefa '{tarefaSelecionada.Titulo}'?",
                                              "Confirmar remoção",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Delegar ao Model a remoção da tarefa
                    modelTarefas.RemoverTarefa(tarefaSelecionada.Id);

                    MessageBox.Show($"Tarefa '{tarefaSelecionada.Titulo}' removida com sucesso!",
                                  "Tarefa removida",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao remover tarefa",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void DgTarefas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Habilitar ou desabilitar os botões de editar e remover com base na seleção
            btnEditarTarefa.IsEnabled = dgTarefas.SelectedItem != null;
            btnRemoverTarefa.IsEnabled = dgTarefas.SelectedItem != null;
        }

        #endregion

        #region SearchBar (Pesquisa)

        private void TbPesquisa_GotFocus(object sender, RoutedEventArgs e)
        {
            // Limpar o texto de placeholder quando o campo recebe foco
            if (tbPesquisa.Text == "Pesquisar Tarefas...")
            {
                tbPesquisa.Text = "";
                tbPesquisa.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TbPesquisa_LostFocus(object sender, RoutedEventArgs e)
        {
            // Restaurar o texto de placeholder se o campo estiver vazio
            if (string.IsNullOrWhiteSpace(tbPesquisa.Text))
            {
                tbPesquisa.Text = "Pesquisar Tarefas...";
                tbPesquisa.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void TbPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Ignorar se o texto for o placeholder
                if (tbPesquisa.Text == "Pesquisar Tarefas..." || string.IsNullOrWhiteSpace(tbPesquisa.Text))
                {
                    AtualizarDataGrid();
                    return;
                }

                // Delegar ao Model a pesquisa
                var resultados = modelTarefas.PesquisarTarefas(tbPesquisa.Text);
                dgTarefas.ItemsSource = resultados;
            }
            catch (Exception ex)
            {
                // Em caso de erro na pesquisa, mostrar todas as tarefas
                AtualizarDataGrid();
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Atualiza a DataGrid com todas as tarefas do Model
        /// </summary>
        private void AtualizarDataGrid()
        {
            dgTarefas.ItemsSource = modelTarefas.ObterTodasTarefas();

            // Atualizar título da janela com informação de peso (se disponível)
            AtualizarInformacoesPeso();
        }

        /// <summary>
        /// Atualiza informações de peso no cabeçalho (se necessário)
        /// </summary>
        private void AtualizarInformacoesPeso()
        {
            try
            {
                // Você pode usar isso para mostrar info no cabeçalho se quiser
                var relatorio = modelTarefas.ObterRelatorioPesos();
                // Exemplo: poderia atualizar um TextBlock no cabeçalho
                // tbRelatorioPeso.Text = relatorio;
            }
            catch
            {
                // Ignorar erros de atualização de interface
            }
        }

        #endregion
    }
}