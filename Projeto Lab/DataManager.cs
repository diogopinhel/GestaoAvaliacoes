using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using Projecto_Lab.Classes;
using Projecto_Lab.Models;

namespace Projecto_Lab.Persistence
{
    /// <summary>
    /// DataManager Corrigido - Dispara eventos ao carregar dados
    /// </summary>
    public class DataManager
    {
        private readonly string dataDirectory;
        private readonly string alunosFilePath;
        private readonly string gruposFilePath;
        private readonly string tarefasFilePath;
        private readonly string notasFilePath;
        private readonly string perfilFilePath;

        private readonly ModelAlunos modelAlunos;
        private readonly ModelGrupos modelGrupos;
        private readonly ModelTarefas modelTarefas;
        private readonly ModelNotas modelNotas;
        private readonly ModelPerfil modelPerfil;

        private bool isLoading = false;

        public DataManager(ModelAlunos alunos, ModelGrupos grupos, ModelTarefas tarefas,
                          ModelNotas notas, ModelPerfil perfil)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dataDirectory = Path.Combine(documentsPath, "Projecto_Lab_Data");

            if (!Directory.Exists(dataDirectory))
                Directory.CreateDirectory(dataDirectory);

            alunosFilePath = Path.Combine(dataDirectory, "alunos.xml");
            gruposFilePath = Path.Combine(dataDirectory, "grupos.xml");
            tarefasFilePath = Path.Combine(dataDirectory, "tarefas.xml");
            notasFilePath = Path.Combine(dataDirectory, "notas.xml");
            perfilFilePath = Path.Combine(dataDirectory, "perfil.xml");

            modelAlunos = alunos;
            modelGrupos = grupos;
            modelTarefas = tarefas;
            modelNotas = notas;
            modelPerfil = perfil;

            SubscreverEventos();
        }

        #region Subscrição de Eventos

        private void SubscreverEventos()
        {
            if (modelAlunos != null)
            {
                modelAlunos.DadosAlterados += OnAlunosDadosAlterados;
                modelAlunos.AlunoModificado += OnAlunoModificado;
            }

            if (modelGrupos != null)
            {
                modelGrupos.DadosAlterados += OnGruposDadosAlterados;
                modelGrupos.GrupoModificado += OnGrupoModificado;
            }

            if (modelTarefas != null)
            {
                modelTarefas.DadosAlterados += OnTarefasDadosAlterados;
                modelTarefas.TarefaModificada += OnTarefaModificada;
            }

            if (modelNotas != null)
            {
                modelNotas.DadosAlterados += OnNotasDadosAlterados;
                modelNotas.NotaModificada += OnNotaModificada;
            }

            if (modelPerfil != null)
            {
                modelPerfil.DadosAlterados += OnPerfilDadosAlterados;
                modelPerfil.PerfilModificado += OnPerfilModificado;
            }
        }

        // Event handlers
        private void OnAlunosDadosAlterados() { if (!isLoading) GuardarAlunos(); }
        private void OnAlunoModificado(string numero, string operacao) { if (!isLoading) GuardarAlunos(); }
        private void OnGruposDadosAlterados() { if (!isLoading) GuardarGrupos(); }
        private void OnGrupoModificado(string id, string operacao) { if (!isLoading) GuardarGrupos(); }
        private void OnTarefasDadosAlterados() { if (!isLoading) GuardarTarefas(); }
        private void OnTarefaModificada(int id, string operacao) { if (!isLoading) GuardarTarefas(); }
        private void OnNotasDadosAlterados() { if (!isLoading) GuardarNotas(); }
        private void OnNotaModificada(int id, string operacao) { if (!isLoading) GuardarNotas(); }
        private void OnPerfilDadosAlterados() { if (!isLoading) GuardarPerfil(); }
        private void OnPerfilModificado(string info) { if (!isLoading) GuardarPerfil(); }

        #endregion

        #region Métodos Principais

        public void CarregarTodosDados()
        {
            // Verificar se existem dados para carregar
            if (!ExistemDadosGuardados())
            {
                System.Diagnostics.Debug.WriteLine("📂 Nenhum dado guardado encontrado. A usar dados de exemplo.");
                return;
            }

            isLoading = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("📂 Carregando dados guardados...");

                // Carregar dados
                CarregarPerfil();
                CarregarAlunos();
                CarregarTarefas();
                CarregarGrupos();
                CarregarNotas();

                System.Diagnostics.Debug.WriteLine("✅ Dados carregados! Notificando Views...");

                // ✅ CORRIGIDO: Disparar eventos APÓS carregar
                DispararEventosCarregamento();

                System.Diagnostics.Debug.WriteLine("✅ Views notificadas!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar dados: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Dispara eventos para notificar as Views
        /// </summary>
        private void DispararEventosCarregamento()
        {
            try
            {
                // Usar reflexão para disparar eventos DadosCarregados
                DispararEventoDadosCarregados(modelAlunos);
                DispararEventoDadosCarregados(modelGrupos);
                DispararEventoDadosCarregados(modelTarefas);
                DispararEventoDadosCarregados(modelNotas);
                DispararEventoDadosCarregados(modelPerfil);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao disparar eventos: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispara evento DadosCarregados usando reflexão
        /// </summary>
        private void DispararEventoDadosCarregados(object model)
        {
            if (model == null) return;

            try
            {
                var eventInfo = model.GetType().GetEvent("DadosCarregados");
                var fieldInfo = model.GetType().GetField("DadosCarregados",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (fieldInfo != null)
                {
                    var eventDelegate = fieldInfo.GetValue(model) as MulticastDelegate;
                    eventDelegate?.DynamicInvoke();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao disparar evento para {model.GetType().Name}: {ex.Message}");
            }
        }

        private bool ExistemDadosGuardados()
        {
            return File.Exists(alunosFilePath) ||
                   File.Exists(gruposFilePath) ||
                   File.Exists(tarefasFilePath) ||
                   File.Exists(notasFilePath) ||
                   File.Exists(perfilFilePath);
        }

        public void GuardarTodosDados()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("💾 Guardando todos os dados...");

                GuardarPerfil();
                GuardarAlunos();
                GuardarGrupos();
                GuardarTarefas();
                GuardarNotas();

                System.Diagnostics.Debug.WriteLine("✅ Todos os dados guardados!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao guardar dados: {ex.Message}");
            }
        }

        #endregion

        #region Persistência Individual

        private void CarregarAlunos()
        {
            try
            {
                if (!File.Exists(alunosFilePath)) return;

                var alunosData = DeserializarXML<AlunosData>(alunosFilePath);
                if (alunosData?.Alunos != null && alunosData.Alunos.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"📂 Carregando {alunosData.Alunos.Count} alunos...");

                    modelAlunos.Lista.Clear();
                    foreach (var alunoData in alunosData.Alunos)
                    {
                        var aluno = new Aluno(alunoData.Numero, alunoData.Nome, alunoData.Email);
                        modelAlunos.Lista.Add(aluno.Numero, aluno);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ {alunosData.Alunos.Count} alunos carregados!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar alunos: {ex.Message}");
            }
        }

        private void GuardarAlunos()
        {
            try
            {
                var alunosData = new AlunosData
                {
                    Alunos = modelAlunos.Lista.Values.Select(a => new AlunoData
                    {
                        Numero = a.Numero,
                        Nome = a.Nome,
                        Email = a.Email
                    }).ToList()
                };
                SerializarXML(alunosData, alunosFilePath);
                System.Diagnostics.Debug.WriteLine($"💾 {alunosData.Alunos.Count} alunos guardados!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao guardar alunos: {ex.Message}");
            }
        }

        private void CarregarGrupos()
        {
            try
            {
                if (!File.Exists(gruposFilePath)) return;

                var gruposData = DeserializarXML<GruposData>(gruposFilePath);
                if (gruposData?.Grupos != null && gruposData.Grupos.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"📂 Carregando {gruposData.Grupos.Count} grupos...");

                    modelGrupos.Lista.Clear();
                    foreach (var grupoData in gruposData.Grupos)
                    {
                        var grupo = new Grupo(grupoData.Id, grupoData.Nome, grupoData.Descricao);

                        if (grupoData.NumerosAlunos != null)
                        {
                            foreach (var numeroAluno in grupoData.NumerosAlunos)
                            {
                                grupo.AdicionarAluno(numeroAluno);
                            }
                        }
                        modelGrupos.Lista.Add(grupo.Id, grupo);
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ {gruposData.Grupos.Count} grupos carregados!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar grupos: {ex.Message}");
            }
        }

        private void GuardarGrupos()
        {
            try
            {
                var gruposData = new GruposData
                {
                    Grupos = modelGrupos.Lista.Values.Select(g => new GrupoData
                    {
                        Id = g.Id,
                        Nome = g.Nome,
                        Descricao = g.Descricao,
                        NumerosAlunos = g.NumerosAlunos?.ToList() ?? new List<string>()
                    }).ToList()
                };
                SerializarXML(gruposData, gruposFilePath);
                System.Diagnostics.Debug.WriteLine($"💾 {gruposData.Grupos.Count} grupos guardados!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao guardar grupos: {ex.Message}");
            }
        }

        private void CarregarTarefas()
        {
            try
            {
                if (!File.Exists(tarefasFilePath)) return;

                var tarefasData = DeserializarXML<TarefasData>(tarefasFilePath);
                if (tarefasData?.Tarefas != null && tarefasData.Tarefas.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"📂 Carregando {tarefasData.Tarefas.Count} tarefas...");

                    modelTarefas.Lista.Clear();
                    int maxId = 0;

                    foreach (var tarefaData in tarefasData.Tarefas)
                    {
                        var tarefa = new Tarefa(tarefaData.Id, tarefaData.Titulo, tarefaData.Descricao,
                                              tarefaData.DataHoraInicio, tarefaData.DataHoraFim, tarefaData.Peso);
                        modelTarefas.Lista.Add(tarefa.Id, tarefa);
                        if (tarefa.Id > maxId) maxId = tarefa.Id;
                    }

                    // Atualizar próximo ID
                    var proximoIdField = typeof(ModelTarefas).GetField("proximoId",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    proximoIdField?.SetValue(modelTarefas, maxId + 1);

                    System.Diagnostics.Debug.WriteLine($"✅ {tarefasData.Tarefas.Count} tarefas carregadas!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar tarefas: {ex.Message}");
            }
        }

        private void GuardarTarefas()
        {
            try
            {
                var tarefasData = new TarefasData
                {
                    Tarefas = modelTarefas.Lista.Values.Select(t => new TarefaData
                    {
                        Id = t.Id,
                        Titulo = t.Titulo,
                        Descricao = t.Descricao,
                        DataHoraInicio = t.DataHoraInicio,
                        DataHoraFim = t.DataHoraFim,
                        Peso = t.Peso
                    }).ToList()
                };
                SerializarXML(tarefasData, tarefasFilePath);
                System.Diagnostics.Debug.WriteLine($"💾 {tarefasData.Tarefas.Count} tarefas guardadas!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao guardar tarefas: {ex.Message}");
            }
        }

        private void CarregarNotas()
        {
            try
            {
                if (!File.Exists(notasFilePath)) return;

                var notasData = DeserializarXML<NotasData>(notasFilePath);
                if (notasData?.Notas != null && notasData.Notas.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"📂 Carregando {notasData.Notas.Count} notas...");

                    modelNotas.Lista.Clear();
                    int maxId = 0;

                    foreach (var notaData in notasData.Notas)
                    {
                        Nota nota;
                        if (notaData.ENotaGrupo)
                        {
                            nota = new Nota(notaData.Id, notaData.TarefaId, notaData.GrupoId, notaData.ValorNota);
                        }
                        else
                        {
                            nota = new Nota(notaData.Id, notaData.TarefaId, notaData.GrupoId,
                                          notaData.NumeroAluno, notaData.ValorNota);
                        }
                        modelNotas.Lista.Add(nota.Id, nota);
                        if (nota.Id > maxId) maxId = nota.Id;
                    }

                    // Atualizar próximo ID
                    var proximoIdField = typeof(ModelNotas).GetField("proximoId",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    proximoIdField?.SetValue(modelNotas, maxId + 1);

                    System.Diagnostics.Debug.WriteLine($"✅ {notasData.Notas.Count} notas carregadas!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar notas: {ex.Message}");
            }
        }

        private void GuardarNotas()
        {
            try
            {
                var notasData = new NotasData
                {
                    Notas = modelNotas.Lista.Values.Select(n => new NotaData
                    {
                        Id = n.Id,
                        TarefaId = n.TarefaId,
                        GrupoId = n.GrupoId,
                        NumeroAluno = n.NumeroAluno,
                        ValorNota = n.ValorNota,
                        ENotaGrupo = n.ENotaGrupo
                    }).ToList()
                };
                SerializarXML(notasData, notasFilePath);
                System.Diagnostics.Debug.WriteLine($"💾 {notasData.Notas.Count} notas guardadas!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao guardar notas: {ex.Message}");
            }
        }

        private void CarregarPerfil()
        {
            try
            {
                if (!File.Exists(perfilFilePath)) return;

                var perfilData = DeserializarXML<PerfilData>(perfilFilePath);
                if (perfilData != null)
                {
                    System.Diagnostics.Debug.WriteLine("📂 Carregando perfil...");

                    var perfil = new Perfil(perfilData.Nome, perfilData.Email, perfilData.Foto,
                                          perfilData.Instituicao, perfilData.Cargo);

                    var perfilField = typeof(ModelPerfil).GetField("<PerfilAtual>k__BackingField",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    perfilField?.SetValue(modelPerfil, perfil);

                    System.Diagnostics.Debug.WriteLine("✅ Perfil carregado!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar perfil: {ex.Message}");
            }
        }

        private void GuardarPerfil()
        {
            try
            {
                if (modelPerfil?.PerfilAtual == null) return;

                var perfilData = new PerfilData
                {
                    Nome = modelPerfil.PerfilAtual.Nome,
                    Email = modelPerfil.PerfilAtual.Email,
                    Foto = modelPerfil.PerfilAtual.Foto,
                    Instituicao = modelPerfil.PerfilAtual.Instituicao,
                    Cargo = modelPerfil.PerfilAtual.Cargo
                };
                SerializarXML(perfilData, perfilFilePath);
                System.Diagnostics.Debug.WriteLine("💾 Perfil guardado!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao guardar perfil: {ex.Message}");
            }
        }

        #endregion

        #region Métodos Auxiliares

        private void SerializarXML<T>(T obj, string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(writer, obj);
            }
        }

        private T DeserializarXML<T>(string filePath) where T : class
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new FileStream(filePath, FileMode.Open))
            {
                return serializer.Deserialize(reader) as T;
            }
        }

        #endregion
    }

    #region Classes de Dados para XML (Inalteradas)

    [Serializable]
    public class AlunosData
    {
        public List<AlunoData> Alunos { get; set; } = new List<AlunoData>();
    }

    [Serializable]
    public class AlunoData
    {
        public string Numero { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }

    [Serializable]
    public class GruposData
    {
        public List<GrupoData> Grupos { get; set; } = new List<GrupoData>();
    }

    [Serializable]
    public class GrupoData
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public List<string> NumerosAlunos { get; set; } = new List<string>();
    }

    [Serializable]
    public class TarefasData
    {
        public List<TarefaData> Tarefas { get; set; } = new List<TarefaData>();
    }

    [Serializable]
    public class TarefaData
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string DataHoraInicio { get; set; }
        public string DataHoraFim { get; set; }
        public string Peso { get; set; }
    }

    [Serializable]
    public class NotasData
    {
        public List<NotaData> Notas { get; set; } = new List<NotaData>();
    }

    [Serializable]
    public class NotaData
    {
        public int Id { get; set; }
        public int TarefaId { get; set; }
        public string GrupoId { get; set; }
        public string NumeroAluno { get; set; }
        public decimal ValorNota { get; set; }
        public bool ENotaGrupo { get; set; }
    }

    [Serializable]
    public class PerfilData
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Foto { get; set; }
        public string Instituicao { get; set; }
        public string Cargo { get; set; }
    }

    #endregion
}