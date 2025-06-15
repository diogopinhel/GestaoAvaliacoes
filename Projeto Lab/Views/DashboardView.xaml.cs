using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Projecto_Lab.Models;

namespace Projecto_Lab.Views
{
    public partial class DashboardView : UserControl
    {
        private App app;
        private ModelAlunos modelAlunos;
        private ModelGrupos modelGrupos;
        private ModelTarefas modelTarefas;
        private ModelNotas modelNotas;
        private ModelPautas modelPautas;
        private ModelPerfil modelPerfil;


        private ObservableCollection<AtividadeRecente> atividadesRecentes;

        // Timer para atualizar tempos relativos
        private DispatcherTimer timerAtualizacao;

        public DashboardView()
        {
            InitializeComponent();

            // Obtém a instância do App (camada de interligação)
            app = App.Current as App;

            // Obter referências dos models
            modelAlunos = app.Model_Alunos;
            modelGrupos = app.Model_Grupos;
            modelTarefas = app.Model_Tarefas;
            modelNotas = app.Model_Notas;
            modelPautas = app.Model_Pautas;
            modelPerfil = app.Model_Perfil;

            // Inicializar ObservableCollection
            atividadesRecentes = new ObservableCollection<AtividadeRecente>();

            // Conectar ao ItemsControl
            lstAtividadesRecentes.ItemsSource = atividadesRecentes;

            // Subscrever aos eventos para atualizações em tempo real
            SubscreverEventos();

            // Inicializar timer para atualizar tempos
            InicializarTimer();

            // Carregar dados dinamicamente
            AtualizarDashboard();

            // Adicionar atividade inicial para testar
            AdicionarAtividadeRecente("Sistema iniciado com sucesso", "🚀");
        }

        /// <summary>
        /// Inicializar timer para atualizar tempos relativos
        /// </summary>
        private void InicializarTimer()
        {
            try
            {
                timerAtualizacao = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(1) // Atualizar a cada minuto
                };
                timerAtualizacao.Tick += (s, e) => AtualizarTemposRelativos();
                timerAtualizacao.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao inicializar timer: {ex.Message}");
            }
        }

        /// <summary>
        /// Subscrever aos eventos dos models para atualização automática
        /// </summary>
        private void SubscreverEventos()
        {
            try
            {
                if (modelAlunos != null)
                {
                    modelAlunos.DadosAlterados += OnDadosAlterados;
                    modelAlunos.AlunoModificado += OnAlunoModificado;
                }

                if (modelGrupos != null)
                {
                    modelGrupos.DadosAlterados += OnDadosAlterados;
                    modelGrupos.GrupoModificado += OnGrupoModificado;
                }

                if (modelTarefas != null)
                {
                    modelTarefas.DadosAlterados += OnDadosAlterados;
                    modelTarefas.TarefaModificada += OnTarefaModificada;
                }

                if (modelNotas != null)
                {
                    modelNotas.DadosAlterados += OnDadosAlterados;
                    modelNotas.NotaModificada += OnNotaModificada;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao subscrever eventos: {ex.Message}");
            }
        }

        #region Event Handlers para atualizações automáticas

        private void OnDadosAlterados()
        {
            Dispatcher.Invoke(() =>
            {
                AtualizarDashboard();
                System.Diagnostics.Debug.WriteLine("🔄 Dashboard atualizada automaticamente");
            });
        }

        private void OnAlunoModificado(string numeroAluno, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                AdicionarAtividadeRecente($"Aluno {numeroAluno} {operacao.ToLower()}", "👤");
                AtualizarDashboard();
            });
        }

        private void OnGrupoModificado(string idGrupo, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                var nomeGrupo = modelGrupos?.ObterGrupo(idGrupo)?.Nome ?? $"Grupo {idGrupo}";
                AdicionarAtividadeRecente($"Grupo '{nomeGrupo}' {operacao.ToLower()}", "👥");
                AtualizarDashboard();
            });
        }

        private void OnTarefaModificada(int idTarefa, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                var nomeTarefa = modelTarefas?.ObterTarefa(idTarefa)?.Titulo ?? $"Tarefa {idTarefa}";
                AdicionarAtividadeRecente($"Tarefa '{nomeTarefa}' {operacao.ToLower()}", "📋");
                AtualizarDashboard();
            });
        }

        private void OnNotaModificada(int idNota, string operacao)
        {
            Dispatcher.Invoke(() =>
            {
                AdicionarAtividadeRecente($"Nota {operacao.ToLower()}", "📝");
                AtualizarDashboard();
            });
        }

        #endregion

        /// <summary>
        /// Método principal para atualizar todos os dados da dashboard
        /// </summary>
        private void AtualizarDashboard()
        {
            try
            {
                AtualizarBoasVindas();
                AtualizarEstatisticasGerais();
                AtualizarAtividadesRecentes();

                System.Diagnostics.Debug.WriteLine("✅ Dashboard atualizada com dados dinâmicos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar dashboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualizar mensagem de boas-vindas com nome do utilizador
        /// </summary>
        private void AtualizarBoasVindas()
        {
            try
            {
                if (FindName("lblBoasVindas") is TextBlock lblBoasVindas)
                {
                    var nomeUtilizador = modelPerfil?.PerfilAtual?.Nome ?? "Utilizador";
                    lblBoasVindas.Text = $"Bem-vindo {nomeUtilizador} ao sistema de gestão de avaliações";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar boas-vindas: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualizar estatísticas gerais (cards principais)
        /// </summary>
        private void AtualizarEstatisticasGerais()
        {
            try
            {
                // Total de Alunos
                var totalAlunos = modelAlunos?.ContarAlunos() ?? 0;
                if (FindName("lblTotalAlunos") is TextBlock lblTotalAlunos)
                {
                    lblTotalAlunos.Text = totalAlunos.ToString();
                }

                // Total de Grupos
                var totalGrupos = modelGrupos?.ContarGrupos() ?? 0;
                if (FindName("lblTotalGrupos") is TextBlock lblTotalGrupos)
                {
                    lblTotalGrupos.Text = totalGrupos.ToString();
                }

                // Tarefas ativas
                var tarefasAtivas = modelTarefas?.ObterTarefasAtivas()?.Count ?? 0;
                if (FindName("lblTarefasAtivas") is TextBlock lblTarefasAtivas)
                {
                    lblTarefasAtivas.Text = tarefasAtivas.ToString();
                }

                System.Diagnostics.Debug.WriteLine($"📊 Estatísticas: {totalAlunos} alunos, {totalGrupos} grupos, {tarefasAtivas} tarefas ativas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar estatísticas gerais: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualizar lista de atividades recentes
        /// </summary>
        private void AtualizarAtividadesRecentes()
        {
            try
            {
                if (FindName("lstAtividadesRecentes") is ItemsControl lstAtividadesRecentes)
                {
                    if (atividadesRecentes.Count > 0)
                    {
                        // Lista já está conectada via ItemsSource, não precisa fazer nada
                        lstAtividadesRecentes.Visibility = Visibility.Visible;

                        // Esconder mensagem de "sem atividades"
                        if (FindName("borderSemAtividades") is Border borderSemAtividades)
                        {
                            borderSemAtividades.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        // Esconder lista e mostrar mensagem
                        lstAtividadesRecentes.Visibility = Visibility.Collapsed;

                        if (FindName("borderSemAtividades") is Border borderSemAtividades)
                        {
                            borderSemAtividades.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar atividades recentes: {ex.Message}");
            }
        }

        /// <summary>
        /// Adicionar nova atividade recente
        /// </summary>
        private void AdicionarAtividadeRecente(string descricao, string icone)
        {
            try
            {
                var novaAtividade = new AtividadeRecente
                {
                    Icone = icone,
                    Descricao = descricao,
                    DataHora = DateTime.Now,
                    TempoRelativo = "Agora mesmo"
                };

                // Adicionar à ObservableCollection (auto-atualiza a UI)
                atividadesRecentes.Insert(0, novaAtividade);

                // Manter só as últimas 20 atividades para não sobrecarregar
                while (atividadesRecentes.Count > 20)
                {
                    atividadesRecentes.RemoveAt(atividadesRecentes.Count - 1);
                }

                // Atualizar visibilidade
                AtualizarAtividadesRecentes();

                System.Diagnostics.Debug.WriteLine($"✅ Atividade adicionada: {descricao} (Total: {atividadesRecentes.Count})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao adicionar atividade recente: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualizar tempos relativos de todas as atividades
        /// </summary>
        private void AtualizarTemposRelativos()
        {
            try
            {
                var agora = DateTime.Now;

                foreach (var atividade in atividadesRecentes)
                {
                    var diferenca = agora - atividade.DataHora;

                    atividade.TempoRelativo = diferenca.TotalMinutes switch
                    {
                        < 1 => "Agora mesmo",
                        < 60 => $"Há {(int)diferenca.TotalMinutes} minuto(s)",
                        < 1440 => $"Há {(int)diferenca.TotalHours} hora(s)",
                        _ => $"Há {(int)diferenca.TotalDays} dia(s)"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao atualizar tempos relativos: {ex.Message}");
            }
        }

        /// <summary>
        /// Método para forçar atualização manual (chamado externamente)
        /// </summary>
        public void ForcarAtualizacao()
        {
            AtualizarDashboard();
            AtualizarTemposRelativos();
            AdicionarAtividadeRecente("Dashboard atualizada manualmente", "🔄");
        }

        /// <summary>
        /// Cleanup ao destruir a view
        /// </summary>
        public void LimparEventos()
        {
            try
            {
                // Parar timer
                timerAtualizacao?.Stop();

                if (modelAlunos != null)
                {
                    modelAlunos.DadosAlterados -= OnDadosAlterados;
                    modelAlunos.AlunoModificado -= OnAlunoModificado;
                }

                if (modelGrupos != null)
                {
                    modelGrupos.DadosAlterados -= OnDadosAlterados;
                    modelGrupos.GrupoModificado -= OnGrupoModificado;
                }

                if (modelTarefas != null)
                {
                    modelTarefas.DadosAlterados -= OnDadosAlterados;
                    modelTarefas.TarefaModificada -= OnTarefaModificada;
                }

                if (modelNotas != null)
                {
                    modelNotas.DadosAlterados -= OnDadosAlterados;
                    modelNotas.NotaModificada -= OnNotaModificada;
                }
            }
            catch
            {
                // Ignorar erros na limpeza
            }
        }
    }

    #region Classes de apoio

    /// <summary>
    /// Classe para atividades recentes com notificação de propriedades
    /// </summary>
    public class AtividadeRecente : INotifyPropertyChanged
    {
        private string _tempoRelativo = string.Empty;

        public string Icone { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }

        public string TempoRelativo
        {
            get => _tempoRelativo;
            set
            {
                if (_tempoRelativo != value)
                {
                    _tempoRelativo = value;
                    OnPropertyChanged(nameof(TempoRelativo));
                }
            }
        }

        // Propriedade calculada para exibição
        public string DescricaoCompleta => $"{Icone} {Descricao}";
        public string DataFormatada => DataHora.ToString("HH:mm");

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}