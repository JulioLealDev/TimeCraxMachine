namespace TimeCrax.Core
{
    /// <summary>
    /// Constantes para nomes de GameObjects usados no jogo.
    /// Centraliza magic strings para facilitar manutenção.
    /// </summary>
    public static class GameObjectNames
    {
        // Prefixos base
        private const string PlateNamePrefix = "plateName0";
        private const string NamePlayerPrefix = "namePlayer0";
        private const string BonusCardSymbolPrefix = "bonusCardSymbol0";
        private const string NumberBonusCardsPrefix = "numberBonusCards0";

        /// <summary>
        /// Retorna o nome do plateName para o índice do jogador (1-based).
        /// Ex: GetPlateName(1) retorna "plateName01"
        /// </summary>
        public static string GetPlateName(int playerNumber) => PlateNamePrefix + playerNumber;

        /// <summary>
        /// Retorna o nome do namePlayer para o índice do jogador (1-based).
        /// Ex: GetNamePlayer(1) retorna "namePlayer01"
        /// </summary>
        public static string GetNamePlayer(int playerNumber) => NamePlayerPrefix + playerNumber;

        /// <summary>
        /// Retorna o nome do bonusCardSymbol para o índice do jogador (1-based).
        /// Ex: GetBonusCardSymbol(1) retorna "bonusCardSymbol01"
        /// </summary>
        public static string GetBonusCardSymbol(int playerNumber) => BonusCardSymbolPrefix + playerNumber;

        /// <summary>
        /// Retorna o nome do numberBonusCards para o índice do jogador (1-based).
        /// Ex: GetNumberBonusCards(1) retorna "numberBonusCards01"
        /// </summary>
        public static string GetNumberBonusCards(int playerNumber) => NumberBonusCardsPrefix + playerNumber;
    }
}
