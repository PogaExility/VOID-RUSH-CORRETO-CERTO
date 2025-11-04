using UnityEngine;
using System.Collections.Generic;

// O relatório detalhado que o sensor produz para o cérebro.
public struct WallAnalysisReport
{
    public bool IsWallDetected;
    public float WallHeight;
    public Vector2 WallTopPosition;
    public List<TraversalOpportunity> Opportunities; // Lista de buracos
}

// Uma estrutura que descreve um único buraco.
public struct TraversalOpportunity
{
    public Vector2 EntryPosition; // Coordenada da base do buraco
    public int HeightInTiles; // 2 ou 3
    public bool RequiresCrouch;
}

[RequireComponent(typeof(AIPlatformerMotor))]
public class AIStalkerWallSensor : MonoBehaviour
{
    [Header("▶ Referências Essenciais")]
    [Tooltip("A sonda que define o ponto de partida para o scan da parede. Use o mesmo objeto do 'Probe_Wall_Base' do NavigationSystem.")]
    public Transform wallScanOriginProbe; // <-- A ADIÇÃO CRÍTICA

    [Header("▶ Configuração do Scanner")]
    public LayerMask groundLayer;
    public float wallDetectionDistance = 0.5f;
    [Tooltip("Altura máxima que o sensor irá escanear.")]
    public float maxScanHeight = 15f;
    [Tooltip("O tamanho de um 'tile' ou 'bloco' do seu jogo.")]
    public float tileSize = 1.0f;

    private AIPlatformerMotor _motor;

    void Awake()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        if (wallScanOriginProbe == null)
        {
            Debug.LogError("ERRO CRÍTICO: 'wallScanOriginProbe' não foi atribuído no Inspector do AIStalkerWallSensor!", this);
            this.enabled = false;
        }
    }

    public WallAnalysisReport AnalyzeWallInFront()
    {
        var report = new WallAnalysisReport
        {
            Opportunities = new List<TraversalOpportunity>(),
            IsWallDetected = false,
            WallHeight = 0f
        };

        // 1. Encontra a superfície da parede usando a SONDA CORRETA.
        RaycastHit2D baseHit = Physics2D.Raycast(wallScanOriginProbe.position, transform.right, wallDetectionDistance, groundLayer);
        if (!baseHit.collider) return report;

        report.IsWallDetected = true;
        Vector2 scanOrigin = new Vector2(baseHit.point.x, wallScanOriginProbe.position.y);

        // 2. Escaneia a parede de baixo para cima, procurando por buracos.
        int consecutiveOpenSpaces = 0;
        for (float currentHeight = tileSize / 2; currentHeight < maxScanHeight; currentHeight += tileSize)
        {
            Vector2 scanPoint = scanOrigin + new Vector2(0, currentHeight);
            bool isSpaceBlocked = Physics2D.Raycast(scanPoint, transform.right, wallDetectionDistance, groundLayer);

            if (!isSpaceBlocked)
            {
                consecutiveOpenSpaces++;
            }
            else
            {
                if (consecutiveOpenSpaces >= 2)
                {
                    var opportunity = new TraversalOpportunity();
                    opportunity.HeightInTiles = consecutiveOpenSpaces;
                    opportunity.EntryPosition = new Vector2(scanPoint.x, scanPoint.y - (consecutiveOpenSpaces * tileSize));
                    report.Opportunities.Add(opportunity);
                }
                consecutiveOpenSpaces = 0;
            }
        }

        // 3. Mede a altura total da parede.
        RaycastHit2D topHit = Physics2D.Raycast(scanOrigin + new Vector2(0, maxScanHeight), Vector2.down, maxScanHeight, groundLayer);
        if (topHit.collider)
        {
            report.WallTopPosition = topHit.point;
            report.WallHeight = topHit.point.y - wallScanOriginProbe.position.y;
        }
        else
        {
            report.WallHeight = maxScanHeight;
        }

        return report;
    }
}