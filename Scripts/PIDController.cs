public class PIDController
{
    float kP, kI, kD;
    float P, I, D;
    float previousError;

    public PIDController(float p, float i, float d)
    {
        kP = p;
        kI = i;
        kD = d;
    }

    public float GetOutput(float currentError, float deltaTime)
    {
        P = currentError;
        I += currentError * deltaTime;
        D = (P - previousError) / deltaTime;
        previousError = currentError;
        return kP * P + kI * I + kD * D;
    }
}
