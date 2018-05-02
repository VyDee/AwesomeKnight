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





    // Use this for initialization

    void Awake()

    {

        this.playerTarget = GameObject.FindGameObjectWithTag("Player").transform;

        this.nav_agent = this.GetComponent<NavMeshAgent>();

        this.anim = this.GetComponent<Animator>();

        this.charController = this.GetComponent<CharacterController>();

        this.initialPosition = this.transform.position;

        this.where_to_navigate = this.transform.position;

    }



    // Update is called once per frame

    void Update()

    {

        if (this.enemy_CurrentState != EnemyState.DEATH)

        {

            EnemyState tempCurrState = this.enemy_CurrentState;

            this.enemy_CurrentState = this.SetEnemyState(this.enemy_CurrentState, this.enemy_LastState, this.enemy_to_player_distance);

            this.enemy_LastState = tempCurrState;



            if (this.finished_movement)

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

                    this.anim.GetCurrentAnimatorStateInfo(0).IsTag("Atk1") ||

                    this.anim.GetCurrentAnimatorStateInfo(0).IsTag("Atk2"))

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

            if (!this.anim.IsInTransition(0) &&

                this.anim.GetCurrentAnimatorStateInfo(0).IsName("Death") &&

                this.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95)

            {

                Destroy(this.gameObject, 2f);

            }

        }

    }



    EnemyState SetEnemyState(EnemyState currentState, EnemyState lastState, float enemyToPlayerDistance)

    {

        enemyToPlayerDistance = Vector3.Distance(this.transform.position, this.playerTarget.position);



        float initialDistance = Vector3.Distance(this.initialPosition, this.transform.position);

        if (initialDistance > this.follow_distance)

        {

            lastState = currentState;

            currentState = EnemyState.GOBACK;

        }

        else if (enemyToPlayerDistance <= this.attack_distance)

        {

            lastState = currentState;

            currentState = EnemyState.ATTACK;

        }

        else if (enemyToPlayerDistance >= this.alert_attack_distance &&

            lastState == EnemyState.PAUSE || lastState == EnemyState.ATTACK)

        {

            lastState = currentState;

            currentState = EnemyState.PAUSE;

        }

        else if (enemyToPlayerDistance <= this.alert_attack_distance && enemyToPlayerDistance > this.attack_distance)

        {

            if (currentState != EnemyState.GOBACK || lastState == EnemyState.WALK)

            {

                lastState = currentState;

                currentState = EnemyState.PAUSE;

            }

        }

        else if (enemyToPlayerDistance > this.alert_attack_distance &&

            lastState != EnemyState.GOBACK && lastState != EnemyState.PAUSE)

        {

            lastState = currentState;

            currentState = EnemyState.WALK;

        }

        return currentState;

    }



    void GetStateControl(EnemyState currState)

    {

        if (currState == EnemyState.RUN || currState == EnemyState.PAUSE)

        {

            if (currState != EnemyState.ATTACK)

            {

                Vector3 targetPos = new Vector3(this.playerTarget.position.x, this.transform.position.y,

                this.playerTarget.position.z);



                if (Vector3.Distance(this.transform.position, targetPos) >= 2.1f)

                {

                    this.anim.SetBool("Walk", false);

                    this.anim.SetBool("Run", true);

                    this.nav_agent.SetDestination(targetPos);

                }

            }

        }

        else if (currState == EnemyState.ATTACK)

        {

            this.anim.SetBool("Run", false);

            this.anim.SetBool("Attack", true);

            this.where_to_move.Set(0f, 0f, 0f);

            this.nav_agent.SetDestination(this.transform.position);

            //this.transform.LookAt(this.playerTarget);

            this.transform.rotation = Quaternion.Slerp(this.transform.rotation,

                Quaternion.LookRotation(this.playerTarget.position - this.transform.position),

            5f * Time.deltaTime);



            if (this.current_attack_time >= this.wait_attack_time)

            {

                int atkRange = Random.Range(1, 3);

                this.anim.SetInteger("Atk", atkRange);

                this.finished_animation = false;

                this.current_attack_time = 0f;

            }

            else

            {

                this.anim.SetInteger("Atk", 0);

                this.current_attack_time += Time.deltaTime;

            }

        }

        else if (currState == EnemyState.GOBACK)

        {

            this.anim.SetBool("Run", true);

            Vector3 targetPos = new Vector3(this.initialPosition.x, this.transform.position.y, this.initialPosition.z);

            this.nav_agent.SetDestination(targetPos);



            if (Vector3.Distance(this.initialPosition, this.transform.position) <= 3.5f)

            {

                this.enemy_LastState = currState;

                currState = EnemyState.WALK;

            }

        }

        else if (currState == EnemyState.WALK)

        {

            this.anim.SetBool("Run", false);

            this.anim.SetBool("Walk", true);

            if (Vector3.Distance(this.transform.position, this.where_to_navigate) <= 2f)

            {

                this.where_to_navigate.x = Random.Range(this.initialPosition.x - 5f, this.initialPosition.x + 5f);

                this.where_to_navigate.z = Random.Range(this.initialPosition.z - 5f, this.initialPosition.z + 5f);

            }

            else

            {

                this.nav_agent.SetDestination(this.where_to_navigate);

            }

        }

        else

        {

            this.anim.SetBool("Run", false);

            this.anim.SetBool("Walk", false);

            this.nav_agent.isStopped = true;

        }

    }

}