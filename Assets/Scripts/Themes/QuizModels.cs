using System;
using System.Collections.Generic;

namespace TimeCrax.Themes
{
    /// <summary>
    /// Tipos de quiz disponíveis para as cartas
    /// </summary>
    public enum QuizType
    {
        None = 0,
        ImageQuiz = 1,
        TextQuiz = 2,
        TrueFalseQuiz = 3,
        CorrelationQuiz = 4
    }

    /// <summary>
    /// Opção de resposta para quizzes de imagem ou texto
    /// </summary>
    [Serializable]
    public class QuizOption
    {
        public string text;
        public string imageUrl;
        public string localImagePath;
    }

    /// <summary>
    /// Quiz com opções em imagem
    /// </summary>
    [Serializable]
    public class ImageQuiz
    {
        public string question;
        public List<QuizOption> options;
        public int correctIndex;
    }

    /// <summary>
    /// Quiz com opções em texto
    /// </summary>
    [Serializable]
    public class TextQuiz
    {
        public string question;
        public List<QuizOption> options;
        public int correctIndex;
    }

    /// <summary>
    /// Quiz de verdadeiro ou falso
    /// </summary>
    [Serializable]
    public class TrueFalseQuiz
    {
        public string statement;
        public bool answer;
    }

    /// <summary>
    /// Item de correlação (imagem + texto)
    /// </summary>
    [Serializable]
    public class CorrelationItem
    {
        public string imageUrl;
        public string localImagePath;
        public string text;
    }

    /// <summary>
    /// Quiz de correlação (associar imagens com textos)
    /// </summary>
    [Serializable]
    public class CorrelationQuiz
    {
        public List<CorrelationItem> items;
    }

    /// <summary>
    /// Dados de quiz de uma carta (pode ter múltiplos tipos)
    /// </summary>
    [Serializable]
    public class CardQuizData
    {
        public ImageQuiz imageQuiz;
        public TextQuiz textQuiz;
        public TrueFalseQuiz trueFalseQuiz;
        public CorrelationQuiz correlationQuiz;

        /// <summary>
        /// Verifica se a carta tem pelo menos um quiz
        /// </summary>
        public bool HasQuiz => imageQuiz != null || textQuiz != null ||
                               trueFalseQuiz != null || correlationQuiz != null;

        /// <summary>
        /// Retorna o tipo de quiz disponível (prioridade: Image > Text > TrueFalse > Correlation)
        /// </summary>
        public QuizType GetAvailableQuizType()
        {
            if (imageQuiz != null) return QuizType.ImageQuiz;
            if (textQuiz != null) return QuizType.TextQuiz;
            if (trueFalseQuiz != null) return QuizType.TrueFalseQuiz;
            if (correlationQuiz != null) return QuizType.CorrelationQuiz;
            return QuizType.None;
        }

        /// <summary>
        /// Retorna uma lista de todos os tipos de quiz disponíveis
        /// </summary>
        public List<QuizType> GetAllAvailableQuizTypes()
        {
            var types = new List<QuizType>();
            if (imageQuiz != null) types.Add(QuizType.ImageQuiz);
            if (textQuiz != null) types.Add(QuizType.TextQuiz);
            if (trueFalseQuiz != null) types.Add(QuizType.TrueFalseQuiz);
            if (correlationQuiz != null) types.Add(QuizType.CorrelationQuiz);
            return types;
        }

        /// <summary>
        /// Retorna um tipo de quiz aleatório entre os disponíveis
        /// </summary>
        public QuizType GetRandomAvailableQuizType()
        {
            var availableTypes = GetAllAvailableQuizTypes();
            if (availableTypes.Count == 0) return QuizType.None;

            int randomIndex = UnityEngine.Random.Range(0, availableTypes.Count);
            return availableTypes[randomIndex];
        }
    }

    #region API Response Classes

    /// <summary>
    /// Response da API para opção de quiz
    /// </summary>
    [Serializable]
    public class QuizOptionResponse
    {
        public string text;
        public string imageUrl;
    }

    /// <summary>
    /// Response da API para ImageQuiz
    /// </summary>
    [Serializable]
    public class ImageQuizResponse
    {
        public string question;
        public List<QuizOptionResponse> options;
        public int correctIndex;
    }

    /// <summary>
    /// Response da API para TextQuiz
    /// </summary>
    [Serializable]
    public class TextQuizResponse
    {
        public string question;
        public List<QuizOptionResponse> options;
        public int correctIndex;
    }

    /// <summary>
    /// Response da API para TrueFalseQuiz
    /// </summary>
    [Serializable]
    public class TrueFalseQuizResponse
    {
        public string statement;
        public bool answer;
    }

    /// <summary>
    /// Response da API para item de correlação
    /// </summary>
    [Serializable]
    public class CorrelationItemResponse
    {
        public string imageUrl;
        public string text;
    }

    /// <summary>
    /// Response da API para CorrelationQuiz
    /// </summary>
    [Serializable]
    public class CorrelationQuizResponse
    {
        public List<CorrelationItemResponse> items;
    }

    #endregion
}
