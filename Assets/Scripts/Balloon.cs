using System.Collections;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public float speed = 1f;
    // upper y limit
    public float upperLimit = 6f;
    public ParticleSystem popEffect;
    public ParticleSystem batEffect;
    public ParticleSystem bombEffect;
    public ParticleSystem exEffect;
    public ParticleSystem bonusEffect;
    public GameObject audioSource;
    public float speedIncrement = 0.1f;
    private float baseSpeed;

    public enum BalloonType 
    { 
        Normal, 
        Bonus, 
        Bomb, 
        Ex, 
        Bat 
    }

    public BalloonType balloonType;
    public int scoreValue = 1;

    // Add MovementPattern enum
    public enum MovementPattern 
    { 
        Straight, 
        Diagonal, 
        Zigzag 
    }
    private MovementPattern movementPattern;

    // Variables for zigzag movement
    public float zigzagAmplitude = 1f;  // Amplitude of the zigzag (horizontal movement)
    public float zigzagFrequency = 1f;  // Frequency of the zigzag

    private Vector3 diagonalDirection;

    void OnEnable()
    {
        Camera mainCamera = Camera.main;
        Vector3 topCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, mainCamera.nearClipPlane));
        upperLimit = topCenter.y + 1f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        int randomValue = Random.Range(0, 100);
        if (randomValue < 5)
        {
            balloonType = BalloonType.Bat;
            scoreValue = 2;
            sr.color = Color.gray;
        }
        else if (randomValue >= 5 && randomValue < 10)
        {
            balloonType = BalloonType.Bomb;
            sr.color = Color.black;
        }
        else if (randomValue >= 10 && randomValue < 20)
        {
            balloonType = BalloonType.Ex;
            sr.color = new Color(1f, 0.5f, 0.5f);
        }
        else if (randomValue >= 20 && randomValue < 30)
        {
            balloonType = BalloonType.Bonus;
            sr.color = Color.yellow;
        }
        else
        {
            balloonType = BalloonType.Normal;
            sr.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
        }

        // randomize movement pattern
        int movementRandom = Random.Range(0, 100);
        if (movementRandom < 50) // 50% chance for straight
        {
            movementPattern = MovementPattern.Straight;
        }
        else if (movementRandom >= 50 && movementRandom < 75) // 25% chance for diagonal
        {
            movementPattern = MovementPattern.Diagonal;

            // decide the diagonal direction (left or right)
            float directionX = Random.Range(0, 2) == 0 ? -1f : 1f;
            diagonalDirection = new Vector3(directionX, 1f, 0f).normalized;
        }
        else
        {
            movementPattern = MovementPattern.Zigzag;

            zigzagAmplitude = Random.Range(0.5f, 1.5f);
            zigzagFrequency = Random.Range(1f, 3f);
        }
    }

    void Start()
    {
        baseSpeed = speed;
    }

    void Update()
    {
        speed = baseSpeed + GameManager.Instance.score * speedIncrement;

        switch (movementPattern)
        {
            case MovementPattern.Straight:
                transform.Translate(Vector3.up * speed * Time.deltaTime);
                break;

            case MovementPattern.Diagonal:
                transform.Translate(diagonalDirection * speed * Time.deltaTime);
                break;

            case MovementPattern.Zigzag:
                float x = zigzagAmplitude * Mathf.Sin(zigzagFrequency * Time.time);
                Vector3 zigzagMovement = new Vector3(x, 1f, 0f).normalized;
                transform.Translate(zigzagMovement * speed * Time.deltaTime);
                break;
        }

        if (transform.position.y >= upperLimit)
        {
            gameObject.SetActive(false);
            // TODO: to think of
            // if (balloonType != BalloonType.Bomb) // don't penalize for bombs
            // {
            GameManager.Instance.ReduceLives(1);
            // }
        }
    }

    void OnMouseDown()
    {
        if (UIManager.Instance.isPaused)
        {
            return;
        }
        // StartCoroutine(PopSound());
        Pop();
    }

    //TODO: doesnt work.
    // IEnumerator PopSound()
    // {
    //     yield return new WaitForSeconds(0);
    //     audioSource.SetActive(true);
    // }

    public void Pop()
    {
        switch (balloonType)
        {
            case BalloonType.Bat:
                if (batEffect != null)
                {
                    ParticleSystem effect = Instantiate(batEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration);
                }
                Invoke(nameof(PopBalloonsAboveAfterEFXDelay), 0.8f);
                break;

            case BalloonType.Bomb:
                if (bombEffect != null)
                {
                    ParticleSystem effect = Instantiate(bombEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration);
                }
                GameManager.Instance.GameOver();
                break;

            case BalloonType.Ex:
                if (exEffect != null)
                {
                    ParticleSystem effect = Instantiate(exEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration);
                }
                GameManager.Instance.ReduceLives(1);
                break;

            case BalloonType.Bonus:
                if (bonusEffect != null)
                {
                    ParticleSystem effect = Instantiate(bonusEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration);
                }
                GameManager.Instance.AddLife(1);
                break;

            case BalloonType.Normal:
            default:
                if (popEffect != null)
                {
                    ParticleSystem effect = Instantiate(popEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration);
                }
                GameManager.Instance.AddScore(scoreValue);
                break;
        }

        gameObject.SetActive(false);
    }

    private void PopBalloonsAboveAfterEFXDelay()
    {
        int numberOfBalloonsAbove = BalloonManager.Instance.PopBalloonsAbove(transform.position.y);
        GameManager.Instance.AddScore(numberOfBalloonsAbove);
    }
}