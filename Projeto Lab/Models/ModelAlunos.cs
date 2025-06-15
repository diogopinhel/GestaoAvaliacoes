using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Models
{
    public class ModelAlunos
    {
        // Estrutura de dados principal - Dictionary para acesso rápido por número
        public Dictionary<string, Aluno> Lista { get; private set; }

        // Eventos para comunicação com as Views
        public event MetodosSemParametros DadosCarregados;
        public event MetodosSemParametros DadosAlterados;
        public event MetodosSemParametros ImportacaoTerminada;
        public event MetodosSemParametros ExportacaoTerminada;
        public event MetodosComErro ErroOcorrido;
        public event AlunoAlterado AlunoModificado;

        public ModelAlunos()
        {
            Lista = new Dictionary<string, Aluno>();
        }

        public void AdicionarAluno(string numero, string nome, string email)
        {
            try
            {
                // Validações
                if (string.IsNullOrWhiteSpace(numero))
                    throw new OperacaoInvalidaException("Número do aluno não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(nome))
                    throw new OperacaoInvalidaException("Nome do aluno não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(email))
                    throw new OperacaoInvalidaException("Email do aluno não pode estar vazio.");

                if (Lista.ContainsKey(numero))
                    throw new OperacaoInvalidaException($"Já existe um aluno com o número {numero}.");

                // Validar formato do email
                if (!ValidarEmail(email))
                    throw new OperacaoInvalidaException("Formato de email inválido.");

                // Criar e adicionar o aluno
                var novoAluno = new Aluno(numero, nome, email);
                Lista.Add(numero, novoAluno);

                // Notificar alteração
                DadosAlterados?.Invoke();
                AlunoModificado?.Invoke(numero, "Adicionado");
            }
            catch (OperacaoInvalidaException)
            {
                throw; // Re-throw para manter a exceção específica
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao adicionar aluno: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza um aluno existente
        /// </summary>
        public void AtualizarAluno(string numeroOriginal, string novoNumero, string nome, string email)
        {
            try
            {
                if (!Lista.ContainsKey(numeroOriginal))
                    throw new OperacaoInvalidaException($"Não existe um aluno com o número {numeroOriginal}.");

                // Validações
                if (string.IsNullOrWhiteSpace(novoNumero))
                    throw new OperacaoInvalidaException("Número do aluno não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(nome))
                    throw new OperacaoInvalidaException("Nome do aluno não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(email))
                    throw new OperacaoInvalidaException("Email do aluno não pode estar vazio.");

                // Se o número mudou, verificar se o novo número já existe
                if (numeroOriginal != novoNumero && Lista.ContainsKey(novoNumero))
                    throw new OperacaoInvalidaException($"Já existe um aluno com o número {novoNumero}.");

                // Validar formato do email
                if (!ValidarEmail(email))
                    throw new OperacaoInvalidaException("Formato de email inválido.");

                // Remover o aluno antigo
                Lista.Remove(numeroOriginal);

                // Criar aluno atualizado
                var alunoAtualizado = new Aluno(novoNumero, nome, email);
                Lista.Add(novoNumero, alunoAtualizado);

                // Notificar alteração
                DadosAlterados?.Invoke();
                AlunoModificado?.Invoke(novoNumero, "Atualizado");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao atualizar aluno: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove um aluno da lista
        /// </summary>
        public void RemoverAluno(string numero)
        {
            try
            {
                if (!Lista.ContainsKey(numero))
                    throw new OperacaoInvalidaException($"Não existe um aluno com o número {numero}.");

                Lista.Remove(numero);

                // Notificar alteração
                DadosAlterados?.Invoke();
                AlunoModificado?.Invoke(numero, "Removido");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao remover aluno: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém um aluno pelo número
        /// </summary>
        public Aluno ObterAluno(string numero)
        {
            Lista.TryGetValue(numero, out Aluno aluno);
            return aluno;
        }

        /// <summary>
        /// Obtém todos os alunos como lista
        /// </summary>
        public List<Aluno> ObterTodosAlunos()
        {
            return Lista.Values.OrderBy(a => a.Numero).ToList();
        }

        /// <summary>
        /// Pesquisa alunos por número ou nome
        /// </summary>
        public List<Aluno> PesquisarAlunos(string termoPesquisa)
        {
            if (string.IsNullOrWhiteSpace(termoPesquisa))
                return ObterTodosAlunos();

            var termo = termoPesquisa.ToLower();
            return Lista.Values.Where(a =>
                a.Numero.ToLower().Contains(termo) ||
                a.Nome.ToLower().Contains(termo) ||
                a.Email.ToLower().Contains(termo)
            ).OrderBy(a => a.Numero).ToList();
        }

        /// <summary>
        /// Importa alunos de um arquivo CSV com detecção automática de encoding
        /// </summary>
        public void ImportarAlunosCSV()
        {
            try
            {
                // Configurar e mostrar o diálogo de arquivo
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    Title = "Selecionar ficheiro CSV de alunos"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    ImportarAlunosDeArquivo(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ErroOcorrido?.Invoke($"Erro ao importar CSV: {ex.Message}");
            }
        }

        /// <summary>
        /// Importa alunos de um arquivo específico
        /// </summary>
        private void ImportarAlunosDeArquivo(string caminhoArquivo)
        {
            try
            {
                // Ler todos os bytes do arquivo
                byte[] fileBytes = File.ReadAllBytes(caminhoArquivo);

                // Detectar encoding e converter para string
                string content = DetectEncodingAndGetString(fileBytes);

                // Dividir conteúdo em linhas
                string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                int importedCount = 0;
                int errorCount = 0;
                List<Aluno> newStudents = new List<Aluno>();

                foreach (string line in lines)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Dividir a linha CSV por vírgula
                        string[] parts = line.Split(',');

                        // Precisamos de pelo menos 3 partes (nome, número, email)
                        if (parts.Length < 3)
                        {
                            errorCount++;
                            continue;
                        }

                        // Obter dados das partes
                        string name = parts[0].Trim().Replace("\"", "");
                        string numberStr = parts[1].Trim().Replace("\"", "");
                        string email = parts[2].Trim().Replace("\"", "");

                        // Validar número
                        if (string.IsNullOrWhiteSpace(numberStr))
                        {
                            errorCount++;
                            continue;
                        }

                        // Validar se já existe ou se é duplicado na importação
                        if (Lista.ContainsKey(numberStr) || newStudents.Any(a => a.Numero == numberStr))
                        {
                            errorCount++;
                            continue;
                        }

                        // Validar email básico
                        if (!ValidarEmail(email))
                        {
                            errorCount++;
                            continue;
                        }

                        // Criar novo aluno
                        var student = new Aluno(numberStr, name, email);
                        newStudents.Add(student);
                        importedCount++;
                    }
                    catch (Exception lineEx)
                    {
                        Console.WriteLine($"Erro com linha: {line}, Mensagem: {lineEx.Message}");
                        errorCount++;
                    }
                }

                // Adicionar todos os alunos analisados com sucesso
                foreach (var student in newStudents)
                {
                    Lista.Add(student.Numero, student);
                }

                // Notificar alterações
                if (importedCount > 0)
                {
                    DadosAlterados?.Invoke();
                    ImportacaoTerminada?.Invoke();
                }

                // Mostrar resultados da importação via evento
                string mensagem = $"Importação concluída.\n{importedCount} alunos importados com sucesso.\n{errorCount} linhas com erros.";
                ErroOcorrido?.Invoke(mensagem); // Usando este evento para mostrar info também
            }
            catch (Exception ex)
            {
                ErroOcorrido?.Invoke($"Erro ao importar o ficheiro: {ex.Message}");
            }
        }

        /// <summary>
        /// Detecta o encoding do arquivo e converte para string
        /// </summary>
        private string DetectEncodingAndGetString(byte[] fileBytes)
        {
            // Verificar BOM (Byte Order Mark) para detectar encoding
            if (fileBytes.Length >= 3 &&
                fileBytes[0] == 0xEF &&
                fileBytes[1] == 0xBB &&
                fileBytes[2] == 0xBF)
            {
                // UTF-8 com BOM
                return Encoding.UTF8.GetString(fileBytes, 3, fileBytes.Length - 3);
            }
            else if (fileBytes.Length >= 2 &&
                     fileBytes[0] == 0xFE &&
                     fileBytes[1] == 0xFF)
            {
                // UTF-16 BE (Big Endian)
                return Encoding.BigEndianUnicode.GetString(fileBytes, 2, fileBytes.Length - 2);
            }
            else if (fileBytes.Length >= 2 &&
                     fileBytes[0] == 0xFF &&
                     fileBytes[1] == 0xFE)
            {
                // UTF-16 LE (Little Endian)
                return Encoding.Unicode.GetString(fileBytes, 2, fileBytes.Length - 2);
            }

            // Tentar diferentes encodings por ordem de prioridade

            // Primeiro tentar ISO-8859-1 (Latin1) que funciona bem com Português
            try
            {
                return Encoding.GetEncoding(28591).GetString(fileBytes);
            }
            catch { }

            // Tentar UTF-8
            try
            {
                return Encoding.UTF8.GetString(fileBytes);
            }
            catch { }

            // Por último, usar encoding padrão do sistema
            return Encoding.Default.GetString(fileBytes);
        }

        /// <summary>
        /// Limpa todos os dados
        /// </summary>
        public void LimparDados()
        {
            Lista.Clear();
            DadosAlterados?.Invoke();
        }

        /// <summary>
        /// Conta total de alunos
        /// </summary>
        public int ContarAlunos()
        {
            return Lista.Count;
        }

        /// <summary>
        /// Valida formato básico de email
        /// </summary>
        private bool ValidarEmail(string email)
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
    }
}