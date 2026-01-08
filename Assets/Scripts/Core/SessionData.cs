namespace TimeCrax.Core
{
    /// <summary>
    /// Armazena dados de sessão do jogo em memória.
    /// Substitui PlayerPrefs para dados que não precisam persistir entre sessões.
    ///
    /// Vantagens:
    /// - Mais rápido que PlayerPrefs (não acessa disco)
    /// - Não persiste dados sensíveis
    /// - Dados limpos automaticamente ao fechar o jogo
    /// </summary>
    public static class SessionData
    {
        /// <summary>
        /// Nickname do jogador atual.
        /// </summary>
        public static string Nickname { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o jogo foi iniciado (jogador entrou em uma partida).
        /// </summary>
        public static bool GameStarted { get; set; } = false;

        /// <summary>
        /// Número de jogadores selecionado para a partida.
        /// </summary>
        public static int NumberOfPlayers { get; set; } = 1;

        /// <summary>
        /// Dificuldade do jogo selecionada (Easy, Normal, Hard).
        /// </summary>
        public static string GameDifficulty { get; set; } = "Normal";

        /// <summary>
        /// Reseta todos os dados da sessão para os valores padrão.
        /// </summary>
        public static void Reset()
        {
            Nickname = string.Empty;
            GameStarted = false;
            NumberOfPlayers = 1;
            GameDifficulty = "Normal";
        }
    }
}
