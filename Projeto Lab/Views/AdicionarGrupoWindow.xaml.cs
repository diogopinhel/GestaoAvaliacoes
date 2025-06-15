using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class AdicionarGrupoWindow : Window
    {
        private App app;
        private ModelAlunos modelAlunos;
        private ModelGrupos modelGrupos;

        // Listas observáveis para os alunos
        public ObservableCollection<AlunoDisplay> AlunosDisponiveis { get; set; }
        public ObservableCollection<AlunoDisplay> AlunosNoGrupo { get; set; }

        // Propriedade para armazenar o novo grupo criado
        public Grupo NovoGrupo { get; private set; }

        public AdicionarGrupoWindow()
        {
            InitializeComponent();

            // Obtém a instância do App e os Models
            app = App.Current as App;
            modelAlunos = app.Model_Alunos;
            modelGrupos = app.Model_Grupos;

            // Inicializar coleções
            AlunosDisponiveis = new ObservableCollection<AlunoDisplay>();
            AlunosNoGrupo = new ObservableCollection<AlunoDisplay>();

            // Configurar DataContext
            lstAlunosDisponiveis.ItemsSource = AlunosDisponiveis;
            lstAlunosGrupo.ItemsSource = AlunosNoGrupo;

            // Carregar alunos disponíveis
            CarregarAlunosDisponiveis();

            // Definir próximo ID
            DefinirProximoId();

            // Colocar foco no campo nome
            txtNome.Focus();
        }

        private void CarregarAlunosDisponiveis()
        {
            AlunosDisponiveis.Clear();

            // Usar o método do ModelGrupos para obter alunos disponíveis
            var alunosDisponiveis = modelGrupos.ObterAlunosDisponiveis();

            foreach (var aluno in alunosDisponiveis.OrderBy(a => a.Nome))
            {
                AlunosDisponiveis.Add(new AlunoDisplay
                {
                    Numero = aluno.Numero,
                    Nome = aluno.Nome,
                    Email = aluno.Email,
                    DisplayText = $"{aluno.Numero} - {aluno.Nome}"
                });
            }
        }

        private void DefinirProximoId()
        {
            // Usar o método do ModelGrupos para gerar próximo ID
            txtId.Text = modelGrupos.GerarProximoId();
        }

        private void BtnAdicionarAluno_Click(object sender, RoutedEventArgs e)
        {
            // Mover alunos selecionados da lista disponível para o grupo
            var alunosSelecionados = lstAlunosDisponiveis.SelectedItems.Cast<AlunoDisplay>().ToList();

            foreach (var aluno in alunosSelecionados)
            {
                AlunosDisponiveis.Remove(aluno);
                AlunosNoGrupo.Add(aluno);
            }
        }

        private void BtnRemoverAluno_Click(object sender, RoutedEventArgs e)
        {
            // Mover alunos selecionados do grupo para a lista disponível
            var alunosSelecionados = lstAlunosGrupo.SelectedItems.Cast<AlunoDisplay>().ToList();

            foreach (var aluno in alunosSelecionados)
            {
                AlunosNoGrupo.Remove(aluno);
                AlunosDisponiveis.Add(aluno);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Fechar a janela sem criar o grupo
            this.DialogResult = false;
            this.Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validação básica
                if (string.IsNullOrWhiteSpace(txtNome.Text))
                {
                    MessageBox.Show("Por favor, preencha o nome do grupo.",
                                  "Campo obrigatório",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    txtNome.Focus();
                    return;
                }

                string id = txtId.Text;
                string nome = txtNome.Text;

                // And update this line to use an empty string for description
                modelGrupos.AdicionarGrupo(id, nome, "");

                // Rest of the code remains unchanged
                // Adicionar alunos ao grupo
                foreach (var alunoDisplay in AlunosNoGrupo)
                {
                    modelGrupos.AdicionarAlunoAoGrupo(id, alunoDisplay.Numero);
                }

                // Obter o grupo criado para retornar
                NovoGrupo = modelGrupos.ObterGrupo(id);

                // Fechar a janela com sucesso
                this.DialogResult = true;
                this.Close();

                MessageBox.Show($"Grupo '{nome}' criado com sucesso!",
                              "Grupo criado",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao criar grupo",
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
    }
}