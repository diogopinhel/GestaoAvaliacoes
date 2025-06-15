using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projecto_Lab
{
    public class OperacaoInvalidaException : ApplicationException
    {
        public OperacaoInvalidaException(string mensagem)
            : base(mensagem)
        {
        }

        public OperacaoInvalidaException(string mensagem, Exception innerException)
            : base(mensagem, innerException)
        {
        }
    }
}