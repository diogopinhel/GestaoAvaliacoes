using System.Windows;
using Projecto_Lab.Views;
using Projecto_Lab.Models;
using Projecto_Lab.Persistence;

namespace Projecto_Lab
{
    public partial class App : Application
    {
        // Models de Negócio
        public ModelAlunos Model_Alunos { get; private set; }
        public ModelGrupos Model_Grupos { get; private set; }
        public ModelTarefas Model_Tarefas { get; private set; }
        public ModelNotas Model_Notas { get; private set; }
        public ModelPautas Model_Pautas { get; private set; }
        public ModelPerfil Model_Perfil { get; private set; }
        public DataManager DataManager { get; private set; }

        // Views
        public MainWindow MainView { get; private set; }
        public GestaoAlunosView AlunosView { get; private set; }
        public GestaoTarefasView TarefasView { get; private set; }
        public GestaoGruposView GruposView { get; private set; }
        public GestaoNotasView NotasView { get; private set; }
        public GestaoPautasView PautasView { get; private set; }
        public GestaoPerfilView PerfilView { get; private set; }

        public App()
        {
            // Instanciar Models
            Model_Perfil = new ModelPerfil();
            Model_Alunos = new ModelAlunos();
            Model_Grupos = new ModelGrupos(Model_Alunos);
            Model_Tarefas = new ModelTarefas();
            Model_Notas = new ModelNotas(Model_Alunos, Model_Grupos, Model_Tarefas);
            Model_Pautas = new ModelPautas(Model_Alunos, Model_Grupos, Model_Tarefas, Model_Notas);

            // Inicializar persistência
            DataManager = new DataManager(Model_Alunos, Model_Grupos, Model_Tarefas, Model_Notas, Model_Perfil);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Carregar dados DEPOIS de criar as Views
            // Assim as Views subscrevem aos eventos primeiro

            // 1. Criar Views primeiro
            AlunosView = new GestaoAlunosView();
            TarefasView = new GestaoTarefasView();
            GruposView = new GestaoGruposView();
            NotasView = new GestaoNotasView();
            PautasView = new GestaoPautasView();
            PerfilView = new GestaoPerfilView();

            // 2. Carregar dados - as Views vão ser notificadas automaticamente
            DataManager.CarregarTodosDados();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Guardar dados antes de fechar
            DataManager?.GuardarTodosDados();

            base.OnExit(e);
        }
    }
}