using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class GestaoGruposView : UserControl
    {
        private App app;
        private ModelGrupos modelGrupos;
        private ModelAlunos modelAlunos;
        private List<GrupoViewModel> listaGruposView; // Lista para a DataGrid

        public GestaoGruposView()
        {
            InitializeComponent();

            // Obtém a instância do App e os Models
            app = App.Current as App;
            modelGrupos = app.Model_Grupos;
            modelAlunos = app.Model_Alunos;

            // Inicializar lista de visualização
            listaGruposView = new List<GrupoViewModel>();

            // Configurar eventos do Model
            ConfigurarEventosModel();

            // Carregar dados iniciais
            CarregarDados();

            // Configurar event handlers dos botões
            btnCriarGrupo.Click += BtnCriarGrupo_Click;
            btnEditarGrupo.Click += BtnEditarGrupo_Click;
            btnRemoverGrupo.Click += BtnRemoverGrupo_Click;
            dgGrupos.SelectionChanged += DgGrupos_SelectionChanged;

            // Desabilitar botões até selecionar um grupo
            btnEditarGrupo.IsEnabled = false;
            btnRemoverGrupo.IsEnabled = false;

            // Configurar SearchBar
            ConfigurarSearchBar();
        }

        private void ConfigurarEventosModel()
        {
            // Escutar eventos do modelo para atualizar a interface
            modelGrupos.DadosCarregados += () => CarregarDados();
            modelGrupos.DadosAlterados += () => CarregarDados();
            modelGrupos.GrupoModificado += (id, operacao) =>
            {
                CarregarDados();
                // Mostrar notificação se necessário
            };
        }

        private void CarregarDados()
        {
            try
            {
                // Obter todos os grupos do modelo
                var grupos = modelGrupos.ObterTodosGrupos();

                // Converter para ViewModels para a DataGrid
                listaGruposView = grupos.Select(grupo => new GrupoViewModel
                {
                    Id = int.TryParse(grupo.Id, out int id) ? id : 0,
                    Nome = grupo.Nome,
                    Descricao = grupo.Descricao,
                    NumeroAlunos = grupo.NumeroAlunos,
                    AlunosNomes = ObterNomesAlunosDoGrupo(grupo),
                    DataCriacao = grupo.DataCriacao
                }).OrderBy(g => g.Id).ToList();

                // Atualizar DataGrid
                AtualizarDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar grupos: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private string ObterNomesAlunosDoGrupo(Grupo grupo)
        {
            if (grupo.NumerosAlunos == null || grupo.NumerosAlunos.Count == 0)
                return string.Empty;

            var nomes = new List<string>();
            foreach (var numeroAluno in grupo.NumerosAlunos)
            {
                if (modelAlunos.Lista.TryGetValue(numeroAluno, out Aluno aluno))
                {
                    nomes.Add(aluno.Nome);
                }
            }

            return string.Join(", ", nomes);
        }

        private void BtnCriarGrupo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Criar a janela de adicionar grupo
                var janela = new AdicionarGrupoWindow();

                // Mostrar a janela como modal
                bool? resultado = janela.ShowDialog();

                // Se o usuário criou um grupo com sucesso
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

        private void BtnEditarGrupo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var grupoViewModel = dgGrupos.SelectedItem as GrupoViewModel;
                if (grupoViewModel == null)
                {
                    MessageBox.Show("Por favor, selecione um grupo para editar.",
                                  "Nenhum grupo selecionado",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                // Obter o grupo do modelo
                var grupo = modelGrupos.ObterGrupo(grupoViewModel.Id.ToString());
                if (grupo == null)
                {
                    MessageBox.Show("Grupo não encontrado.",
                                  "Erro",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return;
                }

                // Criar a janela de editar grupo
                var janela = new EditarGrupoWindow(grupo);

                // Mostrar a janela como modal
                bool? resultado = janela.ShowDialog();

                // Se o usuário editou o grupo com sucesso
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

        private void BtnRemoverGrupo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var grupoViewModel = dgGrupos.SelectedItem as GrupoViewModel;
                if (grupoViewModel == null)
                {
                    MessageBox.Show("Por favor, selecione um grupo para remover.",
                                  "Nenhum grupo selecionado",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    return;
                }

                var resultado = MessageBox.Show($"Tem certeza que deseja remover o grupo '{grupoViewModel.Nome}'?",
                                              "Confirmar remoção",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Remover do modelo
                    modelGrupos.RemoverGrupo(grupoViewModel.Id.ToString());

                    // Desabilitar botões
                    btnEditarGrupo.IsEnabled = false;
                    btnRemoverGrupo.IsEnabled = false;

                    // A interface será atualizada automaticamente via eventos
                }
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao remover grupo",
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

        private void DgGrupos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Habilitar ou desabilitar botões com base na seleção
            btnEditarGrupo.IsEnabled = dgGrupos.SelectedItem != null;
            btnRemoverGrupo.IsEnabled = dgGrupos.SelectedItem != null;
        }

        // Método auxiliar para atualizar DataGrid
        private void AtualizarDataGrid()
        {
            dgGrupos.ItemsSource = null;
            dgGrupos.ItemsSource = listaGruposView;
        }

        // SearchBar: Configuração
        private void ConfigurarSearchBar()
        {
            tbPesquisa.GotFocus += TbPesquisa_GotFocus;
            tbPesquisa.LostFocus += TbPesquisa_LostFocus;
            tbPesquisa.TextChanged += TbPesquisa_TextChanged;
        }

        private void TbPesquisa_GotFocus(object sender, RoutedEventArgs e)
        {
            // Limpar o texto de placeholder quando o campo recebe foco
            if (tbPesquisa.Text == "Pesquisar grupo...")
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
                tbPesquisa.Text = "Pesquisar grupo...";
                tbPesquisa.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void TbPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Ignorar se o texto for o placeholder
                if (tbPesquisa.Text == "Pesquisar grupo..." || string.IsNullOrWhiteSpace(tbPesquisa.Text))
                {
                    dgGrupos.ItemsSource = listaGruposView;
                    return;
                }

                // Pesquisar usando o modelo
                var textoPesquisa = tbPesquisa.Text;
                var gruposEncontrados = modelGrupos.PesquisarGrupos(textoPesquisa);

                // Converter para ViewModels
                var resultadosFiltrados = gruposEncontrados.Select(grupo => new GrupoViewModel
                {
                    Id = int.TryParse(grupo.Id, out int id) ? id : 0,
                    Nome = grupo.Nome,
                    Descricao = grupo.Descricao,
                    NumeroAlunos = grupo.NumeroAlunos,
                    AlunosNomes = ObterNomesAlunosDoGrupo(grupo),
                    DataCriacao = grupo.DataCriacao
                }).ToList();

                // Atualizar a DataGrid com os resultados filtrados
                dgGrupos.ItemsSource = resultadosFiltrados;
            }
            catch (Exception ex)
            {
                // Em caso de erro na pesquisa, mostrar todos os grupos
                dgGrupos.ItemsSource = listaGruposView;
            }
        }
    }

    // Classe ViewModel para representar um grupo na DataGrid
    public class GrupoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int NumeroAlunos { get; set; }
        public string AlunosNomes { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    // Classe auxiliar para exibir alunos nas listas (reutilizada pelas janelas)
    public class AlunoDisplay
    {
        public string Numero { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string DisplayText { get; set; }
    }
}