using UnityEngine;

public class Transform
{
    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;

    // Constructor
    public CustomTransform()
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        scale = Vector3.one;
    }

    // Position Getters and Setters
    public Vector3 GetPosition()
    {
        return position;
    }

    public void SetPosition(Vector3 newPosition)
    {
        position = newPosition;
    }

    // Rotation Getters and Setters
    public Quaternion GetRotation()
    {
        return rotation;
    }

    public void SetRotation(Quaternion newRotation)
    {
        rotation = newRotation;
    }

    // Scale Getters and Setters
    public Vector3 GetScale()
    {
        return scale;
    }

    public void SetScale(Vector3 newScale)
    {
        scale = newScale;
    }

    // Translate (Move by a delta)
    public void Translate(Vector3 translation)
    {
        position += translation;
    }

    // Rotate by a delta Quaternion
    public void Rotate(Quaternion deltaRotation)
    {
        rotation = deltaRotation * rotation;
    }

    // Rotate by Euler angles (e.g., Vector3(0, 90, 0))
    public void Rotate(Vector3 eulerAngles)
    {
        rotation = Quaternion.Euler(eulerAngles) * rotation;
    }

    // Scaling by a factor
    public void Scale(Vector3 scaleFactor)
    {
        scale = Vector3.Scale(scale, scaleFactor);
    }
}
