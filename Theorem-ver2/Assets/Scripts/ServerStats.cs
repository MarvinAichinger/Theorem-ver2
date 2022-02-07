using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ServerStats : NetworkBehaviour
{
    public NetworkConnection player1;
    public NetworkConnection player2;
    public bool isSecondPlayer = false;
}
