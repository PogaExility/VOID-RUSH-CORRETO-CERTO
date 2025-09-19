using UnityEngine;
using Unity.Cinemachine; // O namespace correto que você descobriu!

/// <summary>
/// Este script encontra automaticamente o jogador na cena e o define como o alvo 
/// (Follow e LookAt) para a CinemachineCamera.
/// Anexe este script ao mesmo GameObject que contém o componente CinemachineCamera.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))] // <-- MUDANÇA AQUI
public class CinemachineTargetSetter : MonoBehaviour
{
    private CinemachineCamera vcam; // <-- MUDANÇA AQUI

    void Awake()
    {
        // Pega a referência da Virtual Camera neste mesmo GameObject.
        vcam = GetComponent<CinemachineCamera>(); // <-- MUDANÇA AQUI
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
            Debug.Log("CinemachineTargetSetter: Alvo da câmera definido para o Player.");
        }
        else
        {
            Debug.LogWarning("CinemachineTargetSetter: Não foi possível encontrar um GameObject com a tag 'Player' para a câmera seguir.");
        }
    }
}