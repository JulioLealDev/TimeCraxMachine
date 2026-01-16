using System;
using System.Collections.Generic;

namespace TimeCrax.Themes
{
    [Serializable]
    public class ThemeCard
    {
        public string id;
        public int orderIndex;
        public int year;
        public string era;
        public string title;
        public string imageUrl;
        public string localImagePath;
        public CardQuizData quizData; // Dados do quiz da carta
    }

    [Serializable]
    public class ThemeData
    {
        public string id;
        public string name;
        public string version;
        public string creatorName;
        public string resume;
        public string recommendation;
        public string coverImageUrl;
        public string localCoverPath;
        public int cardCount;
        public List<ThemeCard> cards = new List<ThemeCard>();
        public long downloadedAt;
    }

    [Serializable]
    public class ThemeListItem
    {
        public string id;
        public string name;
        public string image;
        public bool readyToPlay;
        public string creatorName;
        public string createdAt;
        public string resume;
        public string recommendation;
        public int numberOfCards;
    }

    [Serializable]
    public class ThemeStorageResponse
    {
        public List<ThemeListItem> items;
        public int page;
        public int pageSize;
        public int totalCount;
        public int totalPages;
    }

    [Serializable]
    public class ThemeDownloadResponse
    {
        public string id;
        public string name;
        public string version;
        public string creatorName;
        public string resume;
        public string recommendation;
        public string image;           // Nova API: imagem de capa
        public string coverImageUrl;   // Manter para compatibilidade
        public string uploadSessionId; // Nova API
        public int cardCount;
        public List<ThemeCardResponse> cards;
    }

    [Serializable]
    public class ThemeCardResponse
    {
        public string id;
        public int orderIndex;
        public int year;
        public string era;
        public string title;
        public string imageUrl;

        // Quizzes da API
        public ImageQuizResponse imageQuiz;
        public TextQuizResponse textQuiz;
        public TrueFalseQuizResponse trueFalseQuiz;
        public CorrelationQuizResponse correlationQuiz;
    }

    [Serializable]
    public class LocalThemeManifest
    {
        public List<ThemeData> themes = new List<ThemeData>();
    }
}
