using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class GestaoPerfilView : UserControl
    {
        private App app;
        private ModelPerfil modelPerfil;

        public GestaoPerfilView()
        {
            InitializeComponent();

            // Configurar foto bonita ANTES de qualquer coisa
            ConfigurarFotoBonita();

            // Obtém a instância do App (camada de interligação)
            app = App.Current as App;
            modelPerfil = app.Model_Perfil;

            // Subscrever aos eventos do Model (padrão MVVM Simplificado)
            if (modelPerfil != null)
            {
                modelPerfil.DadosCarregados += ModelPerfil_DadosCarregados;
                modelPerfil.DadosAlterados += ModelPerfil_DadosAlterados;
                modelPerfil.ErroOcorrido += ModelPerfil_ErroOcorrido;
                modelPerfil.PerfilModificado += ModelPerfil_PerfilModificado;
            }

            // Carregar dados iniciais (mas manter foto bonita)
            CarregarDadosInterface();

            System.Diagnostics.Debug.WriteLine("✅ GestaoPerfilView inicializada com foto bonita garantida");
        }

        /// <summary>
        ///  CONFIGURAR foto bonita garantida (nunca amarelo)
        /// </summary>
        private void ConfigurarFotoBonita()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🎨 Configurando foto bonita...");

                // Sua imagem da pasta Images
                if (TentarCarregarImagemProjeto())
                {
                    System.Diagnostics.Debug.WriteLine("✅ Imagem do projeto carregada!");
                    return;
                }

                // Avatar com iniciais bonito
                string nomeUtilizador = ObterNomeUtilizador();
                string iniciais = GerarIniciais(nomeUtilizador);
                CriarAvatarComIniciais(iniciais);

                System.Diagnostics.Debug.WriteLine($"✅ Avatar criado com iniciais: {iniciais}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao configurar foto: {ex.Message}");

                // Fundo roxo sem amarelo
                ellipsePerfil.Fill = new SolidColorBrush(Color.FromRgb(112, 101, 240));
                imgPerfil.ImageSource = null;
            }
        }

        /// <summary>
        /// Obter nome do utilizador (do model ou do Windows)
        /// </summary>
        private string ObterNomeUtilizador()
        {
            try
            {
                // Tentar do model primeiro
                if (modelPerfil?.PerfilAtual?.Nome != null)
                {
                    return modelPerfil.PerfilAtual.Nome;
                }

                // Tentar do Windows
                return Environment.UserName ?? "Utilizador";
            }
            catch
            {
                return "Utilizador";
            }
        }

        /// <summary>
        /// Tentar carregar imagem do projeto
        /// </summary>
        private bool TentarCarregarImagemProjeto()
        {
            var caminhosPossiveis = new[]
            {
                "pack://application:,,,/Images/default_image.png",
                "/Images/default_image.png",
                "Images/default_image.png"
            };

            foreach (var caminho in caminhosPossiveis)
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(caminho, UriKind.RelativeOrAbsolute));
                    imgPerfil.ImageSource = bitmap;
                    ellipsePerfil.Fill = new SolidColorBrush(Colors.Transparent);
                    return true;
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }

        /// <summary>
        ///  Criar avatar bonito com iniciais
        /// </summary>
        private void CriarAvatarComIniciais(string iniciais)
        {
            try
            {
                // Criar um DrawingVisual para desenhar
                var drawingVisual = new DrawingVisual();

                using (var drawingContext = drawingVisual.RenderOpen())
                {
                    // Fundo gradiente bonito
                    var gradientBrush = new RadialGradientBrush();
                    gradientBrush.Center = new Point(0.5, 0.5);
                    gradientBrush.RadiusX = 0.5;
                    gradientBrush.RadiusY = 0.5;
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(112, 101, 240), 0.0)); // #7065F0
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(93, 156, 236), 1.0));  // #5D9CEC

                    // Desenhar círculo de fundo
                    drawingContext.DrawEllipse(gradientBrush, null, new Point(60, 60), 60, 60);

                    // Texto das iniciais
                    var formattedText = new FormattedText(
                        iniciais,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                        40, // Tamanho maior
                        Brushes.White,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    // Centralizar texto
                    var textX = (120 - formattedText.Width) / 2;
                    var textY = (120 - formattedText.Height) / 2;

                    drawingContext.DrawText(formattedText, new Point(textX, textY));
                }

                // Converter para imagem
                var renderTarget = new RenderTargetBitmap(120, 120, 96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(drawingVisual);
                renderTarget.Freeze();

                // Aplicar
                imgPerfil.ImageSource = renderTarget;
                ellipsePerfil.Fill = new SolidColorBrush(Colors.Transparent);

                System.Diagnostics.Debug.WriteLine($"✅ Avatar criado com iniciais: {iniciais}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao criar avatar: {ex.Message}");

                // Fallback simples
                var gradientBrush = new RadialGradientBrush();
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(112, 101, 240), 0.0));
                gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(93, 156, 236), 1.0));
                ellipsePerfil.Fill = gradientBrush;
                imgPerfil.ImageSource = null;
            }
        }

        /// <summary>
        /// Gerar iniciais do nome
        /// </summary>
        private string GerarIniciais(string nomeCompleto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeCompleto))
                    return "U";

                var palavras = nomeCompleto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (palavras.Length == 1)
                {
                    return palavras[0].Length >= 2 ? palavras[0].Substring(0, 2).ToUpper() : palavras[0].ToUpper();
                }
                else if (palavras.Length >= 2)
                {
                    return (palavras[0].Substring(0, 1) + palavras[palavras.Length - 1].Substring(0, 1)).ToUpper();
                }

                return nomeCompleto.Substring(0, 1).ToUpper();
            }
            catch
            {
                return "U";
            }
        }

        #region Event Handlers do Model

        private void ModelPerfil_DadosCarregados()
        {
            Dispatcher.Invoke(() =>
            {
                CarregarDadosInterface();
            });
        }

        private void ModelPerfil_DadosAlterados()
        {
            Dispatcher.Invoke(() =>
            {
                CarregarDadosInterface();

                // ATUALIZAR iniciais se o nome mudou
                if (modelPerfil?.PerfilAtual?.Nome != null)
                {
                    string iniciais = GerarIniciais(modelPerfil.PerfilAtual.Nome);

                    // SÓ recriar avatar se não há foto personalizada válida
                    if (!TemFotoPersonalizadaValida())
                    {
                        CriarAvatarComIniciais(iniciais);
                    }
                }
            });
        }

        private void ModelPerfil_ErroOcorrido(string mensagemErro)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(mensagemErro, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        private void ModelPerfil_PerfilModificado(string mensagem)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(mensagem, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                CarregarDadosInterface();
            });
        }

        #endregion

        #region Event Handlers da Interface

        private void BtnEditarFoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                modelPerfil.SelecionarNovaFoto();
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message, "Erro ao alterar foto", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbNome.Text))
                {
                    MessageBox.Show("O campo Nome é obrigatório.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbNome.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(tbEmail.Text))
                {
                    MessageBox.Show("O campo Email é obrigatório.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbEmail.Focus();
                    return;
                }

                modelPerfil.AtualizarPerfil(tbNome.Text.Trim(), tbEmail.Text.Trim());
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message, "Erro ao salvar perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CarregarDadosInterface();
            MessageBox.Show("Alterações canceladas. Os dados originais foram restaurados.",
                          "Cancelado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Carregar dados da interface (SEM mexer na foto)
        /// </summary>
        private void CarregarDadosInterface()
        {
            try
            {
                if (modelPerfil?.PerfilAtual == null)
                {
                    CarregarDadosPadrao();
                    return;
                }

                var perfil = modelPerfil.PerfilAtual;

                // Atualizar textos
                tbNomeDisplay.Text = perfil.Nome ?? "Utilizador";
                tbEmailDisplay.Text = perfil.Email ?? "email@exemplo.com";
                tbCargoDisplay.Text = "Cargo: Professor";
                tbDataCriacaoDisplay.Text = $"Criado em: {perfil.DataCriacao:dd/MM/yyyy}";

                tbNome.Text = perfil.Nome ?? "";
                tbEmail.Text = perfil.Email ?? "";

                // SÓ carregar foto se for realmente válida e personalizada
                if (TemFotoPersonalizadaValida())
                {
                    CarregarFotoPersonalizada(perfil.Foto);
                }
                // Manter o avatar bonito que já está
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar dados: {ex.Message}");
                CarregarDadosPadrao();
            }
        }

        /// <summary>
        /// Verificar se tem foto personalizada válida
        /// </summary>
        private bool TemFotoPersonalizadaValida()
        {
            try
            {
                var foto = modelPerfil?.PerfilAtual?.Foto;

                return !string.IsNullOrWhiteSpace(foto) &&
                       File.Exists(foto) &&
                       !foto.Contains("default_profile.png") && // Não é a foto padrão do model
                       new FileInfo(foto).Length > 1000; // Arquivo tem tamanho decente
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Carregar foto personalizada válida
        /// </summary>
        private void CarregarFotoPersonalizada(string caminhoFoto)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(caminhoFoto, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.EndInit();
                bitmap.Freeze();

                imgPerfil.ImageSource = bitmap;
                ellipsePerfil.Fill = new SolidColorBrush(Colors.Transparent);

                System.Diagnostics.Debug.WriteLine($"✅ Foto personalizada carregada: {caminhoFoto}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar foto personalizada: {ex.Message}");
                // Manter avatar atual
            }
        }

        private void CarregarDadosPadrao()
        {
            try
            {
                tbNomeDisplay.Text = "Utilizador";
                tbEmailDisplay.Text = "email@exemplo.com";
                tbCargoDisplay.Text = "Cargo: Professor";
                tbDataCriacaoDisplay.Text = $"Criado em: {DateTime.Now:dd/MM/yyyy}";

                tbNome.Text = "";
                tbEmail.Text = "";

                // MANTER avatar bonito atual (não recriar)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar dados padrão: {ex.Message}");
            }
        }

        #endregion

        #region Cleanup

        public void LimparEventos()
        {
            try
            {
                if (modelPerfil != null)
                {
                    modelPerfil.DadosCarregados -= ModelPerfil_DadosCarregados;
                    modelPerfil.DadosAlterados -= ModelPerfil_DadosAlterados;
                    modelPerfil.ErroOcorrido -= ModelPerfil_ErroOcorrido;
                    modelPerfil.PerfilModificado -= ModelPerfil_PerfilModificado;
                }
            }
            catch
            {
                // Ignorar erros na limpeza
            }
        }

        #endregion
    }
}