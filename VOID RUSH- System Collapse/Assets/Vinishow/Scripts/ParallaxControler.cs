using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// É NECESSÁRIO ADICIONAR ESTA LINHA PARA O SCRIPT RECONHECER O CINEMACHINE
using Unity.Cinemachine;

public class ParallaxController : MonoBehaviour
{
    private Transform cam;
    private Vector3 camStartPos;
    private float distance;

    private GameObject[] backgrounds;
    private Material[] mat;
    private float[] backSpeed;
    private float farthestBack;

    [Range(0.01f, 0.05f)]
    public float parallaxSpeed;

    void Start()
    {
        StartCoroutine(FindCameraAndInitialize());
    }

    private IEnumerator FindCameraAndInitialize()
    {
        // Variável temporária para encontrar o CinemachineBrain.
        CinemachineBrain cinemachineBrain = null;

        // Loop que roda até que o CinemachineBrain seja encontrado.
        while (cinemachineBrain == null)
        {
            // MODIFICAÇÃO PRINCIPAL AQUI: Procuramos pelo componente, não pela tag.
            cinemachineBrain = FindObjectOfType<CinemachineBrain>();

            // Se não encontrou, espera até o próximo frame.
            if (cinemachineBrain == null)
            {
                yield return null;
            }
        }

        // --- Câmera encontrada! Agora pegamos o transform dela e continuamos ---
        cam = cinemachineBrain.transform;

        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++)
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++)
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            return;
        }

        distance = cam.position.x - camStartPos.x;
        transform.position = new Vector3(cam.position.x, transform.position.y, 0);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(distance, 0) * speed);
        }
    }
}