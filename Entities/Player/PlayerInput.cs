using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour {

    PlayerController player;

    NavGraphPlatformer graph;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        graph = GameObject.FindGameObjectWithTag("Graph").GetComponent<NavGraphPlatformer>();
        
    }

    private void Update()
    {

        if (player.isDead)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                player.ResetGame();
            }
            return;
        }

        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        if (Input.GetKeyUp(KeyCode.K))
        {
            player.MeleeAttack();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.OnJumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            player.OnJumpInputUp();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            player.OnSprintInputHold();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            player.OnSprintInputUp();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            player.OnDodgeInputDown(false);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            player.OnDodgeInputDown(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            player.ResetPosition();
            
        }

        if (Input.GetKeyDown(KeyCode.Insert))
        {
            player.SavePosition();
        }

        if (Input.GetMouseButtonDown(0))
        {
            player.MeleeAttack();
        }
    }
}
