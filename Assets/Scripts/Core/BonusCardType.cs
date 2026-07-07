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
        Repair,

        /// <summary>
        /// Adiciona +60 segundos ao timer do turno.
        /// Pode ser ativada a qualquer momento.
        /// </summary>
        Time,

        /// <summary>
        /// Pula o quiz atual e sorteia outro.
        /// Só pode ser ativada durante um quiz ativo.
        /// </summary>
        SkipQuiz,

        /// <summary>
        /// Elimina uma opção incorreta do quiz.
        /// Só pode ser ativada durante um quiz ativo.
        /// </summary>
        KillOption,

        /// <summary>
        /// Permite uma segunda tentativa ao errar o slot da timeline.
        /// Deve ser ativada antes de selecionar o slot.
        /// </summary>
        SecondChance,

        /// <summary>
        /// Baixa a temperatura do termômetro para o primeiro nível.
        /// Pode ser ativada a qualquer momento.
        /// </summary>
        Thermometer
    }
}
