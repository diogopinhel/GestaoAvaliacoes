using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    /// <summary>
    /// GestaoPautasView CORRIGIDA - Sem duplicação e com carregamento automático
    /// </summary>
    public partial class GestaoPautasView : UserControl
    {
        private App app;
        private ModelPautas modelPautas;
        private List<PautaViewModel> listaPautas;

        // Controle para evitar duplicação de colunas
        private bool colunasJaGeradas = false;

        public GestaoPautasView()
        {
            InitializeComponent();

            app = App.Current as App;
            modelPautas = app.Model_Pautas;
            listaPautas = new List<PautaViewModel>();

            // Subscrever aos eventos do Model
            modelPautas.DadosCarregados += ModelPautas_DadosCarregados;
            modelPautas.DadosAlterados += ModelPautas_DadosAlterados;
            modelPautas.ErroOcorrido += ModelPautas_ErroOcorrido;

            // Configurar listeners mais tarde
            this.Loaded += GestaoPautasView_Loaded;
        }

        #region Event Handler para quando a View é carregada

        private void GestaoPautasView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Carregar dados quando a view é mostrada
                System.Diagnostics.Debug.WriteLine("🔄 GestaoPautasView carregada - Atualizando dados...");
                CarregarDados();

                // Configurar listeners de redimensionamento apenas uma vez
                if (!colunasJaGeradas)
                {
                    ConfigurarListenersRedimensionamento();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar GestaoPautasView: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers do Model

        private void ModelPautas_DadosCarregados()
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("🔄 ModelPautas_DadosCarregados - Atualizando pautas...");
                CarregarDados();
            });
        }

        private void ModelPautas_DadosAlterados()
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("🔄 ModelPautas_DadosAlterados - Atualizando pautas...");
                CarregarDados();
            });
        }

        private void ModelPautas_ErroOcorrido(string mensagemErro)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(mensagemErro, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        #endregion

        #region Métodos de Carregamento

        /// <summary>
        /// Carrega dados e previne duplicação de colunas
        /// </summary>
        private void CarregarDados()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📊 Carregando dados das pautas...");

                // Obter dados do modelo
                listaPautas = modelPautas.ObterPautaCompleta();
                System.Diagnostics.Debug.WriteLine($"📊 {listaPautas.Count} alunos na pauta");

                // Só gerar colunas uma vez
                if (!colunasJaGeradas)
                {
                    GerarColunasTarefasDinamicas();
                    colunasJaGeradas = true;
                    System.Diagnostics.Debug.WriteLine("✅ Colunas geradas pela primeira vez");
                }
                else
                {
                    // Se já existem colunas, verificar se precisa atualizar
                    VerificarAtualizacaoColunas();
                }

                // Atualizar DataGrid
                AtualizarDataGrid();

                // Atualizar estatísticas
                AtualizarEstatisticas();

                // Verificar dados do histograma
                VerificarDadosHistograma();

                System.Diagnostics.Debug.WriteLine("✅ Dados das pautas carregados com sucesso!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar dados da pauta: {ex.Message}");
                MessageBox.Show($"Erro ao carregar dados da pauta: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Verifica se precisa atualizar colunas (quando tarefas mudam)
        /// </summary>
        private void VerificarAtualizacaoColunas()
        {
            try
            {
                var tarefasAtuais = modelPautas.ObterTodasTarefasParaPauta();
                var colunasTarefasExistentes = dgPautas.Columns.OfType<DataGridTemplateColumn>()
                    .Where(c => c.GetValue(FrameworkElement.TagProperty) is Tarefa)
                    .Count();

                // Se o número de tarefas mudou, regenerar colunas
                if (tarefasAtuais.Count != colunasTarefasExistentes)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Número de tarefas mudou: {colunasTarefasExistentes} → {tarefasAtuais.Count}");
                    colunasJaGeradas = false; // Reset flag
                    GerarColunasTarefasDinamicas();
                    colunasJaGeradas = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao verificar colunas: {ex.Message}");
            }
        }

        /// <summary>
        ///  Gera colunas dinâmicas SEM duplicação
        /// </summary>
        private void GerarColunasTarefasDinamicas()
        {
            try
            {
                var tarefas = modelPautas.ObterTodasTarefasParaPauta();
                System.Diagnostics.Debug.WriteLine($"📊 Gerando colunas para {tarefas.Count} tarefas...");

                // Limpar TODAS as colunas de tarefas primeiro
                LimparColunasTarefas();

                // Adicionar coluna para cada tarefa
                for (int i = 0; i < tarefas.Count; i++)
                {
                    var tarefa = tarefas[i];
                    AdicionarColunaTarefa(i, tarefa);
                }

                System.Diagnostics.Debug.WriteLine($"✅ {tarefas.Count} colunas geradas!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao gerar colunas: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove TODAS as colunas de tarefas existentes
        /// </summary>
        private void LimparColunasTarefas()
        {
            try
            {
                //  Identificar corretamente colunas de tarefas
                var colunasParaRemover = new List<DataGridColumn>();

                foreach (var coluna in dgPautas.Columns)
                {
                    // Verificar se é coluna de tarefa usando o Tag
                    if (coluna.GetValue(FrameworkElement.TagProperty) is Tarefa)
                    {
                        colunasParaRemover.Add(coluna);
                    }
                    // ALTERNATIVA: Verificar por header que contém "T" seguido de número
                    else if (coluna.Header != null)
                    {
                        var headerText = coluna.Header.ToString();
                        if (headerText.Contains("📝") ||
                            (headerText.StartsWith("T") && headerText.Length > 1 && char.IsDigit(headerText[1])))
                        {
                            colunasParaRemover.Add(coluna);
                        }
                    }
                }

                // Remover as colunas identificadas
                foreach (var coluna in colunasParaRemover)
                {
                    dgPautas.Columns.Remove(coluna);
                    System.Diagnostics.Debug.WriteLine($"🗑️ Removida coluna: {coluna.Header}");
                }

                System.Diagnostics.Debug.WriteLine($"🗑️ {colunasParaRemover.Count} colunas removidas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao limpar colunas: {ex.Message}");
            }
        }

        /// <summary>
        /// Adiciona uma coluna dinâmica para uma tarefa específica
        /// </summary>
        private void AdicionarColunaTarefa(int indiceTarefa, Tarefa tarefa)
        {
            try
            {
                var coluna = new DataGridTemplateColumn
                {
                    Header = CriarHeaderDinamico(tarefa),
                    Width = new DataGridLength(120),
                    CanUserSort = true,
                    CanUserResize = true
                };

                // Armazenar referência à tarefa no Tag
                coluna.SetValue(FrameworkElement.TagProperty, tarefa);

                var template = CriarTemplateCelulaTarefa(indiceTarefa, tarefa);
                coluna.CellTemplate = template;

                // Inserir antes da coluna Final
                var posicaoInsercao = dgPautas.Columns.Count - 1;
                dgPautas.Columns.Insert(posicaoInsercao, coluna);

                System.Diagnostics.Debug.WriteLine($"➕ Adicionada coluna: T{tarefa.Id} - {tarefa.Titulo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao adicionar coluna tarefa {tarefa.Id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria header dinâmico que se adapta à largura da coluna
        /// </summary>
        private FrameworkElement CriarHeaderDinamico(Tarefa tarefa)
        {
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var icone = new TextBlock
            {
                Text = "📝",
                FontSize = 14,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var textoHeader = new TextBlock
            {
                Text = $"T{tarefa.Id} - {TruncateString(tarefa.Titulo, 10)}",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Name = $"HeaderText_{tarefa.Id}"
            };

            // Tooltip com informação completa
            var tooltip = new ToolTip();
            var tooltipContent = new StackPanel();

            tooltipContent.Children.Add(new TextBlock
            {
                Text = $"📋 {tarefa.Titulo}",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });

            if (!string.IsNullOrEmpty(tarefa.Descricao))
            {
                tooltipContent.Children.Add(new TextBlock
                {
                    Text = $"📝 {tarefa.Descricao}",
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 5),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 300
                });
            }

            tooltipContent.Children.Add(new TextBlock
            {
                Text = $"📅 {tarefa.DataHoraInicio} → {tarefa.DataHoraFim}",
                FontSize = 11,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 3)
            });

            tooltipContent.Children.Add(new TextBlock
            {
                Text = $"⚖️ Peso: {tarefa.Peso}",
                FontSize = 11,
                Foreground = Brushes.Gray
            });

            tooltip.Content = tooltipContent;
            headerPanel.ToolTip = tooltip;
            headerPanel.Children.Add(icone);
            headerPanel.Children.Add(textoHeader);
            headerPanel.Tag = tarefa;

            return headerPanel;
        }

        /// <summary>
        /// Cria template de célula para uma tarefa específica
        /// </summary>
        private DataTemplate CriarTemplateCelulaTarefa(int indiceTarefa, Tarefa tarefa)
        {
            var template = new DataTemplate();
            template.VisualTree = new FrameworkElementFactory(typeof(Border));
            var borderFactory = template.VisualTree;

            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(8, 6, 8, 6));
            borderFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            var backgroundBinding = new Binding($"StatusTarefas[{indiceTarefa}]")
            {
                Converter = new StatusParaCorConverter()
            };
            borderFactory.SetBinding(Border.BackgroundProperty, backgroundBinding);

            var borderBrushBinding = new Binding($"StatusTarefas[{indiceTarefa}]")
            {
                Converter = new StatusParaBordaConverter()
            };
            borderFactory.SetBinding(Border.BorderBrushProperty, borderBrushBinding);
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1, 1, 1, 1));

            var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetValue(TextBlock.FontSizeProperty, 12.0);
            textFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            textFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            var textBinding = new Binding($"NotasTarefas[{indiceTarefa}]")
            {
                Converter = new NotaParaTextoConverter()
            };
            textFactory.SetBinding(TextBlock.TextProperty, textBinding);

            var foregroundBinding = new Binding($"StatusTarefas[{indiceTarefa}]")
            {
                Converter = new StatusParaTextoConverter()
            };
            textFactory.SetBinding(TextBlock.ForegroundProperty, foregroundBinding);

            borderFactory.AppendChild(textFactory);
            return template;
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        #endregion

        #region Métodos existentes (inalterados)

        private void AtualizarDataGrid()
        {
            dgPautas.ItemsSource = null;
            dgPautas.ItemsSource = listaPautas;
        }

        private void AtualizarEstatisticas()
        {
            try
            {
                var stats = modelPautas.ObterEstatisticas();
                tbTotalAlunos.Text = stats.TotalAlunos.ToString();
                tbAlunosAvaliados.Text = stats.AlunosAvaliados.ToString();
                tbPorAvaliar.Text = stats.AlunosPorAvaliar.ToString();
                tbMediaGeral.Text = stats.MediaGeral.ToString("F1");
            }
            catch (Exception)
            {
                tbTotalAlunos.Text = "0";
                tbAlunosAvaliados.Text = "0";
                tbPorAvaliar.Text = "0";
                tbMediaGeral.Text = "0.0";
            }
        }

        private void VerificarDadosHistograma()
        {
            try
            {
                var stats = modelPautas.ObterEstatisticas();

                if (stats.AlunosAvaliados == 0)
                {
                    btnAmpliarGrafico.Content = "📊 Sem dados para histograma";
                    btnAmpliarGrafico.IsEnabled = true;
                }
                else
                {
                    btnAmpliarGrafico.Content = $"📊 Ver Histograma ({stats.AlunosAvaliados} avaliados)";
                    btnAmpliarGrafico.IsEnabled = true;
                }

                AtualizarHistogramaVisual();
            }
            catch
            {
                btnAmpliarGrafico.Content = "📊 Erro no histograma";
                btnAmpliarGrafico.IsEnabled = false;
            }
        }

        private void AtualizarHistogramaVisual()
        {
            try
            {
                var distribuicao = modelPautas.ObterDistribuicaoHistograma();

                if (distribuicao == null || distribuicao.Count < 10)
                {
                    distribuicao = new List<int>(new int[10]);
                }

                AtualizarHistogramaPorIndice(distribuicao);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar histograma visual: {ex.Message}");
            }
        }

        private void AtualizarHistogramaPorIndice(List<int> distribuicao)
        {
            try
            {
                var nomesTextBlocks = new[]
                {
                    "tbHist0", "tbHist1", "tbHist2", "tbHist3", "tbHist4",
                    "tbHist5", "tbHist6", "tbHist7", "tbHist8", "tbHist9"
                };

                var nomesBorders = new[]
                {
                    "borderHist0", "borderHist1", "borderHist2", "borderHist3", "borderHist4",
                    "borderHist5", "borderHist6", "borderHist7", "borderHist8", "borderHist9"
                };

                var maxValor = distribuicao.Where(x => x > 0).DefaultIfEmpty(1).Max();
                var alturaMaxima = 150.0;
                var alturaMinima = 5.0;

                for (int i = 0; i < Math.Min(10, distribuicao.Count); i++)
                {
                    var valor = distribuicao[i];

                    var textBlock = FindName(nomesTextBlocks[i]) as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Text = valor.ToString();

                        if (valor == 0)
                        {
                            textBlock.Foreground = System.Windows.Media.Brushes.Gray;
                        }
                        else if (i < 4)
                        {
                            textBlock.Foreground = System.Windows.Media.Brushes.Red;
                        }
                        else
                        {
                            textBlock.Foreground = System.Windows.Media.Brushes.Green;
                        }
                    }

                    var border = FindName(nomesBorders[i]) as Border;
                    if (border != null)
                    {
                        double altura;
                        if (valor == 0)
                        {
                            altura = alturaMinima;
                        }
                        else
                        {
                            altura = alturaMinima + ((valor * (alturaMaxima - alturaMinima)) / maxValor);
                        }

                        border.Height = Math.Max(alturaMinima, altura);
                        border.Opacity = valor == 0 ? 0.3 : 1.0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar por índice: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers da Interface

        private void BtnAmpliarGrafico_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var distribuicao = modelPautas.ObterDistribuicaoHistograma();
                var stats = modelPautas.ObterEstatisticas();

                if (stats.AlunosAvaliados == 0)
                {
                    MessageBox.Show("📊 HISTOGRAMA DE NOTAS\n\n" +
                                  "Ainda não há alunos avaliados para gerar o histograma.\n\n" +
                                  "💡 Para ver o histograma:\n" +
                                  "1. Acesse 'Gestão de Notas'\n" +
                                  "2. Selecione uma tarefa\n" +
                                  "3. Avalie os grupos\n" +
                                  "4. Volte aqui para ver a distribuição das notas",
                                  "Sem dados para histograma",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var intervalos = new[] { "0-2", "3-5", "6-8", "9", "10-11", "12-13", "14-15", "16-17", "18-19", "20" };

                string dadosHistograma = "📊 DISTRIBUIÇÃO DINÂMICA DE NOTAS:\n";
                dadosHistograma += "🔄 Baseado nas avaliações em tempo real\n\n";

                for (int i = 0; i < Math.Min(intervalos.Length, distribuicao.Count); i++)
                {
                    var intervalo = intervalos[i];
                    var quantidade = distribuicao[i];
                    var percentual = stats.AlunosAvaliados > 0 ? (quantidade * 100.0 / stats.AlunosAvaliados) : 0;

                    bool isAprovado = !intervalo.Contains("0-") && !intervalo.Contains("3-") &&
                                     !intervalo.Contains("6-") && intervalo != "9";
                    string cor = isAprovado ? "🟢" : "🔴";

                    if (quantidade > 0 || i < 4)
                    {
                        dadosHistograma += $"{cor} {intervalo}: {quantidade} aluno(s) ({percentual:F1}%)\n";
                    }
                }

                var aprovados = 0;
                for (int i = 4; i < distribuicao.Count; i++)
                {
                    aprovados += distribuicao[i];
                }
                var taxaAprovacao = stats.AlunosAvaliados > 0 ? (aprovados * 100.0 / stats.AlunosAvaliados) : 0;

                dadosHistograma += $"\n📈 ESTATÍSTICAS ATUALIZADAS EM TEMPO REAL:\n";
                dadosHistograma += $"• Total de Alunos: {stats.TotalAlunos}\n";
                dadosHistograma += $"• Alunos Avaliados: {stats.AlunosAvaliados}\n";
                dadosHistograma += $"• Por Avaliar: {stats.AlunosPorAvaliar}\n";
                dadosHistograma += $"• Média Geral: {stats.MediaGeral:F1} valores\n";
                dadosHistograma += $"• Taxa de Aprovação: {taxaAprovacao:F1}%\n\n";

                dadosHistograma += "💡 Os dados atualizam automaticamente conforme as notas são atribuídas na 'Gestão de Notas'.";

                MessageBox.Show(dadosHistograma,
                              "📊 Histograma Dinâmico - Distribuição de Notas",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao mostrar histograma dinâmico: {ex.Message}",
                              "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Redimensionamento (Simplificado)

        private System.Windows.Threading.DispatcherTimer timerRedimensionamento;

        private void ConfigurarListenersRedimensionamento()
        {
            try
            {
                timerRedimensionamento = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };

                timerRedimensionamento.Tick += (sender, e) =>
                {
                    // Lógica simples de redimensionamento se necessário
                };

                timerRedimensionamento.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao configurar redimensionamento: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        public void LimparEventos()
        {
            try
            {
                if (timerRedimensionamento != null)
                {
                    timerRedimensionamento.Stop();
                    timerRedimensionamento = null;
                }

                if (modelPautas != null)
                {
                    modelPautas.DadosCarregados -= ModelPautas_DadosCarregados;
                    modelPautas.DadosAlterados -= ModelPautas_DadosAlterados;
                    modelPautas.ErroOcorrido -= ModelPautas_ErroOcorrido;
                }
            }
            catch
            {
                // Ignorar erros na limpeza
            }
        }

        #endregion

        #region Conversores (Inalterados)

        public class StatusParaCorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var status = value?.ToString() ?? "SemNota";

                switch (status)
                {
                    case "Aprovado":
                        return new SolidColorBrush(Color.FromRgb(212, 237, 218));
                    case "Reprovado":
                        return new SolidColorBrush(Color.FromRgb(248, 215, 218));
                    default:
                        return new SolidColorBrush(Color.FromRgb(241, 243, 244));
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class StatusParaBordaConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var status = value?.ToString() ?? "SemNota";

                switch (status)
                {
                    case "Aprovado":
                        return new SolidColorBrush(Color.FromRgb(40, 167, 69));
                    case "Reprovado":
                        return new SolidColorBrush(Color.FromRgb(220, 53, 69));
                    default:
                        return new SolidColorBrush(Color.FromRgb(221, 221, 221));
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class NotaParaTextoConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is decimal nota)
                {
                    return nota.ToString("F1");
                }
                return "—";
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class StatusParaTextoConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var status = value?.ToString() ?? "SemNota";

                switch (status)
                {
                    case "Aprovado":
                        return new SolidColorBrush(Color.FromRgb(21, 87, 36));
                    case "Reprovado":
                        return new SolidColorBrush(Color.FromRgb(114, 28, 36));
                    default:
                        return new SolidColorBrush(Color.FromRgb(153, 153, 153));
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}