using UnityEngine;

[System.Serializable]
public struct SerializableQuaternion
{
    public float x, y, z, w;
    
    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }
    
    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
    
    public static implicit operator Quaternion(SerializableQuaternion sQuaternion)
    {
        return new Quaternion(sQuaternion.x, sQuaternion.y, sQuaternion.z, sQuaternion.w);
    }
    
    public static implicit operator SerializableQuaternion(Quaternion quaternion)
    {
        return new SerializableQuaternion(quaternion);
    }
}