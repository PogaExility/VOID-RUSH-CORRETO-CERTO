#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyMotor))]
public class EnemyMotorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenha o Inspector padrão (todas as variáveis que já existem)
        DrawDefaultInspector();

        // Pega uma referência ao script EnemyMotor
        EnemyMotor motor = (EnemyMotor)target;

        // Adiciona um espaço de 10 pixels
        GUILayout.Space(10);

        // Cria o Botão Visual
        // Se clicar no botão, executa o comando
        if (GUILayout.Button("VIRAR INIMIGO (Flip)"))
        {
            motor.DebugFlip();
        }
    }
}
#endif