using System;
using System.Windows;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Views
{
    public partial class AdicionarAlunoWindow : Window
    {
        public Aluno NovoAluno { get; private set; }

        public AdicionarAlunoWindow()
        {
            InitializeComponent();
            txtNumero.Focus();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(txtNumero.Text))
                {
                    MessageBox.Show("Por favor, preencha o número do aluno.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumero.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNome.Text))
                {
                    MessageBox.Show("Por favor, preencha o nome do aluno.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNome.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Por favor, preencha o email do aluno.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return;
                }

                // Adicionar aluno ao modelo em vez de só criar objeto
                var app = App.Current as App;
                var modelAlunos = app.Model_Alunos;

                // Adicionar através do modelo (que vai disparar eventos e validações)
                modelAlunos.AdicionarAluno(txtNumero.Text.Trim(), txtNome.Text.Trim(), txtEmail.Text.Trim());

                // Guardar dados automaticamente
                app.DataManager.GuardarTodosDados();

                // Criar objeto para retorno (se necessário para compatibilidade)
                NovoAluno = new Aluno(txtNumero.Text.Trim(), txtNome.Text.Trim(), txtEmail.Text.Trim());

                // Fechar a janela com sucesso
                DialogResult = true;
                Close();

                // Mostrar mensagem de sucesso
                MessageBox.Show($"Aluno '{txtNome.Text.Trim()}' adicionado com sucesso!",
                              "Aluno adicionado",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message, "Erro ao adicionar aluno", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}