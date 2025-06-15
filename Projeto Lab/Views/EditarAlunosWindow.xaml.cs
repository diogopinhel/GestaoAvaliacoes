using System;
using System.Windows;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Views
{
    public partial class EditarAlunosWindow : Window
    {
        private Aluno alunoOriginal;
        private string numeroOriginal;
        public bool DadosAlterados { get; private set; }

        /// <summary>
        /// Construtor que recebe o aluno a ser editado
        /// </summary>
        /// <param name="aluno">Aluno a ser editado</param>
        public EditarAlunosWindow(Aluno aluno)
        {
            InitializeComponent();

            if (aluno == null)
                throw new ArgumentNullException(nameof(aluno), "Aluno não pode ser nulo");

            alunoOriginal = aluno;
            numeroOriginal = aluno.Numero;
            DadosAlterados = false;

            CarregarDadosAluno();
            ConfigurarFoco();
        }

        #region Inicialização

        /// <summary>
        /// Carrega os dados atuais do aluno na interface
        /// </summary>
        private void CarregarDadosAluno()
        {
            try
            {
                // Mostrar dados atuais
                tbNumeroAtual.Text = alunoOriginal.Numero;
                tbNomeAtual.Text = alunoOriginal.Nome;
                tbEmailAtual.Text = alunoOriginal.Email;

                // Preencher campos de edição com dados atuais
                txtNovoNumero.Text = alunoOriginal.Numero;
                txtNovoNome.Text = alunoOriginal.Nome;
                txtNovoEmail.Text = alunoOriginal.Email;

                // Atualizar título da janela
                Title = $"Editar Aluno - {alunoOriginal.Nome}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados do aluno: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Configura o foco inicial
        /// </summary>
        private void ConfigurarFoco()
        {
            txtNovoNumero.Focus();
            txtNovoNumero.SelectAll();
        }

        #endregion

        #region Event Handlers dos Botões

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Verificar se houve alterações
                if (HouveAlteracoes())
                {
                    var resultado = MessageBox.Show(
                        "Tem alterações não guardadas. Tem certeza que deseja cancelar?",
                        "Confirmar cancelamento",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.No)
                        return;
                }

                DadosAlterados = false;
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cancelar: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var resultado = MessageBox.Show(
                    "Tem certeza que deseja limpar todos os campos e restaurar os dados originais?",
                    "Confirmar limpeza",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    CarregarDadosAluno();
                    txtNovoNumero.Focus();
                    txtNovoNumero.SelectAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao limpar campos: {ex.Message}",
                              "Erro",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validações básicas
                if (!ValidarCampos())
                    return;

                // Verificar se houve alterações
                if (!HouveAlteracoes())
                {
                    MessageBox.Show("Não foram feitas alterações nos dados do aluno.",
                                  "Sem alterações",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return;
                }

                // Confirmar alterações
                var confirmacao = MessageBox.Show(
                    $"Confirma a atualização dos dados do aluno?\n\n" +
                    $"Número: {txtNovoNumero.Text.Trim()}\n" +
                    $"Nome: {txtNovoNome.Text.Trim()}\n" +
                    $"Email: {txtNovoEmail.Text.Trim()}",
                    "Confirmar alterações",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacao == MessageBoxResult.No)
                    return;

                // Atualizar através do modelo
                var app = App.Current as App;
                var modelAlunos = app.Model_Alunos;

                modelAlunos.AtualizarAluno(
                    numeroOriginal,
                    txtNovoNumero.Text.Trim(),
                    txtNovoNome.Text.Trim(),
                    txtNovoEmail.Text.Trim()
                );

                // Guardar dados automaticamente
                app.DataManager.GuardarTodosDados();

                // Marcar como alterado e fechar
                DadosAlterados = true;
                DialogResult = true;
                Close();

                // Mostrar mensagem de sucesso
                MessageBox.Show($"Dados do aluno '{txtNovoNome.Text.Trim()}' atualizados com sucesso!",
                              "Aluno atualizado",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao atualizar aluno",
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

        #endregion

        #region Métodos de Validação

        /// <summary>
        /// Valida todos os campos da interface
        /// </summary>
        /// <returns>True se todos os campos são válidos</returns>
        private bool ValidarCampos()
        {
            // Validar número
            if (string.IsNullOrWhiteSpace(txtNovoNumero.Text))
            {
                MessageBox.Show("Por favor, preencha o número do aluno.",
                              "Campo obrigatório",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                txtNovoNumero.Focus();
                return false;
            }

            // Validar nome
            if (string.IsNullOrWhiteSpace(txtNovoNome.Text))
            {
                MessageBox.Show("Por favor, preencha o nome do aluno.",
                              "Campo obrigatório",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                txtNovoNome.Focus();
                return false;
            }

            // Validar email
            if (string.IsNullOrWhiteSpace(txtNovoEmail.Text))
            {
                MessageBox.Show("Por favor, preencha o email do aluno.",
                              "Campo obrigatório",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                txtNovoEmail.Focus();
                return false;
            }

            // Validar formato do email
            if (!ValidarFormatoEmail(txtNovoEmail.Text.Trim()))
            {
                MessageBox.Show("Por favor, insira um email válido.",
                              "Email inválido",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                txtNovoEmail.Focus();
                txtNovoEmail.SelectAll();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida o formato do email
        /// </summary>
        /// <param name="email">Email a validar</param>
        /// <returns>True se o formato é válido</returns>
        private bool ValidarFormatoEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se houve alterações nos dados
        /// </summary>
        /// <returns>True se houve alterações</returns>
        private bool HouveAlteracoes()
        {
            var numeroAtual = txtNovoNumero.Text?.Trim() ?? string.Empty;
            var nomeAtual = txtNovoNome.Text?.Trim() ?? string.Empty;
            var emailAtual = txtNovoEmail.Text?.Trim() ?? string.Empty;

            return numeroAtual != alunoOriginal.Numero ||
                   nomeAtual != alunoOriginal.Nome ||
                   emailAtual != alunoOriginal.Email;
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Obtém os dados atualizados do aluno (caso tenham sido guardados)
        /// </summary>
        /// <returns>Aluno com dados atualizados ou null se não foram guardados</returns>
        public Aluno ObterAlunoAtualizado()
        {
            if (!DadosAlterados)
                return null;

            try
            {
                var app = App.Current as App;
                var modelAlunos = app.Model_Alunos;

                // Buscar o aluno com o novo número
                return modelAlunos.ObterAluno(txtNovoNumero.Text.Trim());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Força a atualização da interface com novos dados
        /// </summary>
        /// <param name="novoAluno">Novo aluno para mostrar</param>
        public void AtualizarInterfaceComNovoAluno(Aluno novoAluno)
        {
            if (novoAluno == null)
                return;

            try
            {
                alunoOriginal = novoAluno;
                numeroOriginal = novoAluno.Numero;
                CarregarDadosAluno();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar interface: {ex.Message}");
            }
        }

        #endregion

        #region Override de Eventos da Window

        /// <summary>
        /// Override para capturar o fechamento da janela
        /// </summary>
        /// <param name="e">Argumentos do evento</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Se estiver fechando sem ter guardado e há alterações
                if (DialogResult != true && HouveAlteracoes())
                {
                    var resultado = MessageBox.Show(
                        "Tem alterações não guardadas. Tem certeza que deseja fechar?",
                        "Confirmar fecho",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao fechar janela: {ex.Message}");
                base.OnClosing(e);
            }
        }

        #endregion
    }
}