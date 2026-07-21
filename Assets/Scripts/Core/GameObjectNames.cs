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
        private const string PlateNameTextPrefix = "plateNameText0";
        private const string BonusCardSymbolPrefix = "bonusCardSymbol0";
        private const string NumberBonusCardsPrefix = "bonusCardText0";

        /// <summary>
        /// Retorna o nome do plateName para o índice do jogador (1-based).
        /// Ex: GetPlateName(1) retorna "plateName01"
        /// </summary>
        public static string GetPlateName(int playerNumber) => PlateNamePrefix + playerNumber;

        /// <summary>
        /// Retorna o nome do plateNameText para o índice do jogador (1-based).
        /// Ex: GetPlateNameText(1) retorna "plateNameText01"
        /// </summary>
        public static string GetPlateNameText(int playerNumber) => PlateNameTextPrefix + playerNumber;

        /// <summary>
        /// Retorna o nome do bonusCardSymbol para o índice do jogador (1-based).
        /// Ex: GetBonusCardSymbol(1) retorna "bonusCardSymbol01"
        /// </summary>
        public static string GetBonusCardSymbol(int playerNumber) => BonusCardSymbolPrefix + playerNumber;

        /// <summary>
        /// Retorna o nome do bonusCardText para o índice do jogador (1-based).
        /// Ex: GetNumberBonusCards(1) retorna "bonusCardText01"
        /// </summary>
        public static string GetNumberBonusCards(int playerNumber) => NumberBonusCardsPrefix + playerNumber;
    }
}
