using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class EditarGrupoWindow : Window
    {
        private App app;
        private ModelAlunos modelAlunos;
        private ModelGrupos modelGrupos;
        private Grupo grupoOriginal;

        // Lista observável para os alunos do grupo
        public ObservableCollection<AlunoDisplay> AlunosNoGrupo { get; set; }

        // Lista observável para os alunos disponíveis no ComboBox
        public ObservableCollection<AlunoDisplay> AlunosDisponiveis { get; set; }

        // Propriedade para armazenar o grupo atualizado
        public Grupo GrupoAtualizado { get; private set; }

        public EditarGrupoWindow(Grupo grupo)
        {
            InitializeComponent();

            if (grupo == null)
                throw new ArgumentNullException(nameof(grupo));

            grupoOriginal = grupo;

            // Obtém a instância do App e os Models
            app = (App)App.Current;
            modelAlunos = app.Model_Alunos;
            modelGrupos = app.Model_Grupos;

            // Inicializar coleções
            AlunosNoGrupo = new ObservableCollection<AlunoDisplay>();
            AlunosDisponiveis = new ObservableCollection<AlunoDisplay>();

            // Configurar controles
            lstAlunosGrupo.ItemsSource = AlunosNoGrupo;
            cbAlunosDisponiveis.ItemsSource = AlunosDisponiveis;

            // Carregar dados do grupo
            CarregarDadosGrupo();

            // Carregar alunos disponíveis
            CarregarAlunosDisponiveis();

            // Colocar foco no campo nome
            txtNome.Focus();
            txtNome.SelectAll();
        }

        private void CarregarDadosGrupo()
        {
            // Preencher campos básicos
            txtId.Text = grupoOriginal.Id;
            txtNome.Text = grupoOriginal.Nome;
            txtBadgeGrupo.Text = grupoOriginal.Nome;

            // Carregar alunos do grupo
            CarregarAlunosDoGrupo();
        }

        private void CarregarAlunosDoGrupo()
        {
            AlunosNoGrupo.Clear();

            // Obter alunos do grupo usando os números
            foreach (var numeroAluno in grupoOriginal.NumerosAlunos)
            {
                var aluno = modelAlunos.Lista.Values.FirstOrDefault(a => a.Numero == numeroAluno);
                if (aluno != null)
                {
                    AlunosNoGrupo.Add(new AlunoDisplay
                    {
                        Numero = aluno.Numero,
                        Nome = aluno.Nome,
                        Email = aluno.Email,
                        DisplayText = $"{aluno.Numero} - {aluno.Nome}"
                    });
                }
            }
        }

        private void CarregarAlunosDisponiveis()
        {
            AlunosDisponiveis.Clear();

            // Obter todos os alunos disponíveis (não estão em outros grupos)
            var alunosDisponiveis = modelGrupos.ObterAlunosDisponiveis();

            // Também incluir alunos que estão no grupo atual (para poderem ser removidos e readicionados)
            var numerosAlunosNoGrupoAtual = grupoOriginal.NumerosAlunos.ToHashSet();
            var alunosDoGrupoAtual = modelAlunos.Lista.Values
                .Where(a => numerosAlunosNoGrupoAtual.Contains(a.Numero))
                .ToList();

            // Combinar alunos disponíveis com alunos do grupo atual
            var todosAlunosDisponiveis = alunosDisponiveis.Concat(alunosDoGrupoAtual)
                .Distinct()
                .OrderBy(a => a.Nome);

            // Filtrar apenas os que NÃO estão atualmente mostrados na lista do grupo
            var numerosAlunosJaNaLista = AlunosNoGrupo.Select(a => a.Numero).ToHashSet();

            foreach (var aluno in todosAlunosDisponiveis)
            {
                if (!numerosAlunosJaNaLista.Contains(aluno.Numero))
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
        }

        private void CbAlunosDisponiveis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Event handler vazio - a adição é feita pelo botão
        }

        private void BtnAdicionarAluno_Click(object sender, RoutedEventArgs e)
        {
            var alunoSelecionado = cbAlunosDisponiveis.SelectedItem as AlunoDisplay;

            if (alunoSelecionado == null)
            {
                MessageBox.Show("Por favor, selecione um aluno da lista para adicionar ao grupo.",
                              "Nenhum aluno selecionado",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            // Adicionar o aluno ao grupo na interface
            AlunosNoGrupo.Add(new AlunoDisplay
            {
                Numero = alunoSelecionado.Numero,
                Nome = alunoSelecionado.Nome,
                Email = alunoSelecionado.Email,
                DisplayText = alunoSelecionado.DisplayText
            });

            // Recarregar lista de disponíveis
            CarregarAlunosDisponiveis();

            // Limpar seleção do ComboBox
            cbAlunosDisponiveis.SelectedIndex = -1;
            if (cbAlunosDisponiveis.IsEditable)
                cbAlunosDisponiveis.Text = "Selecionar aluno disponível...";
        }

        private void BtnRemoverAluno_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var alunoParaRemover = button?.Tag as AlunoDisplay;

            if (alunoParaRemover != null)
            {
                // Remover do grupo na interface
                AlunosNoGrupo.Remove(alunoParaRemover);

                // Recarregar lista de disponíveis
                CarregarAlunosDisponiveis();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Fechar a janela sem salvar alterações
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

                string novoNome = txtNome.Text;
                string novaDescricao = grupoOriginal.Descricao; // Manter descrição original

                // Atualizar dados básicos do grupo
                modelGrupos.AtualizarGrupo(grupoOriginal.Id, novoNome, novaDescricao);

                // Obter o grupo atualizado
                var grupoAtualizado = modelGrupos.ObterGrupo(grupoOriginal.Id);

                // Sincronizar alunos do grupo
                // 1. Remover alunos que não estão mais na lista
                var numerosAlunosNaInterface = AlunosNoGrupo.Select(a => a.Numero).ToHashSet();
                var alunosParaRemover = grupoAtualizado.NumerosAlunos
                    .Where(numero => !numerosAlunosNaInterface.Contains(numero))
                    .ToList();

                foreach (var numeroAluno in alunosParaRemover)
                {
                    modelGrupos.RemoverAlunoDoGrupo(grupoOriginal.Id, numeroAluno);
                }

                // 2. Adicionar alunos que foram adicionados na interface
                var numerosAlunosNoGrupo = grupoAtualizado.NumerosAlunos.ToHashSet();
                var alunosParaAdicionar = numerosAlunosNaInterface
                    .Where(numero => !numerosAlunosNoGrupo.Contains(numero))
                    .ToList();

                foreach (var numeroAluno in alunosParaAdicionar)
                {
                    modelGrupos.AdicionarAlunoAoGrupo(grupoOriginal.Id, numeroAluno);
                }

                // Obter o grupo final atualizado
                GrupoAtualizado = modelGrupos.ObterGrupo(grupoOriginal.Id);

                // Fechar a janela com sucesso
                this.DialogResult = true;
                this.Close();

                MessageBox.Show($"Grupo '{novoNome}' atualizado com sucesso!",
                              "Grupo atualizado",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao atualizar grupo",
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