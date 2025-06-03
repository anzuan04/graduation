// ============================================================================
// VirtualBullet.cs - 가상 총알 시뮬레이션 (오브젝트 없음)
// ============================================================================
using UnityEngine;
using System.Collections;

[System.Serializable]
public class VirtualBullet
{
    public Vector2 startPosition;
    public Vector2 currentPosition;
    public Vector2 direction;
    public float speed;
    public float damage;
    public float maxRange;
    public BaseCharacter shooter;
    public float travelDistance;

    public VirtualBullet(Vector2 start, Vector2 dir, float spd, float dmg, float range, BaseCharacter sh)
    {
        startPosition = start;
        currentPosition = start;
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        maxRange = range;
        shooter = sh;
        travelDistance = 0f;
    }

    public bool UpdatePosition(float deltaTime)
    {
        Vector2 previousPosition = currentPosition;
        Vector2 movement = direction * speed * deltaTime;
        currentPosition += movement;
        travelDistance += movement.magnitude;

        return travelDistance < maxRange;
    }

    public Vector2 GetPreviousPosition(float deltaTime)
    {
        return currentPosition - (direction * speed * deltaTime);
    }
}