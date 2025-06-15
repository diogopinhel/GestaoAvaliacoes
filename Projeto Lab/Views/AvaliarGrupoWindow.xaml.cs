using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class AvaliarGrupoWindow : Window
    {
        private App app;
        private ModelNotas modelNotas;
        private ModelAlunos modelAlunos;
        private ModelGrupos modelGrupos;
        private ModelTarefas modelTarefas;

        private bool isNotaGrupo = true; // Controla o modo atual
        private bool isEditMode = false; // Controla se é edição ou criação
        private NotaViewModel notaEditando = null; // Nota sendo editada

        // Informações da tarefa e grupo selecionados
        private Tarefa tarefaSelecionada;
        private Grupo grupoSelecionado;

        // Lista de TextBoxes para notas individuais
        private List<TextBox> notasIndividuaisInputs = new List<TextBox>();
        private List<Aluno> alunosDoGrupo = new List<Aluno>();

        #region Construtores

        // Construtor para modo de adição
        public AvaliarGrupoWindow(Tarefa tarefa, Grupo grupo)
        {
            InitializeComponent();
            tarefaSelecionada = tarefa;
            grupoSelecionado = grupo;
            InicializarWindow();
        }

        // Construtor para modo de edição
        public AvaliarGrupoWindow(NotaViewModel notaParaEditar)
        {
            InitializeComponent();
            isEditMode = true;
            notaEditando = notaParaEditar;
            InicializarWindow();
            CarregarDadosParaEdicao();
        }

        #endregion

        #region Inicialização

        private void InicializarWindow()
        {
            // Obter referências
            app = App.Current as App;
            modelNotas = app.Model_Notas;
            modelAlunos = app.Model_Alunos;
            modelGrupos = app.Model_Grupos;
            modelTarefas = app.Model_Tarefas;

            // Configurar interface
            ConfigurarInterface();
            CarregarInformacoes();
            AtualizarModoInterface();
        }

        private void ConfigurarInterface()
        {
            // Configurar título baseado no modo
            if (isEditMode)
            {
                tbTitulo.Text = "Editar Avaliação";
                btnGuardar.Content = "💾 Atualizar";
            }
            else
            {
                tbTitulo.Text = "Avaliar Grupo";
                btnGuardar.Content = "💾 Guardar Nota";
            }
        }

        private void CarregarInformacoes()
        {
            try
            {
                if (isEditMode && notaEditando != null)
                {
                    // Buscar tarefa e grupo baseado na nota sendo editada
                    tarefaSelecionada = modelTarefas.ObterTarefa(notaEditando.TarefaId);
                    grupoSelecionado = modelGrupos.ObterGrupo(notaEditando.GrupoId);
                }

                // Atualizar informações na interface
                if (tarefaSelecionada != null)
                {
                    tbTarefaNome.Text = tarefaSelecionada.Titulo;
                    tbTarefaDetalhes.Text = $"Peso: {tarefaSelecionada.Peso}% • Data limite: {tarefaSelecionada.DataHoraFim:dd/MM/yyyy}";
                }

                if (grupoSelecionado != null)
                {
                    tbGrupoNome.Text = $"🔵 {grupoSelecionado.Nome}";

                    // Carregar alunos do grupo
                    alunosDoGrupo = modelGrupos.ObterAlunosDoGrupo(grupoSelecionado.Id);
                    var nomesAlunos = alunosDoGrupo.Select(a => a.Nome).ToList();
                    tbGrupoMembros.Text = $"{string.Join(", ", nomesAlunos)} ({alunosDoGrupo.Count} membros)";

                    // Criar inputs para notas individuais
                    CriarInputsNotasIndividuais();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar informações: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void CarregarDadosParaEdicao()
        {
            if (notaEditando == null) return;

            try
            {
                // Buscar nota original
                var notaOriginal = modelNotas.Lista.Values.FirstOrDefault(n => n.Id == notaEditando.Id);
                if (notaOriginal != null)
                {
                    isNotaGrupo = notaOriginal.ENotaGrupo;
                    AtualizarModoInterface();

                    if (isNotaGrupo)
                    {
                        tbNotaGrupo.Text = notaEditando.ValorNota.ToString("F1", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // Para notas individuais, carregar a nota específica do aluno
                        CarregarNotaIndividualParaEdicao();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados para edição: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void CarregarNotaIndividualParaEdicao()
        {
            if (notaEditando == null || alunosDoGrupo == null) return;

            // Encontrar o TextBox correspondente ao aluno da nota sendo editada
            var alunoIndex = alunosDoGrupo.FindIndex(a => a.Nome == notaEditando.Alunos);
            if (alunoIndex >= 0 && alunoIndex < notasIndividuaisInputs.Count)
            {
                notasIndividuaisInputs[alunoIndex].Text = notaEditando.ValorNota.ToString("F1", CultureInfo.InvariantCulture);
                AtualizarProgressoIndividual();
            }
        }

        #endregion

        #region Interface - Criação de Inputs

        private void CriarInputsNotasIndividuais()
        {
            spAlunosIndividuais.Children.Clear();
            notasIndividuaisInputs.Clear();

            foreach (var aluno in alunosDoGrupo)
            {
                // Container para cada aluno
                var border = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA")),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(15, 12, 15, 12), // Corrigido: 4 parâmetros
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Informações do aluno
                var stackPanelAluno = new StackPanel();

                var nomeAluno = new TextBlock
                {
                    Text = aluno.Nome,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#495057"))
                };

                var numeroAluno = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA")),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 3, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                var numeroText = new TextBlock
                {
                    Text = $"Nº {aluno.Numero}",
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C757D"))
                };

                numeroAluno.Child = numeroText;
                stackPanelAluno.Children.Add(nomeAluno);
                stackPanelAluno.Children.Add(numeroAluno);

                // Input da nota
                var stackPanelNota = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var inputNota = new TextBox
                {
                    Style = (Style)FindResource("StudentNoteInput"),
                    Tag = aluno // Armazenar referência do aluno
                };

                inputNota.TextChanged += InputNotaIndividual_TextChanged;
                notasIndividuaisInputs.Add(inputNota);

                var labelEscala = new TextBlock
                {
                    Text = "/20",
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C757D")),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, 0, 0)
                };

                stackPanelNota.Children.Add(inputNota);
                stackPanelNota.Children.Add(labelEscala);

                Grid.SetColumn(stackPanelAluno, 0);
                Grid.SetColumn(stackPanelNota, 1);

                grid.Children.Add(stackPanelAluno);
                grid.Children.Add(stackPanelNota);
                border.Child = grid;

                // Efeito hover
                border.MouseEnter += (s, e) =>
                {
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
                    border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = (Color)ColorConverter.ConvertFromString("#1976D2"),
                        Direction = 270,
                        ShadowDepth = 2,
                        Opacity = 0.1,
                        BlurRadius = 8
                    };
                };

                border.MouseLeave += (s, e) =>
                {
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA"));
                    border.Effect = null;
                };

                spAlunosIndividuais.Children.Add(border);
            }

            AtualizarProgressoIndividual();
        }

        #endregion

        #region Event Handlers - Modo Toggle

        private void BtnNotaGrupo_Click(object sender, RoutedEventArgs e)
        {
            isNotaGrupo = true;
            AtualizarModoInterface();
        }

        private void BtnNotasIndividuais_Click(object sender, RoutedEventArgs e)
        {
            isNotaGrupo = false;
            AtualizarModoInterface();
        }

        private void AtualizarModoInterface()
        {
            if (isNotaGrupo)
            {
                // Modo Nota de Grupo
                btnNotaGrupo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7065F0"));
                btnNotaGrupo.Foreground = Brushes.White;
                btnNotaGrupo.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7065F0"));

                btnNotasIndividuais.Background = Brushes.White;
                btnNotasIndividuais.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7065F0"));
                btnNotasIndividuais.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA"));

                borderNotaGrupo.Visibility = Visibility.Visible;
                borderNotasIndividuais.Visibility = Visibility.Collapsed;

                tbTitulo.Text = isEditMode ? "Editar Nota de Grupo" : "Avaliar Grupo";
                btnGuardar.Content = "💾 Guardar Nota";
            }
            else
            {
                // Modo Notas Individuais
                btnNotasIndividuais.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7065F0"));
                btnNotasIndividuais.Foreground = Brushes.White;
                btnNotasIndividuais.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7065F0"));

                btnNotaGrupo.Background = Brushes.White;
                btnNotaGrupo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7065F0"));
                btnNotaGrupo.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA"));

                borderNotaGrupo.Visibility = Visibility.Collapsed;
                borderNotasIndividuais.Visibility = Visibility.Visible;

                tbTitulo.Text = isEditMode ? "Editar Notas Individuais" : "Avaliar Individualmente";
                btnGuardar.Content = "💾 Guardar Notas";
            }
        }

        #endregion

        #region Event Handlers - Inputs

        private void TbNotaGrupo_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && !string.IsNullOrEmpty(textBox.Text))
            {
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"));
                textBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FFF8"));
            }
            else if (textBox != null)
            {
                textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA"));
                textBox.Background = Brushes.White;
            }
        }

        private void InputNotaIndividual_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28A745"));
                    textBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FFF8"));
                }
                else
                {
                    textBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CED4DA"));
                    textBox.Background = Brushes.White;
                }

                AtualizarProgressoIndividual();
            }
        }

        private void AtualizarProgressoIndividual()
        {
            var notasPreenchidas = notasIndividuaisInputs.Count(input => !string.IsNullOrWhiteSpace(input.Text));
            tbProgressoIndividual.Text = $"{notasPreenchidas}/{notasIndividuaisInputs.Count} alunos avaliados";
        }

        #endregion

        #region Event Handlers - Botões

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isEditMode)
                {
                    AtualizarNota();
                }
                else
                {
                    AdicionarNota();
                }
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro de Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        #region Lógica de Negócio

        private void AdicionarNota()
        {
            if (tarefaSelecionada == null || grupoSelecionado == null)
            {
                MessageBox.Show("Erro: Informações da tarefa ou grupo não encontradas.",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                return;
            }

            if (isNotaGrupo)
            {
                AdicionarNotaGrupo();
            }
            else
            {
                AdicionarNotasIndividuais();
            }
        }

        private void AdicionarNotaGrupo()
        {
            // Validar nota
            if (string.IsNullOrWhiteSpace(tbNotaGrupo.Text))
            {
                MessageBox.Show("Por favor, insira a nota do grupo.",
                              "Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(tbNotaGrupo.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal nota))
            {
                MessageBox.Show("Por favor, insira uma nota válida (0-20).",
                              "Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // Adicionar nota através do modelo
            modelNotas.AdicionarNotaGrupo(tarefaSelecionada.Id, grupoSelecionado.Id, nota);

            MessageBox.Show("Nota de grupo adicionada com sucesso!",
                          "Sucesso",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void AdicionarNotasIndividuais()
        {
            if (!notasIndividuaisInputs.Any())
            {
                MessageBox.Show("Não há alunos no grupo selecionado.",
                              "Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            int notasAdicionadas = 0;
            var erros = new List<string>();

            for (int i = 0; i < notasIndividuaisInputs.Count; i++)
            {
                var input = notasIndividuaisInputs[i];
                var aluno = alunosDoGrupo[i];

                if (string.IsNullOrWhiteSpace(input.Text))
                    continue; // Pular alunos sem nota

                if (decimal.TryParse(input.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal nota))
                {
                    try
                    {
                        modelNotas.AdicionarNotaIndividual(tarefaSelecionada.Id, grupoSelecionado.Id, aluno.Numero, nota);
                        notasAdicionadas++;
                    }
                    catch (OperacaoInvalidaException ex)
                    {
                        erros.Add($"{aluno.Nome}: {ex.Message}");
                    }
                }
                else
                {
                    erros.Add($"{aluno.Nome}: Nota inválida");
                }
            }

            // Mostrar resultado
            string mensagem = $"{notasAdicionadas} nota(s) individual(is) adicionada(s) com sucesso!";

            if (erros.Any())
            {
                mensagem += $"\n\nErros encontrados:\n{string.Join("\n", erros)}";
            }

            MessageBox.Show(mensagem,
                          notasAdicionadas > 0 ? "Sucesso" : "Erro",
                          MessageBoxButton.OK,
                          notasAdicionadas > 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (notasAdicionadas > 0)
            {
                DialogResult = true;
                Close();
            }
        }

        private void AtualizarNota()
        {
            if (notaEditando == null) return;

            if (isNotaGrupo)
            {
                AtualizarNotaGrupo();
            }
            else
            {
                AtualizarNotaIndividual();
            }
        }

        private void AtualizarNotaGrupo()
        {
            // Validar nota
            if (string.IsNullOrWhiteSpace(tbNotaGrupo.Text))
            {
                MessageBox.Show("Por favor, insira a nota do grupo.",
                              "Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(tbNotaGrupo.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal nota))
            {
                MessageBox.Show("Por favor, insira uma nota válida (0-20).",
                              "Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // Atualizar nota através do modelo
            modelNotas.AtualizarNota(notaEditando.Id, nota);

            MessageBox.Show("Nota de grupo atualizada com sucesso!",
                          "Sucesso",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void AtualizarNotaIndividual()
        {
            // Encontrar o input com nota preenchida
            var inputComNota = notasIndividuaisInputs.FirstOrDefault(input => !string.IsNullOrWhiteSpace(input.Text));
            if (inputComNota == null)
            {
                MessageBox.Show("Por favor, insira uma nota para pelo menos um aluno.",
                              "Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(inputComNota.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal nota))
            {
                MessageBox.Show("Por favor, insira uma nota válida (0-20).",
                              "Validação",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // Atualizar nota através do modelo
            modelNotas.AtualizarNota(notaEditando.Id, nota);

            MessageBox.Show("Nota individual atualizada com sucesso!",
                          "Sucesso",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        #endregion
    }
}