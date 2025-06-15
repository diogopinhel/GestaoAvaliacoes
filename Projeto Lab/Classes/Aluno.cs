using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projecto_Lab.Classes
{
    public class Aluno
    {
        public string Numero { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }

        public Aluno(string numero, string nome, string email)
        {
            Numero = numero;
            Nome = nome;
            Email = email;
        }

        // Construtor vazio para casos específicos (se necessário)
        public Aluno()
        {
            Numero = string.Empty;
            Nome = string.Empty;
            Email = string.Empty;
        }

        // Override ToString para facilitar debug e exibição
        public override string ToString()
        {
            return $"{Numero} - {Nome} - {Email}";
        }

        // Override Equals para comparação de alunos (útil para evitar duplicados)
        public override bool Equals(object obj)
        {
            if (obj is Aluno outroAluno)
            {
                return Numero == outroAluno.Numero;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Numero?.GetHashCode() ?? 0;
        }
    }
}