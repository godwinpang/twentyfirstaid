using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class animationStateController : MonoBehaviour
{
    Animator animator; 
    System.Random rand;

    public AudioSource audioSource;
    public AudioClip introductionClip;
    public AudioClip slowDieClip;
    public AudioClip cprClip;
    public AudioClip huhClip;
    
    public GameObject rightHand;
    public GameObject chestCube;
    public Collider chestCollider;

    public GameObject canvas;
    public Text score;
    public Text grade;

    // Pause times (seconds) 
    public float ymcaTime = 23.0f; 
    public float slowDiePauseTime = 18.0f; 
    public float cprTime = 30.0f; 
    public float cprInstrTime = 20.0f;
    public float cprDuration = 10.0f;
    public float volume=0.9f; // Clip volume

    public bool inChest = false;
    public bool isOver = false;
    public int totalScore = 0;
    public int numPresses = 0;
    public LayerMask chestLayerMask; // Set to everything
    public int framesInBetweenCPRPresses = 1;
    public float MinPressTime = 0.45f;
    public float MaxPressTime = 0.55f;
    public int MaxPressScore = 100;
    // penalty = deviation amount / DeviationUnit * DeviationPenalty * MaxPressScore
    public float DeviationPenalty = 0.1f;
    public float DeviationUnit= 0.1f;
    public bool played_slow_die = false;
    public bool played_cpr_instr = false;
    private bool patient_dead = false;

    public Text pumpText;
    public string PUMP = "PUMP";
    public string NO_PUMP = "";

    public float pumpTimer = 0.0f;
    // we want pump every 0.6 seconds, so we should flip between pump and empty text 
    // "PUMP" && pumpTimer > pumpT => ""
    // "" && pumpTimer > letGo => "PUMP"
    public float pumpThreshold = 0.4f;
    public float letGoThreshold = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>(); 
        animator.SetBool("havingHeartAttack", false);
        rand = new System.Random();
        chestCollider = chestCube.GetComponent<Collider>();
        audioSource.PlayOneShot(introductionClip, volume);
        ymcaTime = 23.0f;
        slowDiePauseTime = 18.0f;
        MinPressTime = 0.45f;
        MaxPressTime = 0.55f;
        framesInBetweenCPRPresses = 1;
        pumpText.text = NO_PUMP;
    }

    // Update is called once per frame
    void Update()
    {  
        if (cprTime <= 0.0f) {
            return;
        }

        
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Ymca Dance")) {
            // Keep doing YMCA for a bit
            if (ymcaTime > 0.0f)
            {
                ymcaTime -= Time.deltaTime;
                return;
            } 
            bool isHavingHeartAttack = animator.GetBool("havingHeartAttack");
            if (!isHavingHeartAttack) {
                if (rand.NextDouble() >= 0.995f) {
                    Debug.Log("give guy heart attack");
                    animator.SetBool("havingHeartAttack", true);
                    animator.SetFloat("dyingSpeed", (float)(rand.NextDouble()));
                    return;
                } 
                return;
                
            }
        }
        

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Mutant Slow Dying-0")) {
            // Play "Oh no! Get asprin!  "
            if (!played_slow_die) {
                audioSource.PlayOneShot(slowDieClip, volume);
                played_slow_die = true;
            }
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Mutant Slow Dying-1")) {
            // Pause the animation for length of audio
            
            //Debug.Log("slowDiePauseTime " + slowDiePauseTime);
            if (slowDiePauseTime > 0)
            {
                animator.speed = 0; 
                slowDiePauseTime -= Time.deltaTime;
                return;
            } 
            animator.speed = 0.5f;
            return;
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Mutant Slow Dying-2")) {
            animator.speed = 0.7f;
            return;
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Mutant Dead")) {
            // Play CPR instructions
            if(!played_cpr_instr) {
                animator.speed = 0.5f;
                audioSource.PlayOneShot(cprClip, volume);
                played_cpr_instr = true;
                return;
            }
            //Debug.Log("cprInstrTime " + cprInstrTime);
            if (cprInstrTime > 0)
            {
                animator.speed = 0; 
                cprInstrTime -= Time.deltaTime;
                return;
            } 
            patient_dead = true;
        }

        if (!patient_dead) {
            return; 
        }

        // If performing CPR
        cprTime -= Time.deltaTime;
        if (cprTime <= 0.0f)
        {
            // show stuff
            string avg_score = getAverageScore(totalScore, numPresses);
            score.text = "Score: " + avg_score;
            grade.text = "Grade: " + getGrade(int.Parse(avg_score));
            return;
        }

        Debug.Log("pumping");

        pumpTimer += Time.deltaTime;
        if (pumpText.text == PUMP && pumpTimer >= pumpThreshold)
        {
            pumpText.text = NO_PUMP;
            pumpTimer = 0;
        }
        else if (pumpText.text == NO_PUMP && pumpTimer >= letGoThreshold)
        {
            pumpText.text = PUMP;
            pumpTimer = 0;
        }

        Collider[] overlaps = Physics.OverlapSphere(rightHand.transform.position, 0.1f);
        Debug.Log(overlaps.Length);
        
        if (!inChest && overlaps.Length >= 2)
        {
            audioSource.PlayOneShot(huhClip, volume);
            framesInBetweenCPRPresses++;
            inChest = true;
        }
        else if (inChest && overlaps.Length < 2)
        {
            // leaving chest
            float timeBetweenPress = framesInBetweenCPRPresses * Time.deltaTime;
            Debug.Log("timeBetweenPress: " + timeBetweenPress);

            int pressScore = getPressScore(timeBetweenPress);
            Debug.Log("pressScore: " +pressScore);
            totalScore += pressScore;
            framesInBetweenCPRPresses = 0;
            numPresses++;
            Debug.Log("numPresses: " + numPresses);
            inChest = false;
        }
        else if (numPresses > 0) 
        {
            // there's no CPR press
            framesInBetweenCPRPresses++;
        }

        // game is over
    }


    // Should press 100-120 a minute
    // That's 0.5-0.6 seconds in between each press
    // Score is calculated out of 100.
    // each 0.1s deviation is punished by 10 points
    int getPressScore(float secondsSincePress) {
        float deviation = 0;

        if (secondsSincePress >= MinPressTime && secondsSincePress <= MaxPressTime)
        {
            return MaxPressScore;
        }
        else if (secondsSincePress >= MaxPressTime)
        {
            deviation = secondsSincePress - MaxPressTime;
        } else {
            deviation = MinPressTime - secondsSincePress;
        }

        int penalty = (int)(deviation / DeviationUnit * DeviationPenalty * MaxPressScore);
        int score = MaxPressScore - penalty;

        if (score < 0) {
            return 0;
        }
        return score;
    } 

    string getAverageScore(int totalScore, int numPresses)
    {
        if (numPresses == 0) {
            return "0";
        }
        return (totalScore / numPresses).ToString();
    }

    string getGrade(int score)
    {
        if (score > 80) {
            return "A";
        } else if (score > 65) {
            return "B";
        } else if (score > 50) {
            return "C";
        } else {
            return "F";
        }
    }
}