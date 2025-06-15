using System;
using System.Collections.Generic;
using System.Linq;

namespace Projecto_Lab.Classes
{
    public class Grupo
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public List<string> NumerosAlunos { get; set; }

        // Construtor com parâmetros
        public Grupo(string id, string nome, string descricao = "")
        {
            Id = id ?? string.Empty;
            Nome = nome ?? string.Empty;
            Descricao = descricao ?? string.Empty;
            DataCriacao = DateTime.Now;
            NumerosAlunos = new List<string>();
        }

        // Construtor vazio
        public Grupo()
        {
            Id = string.Empty;
            Nome = string.Empty;
            Descricao = string.Empty;
            DataCriacao = DateTime.Now;
            NumerosAlunos = new List<string>();
        }

        // Propriedades calculadas
        public int NumeroAlunos => NumerosAlunos?.Count ?? 0;

        public string AlunosNomes
        {
            get
            {
                if (NumerosAlunos == null || NumerosAlunos.Count == 0)
                    return string.Empty;

                // Buscar nomes dos alunos 
                return string.Join(", ", NumerosAlunos);
            }
        }

        // Métodos para gerenciar alunos
        public void AdicionarAluno(string numeroAluno)
        {
            if (string.IsNullOrWhiteSpace(numeroAluno))
                return;

            if (!NumerosAlunos.Contains(numeroAluno))
            {
                NumerosAlunos.Add(numeroAluno);
            }
        }

        public void RemoverAluno(string numeroAluno)
        {
            if (string.IsNullOrWhiteSpace(numeroAluno))
                return;

            NumerosAlunos.Remove(numeroAluno);
        }

        public bool ContemAluno(string numeroAluno)
        {
            return NumerosAlunos.Contains(numeroAluno);
        }

        public void LimparAlunos()
        {
            NumerosAlunos.Clear();
        }

        // Override ToString para facilitar debug
        public override string ToString()
        {
            return $"{Id} - {Nome} ({NumeroAlunos} alunos)";
        }

        // Override Equals para comparação
        public override bool Equals(object obj)
        {
            if (obj is Grupo outroGrupo)
            {
                return Id == outroGrupo.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}