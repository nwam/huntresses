using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodPool {

    // Singleton
    private static BloodPool instance;

    public static BloodPool Instance() {
        if(instance == null) {
            instance = new BloodPool();
        }
        return instance;
    }
    
    public const int CAPACITY = 100;
    // amount of blood in the pool - starts full, cannot go below 0
    private int available = CAPACITY;

    public bool Withdraw(int amount) {
        if(amount > available) {
            available -= amount;
            return true;
        }
        else {
            // Not enough available
            return false;
        }        
    }

    public void Fill(int amount) {
        available += amount;
        if(available > CAPACITY) {
            available = CAPACITY;
        }
    }

}
