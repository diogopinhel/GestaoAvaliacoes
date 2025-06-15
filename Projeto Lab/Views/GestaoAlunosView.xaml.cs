using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class GestaoAlunosView : UserControl
    {
        private App app;
        private ModelAlunos modelAlunos;

        public GestaoAlunosView()
        {
            InitializeComponent();

            // Obtém a instância do App (camada de interligação)
            app = App.Current as App;
            modelAlunos = app.Model_Alunos;

            // Subscrever aos eventos do Model (padrão MVVM Simplificado)
            modelAlunos.DadosCarregados += Model_Alunos_DadosCarregados;
            modelAlunos.DadosAlterados += Model_Alunos_DadosAlterados;
            modelAlunos.ImportacaoTerminada += Model_Alunos_ImportacaoTerminada;
            modelAlunos.ExportacaoTerminada += Model_Alunos_ExportacaoTerminada;
            modelAlunos.ErroOcorrido += Model_Alunos_ErroOcorrido;
            modelAlunos.AlunoModificado += Model_Alunos_AlunoModificado;

            // Configurar eventos da interface
            ConfigurarEventosInterface();

            // Configurar colunas da DataGrid com lápis e cruz
            ConfigurarDataGridComAcoes();

            // Carregar dados iniciais
            AtualizarDataGrid();
        }

        /// <summary>
        /// Configura DataGrid com coluna de ações (editar + remover)
        /// </summary>
        private void ConfigurarDataGridComAcoes()
        {
            try
            {
                // Verificar se a coluna de ações já existe
                var colunaAcoes = dgAlunos.Columns.FirstOrDefault(c => c.Header?.ToString() == "Ações");

                if (colunaAcoes == null)
                {
                    // Criar coluna de ações com lápis e cruz
                    var novaColuna = new DataGridTemplateColumn
                    {
                        Header = "Ações",
                        Width = new DataGridLength(120), // Largura para 2 botões
                        CanUserSort = false,
                        CanUserResize = false
                    };

                    // Criar template da célula com StackPanel para 2 botões
                    var template = new DataTemplate();
                    var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
                    stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                    stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    stackPanelFactory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);

                    // Botão de Editar (Lápis)
                    var buttonEditarFactory = new FrameworkElementFactory(typeof(Button));
                    buttonEditarFactory.SetValue(Button.ContentProperty, "✏️");
                    buttonEditarFactory.SetValue(Button.FontSizeProperty, 16.0);
                    buttonEditarFactory.SetValue(Button.BackgroundProperty, System.Windows.Media.Brushes.Transparent);
                    buttonEditarFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
                    buttonEditarFactory.SetValue(Button.CursorProperty, System.Windows.Input.Cursors.Hand);
                    buttonEditarFactory.SetValue(Button.ToolTipProperty, "Editar aluno");
                    buttonEditarFactory.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    buttonEditarFactory.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                    buttonEditarFactory.SetValue(Button.WidthProperty, 30.0);
                    buttonEditarFactory.SetValue(Button.HeightProperty, 30.0);
                    buttonEditarFactory.SetValue(Button.MarginProperty, new Thickness(0, 0, 8, 0));

                    // Event handler para editar
                    buttonEditarFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEditarAluno_Click));
                    buttonEditarFactory.SetBinding(Button.TagProperty, new System.Windows.Data.Binding("."));

                    // Botão de Remover (Cruz)
                    var buttonRemoverFactory = new FrameworkElementFactory(typeof(Button));
                    buttonRemoverFactory.SetValue(Button.ContentProperty, "❌");
                    buttonRemoverFactory.SetValue(Button.FontSizeProperty, 16.0);
                    buttonRemoverFactory.SetValue(Button.BackgroundProperty, System.Windows.Media.Brushes.Transparent);
                    buttonRemoverFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
                    buttonRemoverFactory.SetValue(Button.CursorProperty, System.Windows.Input.Cursors.Hand);
                    buttonRemoverFactory.SetValue(Button.ToolTipProperty, "Remover aluno");
                    buttonRemoverFactory.SetValue(Button.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    buttonRemoverFactory.SetValue(Button.VerticalAlignmentProperty, VerticalAlignment.Center);
                    buttonRemoverFactory.SetValue(Button.WidthProperty, 30.0);
                    buttonRemoverFactory.SetValue(Button.HeightProperty, 30.0);
                    buttonRemoverFactory.SetValue(Button.MarginProperty, new Thickness(8, 0, 0, 0));

                    // Event handler para remover
                    buttonRemoverFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnRemoverAluno_Click));
                    buttonRemoverFactory.SetBinding(Button.TagProperty, new System.Windows.Data.Binding("."));

                    // Adicionar botões ao StackPanel
                    stackPanelFactory.AppendChild(buttonEditarFactory);
                    stackPanelFactory.AppendChild(buttonRemoverFactory);

                    template.VisualTree = stackPanelFactory;
                    novaColuna.CellTemplate = template;

                    // Adicionar coluna no final
                    dgAlunos.Columns.Add(novaColuna);

                    System.Diagnostics.Debug.WriteLine("✅ Coluna de ações com lápis e cruz adicionada!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao configurar DataGrid: {ex.Message}");
            }
        }

        private void ConfigurarEventosInterface()
        {
            // Event handlers dos botões
            btnAdicionarAluno.Click += BtnAdicionarAluno_Click;
            btnImportarCSV.Click += BtnImportarCSV_Click;
            btnRemoverTodos.Click += BtnRemoverTodos_Click; // Botão remover todos

            // Event handlers da DataGrid
            dgAlunos.SelectionChanged += DgAlunos_SelectionChanged;

            // Event handlers da SearchBar
            tbPesquisa.GotFocus += TbPesquisa_GotFocus;
            tbPesquisa.LostFocus += TbPesquisa_LostFocus;
            tbPesquisa.TextChanged += TbPesquisa_TextChanged;
        }

        #region Event Handlers do Model (MVVM Pattern)

        private void Model_Alunos_DadosCarregados()
        {
            Dispatcher.Invoke(() =>
            {
                AtualizarDataGrid();
                System.Diagnostics.Debug.WriteLine("✅ Dados de alunos carregados na view!");
            });
        }

        private void Model_Alunos_DadosAlterados()
        {
            Dispatcher.Invoke(() =>
            {
                AtualizarDataGrid();
                System.Diagnostics.Debug.WriteLine("🔄 Dados de alunos alterados - view atualizada!");
            });
        }

        private void Model_Alunos_ImportacaoTerminada()
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Importação concluída com sucesso!\n\nTotal de alunos: {modelAlunos.ContarAlunos()}",
                              "Importação realizada",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            });
        }

        private void Model_Alunos_ExportacaoTerminada()
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Dados exportados com sucesso!",
                              "Exportação realizada",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            });
        }

        private void Model_Alunos_ErroOcorrido(string mensagemErro)
        {
            Dispatcher.Invoke(() =>
            {
                // Determinar tipo de mensagem baseado no conteúdo
                MessageBoxImage icone = mensagemErro.Contains("concluída") || mensagemErro.Contains("sucesso")
                                       ? MessageBoxImage.Information
                                       : MessageBoxImage.Error;

                string titulo = mensagemErro.Contains("concluída") || mensagemErro.Contains("sucesso")
                              ? "Informação"
                              : "Erro";

                MessageBox.Show(mensagemErro, titulo, MessageBoxButton.OK, icone);
            });
        }

        private void Model_Alunos_AlunoModificado(string numeroAluno, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                // Atualizar interface quando aluno é modificado
                AtualizarDataGrid();
                System.Diagnostics.Debug.WriteLine($"🔄 Aluno {numeroAluno} {operacao.ToLower()}!");
            });
        }

        #endregion

        #region Event Handlers da Interface

        private void BtnAdicionarAluno_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("➕ Abrindo janela para adicionar aluno...");

                // Criar a janela de novo aluno
                var janela = new AdicionarAlunoWindow();

                // Mostrar a janela como modal
                bool? resultado = janela.ShowDialog();

                System.Diagnostics.Debug.WriteLine($"✅ Janela fechada com resultado: {resultado}");

                // Se o usuário criou um aluno com sucesso
                if (resultado == true)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Aluno adicionado com sucesso!");
                    // Os dados já foram salvos no modelo pela janela
                    // A interface será atualizada automaticamente via eventos
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao abrir janela: {ex.Message}");
                MessageBox.Show($"Erro ao abrir janela de criação: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event handler para o botão de editar (lápis)
        /// </summary>
        private void BtnEditarAluno_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obter aluno do Tag do botão clicado
                var button = sender as Button;
                var alunoSelecionado = button?.Tag as Aluno;

                if (alunoSelecionado == null)
                {
                    MessageBox.Show("Erro ao identificar o aluno para edição.",
                                  "Erro",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✏️ Abrindo janela para editar aluno: {alunoSelecionado.Nome}");

                // Criar a janela de edição
                var janela = new EditarAlunosWindow(alunoSelecionado);

                // Mostrar a janela como modal
                bool? resultado = janela.ShowDialog();

                System.Diagnostics.Debug.WriteLine($"✅ Janela de edição fechada com resultado: {resultado}");

                // Se o usuário guardou as alterações
                if (resultado == true)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Aluno editado com sucesso!");
                    // Os dados já foram salvos no modelo pela janela
                    // A interface será atualizada automaticamente via eventos
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao abrir janela de edição: {ex.Message}");
                MessageBox.Show($"Erro ao abrir janela de edição: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event handler para o botão de remover (cruz)
        /// </summary>
        private void BtnRemoverAluno_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Obter aluno do Tag do botão clicado
                var button = sender as Button;
                var alunoSelecionado = button?.Tag as Aluno;

                if (alunoSelecionado == null)
                {
                    MessageBox.Show("Erro ao identificar o aluno para remoção.",
                                  "Erro",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                var resultado = MessageBox.Show($"Tem certeza que deseja remover o aluno '{alunoSelecionado.Nome}' (Nº {alunoSelecionado.Numero})?",
                                              "Confirmar remoção",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Debug.WriteLine($"🗑️ Removendo aluno: {alunoSelecionado.Nome}");

                    // Delegar ao Model a remoção do aluno
                    modelAlunos.RemoverAluno(alunoSelecionado.Numero);

                    // Guardar dados automaticamente
                    app.DataManager.GuardarTodosDados();

                    MessageBox.Show($"Aluno '{alunoSelecionado.Nome}' removido com sucesso!",
                                  "Aluno removido",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao remover aluno",
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

        private void BtnImportarCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📥 Iniciando importação CSV...");

                modelAlunos.ImportarAlunosCSV();

                // Guardar após importar
                app.DataManager.GuardarTodosDados();

                System.Diagnostics.Debug.WriteLine("✅ Importação CSV concluída!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao importar CSV: {ex.Message}",
                              "Erro de Importação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///  Event handler para remover todos os alunos
        /// </summary>
        private void BtnRemoverTodos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar se existem alunos
                var totalAlunos = modelAlunos.ContarAlunos();
                if (totalAlunos == 0)
                {
                    MessageBox.Show("Não existem alunos para remover.",
                                  "Lista vazia",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return;
                }

                // Confirmação única
                var resultado = MessageBox.Show(
                    $"⚠️ ATENÇÃO: Esta ação irá remover TODOS os {totalAlunos} aluno(s) da base de dados!\n\n" +
                    "Esta operação não pode ser desfeita.\n\n" +
                    "Tem certeza que deseja continuar?",
                    "Confirmar remoção total",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.No)
                    return;

                System.Diagnostics.Debug.WriteLine($"🗑️ Removendo todos os {totalAlunos} alunos...");

                // Executar remoção
                modelAlunos.LimparDados();

                // Guardar dados automaticamente
                app.DataManager.GuardarTodosDados();

                // Mensagem de sucesso
                MessageBox.Show($"✅ Todos os {totalAlunos} aluno(s) foram removidos com sucesso!\n\n" +
                              "A lista de alunos está agora vazia.",
                              "Remoção concluída",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                System.Diagnostics.Debug.WriteLine("✅ Todos os alunos removidos com sucesso!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao remover todos os alunos: {ex.Message}");
                MessageBox.Show($"Erro ao remover todos os alunos: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void DgAlunos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Manter método para compatibilidade futura se necessário
        }

        #endregion

        #region SearchBar (Pesquisa)

        private void TbPesquisa_GotFocus(object sender, RoutedEventArgs e)
        {
            // Limpar o texto de placeholder quando o campo recebe foco
            if (tbPesquisa.Text == "Pesquisar aluno...")
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
                tbPesquisa.Text = "Pesquisar aluno...";
                tbPesquisa.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void TbPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Ignorar se o texto for o placeholder
                if (tbPesquisa.Text == "Pesquisar aluno..." || string.IsNullOrWhiteSpace(tbPesquisa.Text))
                {
                    AtualizarDataGrid();
                    return;
                }

                // Delegar ao Model a pesquisa
                var resultados = modelAlunos.PesquisarAlunos(tbPesquisa.Text);
                dgAlunos.ItemsSource = resultados;
            }
            catch (Exception ex)
            {
                // Em caso de erro na pesquisa, mostrar todos os alunos
                AtualizarDataGrid();
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Atualiza a DataGrid com todos os alunos do Model
        /// </summary>
        private void AtualizarDataGrid()
        {
            try
            {
                var alunos = modelAlunos.ObterTodosAlunos();
                dgAlunos.ItemsSource = alunos;

                System.Diagnostics.Debug.WriteLine($"🔄 DataGrid atualizada com {alunos.Count} alunos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar DataGrid: {ex.Message}");
            }
        }

        #endregion
    }
}