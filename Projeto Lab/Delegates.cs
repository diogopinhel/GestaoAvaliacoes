using System;

namespace Projecto_Lab
{
    // Delegate para métodos sem parâmetros (usado para notificações simples)
    public delegate void MetodosSemParametros();

    // Delegate para métodos com parâmetro string (usado para notificações com informação)
    public delegate void MetodosComString(string informacao);

    // Delegate para métodos com parâmetro de erro (usado para notificações de erro)
    public delegate void MetodosComErro(string mensagemErro);

    // Delegate específico para notificação de aluno adicionado/removido
    public delegate void AlunoAlterado(string numeroAluno, string operacao);

    // Delegate específico para notificação de grupo adicionado/removido/atualizado
    public delegate void GrupoAlterado(string idGrupo, string operacao);

    // Delegate específico para notificação de tarefa adicionada/removida/atualizada
    public delegate void TarefaAlterada(int idTarefa, string operacao);

    // Delegate específico para notificação de nota adicionada/removida/atualizada
    public delegate void NotaAlterada(int idNota, string operacao);
}