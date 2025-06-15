using System;

namespace Projecto_Lab.Classes
{
    public class Nota
    {
        public int Id { get; private set; }
        public int TarefaId { get; private set; }
        public string GrupoId { get; private set; }
        public string NumeroAluno { get; private set; } // Null se for nota de grupo
        public decimal ValorNota { get; private set; }
        public DateTime DataAtribuicao { get; private set; }
        public bool ENotaGrupo { get; private set; }

        // Construtor para nota de grupo
        public Nota(int id, int tarefaId, string grupoId, decimal valorNota)
        {
            Id = id;
            TarefaId = tarefaId;
            GrupoId = grupoId ?? string.Empty;
            NumeroAluno = null; // Null indica que é nota de grupo
            ValorNota = valorNota;
            DataAtribuicao = DateTime.Now;
            ENotaGrupo = true;
        }

        // Construtor para nota individual
        public Nota(int id, int tarefaId, string grupoId, string numeroAluno, decimal valorNota)
        {
            Id = id;
            TarefaId = tarefaId;
            GrupoId = grupoId ?? string.Empty;
            NumeroAluno = numeroAluno ?? string.Empty;
            ValorNota = valorNota;
            DataAtribuicao = DateTime.Now;
            ENotaGrupo = false;
        }

        // Construtor vazio
        public Nota()
        {
            Id = 0;
            TarefaId = 0;
            GrupoId = string.Empty;
            NumeroAluno = null;
            ValorNota = 0;
            DataAtribuicao = DateTime.Now;
            ENotaGrupo = true;
        }

        // Propriedades calculadas
        public string TipoNota => ENotaGrupo ? "Grupo" : "Individual";

        public string DescricaoCompleta
        {
            get
            {
                if (ENotaGrupo)
                    return $"Grupo {GrupoId} - Nota: {ValorNota:F1}";
                else
                    return $"Aluno {NumeroAluno} (Grupo {GrupoId}) - Nota: {ValorNota:F1}";
            }
        }

        // Override ToString para facilitar debug
        public override string ToString()
        {
            return DescricaoCompleta;
        }

        // Override Equals para comparação
        public override bool Equals(object obj)
        {
            if (obj is Nota outraNota)
            {
                return Id == outraNota.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}