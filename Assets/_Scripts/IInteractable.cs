// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Interface genérica para qualquer objeto interagível pelo player (NPCs, arbustos, etc.)
// ----------------------------------------------------------------

public interface IInteractable
{
    /// <summary>Chamado quando o player pressiona E estando em range.</summary>
    void Interact();

    /// <summary>Chamado quando o player sai do range sem interagir.</summary>
    void Cancel();
}
