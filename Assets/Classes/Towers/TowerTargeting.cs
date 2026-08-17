using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class TowerTargeting
{
    public enum TargetType
    {
        First,
        Last,
        Close,
        HighArmor,
        LowArmor,
        HighHP,
        LowHP
    }

    public static Enemy GetTarget(TowerBehavior tower, TargetType targetingType)
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(
            tower.transform.position,
            tower.Range,
            tower.EnemiesLayer
        );

        if (enemiesInRange.Length == 0) return null;

        Enemy bestTarget = null;
        float bestValue = float.PositiveInfinity;
        float bestValue2 = float.PositiveInfinity;
        if (targetingType == TargetType.Last)
        {
            bestValue = float.NegativeInfinity;
        }
        if (targetingType == TargetType.HighArmor || targetingType == TargetType.HighHP)
        {
            bestValue2 = float.NegativeInfinity;
        }

        foreach (Collider collider in enemiesInRange)
        {
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy == null || enemy.NodeIndex >= GameLoopManager.ListOfNodePositions[enemy.Path].Length || enemy.IsDead)
                continue;

            float value;
            float value2;

            switch (targetingType)
            {
                case TargetType.First:
                    value = GetDistanceToEnd(enemy);
                    if (value < bestValue)
                    {
                        bestValue = value;
                        bestTarget = enemy;
                    }
                    break;

                case TargetType.Last:
                    value = GetDistanceToEnd(enemy);
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestTarget = enemy;
                    }
                    break;

                case TargetType.Close:
                    value = Vector3.Distance(tower.transform.position, enemy.transform.position);
                    if (value < bestValue)
                    {
                        bestValue = value;
                        bestTarget = enemy;
                    }
                    break;

                case TargetType.HighArmor:
                    value2 = enemy.Armor;
                    if (value2 > bestValue2)
                    {
                        bestValue2 = value2;
                        bestTarget = enemy;
                        bestValue = GetDistanceToEnd(enemy);
                    }
                    else if (value2 == bestValue2)
                    {
                        value = GetDistanceToEnd(enemy);
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = enemy;
                        }
                    }
                    break;

                case TargetType.LowArmor:
                    value2 = enemy.Armor;
                    if (value2 < bestValue2)
                    {
                        bestValue2 = value2;
                        bestTarget = enemy;
                        bestValue = GetDistanceToEnd(enemy);
                    }
                    else if (value2 == bestValue2)
                    {
                        value = GetDistanceToEnd(enemy);
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = enemy;
                        }
                    }
                    break;
                case TargetType.HighHP:
                    value2 = enemy.Health;
                    if (value2 > bestValue2)
                    {
                        bestValue2 = value2;
                        bestTarget = enemy;
                        bestValue = GetDistanceToEnd(enemy);
                    }
                    else if (value2 == bestValue2)
                    {
                        value = GetDistanceToEnd(enemy);
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = enemy;
                        }
                    }
                    break;
                case TargetType.LowHP:
                    value2 = enemy.Health;
                    if (value2 < bestValue2)
                    {
                        bestValue2 = value2;
                        bestTarget = enemy;
                        bestValue = GetDistanceToEnd(enemy);
                    }
                    else if (value2 == bestValue2)
                    {
                        value = GetDistanceToEnd(enemy);
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = enemy;
                        }
                    }
                    break;
            }
        }

        return bestTarget;
    }

    private static float GetDistanceToEnd(Enemy enemy)
    {
        int CurrPath = enemy.Path;
        float distance = Vector3.Distance(enemy.transform.position, GameLoopManager.ListOfNodePositions[CurrPath][enemy.NodeIndex]);

        float[] CurrNodeDistances = GameLoopManager.ListOfNodeDistances[CurrPath];
        for (int i = enemy.NodeIndex; i < CurrNodeDistances.Length; i++)
        {
            distance += CurrNodeDistances[i];
        }

        return distance;
    }
}