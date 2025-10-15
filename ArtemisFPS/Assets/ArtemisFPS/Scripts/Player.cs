using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArtemisFPS.Scripts
{
    [RequireComponent(typeof(Controller))]
    public class Player : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Controller _controller;


        #region Unity Methods

        private void OnValidate()
        {
            if (_controller == null)
            {
                _controller = GetComponent<Controller>();
            }
        }

        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        #endregion


        #region Input Handing

        private void OnMove(InputValue value) =>
            _controller.MoveInput = value.Get<Vector2>();

        private void OnLook(InputValue value) =>
            _controller.LookInput = value.Get<Vector2>();

        private void OnSprint(InputValue value) =>
            _controller.IsSprintInput = value.isPressed;

        #endregion
    }
}