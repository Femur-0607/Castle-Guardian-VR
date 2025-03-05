using UnityEngine;
using System;

public class WaveEventArgs : EventArgs 
{
    public int WaveNumber { get; }
    public int EnemyCount { get; }
    
    public WaveEventArgs(int waveNumber, int enemyCount) 
    {
        WaveNumber = waveNumber;
        EnemyCount = enemyCount;
    }
}
