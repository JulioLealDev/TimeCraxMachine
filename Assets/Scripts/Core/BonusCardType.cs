namespace TimeCrax.Core
{
    /// <summary>
    /// Tipos de cartas bonus disponíveis no jogo.
    /// </summary>
    public enum BonusCardType
    {
        /// <summary>
        /// Conserta um componente com malfunction.
        /// Auto-usa ao clicar em componente com malfunction.
        /// </summary>
        RepairComponent,

        /// <summary>
        /// Adiciona +60 segundos ao timer do turno.
        /// Pode ser ativada a qualquer momento.
        /// </summary>
        BonusTime,

        /// <summary>
        /// Permite uma segunda tentativa ao errar o slot da timeline.
        /// Deve ser ativada antes de selecionar o slot.
        /// </summary>
        SecondChanceSlot,

        /// <summary>
        /// Baixa a temperatura do termômetro para o primeiro nível.
        /// Pode ser ativada a qualquer momento.
        /// </summary>
        CoolThermometer,

        /// <summary>
        /// Remove uma das opções de resposta do desafio.
        /// Deve ser ativada antes responder o desafio.
        /// </summary>
        KillChallengeOption,

        /// <summary>
        /// Evita ter que responder o desafio.
        /// Deve ser ativada antes responder o desafio.
        /// </summary>
        SkipChallenge
    }
}
