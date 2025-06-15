using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projecto_Lab.Classes
{
    public class Tarefa
    {
        public int Id { get; private set; }
        public string Titulo { get; private set; }
        public string Descricao { get; private set; }
        public string DataHoraInicio { get; private set; }
        public string DataHoraFim { get; private set; }
        public string Peso { get; private set; }
        public DateTime DataCriacao { get; private set; }

        // Construtor principal
        public Tarefa(int id, string titulo, string descricao, string dataHoraInicio, string dataHoraFim, string peso)
        {
            Id = id;
            Titulo = titulo ?? string.Empty;
            Descricao = descricao ?? string.Empty;
            DataHoraInicio = dataHoraInicio ?? string.Empty;
            DataHoraFim = dataHoraFim ?? string.Empty;
            Peso = peso ?? string.Empty;
            DataCriacao = DateTime.Now;
        }

        // Construtor vazio para casos específicos (se necessário)
        public Tarefa()
        {
            Id = 0;
            Titulo = string.Empty;
            Descricao = string.Empty;
            DataHoraInicio = string.Empty;
            DataHoraFim = string.Empty;
            Peso = string.Empty;
            DataCriacao = DateTime.Now;
        }

        // Propriedades calculadas úteis
        public DateTime? DataInicioDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(DataHoraInicio))
                    return null;

                try
                {
                    // Tentar converter formato dd/MM/yyyy HH:mm
                    if (DateTime.TryParseExact(DataHoraInicio, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime resultado))
                        return resultado;

                    // Tentar converter formato dd/MM/yyyy
                    if (DateTime.TryParseExact(DataHoraInicio, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out resultado))
                        return resultado;

                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public DateTime? DataFimDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(DataHoraFim))
                    return null;

                try
                {
                    // Tentar converter formato dd/MM/yyyy HH:mm
                    if (DateTime.TryParseExact(DataHoraFim, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime resultado))
                        return resultado;

                    // Tentar converter formato dd/MM/yyyy
                    if (DateTime.TryParseExact(DataHoraFim, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out resultado))
                        return resultado;

                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public int PesoNumerico
        {
            get
            {
                if (string.IsNullOrEmpty(Peso))
                    return 0;

                try
                {
                    string peso = Peso.Replace("%", "").Trim();
                    return int.TryParse(peso, out int resultado) ? resultado : 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public bool EstaAtiva
        {
            get
            {
                var agora = DateTime.Now;
                var inicio = DataInicioDateTime;
                var fim = DataFimDateTime;

                if (!inicio.HasValue || !fim.HasValue)
                    return false;

                return agora >= inicio.Value && agora <= fim.Value;
            }
        }

        public bool EstaVencida
        {
            get
            {
                var fim = DataFimDateTime;
                return fim.HasValue && DateTime.Now > fim.Value;
            }
        }

        // Override ToString para facilitar debug e exibição
        public override string ToString()
        {
            return $"{Id} - {Titulo} ({Peso})";
        }

        // Override Equals para comparação de tarefas
        public override bool Equals(object obj)
        {
            if (obj is Tarefa outraTarefa)
            {
                return Id == outraTarefa.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}