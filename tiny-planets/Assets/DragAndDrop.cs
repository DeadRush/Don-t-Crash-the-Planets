using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    bool moveAllowed;
    Collider2D col;
    public GameObject restartPanel;
    public GameObject deathEffect;
    public GameObject hideGame;
    private GameMaster gm;

    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        col = GetComponent<Collider2D>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

            if(touch.phase == TouchPhase.Began)
            {
                Collider2D touchedCollider = Physics2D.OverlapPoint(touchPosition);
                
                if(col == touchedCollider) 
                {
                    
                    moveAllowed = true;
                }
            }
            if (touch.phase == TouchPhase.Moved)
            {
                if (moveAllowed)
                {
                    transform.position = new Vector2(touchPosition.x, touchPosition.y);
                }
            }
            if (touch.phase == TouchPhase.Ended)
            {
                moveAllowed = false;
            }

        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Planet")
        {
            var particle=Instantiate(deathEffect, transform.position, Quaternion.identity);
            var particleSystem=particle.GetComponent<ParticleSystem>();
            //ParticleSystem.EmitParams emitOverride = new ParticleSystem.EmitParams();
            //emitOverride.startLifetime = 10f;
            //particleSystem.Emit(emitOverride, 20);
            particleSystem.Play();
            restartPanel.SetActive(true);
            hideGame.SetActive(false);
            gm.GameOver();
            Destroy(collision.gameObject);
        }
    }

}
