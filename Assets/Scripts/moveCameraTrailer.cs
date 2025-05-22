using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class moveCameraTrailer : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float moveSpeed = 5f;   // m/s

    [Header("Rotación")]
    [SerializeField] float lookSpeed = 90f;  // °/s
    [SerializeField] float minPitch = -80f; // límites para pitch
    [SerializeField] float maxPitch = 80f;

    float pitch;   // rotación acumulada en X (vertical)

    void Update()
    {
        if (Keyboard.current == null) return;

        /*--------- TRASLACIÓN (WASD) ---------*/
        float h = (Keyboard.current.aKey.isPressed ? -1 : 0) +
                  (Keyboard.current.dKey.isPressed ? 1 : 0);
        float v = (Keyboard.current.wKey.isPressed ? 1 : 0) +
                  (Keyboard.current.sKey.isPressed ? -1 : 0);

        Vector3 moveDir = new Vector3(h, 0, v).normalized;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.Self);

        /*----------- ROTACIÓN (FLECHAS) ----------*/
        float yaw = 0f;    // girar en Y (izq-der)
        float pitchDelta = 0f;

        if (Keyboard.current.leftArrowKey.isPressed) yaw = -1f;
        if (Keyboard.current.rightArrowKey.isPressed) yaw = 1f;
        if (Keyboard.current.upArrowKey.isPressed) pitchDelta = -1f;
        if (Keyboard.current.downArrowKey.isPressed) pitchDelta = 1f;

        // Acumula pitch y limita
        pitch += pitchDelta * lookSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Aplica rotación (yaw en world, pitch en local)
        transform.Rotate(Vector3.up, yaw * lookSpeed * Time.deltaTime, Space.World);

        Vector3 euler = transform.localEulerAngles;
        euler.x = pitch;
        transform.localEulerAngles = euler;
    }
}
