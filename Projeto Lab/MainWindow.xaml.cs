using Projecto_Lab.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Projecto_Lab
{
    public partial class MainWindow : Window
    {
        private App app;
        private UserControl currentView;

        public MainWindow()
        {
            InitializeComponent();

            // Obtém a instância do App (camada de interligação)
            app = (App)App.Current;

            // Subscrever ao evento de alteração do perfil
            if (app.Model_Perfil != null)
            {
                app.Model_Perfil.DadosAlterados += OnPerfilAlterado;
                app.Model_Perfil.PerfilModificado += OnPerfilModificado;
            }

            // Carregar dados do utilizador e dashboard ao inicializar
            this.Loaded += Window_Loaded;
        }

        /// <summary>
        /// Event handler para quando o perfil é alterado
        /// </summary>
        private void OnPerfilAlterado()
        {
            Dispatcher.Invoke(() =>
            {
                AtualizarDadosUtilizador();
                System.Diagnostics.Debug.WriteLine("🔄 Perfil alterado - atualizando barra lateral");
            });
        }

        /// <summary>
        /// Event handler para quando o perfil é modificado
        /// </summary>
        private void OnPerfilModificado(string mensagem)
        {
            Dispatcher.Invoke(() =>
            {
                AtualizarDadosUtilizador();
                System.Diagnostics.Debug.WriteLine($"🔄 Perfil modificado: {mensagem}");
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Carregar Dashboard como tela inicial
            ShowDashboardView();

            // Marcar Dashboard como selecionada visualmente
            HighlightSelectedMenu("Dashboard");

            // Atualizar dados do utilizador na barra lateral
            AtualizarDadosUtilizador();
        }

        #region Métodos de Navegação

        private void Menu_Dashboard_Click(object sender, MouseButtonEventArgs e)
        {
            ShowDashboardView();
            HighlightSelectedMenu("Dashboard");
        }

        private void Menu_Alunos_Click(object sender, MouseButtonEventArgs e)
        {
            ShowAlunosView();
            HighlightSelectedMenu("Alunos");
        }

        private void Menu_Tarefas_Click(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = app.TarefasView;
            currentView = app.TarefasView;
            HighlightSelectedMenu("Tarefas");
        }

        private void Menu_Grupos_Click(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = app.GruposView;
            currentView = app.GruposView;
            HighlightSelectedMenu("Grupos");
        }

        private void Menu_Notas_Click(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = app.NotasView;
            currentView = app.NotasView;
            HighlightSelectedMenu("Notas");
        }

        private void Menu_Pautas_Click(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = app.PautasView;
            currentView = app.PautasView;
            HighlightSelectedMenu("Pautas");
        }

        private void Menu_Perfil_Click(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = app.PerfilView;
            currentView = app.PerfilView;
            HighlightSelectedMenu("Perfil");
        }

        private void ShowDashboardView()
        {
            // Sempre criar nova instância da DashboardView para garantir dados atualizados
            var dashboardView = new DashboardView();
            MainContent.Content = dashboardView;
            currentView = dashboardView;
        }

        private void ShowAlunosView()
        {
            MainContent.Content = app.AlunosView;
            currentView = app.AlunosView;
        }

        #endregion

        #region Gestão Visual do Menu

        private void HighlightSelectedMenu(string selectedMenu)
        {
            try
            {
                // Resetar todos os menus
                ResetarTodosMenus();

                // Destacar o menu selecionado
                var menuElement = FindName($"Menu_{selectedMenu}") as Border;
                if (menuElement != null)
                {
                    menuElement.Background = new SolidColorBrush(
                        Color.FromArgb(50, 255, 255, 255)); // Branco com transparência
                }

                System.Diagnostics.Debug.WriteLine($"Menu selecionado: {selectedMenu}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao destacar menu: {ex.Message}");
            }
        }

        private void ResetarTodosMenus()
        {
            try
            {
                var menus = new[] { "Dashboard", "Alunos", "Tarefas", "Grupos", "Notas", "Pautas", "Perfil" };

                foreach (var menu in menus)
                {
                    var menuElement = FindName($"Menu_{menu}") as Border;
                    if (menuElement != null)
                    {
                        menuElement.Background = Brushes.Transparent;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao resetar menus: {ex.Message}");
            }
        }

        #endregion

        #region Gestão do Perfil do Utilizador

        private void AtualizarDadosUtilizador()
        {
            try
            {
                var perfil = app.Model_Perfil?.PerfilAtual;
                if (perfil == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Perfil é nulo!");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🔍 Perfil encontrado: Nome='{perfil.Nome}'");

                // Atualizar apenas o nome (cargo é estático)
                if (FindName("lblNomeUtilizador") is TextBlock lblNome)
                {
                    lblNome.Text = perfil.Nome;
                    System.Diagnostics.Debug.WriteLine($"✅ Nome atualizado: {perfil.Nome}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ lblNomeUtilizador não encontrado!");
                }

                // Atualizar foto de perfil
                AtualizarFotoPerfil(perfil);

                System.Diagnostics.Debug.WriteLine($"✅ Dados do utilizador atualizados: {perfil.Nome}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar dados do utilizador: {ex.Message}");
            }
        }

        private void AtualizarFotoPerfil(Projecto_Lab.Classes.Perfil perfil)
        {
            try
            {
                var ellipseFoto = FindName("ellipseFotoPerfil") as Ellipse;
                var imageBrush = FindName("imageBrushPerfil") as ImageBrush;
                var txtIniciais = FindName("txtIniciais") as TextBlock;

                if (ellipseFoto == null || imageBrush == null || txtIniciais == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Elementos de foto não encontrados!");
                    return;
                }

                // Tentar carregar foto real
                if (!string.IsNullOrEmpty(perfil.Foto) && File.Exists(perfil.Foto))
                {
                    try
                    {
                        // Criar BitmapImage com configurações otimizadas
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(perfil.Foto, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        // Aplicar ao ImageBrush
                        imageBrush.ImageSource = bitmap;

                        // Mostrar ellipse, esconder iniciais
                        ellipseFoto.Visibility = Visibility.Visible;
                        txtIniciais.Visibility = Visibility.Collapsed;

                        System.Diagnostics.Debug.WriteLine($"✅ Foto circular aplicada: {perfil.Foto}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar foto: {ex.Message}");
                    }
                }

                // Fallback: usar iniciais
                ellipseFoto.Visibility = Visibility.Collapsed;
                txtIniciais.Visibility = Visibility.Visible;
                txtIniciais.Text = GerarIniciais(perfil.Nome);

                System.Diagnostics.Debug.WriteLine($"ℹ️ Usando iniciais: {txtIniciais.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar foto de perfil: {ex.Message}");
            }
        }

        private string GerarIniciais(string nomeCompleto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomeCompleto))
                    return "U";

                var palavras = nomeCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (palavras.Length == 1)
                {
                    return palavras[0].Length >= 2 ? palavras[0].Substring(0, 2).ToUpper() : palavras[0].ToUpper();
                }
                else
                {
                    return (palavras[0].Substring(0, 1) + palavras[palavras.Length - 1].Substring(0, 1)).ToUpper();
                }
            }
            catch
            {
                return "U";
            }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Força atualização da view atual (útil para dashboard)
        /// </summary>
        public void RefreshCurrentView()
        {
            if (currentView is DashboardView)
            {
                ShowDashboardView(); // Recriar dashboard para atualizar dados
            }
        }

        /// <summary>
        /// Atualiza dados do utilizador na interface (chamado externamente)
        /// </summary>
        public void AtualizarPerfilInterface()
        {
            AtualizarDadosUtilizador();
        }

        /// <summary>
        /// Limpar eventos ao fechar a janela
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                if (app?.Model_Perfil != null)
                {
                    app.Model_Perfil.DadosAlterados -= OnPerfilAlterado;
                    app.Model_Perfil.PerfilModificado -= OnPerfilModificado;
                }
            }
            catch
            {
                // Ignorar erros na limpeza
            }

            base.OnClosed(e);
        }

        #endregion
    }
}