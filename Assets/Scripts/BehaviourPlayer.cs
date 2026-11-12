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
            Vector2 inputMove = Vector2.zero;

            if (Input.GetKey(KeyCode.W)) inputMove.y += 1f;
            if (Input.GetKey(KeyCode.S)) inputMove.y -= 1f;
            if (Input.GetKey(KeyCode.A)) inputMove.x -= 1f;
            if (Input.GetKey(KeyCode.D)) inputMove.x += 1f;

            if (ui != null && ui.MoveDirection.sqrMagnitude > 0.01f)
            {
                inputMove = ui.MoveDirection;
            }
            else if (inputMove.sqrMagnitude > 0.01f)
            {
                inputMove.Normalize();
            }

            move = inputMove;
            agent.Move(agent.speed * Time.deltaTime * new Vector3(move.x, 0, move.y));

            UpdateRotation();

            if (Input.GetKeyDown(KeyCode.E))
            {
                isFire = !isFire;
            }

            bool wantsFire = Input.GetMouseButton(0) || isFire || (ui != null && ui.IsAttackingWithJoystick);

            if (wantsFire)
            {
                player.Fire();
            }
        }
    }

    void UpdateRotation()
    {
        if (!enableRotate)
            return;

        if (ui != null && ui.IsAttackingWithJoystick)
        {
            Vector2 attackDir = ui.AttackDirection;
            Vector3 aimDir = new Vector3(attackDir.x, 0, attackDir.y).normalized;
            if (aimDir.sqrMagnitude > 0.001f)
            {
                transform.forward = aimDir;
            }
        }
        else if (Camera.main != null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 worldPos = ray.GetPoint(enter);
                Vector3 dir = worldPos - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.001f)
                {
                    transform.forward = dir.normalized;
                }
            }
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
