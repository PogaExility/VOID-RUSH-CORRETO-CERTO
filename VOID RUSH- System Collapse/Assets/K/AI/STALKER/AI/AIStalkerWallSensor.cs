using UnityEngine;
using System.Collections.Generic;

public struct WallAnalysisReport
{
    public bool IsWallDetected;
    public float WallHeight;
    public Vector2 WallTopPosition;
    public List<TraversalOpportunity> Opportunities;
}

public struct TraversalOpportunity
{
    public Vector2 EntryPosition;
    public int HeightInTiles;
    public bool RequiresCrouch;
}

[RequireComponent(typeof(AIPlatformerMotor))]
public class AIStalkerWallSensor : MonoBehaviour
{
    private AIPlatformerMotor _motor;

    [Header("▶ Referências Essenciais")]
    public Transform wallScanOriginProbe;

    [Header("▶ Configuração do Scanner")]
    public LayerMask groundLayer;
    public Vector2 wallCheckSize = new Vector2(0.2f, 0.8f);
    public float wallDetectionDistance = 0.5f;
    public float maxScanHeight = 15f;
    public float tileSize = 1.0f;

    void Awake()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        if (wallScanOriginProbe == null)
        {
            Debug.LogError("ERRO CRÍTICO: 'wallScanOriginProbe' não foi atribuído no Inspector!", this);
            this.enabled = false;
        }
    }

    public WallAnalysisReport AnalyzeWallInFront()
    {
        var report = new WallAnalysisReport
        {
            Opportunities = new List<TraversalOpportunity>(),
            IsWallDetected = false,
        };

        Vector2 boxCenter = (Vector2)wallScanOriginProbe.position + (Vector2.right * _motor.currentFacingDirection * (wallCheckSize.x / 2));
        Collider2D wallCollider = Physics2D.OverlapBox(boxCenter, wallCheckSize, 0f, groundLayer);

        if (wallCollider == null)
        {
            return report; // Retorna o relatório vazio, IsWallDetected = false
        }

        report.IsWallDetected = true;
        Vector2 scanOrigin = wallCollider.ClosestPoint(boxCenter);

        int consecutiveOpenSpaces = 0;
        float lastSolidHeight = 0f;

        for (float currentHeight = 0; currentHeight < maxScanHeight; currentHeight += tileSize)
        {
            Vector2 scanStartPoint = new Vector2(scanOrigin.x, transform.position.y + currentHeight);
            bool isSpaceBlocked = Physics2D.Raycast(scanStartPoint, transform.right, wallDetectionDistance, groundLayer);

            if (!isSpaceBlocked)
            {
                consecutiveOpenSpaces++;
            }
            else
            {
                lastSolidHeight = currentHeight;
                if (consecutiveOpenSpaces >= 2)
                {
                    report.Opportunities.Add(new TraversalOpportunity
                    {
                        HeightInTiles = consecutiveOpenSpaces,
                        EntryPosition = new Vector2(scanOrigin.x, scanStartPoint.y - (consecutiveOpenSpaces * tileSize))
                    });
                }
                consecutiveOpenSpaces = 0;
            }
        }

        // Se o scan terminou com espaços abertos, significa que encontramos o topo da parede.
        if (consecutiveOpenSpaces > 0)
        {
            report.WallHeight = lastSolidHeight;
            report.WallTopPosition = new Vector2(scanOrigin.x, transform.position.y + lastSolidHeight);
        }
        else
        {
            // Se o scan terminou com blocos sólidos, a parede é mais alta que o scan.
            report.WallHeight = maxScanHeight;
            report.WallTopPosition = new Vector2(scanOrigin.x, transform.position.y + maxScanHeight);
        }

        return report;
    }

    void OnDrawGizmosSelected()
    {
        if (wallScanOriginProbe != null && _motor != null)
        {
            Gizmos.color = Color.cyan;
            Vector2 boxCenter = (Vector2)wallScanOriginProbe.position + (Vector2.right * _motor.currentFacingDirection * (wallCheckSize.x / 2));
            Gizmos.DrawWireCube(boxCenter, wallCheckSize);
        }
    }
}