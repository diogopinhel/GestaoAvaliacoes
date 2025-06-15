using System;
using System.IO;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using Microsoft.Win32;
using Projecto_Lab.Classes;

namespace Projecto_Lab.Models
{
    public class ModelPerfil
    {
        // Dados do perfil
        public Perfil PerfilAtual { get; private set; }

        // Eventos para comunicação com as Views
        public event MetodosSemParametros DadosCarregados;
        public event MetodosSemParametros DadosAlterados;
        public event MetodosComErro ErroOcorrido;
        public event MetodosComString PerfilModificado;

        // Caminho para foto padrão
        private readonly string caminhoFotoPadrao;
        private readonly string diretorioFotos;

        public ModelPerfil()
        {
            // Configurar diretórios
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            diretorioFotos = Path.Combine(appData, "Projecto_Lab", "Fotos");
            caminhoFotoPadrao = Path.Combine(diretorioFotos, "default_profile.png");

            // Criar diretório se não existir
            if (!Directory.Exists(diretorioFotos))
                Directory.CreateDirectory(diretorioFotos);

            // Carregar dados do Windows
            CarregarDadosDoWindows();
        }

        /// <summary>
        /// Carrega informações do usuário do Windows automaticamente
        /// </summary>
        private void CarregarDadosDoWindows()
        {
            try
            {
                // Obter informações básicas do Windows
                string nome = ObterNomeDoWindows();
                string email = ObterEmailDoWindows();
                string foto = ObterFotoDoWindows();

                // Criar perfil com dados do Windows
                PerfilAtual = new Perfil(nome, email, foto, "Windows User", "Utilizador");

                DadosCarregados?.Invoke();
            }
            catch (Exception ex)
            {
                ErroOcorrido?.Invoke($"Erro ao carregar dados do Windows: {ex.Message}");

                // Criar perfil padrão em caso de erro
                PerfilAtual = new Perfil("Utilizador", "usuario@exemplo.com", caminhoFotoPadrao, "Sistema", "Utilizador");
                DadosCarregados?.Invoke();
            }
        }

        /// <summary>
        /// Obtém o nome do usuário do Windows
        /// </summary>
        private string ObterNomeDoWindows()
        {
            try
            {
                // Tentar obter nome completo
                using (var context = new PrincipalContext(ContextType.Machine))
                {
                    var user = UserPrincipal.FindByIdentity(context, Environment.UserName);
                    if (user != null && !string.IsNullOrWhiteSpace(user.DisplayName))
                    {
                        return user.DisplayName;
                    }
                }
                return Environment.UserName ?? "Utilizador";
            }
            catch
            {
                return Environment.UserName ?? "Utilizador";
            }
        }

        /// <summary>
        /// Obtém o email do usuário (tentativa básica)
        /// </summary>
        private string ObterEmailDoWindows()
        {
            try
            {
                // Tentar obter email do Active Directory
                using (var context = new PrincipalContext(ContextType.Machine))
                {
                    var user = UserPrincipal.FindByIdentity(context, Environment.UserName);
                    if (user != null && !string.IsNullOrWhiteSpace(user.EmailAddress))
                    {
                        return user.EmailAddress;
                    }
                }

                // Fallback: usar nome@dominio.local
                return $"{Environment.UserName?.ToLower()}@{Environment.UserDomainName?.ToLower()}.local";
            }
            catch
            {
                return $"{Environment.UserName?.ToLower()}@exemplo.com";
            }
        }

        /// <summary>
        /// Obtém foto do perfil do Windows (ou usa padrão)
        /// </summary>
        private string ObterFotoDoWindows()
        {
            try
            {
                // Tentar várias localizações de foto de perfil do Windows
                var possiveisCaminhos = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "Microsoft", "Windows", "AccountPictures", "user.png"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Packages", "Microsoft.Windows.AccountPictures_cw5n1h2txyewy", "LocalState", "AccountPictures", "user.png"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                "AppData", "Roaming", "Microsoft", "Windows", "AccountPictures", "user-32.png")
                };

                foreach (var caminho in possiveisCaminhos)
                {
                    if (File.Exists(caminho))
                    {
                        // Copiar para nosso diretório local
                        var nomeArquivo = $"perfil_{Environment.UserName}.png";
                        var caminhoLocal = Path.Combine(diretorioFotos, nomeArquivo);

                        if (!File.Exists(caminhoLocal))
                        {
                            File.Copy(caminho, caminhoLocal, true);
                        }

                        return caminhoLocal;
                    }
                }

                // Se não encontrou, criar foto padrão
                return CriarFotoPadrao();
            }
            catch
            {
                return CriarFotoPadrao();
            }
        }

        /// <summary>
        /// Cria uma foto padrão simples se não existir
        /// </summary>
        private string CriarFotoPadrao()
        {
            try
            {
                if (!File.Exists(caminhoFotoPadrao))
                {
                    // Criar uma imagem padrão simples (você pode substituir por uma imagem real)
                    var bytes = Convert.FromBase64String(
                        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");
                    File.WriteAllBytes(caminhoFotoPadrao, bytes);
                }
                return caminhoFotoPadrao;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Atualiza o perfil do usuário
        /// </summary>
        public void AtualizarPerfil(string nome, string email, string instituicao = "", string cargo = "")
        {
            try
            {
                // Validações
                if (string.IsNullOrWhiteSpace(nome))
                    throw new OperacaoInvalidaException("Nome não pode estar vazio.");

                if (string.IsNullOrWhiteSpace(email))
                    throw new OperacaoInvalidaException("Email não pode estar vazio.");

                if (!ValidarEmail(email))
                    throw new OperacaoInvalidaException("Formato de email inválido.");

                // Atualizar perfil mantendo a foto atual
                PerfilAtual = new Perfil(nome, email, PerfilAtual.Foto, instituicao, cargo);

                // Notificar alteração
                DadosAlterados?.Invoke();
                PerfilModificado?.Invoke("Perfil atualizado com sucesso!");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao atualizar perfil: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza apenas a foto do perfil
        /// </summary>
        public void AtualizarFoto(string novoCaminhoFoto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(novoCaminhoFoto) || !File.Exists(novoCaminhoFoto))
                    throw new OperacaoInvalidaException("Arquivo de foto não encontrado.");

                // Copiar arquivo para nosso diretório
                var extensao = Path.GetExtension(novoCaminhoFoto);
                var nomeArquivo = $"perfil_{DateTime.Now:yyyyMMdd_HHmmss}{extensao}";
                var caminhoDestino = Path.Combine(diretorioFotos, nomeArquivo);

                File.Copy(novoCaminhoFoto, caminhoDestino, true);

                // Atualizar perfil com nova foto
                PerfilAtual = new Perfil(PerfilAtual.Nome, PerfilAtual.Email, caminhoDestino,
                                       PerfilAtual.Instituicao, PerfilAtual.Cargo);

                // Notificar alteração
                DadosAlterados?.Invoke();
                PerfilModificado?.Invoke("Foto de perfil atualizada com sucesso!");
            }
            catch (OperacaoInvalidaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new OperacaoInvalidaException($"Erro ao atualizar foto: {ex.Message}");
            }
        }

        /// <summary>
        /// Permite selecionar nova foto através de diálogo
        /// </summary>
        public void SelecionarNovaFoto()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Todos os arquivos (*.*)|*.*",
                    Title = "Selecionar foto de perfil",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    AtualizarFoto(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ErroOcorrido?.Invoke($"Erro ao selecionar foto: {ex.Message}");
            }
        }

        /// <summary>
        /// Valida formato de email
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

        /// <summary>
        /// Obtém informações resumidas do perfil
        /// </summary>
        public string ObterResumo()
        {
            if (PerfilAtual == null)
                return "Perfil não carregado";

            return $"Nome: {PerfilAtual.Nome}\nEmail: {PerfilAtual.Email}\nInstituição: {PerfilAtual.Instituicao}\nCargo: {PerfilAtual.Cargo}";
        }

        /// <summary>
        /// Obtém o caminho da foto atual
        /// </summary>
        public string ObterCaminhoFoto()
        {
            return PerfilAtual?.Foto ?? caminhoFotoPadrao;
        }

        /// <summary>
        /// Verifica se tem foto personalizada
        /// </summary>
        public bool TemFotoPersonalizada()
        {
            var caminhoAtual = ObterCaminhoFoto();
            return !string.IsNullOrEmpty(caminhoAtual) && caminhoAtual != caminhoFotoPadrao && File.Exists(caminhoAtual);
        }
    }
}