// © 2023 Team Kittens
// -------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartExplosion : MonoBehaviour
{

    public Explosion typeOfExplosion; // Takes in explosion scriptable object(sprites, damage type, etc.) data
    public GameObject newExplosion; // Takes in explosion prefab(sound, collider) data
    private BreakableObject breakableObject; // Object that is broken to cause the explosion
    private ExplosionManager explosionManager; // Contains explosions and deactivates them after they finish
    private int currentSpriteIndex = 0; // Denotes the current index in a sprite sequence for our custom animation system
    private bool explosionStarted; // Has the explosion started yet?
    private Collider2D[] affectedObjects; // Contains which objects are affected by the explosion
    private static ContactFilter2D contactFilter; // Filters affected objects
    private SpriteRenderer spriteRenderer; // Renders sprite
    private BoxCollider2D boxCollider2D; // Explosive barrel collider


    private void OnDrawGizmosSelected() // Allows us to see the explosion's radius in editor
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, typeOfExplosion.explosionRadius);
    }

    // -- USE THIS TO DELAY OBJECTS BREAKING UNTIL EXPLOSION ANIMATION -- 
    IEnumerator WaitForExplosion()  
    {
        yield return new WaitForSeconds(0.25f);
        int collisions = Physics2D.OverlapCircle(transform.position, typeOfExplosion.explosionRadius, new ContactFilter2D().NoFilter(), affectedObjects);
    
        for (int i = 0; i < (affectedObjects.Length - 1); i++)
        {
            if (affectedObjects[i]) // Makes all affected objects take damage according to their own classes
            {
                if (affectedObjects[i].gameObject.tag == "Enemy" && typeOfExplosion.damagesEnemy)
                {
                    affectedObjects[i].gameObject.GetComponent<Enemy>().TakeDamage(typeOfExplosion.amountOfDamage, transform);
                }

                if (affectedObjects[i].gameObject.layer == 8 && typeOfExplosion.damagesPlayer)
                {
                    affectedObjects[i].gameObject.GetComponent<PlayerCharacterController>().TakeDamage((int)(typeOfExplosion.amountOfDamage * 0.5f));
                }

                if ((affectedObjects[i].gameObject.GetComponent<BreakableObject>() != null) && typeOfExplosion.damagesEnvironment)
                {
                    affectedObjects[i].gameObject.GetComponent<BreakableObject>().TakeDamage(typeOfExplosion.amountOfDamage);
                }

            } 
        }
        yield break;
    }

    // Animates the explosion until it runs out of sprites.
    IEnumerator ChangeExplosionSprite()
    {
        while(true) 
        {
            if (currentSpriteIndex >= (typeOfExplosion.explosionSprites.Length)) // If current sprite index is greater than the max index of sprites
            {
                newExplosion.SetActive(false); // Set the explosion to be false
                this.gameObject.SetActive(false); // Additionally set the barrel to be false
                break;
            }
            else {
                yield return new WaitForSeconds(typeOfExplosion.animationSpeed); // Wait a set time before continuing to the next sprite
                newExplosion.GetComponent<SpriteRenderer>().sprite = typeOfExplosion.explosionSprites[currentSpriteIndex];
                currentSpriteIndex = currentSpriteIndex + 1;
            }
        }
        yield break;
    }

    public GameObject CreateExplosion() 
    {
        // Disable barrel sprite and collider upon explosion starting
        spriteRenderer.enabled = !spriteRenderer.enabled;
        boxCollider2D.enabled = !boxCollider2D.enabled;

        if (explosionStarted == false)
        {
            explosionStarted = true;
            if (breakableObject.GetHealth() <= 0 && (breakableObject.explosionStarted == true)) // Verifies the explosion should be playing
            {

                newExplosion = Instantiate(newExplosion, new Vector3(transform.position.x + typeOfExplosion.spawnOffset.x, transform.position.y + typeOfExplosion.spawnOffset.y, transform.position.z), Quaternion.identity);
                newExplosion.transform.parent = explosionManager.transform; // Creates the explosion

                // Sets explosion to first sprite and adds trail effect
                newExplosion.AddComponent<SpriteRenderer>();
                newExplosion.GetComponent<SpriteRenderer>().sprite = typeOfExplosion.explosionSprites[0];
                newExplosion.AddComponent<TrailEffect>();
                newExplosion.GetComponent<TrailEffect>().trailOpacity = 0.5f;

                // Starts the explosion
                StartCoroutine("ChangeExplosionSprite");
                SFXManager.instance.PlaySound(typeOfExplosion.explosionSound, 1.0f);
                StartCoroutine("WaitForExplosion");
                
            }
        }  
        return newExplosion;
    }

    void Start()
    {
        // Plays when the game begins, initializes variables to defaults.
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        boxCollider2D = this.GetComponent<BoxCollider2D>();
        explosionStarted = false;
        affectedObjects = new Collider2D[100];
        breakableObject = this.GetComponent<BreakableObject>();
        explosionManager = GameObject.Find("ExplosionManager").GetComponent<ExplosionManager>();
    }
}