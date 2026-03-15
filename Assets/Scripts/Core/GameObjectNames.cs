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
        private const string RepairCardSymbolPrefix = "repairCardSymbol0";
        private const string NumberRepairCardsPrefix = "numberRepairCards0";

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
        /// Retorna o nome do repairCardSymbol para o índice do jogador (1-based).
        /// Ex: GetRepairCardSymbol(1) retorna "repairCardSymbol01"
        /// </summary>
        public static string GetRepairCardSymbol(int playerNumber) => RepairCardSymbolPrefix + playerNumber;

        /// <summary>
        /// Retorna o nome do numberRepairCards para o índice do jogador (1-based).
        /// Ex: GetNumberRepairCards(1) retorna "numberRepairCards01"
        /// </summary>
        public static string GetNumberRepairCards(int playerNumber) => NumberRepairCardsPrefix + playerNumber;
    }
}
