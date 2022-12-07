using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 5.0f;
    
    public int maxHealth = 5;

    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioSource backgroundManager;
    public AudioClip tickingSound;

    public float timeInvincible = 2.0f;

    public int health { get { return currentHealth; }}
    int currentHealth;

    public float timeBoosting = 4.0f;
    float speedBoostTimer;
    bool isBoosting;

    public GameObject projectilePrefab;
    public int ammo { get { return currentAmmo; }}
    public int currentAmmo;

    public TextMeshProUGUI ammoText;
    
    bool isInvincible;
    float invincibleTimer;
    
    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;
    
    public ParticleSystem hitEffect;
    public ParticleSystem healthPickup;

    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);

    //Timer

    [SerializeField] private TextMeshProUGUI timerUI;
    [SerializeField] private float mainTimer;

    private float timer;
    private bool canCount = false;
    private bool doOnce = false;
    private bool hasPressedKey = false;
    private bool hasMoved = false;
    public GameObject TimerObject;


    // Fixed Robots
    public TextMeshProUGUI fixedText;
    private int scoreFixed = 0;
    
    //Win Text and Lose Text
    public GameObject WinTextObject;
    public GameObject LoseTextObject;
    bool gameOver;
    bool gameWin;

    public static int level = 1;

    AudioSource audioSource;

    
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        audioSource= GetComponent<AudioSource>();

        fixedText.text = "Fixed Robots: " + scoreFixed.ToString() + "/4";

        WinTextObject.SetActive(false);
        LoseTextObject.SetActive(false);
        gameOver = false;
        gameWin = false;

        timer = mainTimer;
        TimerObject.SetActive(false);

    }


    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        
        Vector2 move = new Vector2(horizontal, vertical);
        
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();

            hasMoved = true;
        }
        
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (isBoosting == true)
        {
            speedBoostTimer -= Time.deltaTime;
            speed = 8;

            if (speedBoostTimer < 0)
            {
                isBoosting = false;
                speed = 5;
            }
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            Launch();

            if (currentAmmo > 0)
            {
                ChangeAmmo(-1);
                AmmoText();
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    if (scoreFixed>= 4)
                    {
                        SceneManager.LoadScene("Level 2");
                        level = 2;
                    }

                    else
                    {
                        character.DisplayDialog();
                    }
                }  
            }
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            if (gameOver == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
            }

            if (gameWin == true)
            {
                SceneManager.LoadScene("Level 1");
                level = 1;
            }
        }

        if (hasMoved == false)
        {
            if (hasPressedKey == false)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    timer = mainTimer;
                    canCount = true;
                    doOnce = false;
                    TimerObject.SetActive(true);

                    PlaySound(tickingSound);

                    hasPressedKey = true;
                }
            }
        }

        if (timer >= 0.0f && canCount)
        {
            timer -= Time.deltaTime;
            timerUI.text = "Time: " + timer.ToString("F");
        }

        else if (timer <= 0.0f && !doOnce)
        {
            canCount = false;
            doOnce = true;
            timerUI.text = "Time: " + timer.ToString("0.00");

            LoseTextObject.SetActive(true);

            transform.position = new Vector3(0f, 0f, 0f);
            speed = 0;
            Destroy(gameObject.GetComponent<SpriteRenderer>());

            gameOver = true;

            
        }
    }
    
    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            animator.SetTrigger("Hit");
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;

            PlaySound(hitSound);
            
        ParticleSystem Damage = Instantiate(hitEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }

        if (amount > 0)
        {
            ParticleSystem Heal = Instantiate(healthPickup, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }
        
        if (currentHealth <= 0)
        {
            LoseTextObject.SetActive(true);
            transform.position = new Vector3(0f, 0f, 0f);
            speed = 0;
            Destroy(gameObject.GetComponent<SpriteRenderer>());

            gameOver = true;

            canCount = false;
            doOnce = true;

            

            



        }
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }
    
    public void ChangeAmmo(int amount)
    {
        currentAmmo = Mathf.Abs(currentAmmo + amount);
    }

    public void AmmoText()
    {
        ammoText.text = "Ammo: " + currentAmmo.ToString();
    }

    public void FixedRobots(int amount)
    {
        scoreFixed += amount;
        fixedText.text = "Fixed Robots: " + scoreFixed.ToString() + "/4";

        Debug.Log("Fixed Robots: " + scoreFixed);

        // Talk to Jambi to visit stage 2
        if (scoreFixed == 4 && level == 1)
        {
            WinTextObject.SetActive(true);

            canCount = false;
            doOnce = true;

        
        }

        if (scoreFixed == 4 && level == 2)
        {
            WinTextObject.SetActive(true);

            gameWin = true;

            transform.position = new Vector3(0f, 0f, -10f);
            speed = 0;
            Destroy(gameObject.GetComponent<SpriteRenderer>());

            canCount = false;
            doOnce = true;

            

            
        }

    }

    public void SpeedBoost(int amount)
    {
        if (amount > 0)
        {
            speedBoostTimer = timeBoosting;
            isBoosting = true;
        }
    }
    void Launch()
    {
        if (currentAmmo > 0)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(lookDirection, 300);

            animator.SetTrigger("Launch");

            PlaySound(throwSound);
        }
    }
}