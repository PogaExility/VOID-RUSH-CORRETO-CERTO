using UnityEngine;
using Unity.Cinemachine; // O namespace correto que voc� descobriu!

/// <summary>
/// Este script encontra automaticamente o jogador na cena e o define como o alvo 
/// (Follow e LookAt) para a CinemachineCamera.
/// Anexe este script ao mesmo GameObject que cont�m o componente CinemachineCamera.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))] // <-- MUDAN�A AQUI
public class CinemachineTargetSetter : MonoBehaviour
{
    private CinemachineCamera vcam; // <-- MUDAN�A AQUI

    void Awake()
    {
        // Pega a refer�ncia da Virtual Camera neste mesmo GameObject.
        vcam = GetComponent<CinemachineCamera>(); // <-- MUDAN�A AQUI
    }

    void Start()
    {
        // Procura pelo GameObject do jogador usando a tag "Player".
        GameObject playerTarget = GameObject.FindGameObjectWithTag("Player");

        if (playerTarget != null)
        {
            // Se encontrou o jogador, define o transform dele como alvo de Follow e LookAt.
            Transform targetTransform = playerTarget.transform;
            vcam.Follow = targetTransform;
            vcam.LookAt = targetTransform;
            Debug.Log("CinemachineTargetSetter: Alvo da c�mera definido para o Player.");
        }
        else
        {
            Debug.LogWarning("CinemachineTargetSetter: N�o foi poss�vel encontrar um GameObject com a tag 'Player' para a c�mera seguir.");
        }
    }
}