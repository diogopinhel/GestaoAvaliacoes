using System;
using System.Windows;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class NovaTarefaWindow : Window
    {
        private App app;
        private ModelTarefas modelTarefas;

        public NovaTarefaWindow()
        {
            InitializeComponent();

            // Obtém a instância do App (camada de interligação)
            app = App.Current as App;
            modelTarefas = app.Model_Tarefas;

            // Inicializar campos de data com a data atual
            dpInicio.SelectedDate = DateTime.Today;
            dpFim.SelectedDate = DateTime.Today.AddDays(7);

            // Valores padrão para os campos de hora
            tbHoraInicio.Text = "00:00";
            tbHoraFim.Text = "23:59";

            // Valor padrão para o peso
            tbPeso.Text = "10";

            // Colocar o foco no primeiro campo
            tbTitulo.Focus();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Fechar a janela sem adicionar a tarefa
            this.DialogResult = false;
            this.Close();
        }

        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validação básica
                if (string.IsNullOrWhiteSpace(tbTitulo.Text))
                {
                    MessageBox.Show("Por favor, preencha o título da tarefa.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbTitulo.Focus();
                    return;
                }

                // Validar datas
                if (!dpInicio.SelectedDate.HasValue)
                {
                    MessageBox.Show("Por favor, selecione uma data de início.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpInicio.Focus();
                    return;
                }

                if (!dpFim.SelectedDate.HasValue)
                {
                    MessageBox.Show("Por favor, selecione uma data de fim.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpFim.Focus();
                    return;
                }

                if (dpFim.SelectedDate < dpInicio.SelectedDate)
                {
                    MessageBox.Show("A data de fim deve ser posterior à data de início.", "Data inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpFim.Focus();
                    return;
                }

                // Validar formato da hora
                if (!ValidarFormatoHora(tbHoraInicio.Text))
                {
                    MessageBox.Show("Por favor, insira a hora de início no formato HH:MM.", "Formato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbHoraInicio.Focus();
                    return;
                }

                if (!ValidarFormatoHora(tbHoraFim.Text))
                {
                    MessageBox.Show("Por favor, insira a hora de fim no formato HH:MM.", "Formato inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbHoraFim.Focus();
                    return;
                }

                // Validar peso
                int peso;
                if (!int.TryParse(tbPeso.Text, out peso) || peso <= 0 || peso > 100)
                {
                    MessageBox.Show("O peso deve ser um número entre 1 e 100.", "Valor inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbPeso.Focus();
                    return;
                }

                // Preparar dados para o Model
                string titulo = tbTitulo.Text.Trim();
                string descricao = tbDescricao.Text?.Trim() ?? string.Empty;
                string dataHoraInicio = $"{dpInicio.SelectedDate.Value:dd/MM/yyyy} {tbHoraInicio.Text}";
                string dataHoraFim = $"{dpFim.SelectedDate.Value:dd/MM/yyyy} {tbHoraFim.Text}";
                string pesoStr = $"{peso}%";

                // Delegar ao Model a adição da tarefa
                modelTarefas.AdicionarTarefa(titulo, descricao, dataHoraInicio, dataHoraFim, pesoStr);

                // Fechar a janela com sucesso
                this.DialogResult = true;
                this.Close();

                MessageBox.Show($"Tarefa '{titulo}' criada com sucesso!",
                              "Tarefa criada",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao criar tarefa",
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

        private bool ValidarFormatoHora(string hora)
        {
            // Verificar se a hora está no formato HH:MM
            if (string.IsNullOrWhiteSpace(hora))
                return false;

            string[] partes = hora.Split(':');
            if (partes.Length != 2)
                return false;

            int horas, minutos;
            if (!int.TryParse(partes[0], out horas) || !int.TryParse(partes[1], out minutos))
                return false;

            if (horas < 0 || horas > 23 || minutos < 0 || minutos > 59)
                return false;

            return true;
        }
    }
}