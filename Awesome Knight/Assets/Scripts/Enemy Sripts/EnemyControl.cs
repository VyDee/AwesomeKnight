using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum EnemyState
{
    IDLE,
    WALK,
    RUN,
    PAUSE,
    GOBACK,
    ATTACK,
    DEATH
}
public class EnemyControl : MonoBehaviour
{
    private float attack_distance = 1.5f;
    private float alert_attack_distance = 8f;
    private float follow_distance = 15f;
    private float enemy_to_player_distance;

    [HideInInspector]
    public EnemyState enemy_CurrentState = EnemyState.IDLE;
    private EnemyState enemy_LastState = EnemyState.IDLE;

    private Transform playerTarget;
    private Vector3 initialPosition;

    private float move_speed = 2f;
    private float walk_speed = 1f;
    private CharacterController charController;
    private Vector3 where_to_move = Vector3.zero;

    private float current_attack_time;
    private float wait_attack_time = 1f;

    private Animator anim;
    private bool finished_animation = true;
    private bool finished_movement = true;

    private NavMeshAgent nav_agent;
    private Vector3 where_to_navigate;

	void Awake ()
    {
        this.charController = this.GetComponent<CharacterController>();
        this.anim = this.GetComponent<Animator>();
        this.agent = this.GetComponent<NavMeshAgent>();
        this.playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        this.initialPosition = this.transform.position;
        this.where_to_navigate = this.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(this.enemy_CurrentState != EnemyState.DEATH)
        {
            enemy_CurrentState = this.SetEnemyState();
            if(this.finished_movement)
            {
                this.GetStateControl(this.enemy_CurrentState);
            }
            else
            {
                if (!this.anim.IsInTransition(0) && this.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    this.finished_movement = true;
                }
                else if (!this.anim.IsInTransition(0) &&
                    this.anim.GetCurrentAnimatorStateInfo(0).IsName("Atk1") ||
                    this.anim.GetCurrentAnimatorStateInfo(0).IsName("Atk2"))
                {
                    this.anim.SetInteger("Atk", 0);
                }
            }
        }
        else
        {
            this.anim.SetBool("Death", true);
            this.charController.enabled = false;
            this.nav_agent.enabled = false;
            if(!this.anim.IsInTransition(0) &&
                this.anim.GetCurrentAnimatorStateInfo(0).IsName("Death") &&
                this.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >=0.95) 
            {
                Destroy(gameObject, 2f);
            }
        }
	}
    EnemyState SetEnemyState (EnemyState currentState,EnemyState lastState, float enemyToPlayerDistance)
    {
        float initialDistance = Vector3.Distance(this.initialPosition, this.transform.position);
        if (initialDistance > this.follow_distance)
        {
            lastState = currentState;
            currentState = EnemyState.GOBACK;
        }
    }
    void GetStateControl(EnemyState state)
    {

    }
}
