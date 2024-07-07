using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public LiquidBottle2D firstBottle;
    public LiquidBottle2D secondBottle;

    public List<LiquidBottle2D> bottles;
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
                                StartCoroutine(CheckForGameOver()); // Check for game over after the color transfer
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

    private IEnumerator CheckForGameOver()
    {
        // Wait until the transfer animation is complete
        yield return new WaitForSeconds(firstBottle.RotationTime * 2); // Adjust this if needed

        if (AreAllBottlesSorted())
        {
            Debug.Log("Game Over: All bottles are sorted!");
            // You can trigger your game over UI or any other logic here
        }
        else
        {
            Debug.Log("Game is not over");
        }
    }

    private bool AreAllBottlesSorted()
    {
        foreach (LiquidBottle2D bottle in bottles)
        {
            if (!IsBottleSorted(bottle))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsBottleSorted(LiquidBottle2D bottle)
    {
        // A bottle is sorted if all its colors are the same and it has 4 colors
        if (bottle.numberOfColorsInBottle != 4)
        {
            return false;
        }

        Color firstColor = bottle.liquidColors[0];
        for (int i = 1; i < bottle.numberOfColorsInBottle; i++)
        {
            if (bottle.liquidColors[i] != firstColor)
            {
                return false;
            }
        }
        return true;
    }
}
