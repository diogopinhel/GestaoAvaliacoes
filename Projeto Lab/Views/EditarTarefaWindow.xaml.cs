using System;
using System.Windows;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class EditarTarefaWindow : Window
    {
        private App app;
        private ModelTarefas modelTarefas;
        private Tarefa tarefaOriginal;

        public EditarTarefaWindow(Tarefa tarefa)
        {
            InitializeComponent();

            if (tarefa == null)
                throw new ArgumentNullException(nameof(tarefa));

            tarefaOriginal = tarefa;

            // Obtém a instância do App (camada de interligação)
            app = App.Current as App;
            modelTarefas = app.Model_Tarefas;

            // Preencher os campos com os dados da tarefa
            PreencherCampos();

            // Colocar o foco no campo título
            tbTitulo.Focus();
            tbTitulo.SelectAll();
        }

        private void PreencherCampos()
        {
            tbId.Text = tarefaOriginal.Id.ToString();
            tbTitulo.Text = tarefaOriginal.Titulo;
            tbDescricao.Text = tarefaOriginal.Descricao ?? "";

            // Processar data e hora de início
            if (!string.IsNullOrEmpty(tarefaOriginal.DataHoraInicio))
            {
                try
                {
                    string[] partes = tarefaOriginal.DataHoraInicio.Split(' ');
                    if (partes.Length >= 1)
                    {
                        // Processar a data
                        string[] dataPartes = partes[0].Split('/');
                        if (dataPartes.Length == 3)
                        {
                            int dia = int.Parse(dataPartes[0]);
                            int mes = int.Parse(dataPartes[1]);
                            int ano = int.Parse(dataPartes[2]);
                            dpInicio.SelectedDate = new DateTime(ano, mes, dia);
                        }
                    }

                    if (partes.Length >= 2)
                    {
                        // Processar a hora
                        tbHoraInicio.Text = partes[1];
                    }
                    else
                    {
                        tbHoraInicio.Text = "00:00";
                    }
                }
                catch
                {
                    // Em caso de erro, use valores padrão
                    dpInicio.SelectedDate = DateTime.Today;
                    tbHoraInicio.Text = "00:00";
                }
            }
            else
            {
                dpInicio.SelectedDate = DateTime.Today;
                tbHoraInicio.Text = "00:00";
            }

            // Processar data e hora de fim
            if (!string.IsNullOrEmpty(tarefaOriginal.DataHoraFim))
            {
                try
                {
                    string[] partes = tarefaOriginal.DataHoraFim.Split(' ');
                    if (partes.Length >= 1)
                    {
                        // Processar a data
                        string[] dataPartes = partes[0].Split('/');
                        if (dataPartes.Length == 3)
                        {
                            int dia = int.Parse(dataPartes[0]);
                            int mes = int.Parse(dataPartes[1]);
                            int ano = int.Parse(dataPartes[2]);
                            dpFim.SelectedDate = new DateTime(ano, mes, dia);
                        }
                    }

                    if (partes.Length >= 2)
                    {
                        // Processar a hora
                        tbHoraFim.Text = partes[1];
                    }
                    else
                    {
                        tbHoraFim.Text = "23:59";
                    }
                }
                catch
                {
                    // Em caso de erro, use valores padrão
                    dpFim.SelectedDate = DateTime.Today.AddDays(7);
                    tbHoraFim.Text = "23:59";
                }
            }
            else
            {
                dpFim.SelectedDate = DateTime.Today.AddDays(7);
                tbHoraFim.Text = "23:59";
            }

            // Processar o peso
            if (!string.IsNullOrEmpty(tarefaOriginal.Peso))
            {
                string peso = tarefaOriginal.Peso.Replace("%", "").Trim();
                tbPeso.Text = peso;
            }
            else
            {
                tbPeso.Text = "10";
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Fechar a janela sem salvar alterações
            this.DialogResult = false;
            this.Close();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
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

                // Delegar ao Model a atualização da tarefa
                modelTarefas.AtualizarTarefa(tarefaOriginal.Id, titulo, descricao, dataHoraInicio, dataHoraFim, pesoStr);

                // Fechar a janela com sucesso
                this.DialogResult = true;
                this.Close();

                MessageBox.Show($"Tarefa '{titulo}' atualizada com sucesso!",
                              "Tarefa atualizada",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (OperacaoInvalidaException ex)
            {
                MessageBox.Show(ex.Message,
                              "Erro ao atualizar tarefa",
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