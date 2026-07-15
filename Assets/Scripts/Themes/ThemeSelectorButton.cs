using UnityEngine;
using TimeCrax.Core;
using TimeCrax.Managers;


namespace TimeCrax.Themes
{
    /// <summary>
    /// Script para o botão 3D de seleção de temas no menu principal.
    /// Deve ser adicionado a um objeto com tag "Selectable" e MeshCollider.
    /// </summary>
    public class ThemeSelectorButton : MonoBehaviour
    {
        [SerializeField] private SuitTop suitTop;
        [SerializeField] private SoundEffects soundEffects;
        [SerializeField] private Canvas inputName;
        [SerializeField] private MenuManager menuManager;


        private void Start()
        {
            // Inscrever no evento de fechamento da tela de temas
            if (ThemeSelectionUI.Instance != null)
            {
                ThemeSelectionUI.Instance.OnPanelClosed += OnThemeSelectionClosed;
            }
        }

        private void OnDestroy()
        {
            if (ThemeSelectionUI.Instance != null)
            {
                ThemeSelectionUI.Instance.OnPanelClosed -= OnThemeSelectionClosed;
            }
        }

        private void OnMouseDown()
        {
            if (!GameManager.TryBeginClick(this)) return;


            // Desabilitar menu enquanto tela de temas está aberta
            menuManager.DesablingMenuOptions();

            // Esconder input de nome se visível
            if (inputName != null)
                inputName.gameObject.SetActive(false);

            // Tocar som
            if (soundEffects != null)
                soundEffects.PressButtonSound();

            // Abrir tela de seleção de temas
            if (ThemeSelectionUI.Instance != null)
            {
                ThemeSelectionUI.Instance.Show();
            }
            else
            {
            }
        }

        /// <summary>
        /// Chamado quando a tela de temas é fechada para reativar o menu
        /// </summary>
        public void OnThemeSelectionClosed()
        {
            menuManager.EnablingMenuOptions();

            if (inputName != null)
                inputName.gameObject.SetActive(true);

            GameManager.ResetClick(this);
        }
    }
}
