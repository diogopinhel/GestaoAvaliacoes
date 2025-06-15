using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projecto_Lab.Classes
{
    public class Perfil
    {
        public string Nome { get; private set; }
        public string Email { get; private set; }
        public string Foto { get; private set; }
        public string Instituicao { get; private set; }
        public string Cargo { get; private set; }
        public DateTime DataCriacao { get; private set; }

        public Perfil(string nome, string email, string foto = "", string instituicao = "", string cargo = "")
        {
            Nome = nome;
            Email = email;
            Foto = foto;
            Instituicao = instituicao;
            Cargo = cargo;
            DataCriacao = DateTime.Now;
        }

        // Método para atualizar dados do perfil (só deve ser chamado pelo Model)
        internal void AtualizarPerfil(string nome, string email, string foto = "", string instituicao = "", string cargo = "")
        {
            Nome = nome;
            Email = email;
            Foto = foto;
            Instituicao = instituicao;
            Cargo = cargo;
        }

        // Método para atualizar apenas a foto (só deve ser chamado pelo Model)
        internal void AtualizarFoto(string novaFoto)
        {
            Foto = novaFoto;
        }
    }
}