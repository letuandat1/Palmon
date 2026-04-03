using UnityEngine;

[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;
    
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
    
    public static implicit operator Vector3(SerializableVector3 sVector)
    {
        return new Vector3(sVector.x, sVector.y, sVector.z);
    }
    
    public static implicit operator SerializableVector3(Vector3 vector)
    {
        return new SerializableVector3(vector);
    }
}