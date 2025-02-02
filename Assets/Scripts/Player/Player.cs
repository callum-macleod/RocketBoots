using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using static UnityEngine.UI.Image;

public class Player : MonoBehaviour
{
    // movement stuff
    float acceleration = 55; // force multiplier to acceleration force
    float deceleration = 15; // force multiplier to deceleration force
    int maxVelocity = 8; // when speed exceeds this value, set movespeed to this value isntead.
    int jumpForce = 6; // force of the jump
    public Vector3 Move { get; private set; }
    bool inAir = true;


    // jet stats
    bool useJets = false;
    bool bufferJets = false;
    float defaultJetForce = 750;
    float maxVerticalVel = 10f;

    float fuelRegenRate = 12.5f;
    float fuelUsageRate = 30f;
    float chargedFuelUsageRate = 50f;
    float maxFuel = 100f;
    float currentFuel;
    float CurrentFuel
    {
        get { return currentFuel; }
        set
        {
            currentFuel = value;
            float newYScale = CurrentFuelAsPercent * fuelMeterOriginalHeight;
            fuelMeter.localScale = new Vector3(fuelMeter.localScale.x, newYScale, fuelMeter.localScale.z);

            float scaleDifference = (fuelMeterOriginalHeight - newYScale);
            float newYPos = fuelMeterOriginalYPos - scaleDifference;
            fuelMeter.localPosition = new Vector3(fuelMeter.localPosition.x, newYPos, fuelMeter.localPosition.z);
        }
    }

    float CurrentFuelAsPercent { get { return CurrentFuel / maxFuel; } }

    float chargeTime = 0f;
    bool releaseCharge = false;
    float chargedJetScale = 0.001f;

    // maximum time that you can charge for
    // i.e. (maxFuel / fuelUsageRate) / 3; means that you can charge up 1 third of your max charge
    float maxChargeTime { get { return (maxFuel / chargedFuelUsageRate) / 3; } }

    Vector3 feetPoint;

    [SerializeField] Transform fuelMeter;
    float fuelMeterOriginalHeight;
    float fuelMeterOriginalYPos;

    // self references
    Rigidbody rigidBody;

    [SerializeField] Transform rotator;
    [SerializeField] GameObject fireFeetPrefab;


    // Start is called before the first frame update
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        fuelMeterOriginalHeight = fuelMeter.transform.localScale.y;
        fuelMeterOriginalYPos = fuelMeter.transform.localPosition.y;
        CurrentFuel = 100f;
        feetPoint = new Vector3(0, GetComponent<CapsuleCollider>().bounds.min.y, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // get directional inputs
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");

        // get inputted movement relative to the rotator/camera rotation
        Move = (Utils.RemoveY(rotator.forward) * zMov + rotator.right * xMov).normalized * acceleration;

        CalculateMovement();

        // if falling, fall with increased (2x) gravity
        if (rigidBody.velocity.y <= 0)
            rigidBody.AddForce(Physics.gravity, ForceMode.Acceleration);

        if (useJets)
        {
            if (CurrentFuel <= 0)
            {
                useJets = false;
                return;
            }

            // help counter downward velocity if falling
            if (rigidBody.velocity.y < 0)
                rigidBody.velocity = Utils.RemoveY(rigidBody.velocity) + (rigidBody.velocity.y * 0.8f * Vector3.up);

            // do jets
            if (rigidBody.velocity.y < maxVerticalVel)
                rigidBody.AddForce(defaultJetForce * Time.deltaTime * (Move.normalized + Vector3.up), ForceMode.Force);

            CurrentFuel -= fuelUsageRate * Time.deltaTime;
            Instantiate(fireFeetPrefab, transform.position - feetPoint, Quaternion.identity);
        }
        else if (CurrentFuel < maxFuel && chargeTime <= 0f)
        {
            // regen fuel
            rigidBody.useGravity = true;
            float regen = fuelRegenRate * Time.deltaTime;
            if (!inAir) regen *= 2;

            CurrentFuel += regen;
        }

        if (releaseCharge)
        {
            if (rigidBody.velocity.y < 0)
                rigidBody.velocity = Utils.RemoveY(rigidBody.velocity);

            float charge = chargeTime * chargedFuelUsageRate;
            rigidBody.AddForce(charge * defaultJetForce * chargedJetScale * Vector3.up, ForceMode.Impulse);

            GameObject fire = Instantiate(fireFeetPrefab, transform.position - feetPoint, Quaternion.identity);
            Destroy(fire, 4);

            chargeTime = 0;
            releaseCharge = false;
        }     
    }

    void CalculateMovement()
    {
        // redistribute velocity
        Vector3 nonVerticalVelocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);  // get velocity without y component

        // if the user is trying to move and current velocity > maximum velocity:
        // prevent them from speeding up, but allow them to direct and counteract their currently high velocity
        if (Move != Vector3.zero && nonVerticalVelocity.magnitude > maxVelocity)
        {
            float A = Vector3.SignedAngle(nonVerticalVelocity * -1, Move, new Vector3(1, 0, 1));
            float aRadian = Mathf.Abs(A / 180);

            // reduce velocity in current direction
            rigidBody.AddForce(nonVerticalVelocity.normalized * (-1 * Move.magnitude * aRadian));
        }

        // apply new movement input
        rigidBody.AddForce(Move);


        // decelleration (if not inputting movement)
        if (Move.magnitude < 0.1f)
            rigidBody.AddForce(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).normalized * deceleration);
    }

    private void Update()
    {
        // jump
        if (Input.GetKeyDown(KeyCode.Space) && !inAir)
            rigidBody.AddForce(jumpForce * rigidBody.mass * Vector3.up, ForceMode.Impulse);

        // determine if use jets
        if (Input.GetKeyDown(KeyCode.Space) || bufferJets)
        {
            // disable gravity while using jet
            if (CurrentFuelAsPercent > 0.1f)
            {
                rigidBody.useGravity = false;
                useJets = true;
                bufferJets = false;
            }
        }

        // re-enable gravity when you stop using jet
        if (Input.GetKeyUp(KeyCode.Space))
        {
            useJets = false;
            rigidBody.useGravity = true;
        }

        // buffer jet usage
        if (!useJets && Input.GetKey(KeyCode.Space) && CurrentFuelAsPercent > 0.07f)
            bufferJets = true;

        // charge right click
        if (Input.GetKey(KeyCode.Mouse1) && CurrentFuel > 0 && chargeTime < maxChargeTime)
        {
            chargeTime += Time.deltaTime;
            CurrentFuel -= chargedFuelUsageRate * Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
            releaseCharge = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;

        if (collision.collider.gameObject.layer == (int)Layers.SolidGround)
            inAir = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.layer == (int)Layers.SolidGround)
            inAir = true;
    }
}
