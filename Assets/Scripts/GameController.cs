using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public LiquidBottle2D firstBottle;
    public LiquidBottle2D secondBottle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition2D = new Vector2(mousePos.x, mousePos.y);

            Debug.DrawLine(mousePosition2D, mousePosition2D + Vector2.up * 0.1f, Color.red, 2.0f);

            // Increase the raycast range and make sure it starts at a Z position that hits the 2D colliders
            RaycastHit2D hit = Physics2D.Raycast(mousePosition2D, Vector2.zero, Mathf.Infinity);

            Debug.Log("Mouse position 2d " + mousePosition2D);

            if(hit.collider != null)
            {
                Debug.Log("Hit collider" + hit.collider.gameObject.name);

                if (hit.collider.GetComponent<LiquidBottle2D>() != null) 
                { 
                    if(firstBottle ==  null)
                    {
                        firstBottle = hit.collider.GetComponent<LiquidBottle2D>();
                    }
                    else
                    {
                        if(firstBottle == hit.collider.GetComponent<LiquidBottle2D>())
                        {
                            firstBottle = null;
                        }
                        else
                        {
                            secondBottle = hit.collider.GetComponent<LiquidBottle2D>();
                            firstBottle._SecondBottleRef = secondBottle;
                            firstBottle.UpdateTopColorValues();
                            secondBottle.UpdateTopColorValues();

                            if(secondBottle.FillBottleColorCheck(firstBottle.topColor) == true)
                            {
                                firstBottle.StartColorTransfer();
                                firstBottle = null;
                                secondBottle = null;
                            }
                            else
                            {
                                firstBottle = null;
                                secondBottle = null;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Raycast did not hit any collider.");
            }

        }
        
    }
}
