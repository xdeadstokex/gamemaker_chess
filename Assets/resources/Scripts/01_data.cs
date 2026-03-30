using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class data : MonoBehaviour {
    public static data mem;
    public GameObject chesspiece; // reserve a space for later assign of unity chesspiece obj/prefab

    void Awake() {
        mem = this;
    }
}