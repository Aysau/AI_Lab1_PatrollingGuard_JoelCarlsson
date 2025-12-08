using UnityEngine;
using UnityEngine.AI;

public class GuardBehaviour : MonoBehaviour
{
    public enum GuardState
    {
        Patrolling,
        Chasing,
        Searching,
        ReturningToPatrol
    }

    [Header("Patrolling")]
    [SerializeField] Transform[] Waypoints;
    [SerializeField] float WaypointTolerance = 0.5f;

    [Header("Vision")]
    [SerializeField] float FieldOfViewAngle = 90f;
    [SerializeField] float ChaseRange = 5f;
    [SerializeField] float LoseRange = 7f;
    [SerializeField] LayerMask VisionBlockingLayers;

    [Header("References")]
    [SerializeField] Transform Player;
    [SerializeField] GuardState CurrentState = GuardState.Patrolling;

    [Header("Searching")]
    [SerializeField] float SearchDuration = 3f;
    private Vector3 _lastKnownPlayerPosition;
    private float _searchTimer;

    int _currentIndex = 0;
    NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }
    void Start() //Initial start for patrolling
    {
        if(Waypoints.Length > 0)
        {
            _agent.SetDestination(Waypoints[_currentIndex].position);
        }
    }

    void Update() //Main state machine update
    {
        switch (CurrentState)
        {
            case GuardState.Patrolling:
                UpdatePatrol();
                break;
            case GuardState.Chasing:
                UpdateChase();
                break;
            case GuardState.Searching:
                UpdateSearching();
                break;
            case GuardState.ReturningToPatrol:
                UpdateReturning();
                break;
        }
    }

    void UpdatePatrol() //When patrolling, check if player is seen, else go to next waypoint
    {
        if (Waypoints.Length == 0)
        {
            return;
        }

        if(IsPlayerInFieldOfView())
        {
            CurrentState = GuardState.Chasing;
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= WaypointTolerance)
        {
            _currentIndex = (_currentIndex + 1) % Waypoints.Length;
            _agent.SetDestination(Waypoints[_currentIndex].position);
        }
    }

    void UpdateChase() //When chasing, check if player is still seen and within chase range, else go to searching
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        if (distanceToPlayer > LoseRange || !IsPlayerInFieldOfView())
        {
            CurrentState = GuardState.ReturningToPatrol;
            _lastKnownPlayerPosition = Player.position;
            _searchTimer = SearchDuration;
            _agent.SetDestination(_lastKnownPlayerPosition);
        }
        else
        {
            _agent.SetDestination(Player.position);
        }
    }

    void UpdateSearching() //When searching, check if player is seen first, else wait for search timer to expire
    {
        _searchTimer -= Time.deltaTime;

        if (IsPlayerInFieldOfView())
        {
            CurrentState = GuardState.Chasing;
            return;
        }

        if(!_agent.pathPending && _agent.remainingDistance > WaypointTolerance)
        {
            return;
        }

        if(_searchTimer <= 0f)
        {
            CurrentState = GuardState.ReturningToPatrol;
            _agent.SetDestination(Waypoints[_currentIndex].position);
        }
    }

    void UpdateReturning() //When returning to patrol, check if player is seen first, else go to last known position then back to patrol
    {
        if(IsPlayerInFieldOfView())
        {
            CurrentState = GuardState.Chasing;
            return;
        }
        if (!_agent.pathPending && _agent.remainingDistance <= WaypointTolerance)
        {
            CurrentState = GuardState.Patrolling;
            _agent.SetDestination(Waypoints[_currentIndex].position);
        }
    }


    bool IsPlayerInFieldOfView() //General vision check including a raycast and dotproduct to see if player is in sight
    {
        Vector3 directionToPlayer = (Player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

        if(distanceToPlayer > ChaseRange)
        {
            return false;
        }

        float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);
        float angleToPlayer = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        if(angleToPlayer > FieldOfViewAngle / 2f)
        {
            return false;
        }

        if(Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hitInfo, distanceToPlayer))
        {
            if(!hitInfo.transform.CompareTag("Player"))
            { 
                return false;
            }
        }

        return true;
    }
}
