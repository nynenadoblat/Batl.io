using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class BehaviourPlayer : MonoBehaviour
{
    NavMeshAgent agent;
    Player player;
    UI ui;

    Vector2 move;

    bool isFire;
    bool enableRotate;

    private IEnumerator Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GetComponent<Player>();
        player.onPerkAvailable += Perk_Availabled;
        player.onNextStageAvailable += NextStage_Availabled;

        yield return null;

        enableRotate = true;
    }

    private void NextStage_Availabled()
    {
        if (this.IsMine())
        {
            EventsHolder.nextStageAvailable?.Invoke(player);
        }
    }

    private void Perk_Availabled(List<PerkID> usedPerks)
    {
        //ui.ShowPerkPanel(usedPerks);
    }

    public void SetUI(UI ui)
    {
        this.ui = ui;

        //this.ui.onJoystick += Joystick_Raised;
        //this.ui.onPerkClick += Perk_Clicked;
    }

    private void Perk_Clicked(PerkID perkID)
    {
        player.PickPerk(perkID);
    }

    private void Joystick_Raised(Vector2 value)
    {
        move = value;
        //move.Normalize();
       
        //agent.SetDestination(transform.position + (new Vector3(move.x, 0, move.y) * 1));
        //agent.Move(new Vector3(move.x, 0, move.y) * Time.deltaTime * agent.speed);

        
    }

    private void Update()
    {
        if (player.IsMine())
        {
            if (Input.GetKey(KeyCode.W))
            {
                move.y = 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                move.x = -1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                move.y = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                move.x = 1;
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                move.y = 0;
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                move.x = 0;
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                move.y = 0;
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                move.x = 0;
            }

            move.Normalize();
            
            //agent.SetDestination(transform.position + (new Vector3(move.x, 0, move.y) * 1));
            agent.Move(agent.speed * Time.deltaTime * new Vector3(move.x, 0, move.y));

            

            UpdateRotation();

            if (Input.GetKeyDown(KeyCode.E))
            {
                isFire = !isFire;
            }

            if(Input.GetMouseButton(0) || isFire)
            {
                player.Fire();
            }
        }
    }

    void UpdateRotation()
    {
        if (enableRotate)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray);

            float distanceToGround = 0;

            foreach (var hit in hits)
            {
                if (hit.collider.gameObject.CompareTag("Ground"))
                {
                    distanceToGround = hit.distance;
                    break;
                }
            }

            Vector3 mousePos = new()
            {
                x = Input.mousePosition.x,
                y = Input.mousePosition.y,
                z = distanceToGround
            };

            var pos = Camera.main.ScreenToWorldPoint(mousePos);

            var dir = pos - transform.position;
            dir.y = 0;
            dir.Normalize();

            transform.forward = dir;
        }
    }

    private void OnDestroy()
    {
        if (ui)
        {
            ui.onJoystick -= Joystick_Raised;
            ui.onPerkClick -= Perk_Clicked;
        }
    }
}
