using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    #region Public Variables
    public LiquidBottle2D firstBottle;
    public LiquidBottle2D secondBottle;
    public List<LiquidBottle2D> bottles;
    #endregion

    #region MonoBehaviour Methods

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

            if(hit.collider != null)
            {
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

    #endregion

    #region Private Variables

    private IEnumerator CheckForGameOver()
    {
        // Wait until the transfer animation is complete
        yield return new WaitForSeconds(firstBottle.RotationTime * 2); 

        if (AreAllBottlesSorted())
        {
            Debug.LogError("Game Over: All bottles are sorted!");

            StartCoroutine(HandleGameOver());
        }
        else
        {
            Debug.Log("Game is not over");
        }
    }

    private bool AreAllBottlesSorted()
    {
        int filledBottlesCount = 0;
        int emptyBottlesCount = 0;

        foreach (LiquidBottle2D bottle in bottles)
        {
            if (bottle.IsEmpty())
            {
                emptyBottlesCount++;
            }
           else if (bottle.IsGameOver())
            {
                filledBottlesCount++;
            }
            Debug.Log("FilledBottlesCount" + filledBottlesCount);
            Debug.Log("EmptyBottlesCount " + emptyBottlesCount);
        }
        // Check if the filled Bottle condition met
        return filledBottlesCount == 6;
    }

    private IEnumerator HandleGameOver() 
    {
        // Handle the game-over logic here
        Debug.Log("Handling game over!");

        yield return new WaitForSeconds(2f);

        if (SceneManager.GetActiveScene().name == "Level7")
        {
            Debug.Log("Load scene 8");
            SceneManager.LoadScene("Level8");
        }

        else if(SceneManager.GetActiveScene().name == "Level8")
        {
            SceneManager.LoadScene("SampleScene");
        }

    }
    #endregion
}
