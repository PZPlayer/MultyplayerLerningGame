using UnityEngine;

namespace Thanks.Player
{
    public class PlayerCursorController : MonoBehaviour, ICursorInfo, IBoostrapble
    {

        void IBoostrapble.BoostrapAwake()
        {
            LockAndHide();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockAndShow();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                LockAndHide();
            }
        }

        /// <summary>
        /// Фиксирует курсор в центре экрана и скрывает его
        /// </summary>
        public void LockAndHide()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Освобождает курсор и делает его видимым
        /// </summary>
        public void UnlockAndShow()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Только фиксирует курсор (не меняет видимость)
        /// </summary>
        public void LockOnly()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Только освобождает курсор (не меняет видимость)
        /// </summary>
        public void UnlockOnly()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Только прячет курсор (не меняет блокировку)
        /// </summary>
        public void HideOnly()
        {
            Cursor.visible = false;
        }

        /// <summary>
        /// Только показывает курсор (не меняет блокировку)
        /// </summary>
        public void ShowOnly()
        {
            Cursor.visible = true;
        }

        /// <summary>
        /// Проверяет, зафиксирован ли курсор
        /// </summary>
        public bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

        /// <summary>
        /// Проверяет, видим ли курсор
        /// </summary>
        public bool IsVisible => Cursor.visible;
    }
}