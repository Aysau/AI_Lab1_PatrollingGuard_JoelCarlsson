using GameAI.Day02_AStar.Grid;
using UnityEngine;
public class AgentSpawner : MonoBehaviour
{
    public SteeringAgent agentPrefab;
    public int agentCount = 10;
    public Vector2 spawnAreaSize = new Vector2(10f, 10f);
    [SerializeField] GameObject startTarget;
    void Start()
    {
        for (int i = 0; i < agentCount; i++)
        {
            Vector3 offset = new Vector3(
            Random.Range(-spawnAreaSize.x * 0.5f,
            spawnAreaSize.x * 0.5f),
            1f,
            Random.Range(-spawnAreaSize.y * 0.5f,
            spawnAreaSize.y * 0.5f)
            );
            Vector3 spawnPos = transform.position + offset;
            Instantiate(agentPrefab, spawnPos, Quaternion.identity).GetComponent<SteeringAgent>().target = startTarget.transform;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position,
        new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
    }
}
