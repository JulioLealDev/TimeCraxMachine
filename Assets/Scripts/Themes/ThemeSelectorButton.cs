using UnityEngine;
using TimeCrax.Core;

namespace TimeCrax.Themes
{
    /// <summary>
    /// Script para o botão 3D de seleção de temas no menu principal.
    /// Deve ser adicionado a um objeto com tag "Selectable" e MeshCollider.
    /// </summary>
    public class ThemeSelectorButton : MonoBehaviour
    {
        [SerializeField] private Menu menu;
        [SerializeField] private SoundEffects soundEffects;
        [SerializeField] private Canvas inputName;

        // Proteção contra clique duplo
        private bool isProcessingClick = false;

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
            // Proteção contra clique duplo
            if (isProcessingClick) return;
            isProcessingClick = true;

            DebugHelper.Log("[ThemeSelectorButton] Clicked");

            // Desabilitar menu enquanto tela de temas está aberta
            if (menu != null)
                menu.DisableMenu();

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
                DebugHelper.Log("[ThemeSelectorButton] ThemeSelectionUI.Instance is null");
            }
        }

        /// <summary>
        /// Chamado quando a tela de temas é fechada para reativar o menu
        /// </summary>
        public void OnThemeSelectionClosed()
        {
            if (menu != null)
                menu.EnableMenu();

            if (inputName != null)
                inputName.gameObject.SetActive(true);

            // Resetar proteção contra clique duplo
            isProcessingClick = false;
        }
    }
}
